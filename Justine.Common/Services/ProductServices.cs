using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Newtonsoft.Json;
using System.Net;

namespace Justine.Common.Services
{
    public class ProductServices : IProductServices
    {
        private readonly IAmazonDynamoDB _context;
        private const string TableName = "Products";

        // Notes on How to work with DynamoDB
        // For saving, Querying, and deleting items, you can use the IDynamoDBContext interface.
        // For creating tables, deleting tables, creating global indexes you can use the IAmazonDynamoDB interface.
        public ProductServices(IAmazonDynamoDB context)
        {
            _context = context;
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            try
            {
                product.CreatedAt = DateTime.UtcNow;

                var request = new PutItemRequest
                {
                    TableName = TableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "ProductId", new AttributeValue { N = product.ProductId.ToString() } },
                        { "Name", new AttributeValue { S = product.Name } },
                        { "Description", new AttributeValue { S = product.Description ?? string.Empty } },
                        { "Price", new AttributeValue { N = product.Price.ToString() } },
                        { "ImageUrl", new AttributeValue { S = product.ImageUrl ?? string.Empty } },
                        { "Quantity", new AttributeValue { N = product.Quantity.ToString() } },
                        { "CreatedAt", new AttributeValue { S = product.CreatedAt?.ToString("o") ?? string.Empty } },
                        { "UpdatedAt", new AttributeValue { S = product.UpdatedAt?.ToString("o") ?? string.Empty } }
                    }
                };
                var response = await _context.PutItemAsync(request);

                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                // get type of exception
                var exceptionType = ex.GetType();

                var productJson = JsonConvert.SerializeObject(product);

                // To get the inner exception and stack trace for more detailed error information
                
                if (ex.ToString() != null)
                {
                    throw new ProductException($"Error adding Product {productJson} \n ERROR: Type {exceptionType} : {ex.ToString()}");
                }

                throw new ProductException($"Error adding Product {productJson} \n ERROR: Type {exceptionType} : {ex.Message}");
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            { 
                var deleteRequest = new DeleteItemRequest
                {
                    TableName = TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "ProductId", new AttributeValue { N = id.ToString() } }
                    },
                    ReturnValues = "ALL_OLD" // content of deleted item will be returned in response, can be used to verify deletion
                };
                var response = await _context.DeleteItemAsync(deleteRequest);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new ProductException($"Failed to delete product with id {id}. HTTP Status: {response.HttpStatusCode}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();

                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new ProductException($"Error deleting Product with id {id}: Type {exceptionType} : {ex.ToString()}");
                }

