using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.KeyManagementService;
using Amazon.SecurityToken;
using Justine.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;

namespace Justine.LambdaWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

            // Let the AWS extensions pick up AWS:Region / AWS:ServiceURL from configuration
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

            // Register AWS services used by the app (these will be constructed with the AWS options above)
            services.AddAWSService<IAmazonCognitoIdentityProvider>();
            services.AddAWSService<IAmazonSecurityTokenService>(); // used to validate credentials locally
            services.AddAWSService<IAmazonKeyManagementService>();
            services.AddAWSService<IAmazonDynamoDB>(); // <-- REGISTERED DynamoDB client

            services.AddSingleton<IEncryptionService, KmsEncryptionService>();

            services.AddTransient<IProductServices, ProductServices>();
            services.AddTransient<IBasketServices, BasketServices>();
            services.AddTransient<IOrderServices, OrderServices>();
            services.AddTransient<IAdminServices, AdminServices>();

            // Minimal CORS policy for local Vite dev server (development-only)
            services.AddCors(options =>
            {
                options.AddPolicy("LocalDev", builder =>
                {
                    builder
                        .WithOrigins("https://localhost:5173", "http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // Cognito settings
            var region = Configuration["Cognito:Region"] ?? "us-east-1";
            var userPoolId = Configuration["Cognito:UserPoolId"] ?? throw new InvalidOperationException("Cognito:UserPoolId is required");
            var appClientId = Configuration["Cognito:AppClientId"] ?? throw new InvalidOperationException("Cognito:AppClientId is required");
            var issuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = issuer;
                    options.RequireHttpsMetadata = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var authHeader = context.Request.Headers["Authorization"].ToString();
                            var readFromCookie = false;
                            if (string.IsNullOrEmpty(authHeader))
                            {
                                var cookieToken = context.Request.Cookies["access_token"];
                                if (!string.IsNullOrEmpty(cookieToken))
                                {
                                    context.Token = cookieToken;
                                    readFromCookie = true;
                                }
                            }

                            var logger = context.HttpContext.RequestServices.GetService<ILogger<Startup>>();
                            logger?.LogDebug(
                                "OnMessageReceived: hasAuthHeader={HasAuthHeader}, tokenLength={Len}, readFromCookie={ReadFromCookie}",
                                !string.IsNullOrEmpty(authHeader),
                                context.Token?.Length ?? 0,
                                readFromCookie);

                            return Task.CompletedTask;
                        },

                        OnTokenValidated = context =>
                        {
                            var clientIdClaim = context.Principal?.FindFirst("client_id")?.Value;
                            var tokenUse = context.Principal?.FindFirst("token_use")?.Value;

                            var logger = context.HttpContext.RequestServices.GetService<ILogger<Startup>>();
                            logger?.LogDebug("OnTokenValidated: client_id={ClientId}, token_use={TokenUse}", clientIdClaim, tokenUse);

                            if (string.IsNullOrEmpty(clientIdClaim) || clientIdClaim != appClientId)
                            {
                                logger?.LogWarning("Invalid client_id claim: {ClientId}", clientIdClaim);
                                context.Fail("Invalid client_id");
                                return Task.CompletedTask;
                            }

                            if (!string.Equals(tokenUse, "access", StringComparison.OrdinalIgnoreCase))
                            {
                                logger?.LogWarning("Token use is not 'access': {TokenUse}", tokenUse);
                                context.Fail("Invalid token use");
                                return Task.CompletedTask;
                            }

                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetService<ILogger<Startup>>();
                            logger?.LogError(context.Exception, "Jwt authentication failed: {Message}", context.Exception.Message);
                            return Task.CompletedTask;
                        }
                    };
                });

            // Ensure authorization services are registered (required for [Authorize] attributes)
            services.AddAuthorization();

            services.AddSwaggerGen(options => { /* ...existing config... */ });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var forwardedOptions = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedProto };
            forwardedOptions.KnownNetworks.Clear();
            forwardedOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedOptions);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Enable CORS BEFORE authentication/authorization
            app.UseCors("LocalDev");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("/login.html");
            });
        }
    }
}