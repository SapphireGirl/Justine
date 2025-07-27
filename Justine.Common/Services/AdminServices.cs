using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Newtonsoft.Json;

namespace Justine.Common.Services
{
    public class AdminServices : IAdminServices
    {
        public readonly IAmazonDynamoDB _dynamoDbClient;

        public AdminServices(IAmazonDynamoDB dynamoDbClient)
        {
            _dynamoDbClient = dynamoDbClient ?? throw new ArgumentNullException(nameof(dynamoDbClient));
        }

        public async Task CreateProductTableAsync()
        {
            const string tableName = "Products";

            // Check if the table already exists
            var existingTables = await _dynamoDbClient.ListTablesAsync();
            if (existingTables.TableNames.Contains(tableName))
            {
                Console.WriteLine($"Table '{tableName}' already exists.");
                return;
            }

            // Define the table schema
            var createTableRequest = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions =
            [
                new AttributeDefinition
                {
                    AttributeName = "ProductId",
                    AttributeType = "S" // String type
                }
            ],
                KeySchema =
            [
                new KeySchemaElement
                {
                    AttributeName = "ProductId",
                    KeyType = "HASH" // Partition key
                }
            ],
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                }
            };

            // Create the table
            try
            {
                var response = await _dynamoDbClient.CreateTableAsync(createTableRequest);
                Console.WriteLine($"Table '{tableName}' created successfully. Status: {response.TableDescription.TableStatus}");

                // Wait for the table to become active
                await WaitForTableToBeActiveAsync(tableName);

                // Populate the table with initial data
                await PopulateProductTableAsync(tableName);
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new AdminException($"Error creating table '{tableName}': {ex.ToString()}");
                }