                throw new ProductException($"Error deleting Product with id  {id}:  Type {exceptionType} : {ex.Message}");
            }
        }

        // Used to populate the front end Product page
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {

                var scanRequest = new ScanRequest
                {
                    TableName = TableName,
                };
                var response = await _context.ScanAsync(scanRequest);
                return response.Items
                    .Select(item => new Product
                    {
                        ProductId = int.Parse(item["ProductId"].N),
                        Name = item["Name"].S,
                        Description = item["Description"].S,
                        Price = decimal.Parse(item["Price"].N),
                        ImageUrl = item["ImageUrl"].S,
                        Quantity = int.Parse(item["Quantity"].N),
                        CreatedAt = DateTime.TryParse(item["CreatedAt"].S, out var createdAt) ? createdAt : (DateTime?)null,
                        UpdatedAt = DateTime.TryParse(item["UpdatedAt"].S, out var updatedAt) ? updatedAt : (DateTime?)null
                    })
                    .ToList();
            }
            catch (ResourceNotFoundException resourceNotFoundException)
            {
                // Table missing — return 404 so client can show admin/seed UI
                var msg = $"Products table not found: {resourceNotFoundException.Message}";
                LambdaLogger.Log(msg);
                throw new ResourceNotFoundException($"Products table not found: {resourceNotFoundException.Message}");
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();

                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new ProductException($"Error getting all Products: Type {exceptionType} : {ex.ToString()}");
                }

                throw new ProductException($"Error getting all Products: Type {exceptionType} : {ex.Message}");
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            try
            {
                var getRequest = new GetItemRequest
                {
                    TableName = TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "ProductId", new AttributeValue { N = id.ToString() } }
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
                    return new Product
                    {
                        ProductId = int.Parse(item["ProductId"].N),
                        Name = item["Name"].S,
                        Description = item["Description"].S,
                        Price = decimal.Parse(item["Price"].N),
                        ImageUrl = item["ImageUrl"].S,
                        Quantity = int.Parse(item["Quantity"].N),
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
                    throw new ProductException($"Error getting Product with id {id} failed: Type {exceptionType} : {ex.ToString()}");
                }

                throw new ProductException($"Error getting Product with id {id} failed: Type {exceptionType} : {ex.Message}");
            }   
        }

        public async Task<Product> UpdateProductAsync(Product productRequest)
        {
            try
            {
                productRequest.UpdatedAt = DateTime.UtcNow;
                var productJson = JsonConvert.SerializeObject(productRequest);

                var request = new PutItemRequest
                {
                    TableName = TableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "ProductId", new AttributeValue { N = productRequest.ProductId.ToString() } },
                        { "Name", new AttributeValue { S = productRequest.Name } },
                        { "Description", new AttributeValue { S = productRequest.Description ?? string.Empty } },
                        { "Price", new AttributeValue { N = productRequest.Price.ToString() } },
                        { "ImageUrl", new AttributeValue { S = productRequest.ImageUrl ?? string.Empty } },
                        { "Quantity", new AttributeValue { N = productRequest.Quantity.ToString() } },
                        { "CreatedAt", new AttributeValue { S = productRequest.CreatedAt?.ToString("o") ?? string.Empty } },
                        { "UpdatedAt", new AttributeValue { S = productRequest.UpdatedAt?.ToString("o") ?? string.Empty } }
                    }
                };
                var response = await _context.PutItemAsync(request);

                return response.HttpStatusCode == HttpStatusCode.OK ? productRequest : throw new ProductException($"Failed to update Product with id {productRequest.ProductId}. HTTP Status: {response.HttpStatusCode}");
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();

                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new ProductException($"Error updating Product with id {productRequest.ProductId} failed: Type {exceptionType}: {ex.ToString()}");
                }

                throw new ProductException($"Error updating Product with id {productRequest.ProductId} failed: Type {exceptionType}: {ex.Message}");
            }
        }

        public async Task<IList<Product>> GetAllProductsAsync(CancellationToken ct = default)
        {
            var request = new ScanRequest
            {
                TableName = TableName,
            };
            var response = await _context.ScanAsync(request);

            return response.Items.Count == 0 ? new List<Product>() : response.Items
                .Select(item => new Product
                {
                    ProductId = int.Parse(item["ProductId"].N),
                    Name = item["Name"].S,
                    Description = item["Description"].S,
                    Price = decimal.Parse(item["Price"].N),
                    ImageUrl = item["ImageUrl"].S,
                    Quantity = int.Parse(item["Quantity"].N),
                    CreatedAt = DateTime.TryParse(item["CreatedAt"].S, out var createdAt) ? createdAt : (DateTime?)null,
                    UpdatedAt = DateTime.TryParse(item["UpdatedAt"].S, out var updatedAt) ? updatedAt : (DateTime?)null
                })
                .ToList();

        }

        public static async Task<bool> CreateProductTableAsync(AmazonDynamoDBClient client, string tableName)
        {
            try
            {


                var response = await client.CreateTableAsync(new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition
                    {
                        AttributeName = "title",
                        AttributeType = ScalarAttributeType.S,
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "year",
                        AttributeType = ScalarAttributeType.N,
                    },
                },
                    KeySchema = new List<KeySchemaElement>()
                {
                    new KeySchemaElement
                    {
                        AttributeName = "year",
                        KeyType = KeyType.HASH,
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "title",
                        KeyType = KeyType.RANGE,
                    },
                },
                    BillingMode = BillingMode.PAY_PER_REQUEST,
                });

                // Wait until the table is ACTIVE and then report success.
                Console.Write("Waiting for table to become active...");

                var request = new DescribeTableRequest
                {
                    TableName = response.TableDescription.TableName,
                };

                TableStatus status;

                int sleepDuration = 2000;

                do
                {
                    System.Threading.Thread.Sleep(sleepDuration);

                    var describeTableResponse = await client.DescribeTableAsync(request);
                    status = describeTableResponse.Table.TableStatus;

                    Console.Write(".");
                }
                while (status != "ACTIVE");

                return status == TableStatus.ACTIVE;
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new ProductException($"Error creating table {tableName}: {ex.ToString()}");
                }

                throw new ProductException($"Error creating table {tableName}: {ex.Message}");
            }
        }
    }
}
