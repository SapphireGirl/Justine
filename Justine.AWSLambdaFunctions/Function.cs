using System;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using Justine.Common.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Justine.AWSLambdaFunctions
{
    public class Function
    {
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            // To test use dotnet-lambda-test-tool-8.0 Tool in the package Manager console
            // Had to go to task manager and kill the process by PID
            // in order to build project after I shut down the test tool
            // Had to update AWS Toolkit to publish with Target .NET 8.0
            try
            {
                Product product = JsonConvert.DeserializeObject<Product>(request.Body);
                //product.Id = Guid.NewGuid();
                product.CreatedAt = DateTime.UtcNow;

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(product),
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    }
                };
            }
            catch (JsonException e)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonConvert.SerializeObject(new { error = $"An error occurred whild executing the function: {e.Message}" }),
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    }
                };
            }
            //var product = new Product
            //{
            //    "Id" = 1,
            //    "Name" = "Sample Product",
            //    "Description" = "This is a sample product.",
            //    "Price" = 19.99m,
            //    "ImageUrl" = "http://example.com/image.jpg",
            //    "CategoryId" = 1,
            //    "CreatedAt" = 09:35:04.6226114,
            //    "UpdatedAt" = 09:35:04.6226114
            //};

            //return JsonConvert.SerializeObject(product, Formatting.Indented)
        }

    }
}


