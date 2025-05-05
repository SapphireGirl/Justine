using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Xunit;
using System.Net;
using Newtonsoft.Json;

namespace Justine.Lambdas.Tests;

public class FunctionsTest
{
    public FunctionsTest()
    {
    }

    [Fact]
    public void GetProductByIdReturnsProduct()
    {
        var context = new TestLambdaContext();
        var functions = new Functions();

        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "GET",
            Path = "/",
            Body = "{\"Id\":\"12345\"}"
        };
        
        var response = functions.GetProductById(request);

        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

        // Deserialize the response body to check its content
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Body);


        // Assert that the "Id" property exists and matches the expected value

        Assert.Contains("12345", response.Body);
        //Assert.Equal("\"Id\":\"12345\"", response.Body);
    }
}
