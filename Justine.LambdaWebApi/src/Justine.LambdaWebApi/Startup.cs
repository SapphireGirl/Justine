using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Amazon.KeyManagementService;

using Justine.Common.Services;

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

            services.AddSingleton<IDynamoDBContext>(sp =>
            {
                var client = sp.GetRequiredService<IAmazonDynamoDB>();
                var builder = new DynamoDBContextBuilder();
                return builder.Build();
            });

            services.AddSingleton<IProductServices, ProductServices>();
            services.AddSingleton<IBasketServices, BasketServices>();
            services.AddSingleton<IOrderServices, OrderServices>();

            services.AddAWSService<IAmazonKeyManagementService>(); // AWS SDK extension method
            services.AddSingleton<IEncryptionService, KmsEncryptionService>();

            // Swagger (optional)
            services.AddSwaggerGen(options => { 
                /* ...existing config... */
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"XXXXXXXXXXXXXXXXXXX.{Configuration["AWS:Region"]}.amazonaws.com/{Configuration["Cognito:UserPoolId"]}";
                    options.Audience = Configuration["Cognito:UserPoolId"];
                });});
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Respect X-Forwarded-Proto from API GW / CloudFront so HTTPS redirection works behind proxy
            var forwardedOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            };
            // allow forwarded headers from any proxy (CloudFront/API Gateway)
            forwardedOptions.KnownNetworks.Clear();
            forwardedOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardedOptions);

            if (!env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // API endpoints still available
                // All non-API requests return login.html
                endpoints.MapFallbackToFile("/login.html");
            });
        }
    }
}