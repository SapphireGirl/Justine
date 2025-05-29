using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.DynamoDBv2;
using Justine.Common.Models;
using Newtonsoft.Json;
using Justine.Common.Services;
using Justine.Common.Exceptions;
using Amazon.DynamoDBv2.DataModel;

// Good DynamoDB Blog: https://www.rahulpnath.com/blog/aws-dynamodb-net-core/

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Justine.Lambdas
{

    // Notes: I had to install AWS SAM CLI to get the Lambda function to work.
    // For windows 11: https://github.com/aws/aws-sam-cli/releases/latest/download/AWS_SAM_CLI_64_PY3.msi
    // This will download the installer for the AWS SAM CLI to the download folder
    // in powershell, run sam --version to check if it is installed
    // I updated docker desktop to the latest version and used this installer to install WSL: wsl.2.4.13.0.x64.msi
    // from https://github.com/microsoft/WSL/releases/tag/2.4.13
    // to read the docker logs :  code $Env:LOCALAPPDATA\Docker\log
    // If Docker Desktop does not come up, go to the system tray and double click on the Docker icon
    // I am using Windows 11 Home edition :  Window's containers can only be utilized on Windows Pro edition or Enterprise edition.
    // Thus in Docker Desktop I cannot see the containers
    // Do I need to  install Amazon.Lambda.Tools? dotnet tool install -g Amazon.Lambda.Tools
    // Use Managed Runtime for Lambdas: AWS takes care of patching and updating the runtime for you.
    // AWS Queues: https://aws.amazon.com/blogs/compute/introducing-aws-lambda-managed-runtimes/
    // To deploy the Lambda function, I can use the AWS CLI
    // dotnet lambda deploy-serverless --stack-name Justine.Lambdas --s3-bucket cloudformation-templates-justine-lambdas
    // take address and open in browser

    // In the Justine.Lambdas project, I have added the following NuGet packages:  dotnet new intall Amazon.Lambda.Templates
    // to view all templates: dotnet new list --tag aws

    // Adding a new project to the solution: ASP.NET Core Web API for Lambda
    public class Functions
    {
        //private object _dynamoDbContext;
        private readonly IProductServices _productServices;
        private readonly IBasketServices _basketServices;
        private readonly IOrderServices _orderServices;

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions(IProductServices productServices,
                         IBasketServices basketServices,
                         IOrderServices orderServices)
        {
            // These services create the IDBynamoDBContext object
            _productServices = productServices;
            _basketServices = basketServices;
            _orderServices = orderServices;
        }


        /// <summary>
        /// Lambda function to get a product by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product to retrieve.</param>
        /// <returns>The product with the specified ID, or null if not found.</returns>

        public async Task<Product> GetProductById(int productId)
        {
            try
            {
                // LambdaLogger will log to Cloudwatch
                LambdaLogger.Log($"GetProductById: Id: {productId}");
                var product = await _productServices.GetProductByIdAsync(productId);

                return product;
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"GetProductById Lambda: Id: {productId}");
                throw new ProductException($"Error getting Product with id {productId} failed: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            try
            {
                // LambdaLogger will log to Cloudwatch
                LambdaLogger.Log($"GetAllProducts Lambda: ");

                var products = await _productServices.GetAllProductsAsync();

                return products;
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"Error GetAllProducts Lambda failed: Error: {ex.Message}");

                throw new ProductException($"GetAllProducts Lambda failed: Error: {ex.Message}");
            }
        }
    }
}