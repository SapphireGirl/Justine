using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Newtonsoft.Json;
using System.Net;

namespace Justine.Common.Services
{
    public class BasketServices : IBasketServices
    {
        private readonly IAmazonDynamoDB _context;
        private const string TableName = "Baskets";
        public BasketServices(IAmazonDynamoDB context)
        {
            _context = context;
        }

        public async Task<Basket> GetBasketByIdAsync(int basketId)
        {
            try
            {
                var getRequest = new GetItemRequest
                {
                    TableName = TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "BasketId", new AttributeValue { N = basketId.ToString() } }
                    }
                };

                var response = await _context.GetItemAsync(getRequest);

                if (response.Item.Count == 0)
                {
                    return null;
                }
                else
                {
                    var item = response.Item;
                    return new Basket
                    {
                        BasketId = int.Parse(item["BasketId"].N),
                        CustomerName = item["CustomerName"].S,
                        Products = JsonConvert.DeserializeObject<List<Product>>(item["Products"].S) ?? new List<Product>(),
                        CreatedAt = DateTime.TryParse(item["CreatedAt"].S, out var createdAt) ? createdAt : (DateTime?)null,
                        UpdatedAt = DateTime.TryParse(item["UpdatedAt"].S, out var updatedAt) ? updatedAt : (DateTime?)null
                    };
                    
                }
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();

                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new ProductException($"Error getting Product with id {basketId} failed: Type {exceptionType} : {ex.ToString()}");
                }

                throw new ProductException($"Error getting Product with id {basketId} failed: Type {exceptionType} : {ex.Message}");
            }
        }

        public async Task<IEnumerable<Basket>> GetAllBasketsAsync()
        {
            try
            {

                var scanRequest = new ScanRequest
                {
                    TableName = TableName,
                };
                var response = await _context.ScanAsync(scanRequest);
                return response.Items
                    .Select(item => new Basket
                    {
                        BasketId = int.Parse(item["BasketId"].N),
                        CustomerName = item["CustomerName"].S,
                        Products = JsonConvert.DeserializeObject<List<Product>>(item["Products"].S) ?? new List<Product>(),
                        CreatedAt = DateTime.TryParse(item["CreatedAt"].S, out var createdAt) ? createdAt : (DateTime?)null,
                        UpdatedAt = DateTime.TryParse(item["UpdatedAt"].S, out var updatedAt) ? updatedAt : (DateTime?)null

                    });
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();

                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new BasketException($"Error getting all Products: Type {exceptionType} : {ex.ToString()}");
                }

                throw new BasketException($"Error getting all Products: Type {exceptionType} : {ex.Message}");
            }
        }

        public async Task<bool> AddBasketAsync(Basket basket)
        {
            try
            {
                basket.CreatedAt = DateTime.UtcNow;

                var request = new PutItemRequest
                {
                    TableName = TableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        {"BasketId", new AttributeValue { N = basket.BasketId.ToString() } },
                        { "CustomerName", new AttributeValue { S = basket.CustomerName } },
                        { "Products", new AttributeValue { S = JsonConvert.SerializeObject(basket.Products) } },
                        { "CreatedAt", new AttributeValue { S = basket.CreatedAt?.ToString("o") } },
                        { "UpdatedAt", new AttributeValue { S = basket.UpdatedAt?.ToString("o") } } 
                    }
                };

                var response = await _context.PutItemAsync(request);

                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                // get type of exception
                var exceptionType = ex.GetType();

                var productJson = JsonConvert.SerializeObject(basket);

                // To get the inner exception and stack trace for more detailed error information

                if (ex.ToString() != null)
                {
                    throw new BasketException($"Error adding Product {productJson} \n ERROR: Type {exceptionType} : {ex.ToString()}");
                }

                throw new BasketException($"Error adding Product {productJson} \n ERROR: Type {exceptionType} : {ex.Message}");
            }
        }

        public async Task<Basket> UpdateBasketAsync(Basket basketRequest)
        {
            try
            {
                basketRequest.UpdatedAt = DateTime.UtcNow;
                var productJson = JsonConvert.SerializeObject(basketRequest);

                var request = new PutItemRequest
                {
                    TableName = TableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "BasketId", new AttributeValue { N = basketRequest.BasketId.ToString() } },
                        { "CustomerName", new AttributeValue { S = basketRequest.CustomerName } },
                        { "Products", new AttributeValue { S = JsonConvert.SerializeObject(basketRequest.Products) } },
                        { "CreatedAt", new AttributeValue { S = basketRequest.CreatedAt?.ToString("o") } },
                        { "UpdatedAt", new AttributeValue { S = basketRequest.UpdatedAt?.ToString("o") } }
                    }
                };
                var response = await _context.PutItemAsync(request);

                return response.HttpStatusCode == HttpStatusCode.OK ? basketRequest : throw new ProductException($"Failed to update Product with id {basketRequest.BasketId}. HTTP Status: {response.HttpStatusCode}");
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();

                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new BasketException($"Error updating Product with id {basketRequest.BasketId} failed: Type {exceptionType}: {ex.ToString()}");
                }

                throw new BasketException($"Error updating Product with id {basketRequest.BasketId} failed: Type {exceptionType}: {ex.Message}");
            }
        }

        public async Task<bool> DeleteBasketAsync(int basketId)
        {
            try
            {
                var deleteRequest = new DeleteItemRequest
                {
                    TableName = TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "BasketId", new AttributeValue { N = basketId.ToString() } }
                    },
                    ReturnValues = "ALL_OLD" // content of deleted item will be returned in response, can be used to verify deletion
                };
                var response = await _context.DeleteItemAsync(deleteRequest);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new ProductException($"Failed to delete product with id {basketId}. HTTP Status: {response.HttpStatusCode}");
                }

                return true;
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();

                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new ProductException($"Error deleting Product with id {basketId}: Type {exceptionType} : {ex.ToString()}");
                }

                throw new ProductException($"Error deleting Product with id  {basketId}:  Type {exceptionType} : {ex.Message}");
            }
        }

        public async Task<IEnumerable<Basket>> GetUsersBasketsByNameAsync(string customerName)
        {
            try
            {
                var queryRequest = new QueryRequest
                {
                    TableName = TableName,
                    IndexName = "CustomerName-index",
                    KeyConditionExpression = "CustomerName = :v_customerName",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":v_customerName", new AttributeValue { S = customerName } }
                    }
                };

                var response = await _context.QueryAsync(queryRequest);
                return response.Items
                    .Select(item => new Basket
                    {
                        BasketId = int.Parse(item["BasketId"].N),
                        CustomerName = item["CustomerName"].S,
                        Products = JsonConvert.DeserializeObject<List<Product>>(item["Products"].S) ?? new List<Product>(),
                        CreatedAt = DateTime.TryParse(item["CreatedAt"].S, out var createdAt) ? createdAt : (DateTime?)null,
                        UpdatedAt = DateTime.TryParse(item["UpdatedAt"].S, out var updatedAt) ? updatedAt : (DateTime?)null
                    });
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new BasketException($"Error getting all Baskets with customer Name: {customerName} {ex.ToString()}");
                }

                throw new BasketException($"Error getting all Baskets with customer Name: {customerName} {ex.Message}");
            }
        }
    }
}
