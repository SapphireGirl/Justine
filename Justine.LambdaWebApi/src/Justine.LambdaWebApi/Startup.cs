using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Justine.Common.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;

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

            // Swagger (optional)
            services.AddSwaggerGen(options => { /* ...existing config... */ });
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

            //// Serve static login page from wwwroot/login.html
            //var webRoot = Path.Combine(env.ContentRootPath, "wwwroot");
            //var fileProvider = new PhysicalFileProvider(webRoot);
            //var defaultFilesOptions = new DefaultFilesOptions { FileProvider = fileProvider };
            //defaultFilesOptions.DefaultFileNames.Clear();
            //defaultFilesOptions.DefaultFileNames.Add("login.html");
            //app.UseDefaultFiles(defaultFilesOptions);
            //app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // API endpoints still available
                // All non-API requests return login.html
                endpoints.MapFallbackToFile("/login.html");
            });

            //app.Run();
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello from Lambda!");
            //});
        }
    }
}