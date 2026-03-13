using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;

namespace Justine.Common.Services
{
    public class AdminServices : IAdminServices
    {
        private readonly IAmazonDynamoDB _dynamo;
        private const int MaxSeedRetries = 5;

        public AdminServices(IAmazonDynamoDB dynamo)
        {
            _dynamo = dynamo;
        }

        public Task CreateProductTableAsync()
            => CreateTableAsync("Products", "ProductId", ScalarAttributeType.N, "Name", ScalarAttributeType.S, seed: true);

        public Task CreateBasketTableAsync()
            => CreateTableAsync("Baskets", "BasketId", ScalarAttributeType.N, "CustomerName", ScalarAttributeType.S, seed: false);

        public Task CreateOrderTableAsync()
            => CreateTableAsync("Orders", "OrderId", ScalarAttributeType.N, "CustomerName", ScalarAttributeType.S, seed: false);

        public async Task<bool> CreateTableAsync(string tableName,
                                                 string primaryKeyName,
                                                 ScalarAttributeType primaryKeyType,
                                                 string? sortKeyName = null,
                                                 ScalarAttributeType? sortKeyType = null,
                                                 bool seed = false)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(primaryKeyName))
            {
                throw new ArgumentException("tableName and primaryKeyName are required.");
            }

            try
            {
                // Check existence safely
                bool tableExists;
                try
                {
                    var desc = await _dynamo.DescribeTableAsync(new DescribeTableRequest { TableName = tableName });
                    tableExists = desc?.Table?.TableStatus == TableStatus.ACTIVE;
                }
                catch (ResourceNotFoundException)
                {
                    tableExists = false;
                }

                if (!tableExists)
                {
                    var attrDefs = new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                        {
                            AttributeName = primaryKeyName,
                            AttributeType = primaryKeyType
                        }
                    };

                    var keySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = primaryKeyName,
                            KeyType = KeyType.HASH
                        }
                    };

                    if (!string.IsNullOrWhiteSpace(sortKeyName))
                    {
                        attrDefs.Add(new AttributeDefinition
                        {
                            AttributeName = sortKeyName!,
                            AttributeType = sortKeyType.Value
                        });

                        keySchema.Add(new KeySchemaElement
                        {
                            AttributeName = sortKeyName!,
                            KeyType = KeyType.RANGE
                        });
                    }

                    var createReq = new CreateTableRequest
                    {
                        TableName = tableName,
                        AttributeDefinitions = attrDefs,
                        KeySchema = keySchema,
                        BillingMode = BillingMode.PAY_PER_REQUEST
                    };

                    await _dynamo.CreateTableAsync(createReq);

                    // wait for ACTIVE
                    var describeReq = new DescribeTableRequest { TableName = tableName };
                    TableStatus status;
                    do
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2));
                        var descResp = await _dynamo.DescribeTableAsync(describeReq);
                        status = descResp.Table.TableStatus;
                    } while (status != TableStatus.ACTIVE);
                }

                if (seed && tableName.Equals("Products", StringComparison.OrdinalIgnoreCase))
                {
                    await SeedProductsAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"CreateTableAsync failed for {tableName}: {ex}");
                throw;
            }
        }

        public async Task SeedProductsAsync()
        {
            var now = DateTime.UtcNow.ToString("o");
            var TableName = "Products";

            var sampleProducts = new List<Dictionary<string, AttributeValue>>
            {
                new()
                {
                    ["ProductId"] = new AttributeValue { N = "1" },
                    ["Name"] = new AttributeValue { S = "Justine Mug" },
                    ["Description"] = new AttributeValue { S = "A mug for the dedicated developer." },
                    ["Price"] = new AttributeValue { N = "12.95" },
                    ["ImageUrl"] = new AttributeValue { S = "" },
                    ["Quantity"] = new AttributeValue { N = "15" },
                    ["CreatedAt"] = new AttributeValue { S = now },
                    ["UpdatedAt"] = new AttributeValue { S = now }
                },
                new()
                {
                    ["ProductId"] = new AttributeValue { N = "2" },
                    ["Name"] = new AttributeValue { S = "Justine T-Shirt" },
                    ["Description"] = new AttributeValue { S = "Comfortable cotton tee." },
                    ["Price"] = new AttributeValue { N = "19.99" },
                    ["ImageUrl"] = new AttributeValue { S = "" },
                    ["Quantity"] = new AttributeValue { N = "30" },
                    ["CreatedAt"] = new AttributeValue { S = now },
                    ["UpdatedAt"] = new AttributeValue { S = now }
                },
                new()
                {
                    ["ProductId"] = new AttributeValue { N = "3" },
                    ["Name"] = new AttributeValue { S = "Justine Sticker Pack" },
                    ["Description"] = new AttributeValue { S = "Decorate your laptop." },
                    ["Price"] = new AttributeValue { N = "4.50" },
                    ["ImageUrl"] = new AttributeValue { S = "" },
                    ["Quantity"] = new AttributeValue { N = "100" },
                    ["CreatedAt"] = new AttributeValue { S = now },
                    ["UpdatedAt"] = new AttributeValue { S = now }
                }
            };

            var writeRequests = sampleProducts.Select(item => new WriteRequest { PutRequest = new PutRequest { Item = item } }).ToList();
            var requestItems = new Dictionary<string, List<WriteRequest>> { [TableName] = writeRequests };

            var batchRequest = new BatchWriteItemRequest { RequestItems = requestItems };
            var batchResponse = await _dynamo.BatchWriteItemAsync(batchRequest);

            int retries = 0;
            while (batchResponse.UnprocessedItems != null && batchResponse.UnprocessedItems.Count > 0 && retries < MaxSeedRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(1 + retries));
                batchResponse = await _dynamo.BatchWriteItemAsync(new BatchWriteItemRequest { RequestItems = batchResponse.UnprocessedItems });
                retries++;
            }
        }
        public Task SeedBasketsAsync()
        {
            var TableName = "Products";
            throw new NotImplementedException();
        }

        public Task SeedOrderAsync()
        {
            throw new NotImplementedException();
        }
        public async Task<bool> DeleteTableAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException(nameof(tableName));

            try
            {
                // If table doesn't exist, return false
                try
                {
                    await _dynamo.DescribeTableAsync(new DescribeTableRequest { TableName = tableName });
                }
                catch (ResourceNotFoundException)
                {
                    return false;
                }

                await _dynamo.DeleteTableAsync(new DeleteTableRequest { TableName = tableName });

                // wait until table is gone
                var describeReq = new DescribeTableRequest { TableName = tableName };
                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    try
                    {
                        var desc = await _dynamo.DescribeTableAsync(describeReq);
                        if (desc.Table.TableStatus == TableStatus.DELETING) continue;
                    }
                    catch (ResourceNotFoundException)
                    {
                        // deleted
                        break;
                    }
                } while (true);

                return true;
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"DeleteTableAsync failed for {tableName}: {ex}");
                throw;
            }
        }
        public Task<bool> DeleteProductTableAsync() => DeleteTableAsync("Products");
        public Task<bool> DeleteBasketTableAsync() => DeleteTableAsync("Baskets");
        public Task<bool> DeleteOrderTableAsync() => DeleteTableAsync("Orders");

        
    }
}