                throw new AdminException($"Failed to create table '{tableName}'. Error: {ex.Message}");
            }
        }

        public async Task CreateBasketTableAsync()
        {
            const string tableName = "Baskets";

            // Check if the table already exists
            var existingTables = await _dynamoDbClient.ListTablesAsync();
            if (existingTables.TableNames.Contains(tableName))
            {
                Console.WriteLine($"Table '{tableName}' already exists.");
                return;
            }

            // Define the table schema
            var createTableRequest = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions =
            [
                new AttributeDefinition
                {
                    AttributeName = "BasketId",
                    AttributeType = "S" // String type
                }
            ],
                KeySchema =
            [
                new KeySchemaElement
                {
                    AttributeName = "BasketId",
                    KeyType = "HASH" // Partition key
                }
            ],
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                }
            };

            // Create the table
            try
            {
                var response = await _dynamoDbClient.CreateTableAsync(createTableRequest);
                Console.WriteLine($"Table '{tableName}' created successfully. Status: {response.TableDescription.TableStatus}");

                // Wait for the table to become active
                await WaitForTableToBeActiveAsync(tableName);

                // Populate the table with initial data
                await PopulateBasketTableAsync(tableName);
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new AdminException($"Error creating table '{tableName}': {ex.ToString()}");
                }

                throw new AdminException($"Failed to create table '{tableName}'. Error: {ex.Message}");
            }
        }

        public async Task CreateOrderTableAsync()
        {
            const string tableName = "Orders";

            // Check if the table already exists
            var existingTables = await _dynamoDbClient.ListTablesAsync();
            if (existingTables.TableNames.Contains(tableName))
            {
                Console.WriteLine($"Table '{tableName}' already exists.");
                return;
            }

            // Define the table schema
            var createTableRequest = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions =
            [
                new() {
                    AttributeName = "OrderId",
                    AttributeType = "S" // String type
                },
                new() {
                    AttributeName = "UserId",
                    AttributeType = "S" // String type
                }
            ],
                KeySchema =
            [
                new KeySchemaElement
                {
                    AttributeName = "OrderId",
                    KeyType = "HASH" // Partition key
                },
                new KeySchemaElement
                {
                    AttributeName = "UserId",
                    KeyType = "RANGE" // Sort key
                }
            ],
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                }
            };

            // Create the table
            try
            {
                var response = await _dynamoDbClient.CreateTableAsync(createTableRequest);
                Console.WriteLine($"Table '{tableName}' created successfully. Status: {response.TableDescription.TableStatus}");

                // Wait for the table to become active
                await WaitForTableToBeActiveAsync(tableName);

                // Populate the table with initial data
                await PopulateOrderTableAsync(tableName);
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new AdminException($"Error creating table '{tableName}': {ex.ToString()}");
                }

                throw new AdminException($"Failed to create table '{tableName}'. Error: {ex.Message}");
            }
        }

        public Task<bool> DeleteAllLambdasAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteBasketTableAsync()
        {
            const string tableName = "Baskets";

            try
            {
                // Check if the table exists
                var existingTables = await _dynamoDbClient.ListTablesAsync();
                if (!existingTables.TableNames.Contains(tableName))
                {
                    Console.WriteLine($"Table '{tableName}' does not exist.");
                    return false;
                }

                // Delete the table
                var deleteTableRequest = new DeleteTableRequest
                {
                    TableName = tableName
                };

                var response = await _dynamoDbClient.DeleteTableAsync(deleteTableRequest);
                Console.WriteLine($"Table '{tableName}' deleted successfully. Status: {response.TableDescription.TableStatus}");

                return true;
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new AdminException($"Error deleting table '{tableName}': {ex.ToString()}");
                }

                throw new AdminException($"Failed to delete table '{tableName}'. Error: {ex.Message}");
            }
        }

        public async Task<bool> DeleteOrderTableAsync()
        {
            const string tableName = "Orders";

            try
            {
                // Check if the table exists
                var existingTables = await _dynamoDbClient.ListTablesAsync();
                if (!existingTables.TableNames.Contains(tableName))
                {
                    Console.WriteLine($"Table '{tableName}' does not exist.");
                    return false;
                }

                // Delete the table
                var deleteTableRequest = new DeleteTableRequest
                {
                    TableName = tableName
                };

                var response = await _dynamoDbClient.DeleteTableAsync(deleteTableRequest);
                Console.WriteLine($"Table '{tableName}' deleted successfully. Status: {response.TableDescription.TableStatus}");

                return true;
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new AdminException($"Error deleting table '{tableName}': {ex.ToString()}");
                }
                throw new AdminException($"Failed to delete table '{tableName}'. Error: {ex.Message}");
            }
        }

        public async Task<bool> DeleteProductTableAsync()
        {
            const string tableName = "Products";

            try
            {
                // Check if the table exists
                var existingTables = await _dynamoDbClient.ListTablesAsync();
                if (!existingTables.TableNames.Contains(tableName))
                {
                    Console.WriteLine($"Table '{tableName}' does not exist.");
                    return false;
                }

                // Delete the table
                var deleteTableRequest = new DeleteTableRequest
                {
                    TableName = tableName
                };

                var response = await _dynamoDbClient.DeleteTableAsync(deleteTableRequest);
                Console.WriteLine($"Table '{tableName}' deleted successfully. Status: {response.TableDescription.TableStatus}");

                return true;
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new AdminException($"Error deleting table '{tableName}': {ex.ToString()}");
                }
                throw new AdminException($"Failed to delete table '{tableName}'. Error: {ex.Message}");
            }
        }

        // private methods
        private async Task WaitForTableToBeActiveAsync(string tableName)
        {
            Console.WriteLine($"Waiting for table '{tableName}' to become active...");
            while (true)
            {
                var tableStatus = await _dynamoDbClient.DescribeTableAsync(new DescribeTableRequest
                {
                    TableName = tableName
                });

                if (tableStatus.Table.TableStatus == TableStatus.ACTIVE)
                {
                    Console.WriteLine($"Table '{tableName}' is now active.");
                    break;
                }

                await Task.Delay(5000); // Wait for 5 seconds before checking again
            }
        }

        private async Task PopulateProductTableAsync(string tableName)
        {
            var initialProducts = new List<Dictionary<string, AttributeValue>>
            {
                new() {
                    { "ProductId", new AttributeValue { S = "1" } },
                    { "Name", new AttributeValue { S = "Product A" } },
                    { "Description", new AttributeValue { S = "Description of Product A" } },
                    { "Price", new AttributeValue { N = "10.99" } },
                    { "Quantity", new AttributeValue { N = "100" } }
                },
                new() {
                    { "ProductId", new AttributeValue { S = "2" } },
                    { "Name", new AttributeValue { S = "Product B" } },
                    { "Description", new AttributeValue { S = "Description of Product B" } },
                    { "Price", new AttributeValue { N = "15.99" } },
                    { "Quantity", new AttributeValue { N = "200" } }
                }
            };

            foreach (var product in initialProducts)
            {
                try
                {
                    await _dynamoDbClient.PutItemAsync(new PutItemRequest
                    {
                        TableName = tableName,
                        Item = product
                    });
                    Console.WriteLine($"Inserted product with ID '{product["ProductId"].S}' into table '{tableName}'.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to insert product with ID '{product["ProductId"].S}'. Error: {ex.Message}");
                }
            }
        }

        private async Task PopulateBasketTableAsync(string tableName)
        {
            var initialBaskets = new List<Dictionary<string, AttributeValue>>
            {
                new() {
                    { "BasketId", new AttributeValue { S = "1" } },
                    { "UserId", new AttributeValue { S = "UserA" } },
                    { "Items", new AttributeValue { S = "[{\"ProductId\":\"1\",\"Quantity\":2},{\"ProductId\":\"2\",\"Quantity\":1}]" } }
                },
                new() {
                    { "BasketId", new AttributeValue { S = "2" } },
                    { "UserId", new AttributeValue { S = "UserB" } },
                    { "Items", new AttributeValue { S = "[{\"ProductId\":\"3\",\"Quantity\":5}]" } }
                }
            };

            foreach (var basket in initialBaskets)
            {
                try
                {
                    await _dynamoDbClient.PutItemAsync(new PutItemRequest
                    {
                        TableName = tableName,
                        Item = basket
                    });
                    Console.WriteLine($"Inserted basket with ID '{basket["BasketId"].S}' into table '{tableName}'.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to insert basket with ID '{basket["BasketId"].S}'. Error: {ex.Message}");
                }
            }
        }

        private async Task PopulateOrderTableAsync(string tableName)
        {
            var initialOrders = new List<Dictionary<string, AttributeValue>>
        {
            new() {
                { "OrderId", new AttributeValue { S = "1001" } },
                { "UserId", new AttributeValue { S = "UserA" } },
                { "OrderDate", new AttributeValue { S = "2023-10-01" } },
                { "Items", new AttributeValue { S = "[{\"ProductId\":\"1\",\"Quantity\":2},{\"ProductId\":\"2\",\"Quantity\":1}]" } },
                { "TotalAmount", new AttributeValue { N = "36.97" } }
            },
            new() {
                { "OrderId", new AttributeValue { S = "1002" } },
                { "UserId", new AttributeValue { S = "UserB" } },
                { "OrderDate", new AttributeValue { S = "2023-10-02" } },
                { "Items", new AttributeValue { S = "[{\"ProductId\":\"3\",\"Quantity\":5}]" } },
                { "TotalAmount", new AttributeValue { N = "79.95" } }
            }
        };

            foreach (var order in initialOrders)
            {
                try
                {
                    await _dynamoDbClient.PutItemAsync(new PutItemRequest
                    {
                        TableName = tableName,
                        Item = order
                    });
                    Console.WriteLine($"Inserted order with ID '{order["OrderId"].S}' into table '{tableName}'.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to insert order with ID '{order["OrderId"].S}'. Error: {ex.Message}");
                }
            }
        }
    }
}
