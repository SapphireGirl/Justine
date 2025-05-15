using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.DynamoDBv2;
using Justine.Common.Models;
using Newtonsoft.Json;
using Justine.Common.Services;

// Good DynamoDB Blog: https://www.rahulpnath.com/blog/aws-dynamodb-net-core/

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Justine.Lambdas;

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
    /// A Lambda function to respond to HTTP Get methods from API Gateway
    /// </summary>
    /// <remarks>
    /// This uses the <see href="https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.Annotations/README.md">Lambda Annotations</see> 
    /// programming model to bridge the gap between the Lambda programming model and a more idiomatic .NET model.
    /// 
    /// This automatically handles reading parameters from an <see cref="APIGatewayProxyRequest"/>
    /// as well as syncing the function definitions to serverless.template each time you build.
    /// 
    /// If you do not wish to use this model and need to manipulate the API Gateway 
    /// objects directly, see the accompanying Readme.md for instructions.
    /// </remarks>
    /// <param name="context">Information about the invocation, function, and execution environment</param>
    /// <returns>The response as an implicit <see cref="APIGatewayProxyResponse"/></returns>
    [LambdaFunction(PackageType = LambdaPackageType.Image)]
    [RestApi(LambdaHttpMethod.Get, "/")]
    public APIGatewayProxyResponse GetProductById(APIGatewayProxyRequest request)
    {
        try
        {
            // LambdaLogger will log to Cloudwatch
            LambdaLogger.Log($"GetProductById: Request: {request}");
            // Get Id from request
            var productId = JsonConvert.DeserializeObject<Product>(request.Body).Id;
            // Get product from DynamoDB

            //var product = _dynamoDbContext.LoadAsync<Product>(productId).Result;

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(request.Body),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }


            };
        }
        catch(JsonException e)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = JsonConvert.SerializeObject(new { error = $"An error occurred while executing the function: {e.Message}" }),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
    }
}
