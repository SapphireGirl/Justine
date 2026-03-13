using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Justine.LambdaWebApi
{
    /// <summary>
    /// Extension methods to configure Cognito-based JWT authentication.
    /// Keeps authentication configuration separated from Startup for SOLID/separation of concerns.
    /// </summary>
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Adds Cognito JWT bearer authentication using values from configuration.
        /// Required configuration keys:
        /// - "AWS:Region"
        /// - "Cognito:UserPoolId"
        /// - "Cognito:AppClientId" (used as Audience). Falls back to UserPoolId if missing.
        /// </summary>
        public static IServiceCollection AddCognitoAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var region = configuration["AWS:Region"] ?? "us-east-1";
            var userPoolId = configuration["Cognito:UserPoolId"] ?? throw new ArgumentException("Cognito:UserPoolId configuration is required");
            var audience = configuration["Cognito:AppClientId"] ?? userPoolId;

            // Cognito issuer authority
            var authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.Audience = audience;
                    options.RequireHttpsMetadata = true;

                    // Optional: tune token validation parameters if needed
                    // options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    // {
                    //     ValidateIssuer = true,
                    //     ValidIssuer = authority,
                    //     ValidateAudience = true,
                    //     ValidAudience = audience
                    // };
                });

            return services;
        }

        /// <summary>
        /// Ensures authentication middleware is wired into the pipeline.
        /// Call this from Startup.Configure before UseAuthorization().
        /// </summary>
        public static IApplicationBuilder UseCognitoAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            return app;
        }
    }
}