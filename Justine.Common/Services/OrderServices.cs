using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Newtonsoft.Json;

namespace Justine.Common.Services
{
    public class OrderServices : IOrderServices
    {
        private readonly IDynamoDBContext _context;
        private const string TableName = "Orders";
        public OrderServices(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            try
            {
                var order = await _context.LoadAsync<Order>(orderId);
                if (order == null) return null;

                return order;
            }
            catch (Exception ex)
            {
                throw new OrderException($"Error getting Product with id {orderId} failed: {ex.Message}", ex);
            }
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            try
            {
                // Get the latest OrderId + 1
                var latestOrderId = await GetNextIdAsync(order.CustomerName);

                order.OrderId = latestOrderId;
                // set up item
                var orderToAdd = new Dictionary<string, AttributeValue>
                {
                    ["OrderId"] = new AttributeValue { N = order.OrderId.ToString() },
                    ["CustomerName"] = new AttributeValue { S = order.CustomerName },
                    ["BasketId"] = new AttributeValue { N = order.BasketId.ToString() },
                    ["OrderDate"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") }
                };

                var request = new PutItemRequest
                {
                    TableName = TableName,
                    Item = orderToAdd
                };

                var response = await _context.LoadAsync<Order>(order.OrderId);

                if (response == null)
                {
                    throw new OrderException($"Product with id {order.OrderId} not found.");
                }

                return response;
            }
            catch (Exception ex)
            {
                var orderJson = JsonConvert.SerializeObject(order);
                throw new OrderException($"Error adding Product {orderJson} \n ERROR: {ex.Message}", ex);
            }

        }

        public async Task<Order?> UpdateOrderAsync(Order orderRequest)
        {
            try
            {
                var order = await _context.LoadAsync<Order>(orderRequest.OrderId);
                if (order == null) return null;
                
                await _context.SaveAsync(order);
                
                return order;
            }
            catch (Exception ex)
            {
                throw new ProductException($"Error updating Product with id {orderRequest.OrderId} failed: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            try
            {
                var order = await _context.LoadAsync<Order>(orderId);
                if (order == null)
                {
                    throw new OrderException($"Order with OrderId {orderId} not found.");
                }

                await _context.DeleteAsync(order);
                return true;
            }
            catch (Exception ex)
            {
                throw new OrderException($"Error deleting Order with OrderId  {orderId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomer(string customerName)
        {
            try
            {
                // Query table using just the sort key
                // Do I need to setup a Global Secondary Index (GSI) for this?
                // Yes
               
                // Define the query conditions
                var queryConfig = new QueryOperationConfig
                {
                    IndexName = "CustomerName-index", // Ensure a GSI is created for CustomerName
                    KeyExpression = new Expression
                    {
                        ExpressionStatement = "CustomerName = :v_customerName",
                        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                        {
                            { ":v_customerName", customerName }
                        }
                    }
                };

                // Execute the query
                var search = _context.FromQueryAsync<Order>(queryConfig);
                var orders = await search.GetRemainingAsync();

                return orders;
                
            }
            catch (Exception ex)
            {
                throw new OrderException($"Error getting Orders with customer {customerName} failed: {ex.Message}", ex);
            }
        }

        public async Task<int> GetNextIdAsync(string customerName)
        {
            // AWSCredentials credentials, RegionEndpoint region
            //var credentials = new BasicAWSCredentials("your-access-key", "your-secret-key");
            //var region = RegionEndpoint.USEast1; // Change to desired region
            try
            {
                // Define the query conditions
                var queryConfig = new QueryOperationConfig
                {
                    IndexName = "CustomerName-index", // Ensure a GSI is created for CustomerName
                    KeyExpression = new Expression
                    {
                        ExpressionStatement = "CustomerName = :v_customerName",
                        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                {
                    { ":v_customerName", customerName }
                }
                    },
                    BackwardSearch = true, // Fetch the latest item first
                    Limit = 1 // Only fetch the most recent item
                };

                // Execute the query
                var search = _context.FromQueryAsync<Order>(queryConfig);
                var orders = await search.GetRemainingAsync();

                // Get the latest OrderId or default to 0 if no items exist
                var latestOrderId = orders.FirstOrDefault()?.OrderId ?? 0;

                return latestOrderId + 1;
            }
            catch (Exception ex)
            {
                throw new OrderException($"Error getting next OrderId for customer {customerName}: {ex.Message}", ex);
            }
        }
    }
}
