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
    public class OrderServices : IOrderServices
    {
        private readonly IAmazonDynamoDB _context;
        private const string TableName = "Orders";
        public OrderServices(IAmazonDynamoDB context)
        {
            _context = context;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            try
            {
                var getRequest = new GetItemRequest
                {
                    TableName = TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "OrderId", new AttributeValue { N = orderId.ToString() } }
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
                    return new Order
                    {
                        OrderId = int.Parse(item["OrderId"].N),
                        CustomerName = item["CustomerName"].S,
                        BasketId = int.Parse(item["BasketId"].N),
                        CreatedAt = DateTime.Parse(item["OrderDate"].S)
                    };
                }
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();

                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new OrderException($"Error getting Product with id {orderId} failed: Type {exceptionType} : {ex.ToString()}");
                }

                throw new OrderException($"Error getting Product with id {orderId} failed: Type {exceptionType} : {ex.Message}");
            }
        }

        public async Task<bool> AddOrderAsync(Order order)
        {
            try
            {

                order.CreatedAt = DateTime.UtcNow;

                var request = new PutItemRequest
                {
                    TableName = TableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "OrderId", new AttributeValue { N = order.OrderId.ToString() } },
                        { "CustomerName", new AttributeValue { S = order.CustomerName } },
                        { "BasketId", new AttributeValue { N = order.BasketId.ToString() } },
                        { "OrderDate", new AttributeValue { S = order.CreatedAt?.ToString("o") } }
                    }
                };

                var response = await _context.PutItemAsync(request);
                return response.HttpStatusCode == HttpStatusCode.OK;


            }
            catch (Exception ex)
            {
                var orderJson = JsonConvert.SerializeObject(order);

                // To get the inner exception and stack trace for more detailed error information

                if (ex.ToString() != null)
                {
                    throw new OrderException($"Error adding Order {orderJson} \n ERROR: {ex.ToString()}");
                }

                throw new OrderException($"Error adding Order {orderJson} \n ERROR: {ex.Message}");
            }
        }

        public async Task<Order> UpdateOrderAsync(Order orderRequest)
        {
            try
            {
                orderRequest.UpdatedAt = DateTime.UtcNow;

                var productJson = JsonConvert.SerializeObject(orderRequest);

                var request = new PutItemRequest
                {
                    TableName = TableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "OrderId", new AttributeValue { N = orderRequest.OrderId.ToString() } },
                        { "CustomerName", new AttributeValue { S = orderRequest.CustomerName } },
                        { "BasketId", new AttributeValue { N = orderRequest.BasketId.ToString() } },
                        { "OrderDate", new AttributeValue { S = orderRequest.CreatedAt.ToString() } },
                        { "UpdatedAt", new AttributeValue { S = orderRequest.UpdatedAt?.ToString("o") ?? string.Empty } },
                    }
                };
                var response = await _context.PutItemAsync(request);

                return response.HttpStatusCode == HttpStatusCode.OK ? orderRequest : throw new ProductException($"Failed to update Product with id {orderRequest.OrderId}. HTTP Status: {response.HttpStatusCode}");
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();

                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new OrderException($"Error updating Product with id {orderRequest.OrderId} failed: Type {exceptionType}: {ex.ToString()}");
                }

                throw new OrderException($"Error updating Product with id {orderRequest.OrderId} failed: Type {exceptionType}: {ex.Message}");
            }
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            try
            {
                var deleteRequest = new DeleteItemRequest
                {
                    TableName = TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "OrderId", new AttributeValue { N = orderId.ToString() } }
                    }
                };
                var response = await _context.DeleteItemAsync(deleteRequest);
                
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new OrderException($"Failed to delete Order with id {orderId}. HTTP Status: {response.HttpStatusCode}");
                }
                return true;
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new OrderException($"Error deleting Order with OrderId {orderId}: {ex.ToString()}");
                }

                throw new OrderException($"Error deleting Order with OrderId {orderId}: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomer(string customerName)
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
                
                return response.Items.Select(item => new Order
                {
                    OrderId = int.Parse(item["OrderId"].N),
                    CustomerName = item["CustomerName"].S,
                    BasketId = int.Parse(item["BasketId"].N),
                    CreatedAt = DateTime.TryParse(item["CreatedAt"].S, out var createdAt) ? createdAt : (DateTime?)null,
                    UpdatedAt = DateTime.TryParse(item["UpdatedAt"].S, out var updatedAt) ? updatedAt : (DateTime?)null
                }).ToList();
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new OrderException($"Error getting Orders with customer {customerName} failed: {ex.ToString()}");
                }

                throw new OrderException($"Error getting Orders with customer {customerName} failed: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            try
            {
                var scanRequest = new ScanRequest
                {
                    TableName = TableName,
                };
                var response = await _context.ScanAsync(scanRequest);
                return response.Items
                    .Select(item => new Order
                    {
                        OrderId = int.Parse(item["OrderId"].N),
                        CustomerName = item["CustomerName"].S,
                        BasketId = int.Parse(item["BasketId"].N),
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
                    throw new OrderException($"Error getting all Orders: {exceptionType}:{ex.ToString()}");
                }

                throw new OrderException($"Error getting all Orders: {exceptionType}:{ex.Message}");
            }
        }
    }
}
