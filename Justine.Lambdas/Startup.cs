using Amazon.Lambda.Annotations;
using Amazon.S3;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Justine.Lambdas.Extensions;
using Justine.Common.Models;
using Justine.Common.Services;

namespace Justine.Lambdas;

// Notes
// Amazon.Lambda.AspNetCoreServer.Hosting Package restore failed. Rolling back package changes for 'Justine.LambdaWebApi'
[LambdaStartup]
public class Startup
{
    /// <summary>
    /// Services for Lambda functions can be registered in the services dependency injection container in this method. 
    ///
    /// The services can be injected into the Lambda function through the containing type's constructor or as a
    /// parameter in the Lambda function using the FromService attribute. Services injected for the constructor have
    /// the lifetime of the Lambda compute container. Services injected as parameters are created within the scope
    /// of the function invocation.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        // Singleton?
        services.AddSingleton<IProductServices, ProductServices>();
        services.AddSingleton<IBasketServices, BasketServices>();
        services.AddSingleton<IOrderServices, OrderServices>();

        //// Example of creating the IConfiguration object and
        //// adding it to the dependency injection container.
        var builder = new ConfigurationBuilder().Build();
                            //.AddJsonFile("appsettings.json", true);

        //// Add AWS Systems Manager as a potential provider for the configuration. This is 
        //// available with the Amazon.Extensions.Configuration.SystemsManager NuGet package.
        //builder.AddSystemsManager("/app/settings");

        //var configuration = builder.Build();
        //services.AddSingleton<IConfiguration>(configuration);

        //// Example of using the AWSSDK.Extensions.NETCore.Setup NuGet package to add
        //// the Amazon S3 service client to the dependency injection container.
        services.AddAWSService<IAmazonS3>();
        services.AddAWSService<IAmazonDynamoDB>();

        
    }
}
