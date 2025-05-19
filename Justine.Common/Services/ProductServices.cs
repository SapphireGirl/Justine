using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Newtonsoft.Json;

namespace Justine.Common.Services
{
    public class ProductServices : IProductServices
    {
        private readonly IDynamoDBContext _context;
        private const string TableName = "Products";

        // Notes on How to work with DynamoDB
        // For saving, Querying, and deleting items, you can use the IDynamoDBContext interface.
        // For creating tables, deleting tables, creating global indexes you can use the IAmazonDynamoDB interface.
        public ProductServices(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            try
            {
                await _context.SaveAsync(product);

                var returnProduct = await _context.LoadAsync<Product>(product.Id);

                return returnProduct;
            }
            catch (Exception ex)
            {
                // get type of exception
                var exceptionType = ex.GetType();

                var productJson = JsonConvert.SerializeObject(product);
                throw new ProductException($"Error adding Product {productJson} \n ERROR: {exceptionType}: {ex.Message}");
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            { 
                var product = await _context.LoadAsync<Product>(id);
                if (product == null)
                {
                    throw new ProductException($"Product with id {id} not found.");
                }

                await _context.DeleteAsync(product);
                return true;
            }
            catch (Exception ex)
            {
                throw new ProductException($"Error deleting Product with id  {id}: {ex.Message}", ex);
            }
        }

        // Used to populate the front end Product page
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {
                var products = await _context.ScanAsync<Product>(new List<ScanCondition>()).GetRemainingAsync();
                if (products == null) return new List<Product>();
                return products;

            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();
                throw new ProductException($"Error getting all Products: {exceptionType}:{ex.Message}", ex);
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _context.LoadAsync<Product>(id);
                if (product == null) return null;

                return product;
            }
            catch (Exception ex)
            {
                throw new ProductException($"Error getting Product with id {id} failed: {ex.Message}", ex);
            }   
        }

        public async Task<Product> UpdateProductAsync(Product productRequest)
        {
            try
            {
                var product = await _context.LoadAsync<Product>(productRequest.Id);
                // check if product exists
                if (product == null) return null;

                await _context.SaveAsync(productRequest);

                return productRequest;
            }
            catch (Exception ex)
            {
                throw new ProductException($"Error updating Product with id {productRequest.Id} failed: {ex.Message}", ex);
            }
        }

        


        /// <summary>
        /// Creates a new Amazon DynamoDB table and then waits for the new
        /// table to become active.
        /// </summary>
        /// <param name="client">An initialized Amazon DynamoDB client object.</param>
        /// <param name="tableName">The name of the table to create.</param>
        /// <returns>A Boolean value indicating the success of the operation.</returns>
        public static async Task<bool> CreateProductTableAsync(AmazonDynamoDBClient client, string tableName)
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
    }
}
