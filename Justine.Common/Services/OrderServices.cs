using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Newtonsoft.Json;

namespace Justine.Common.Services
{
    public class OrderServices : IOrderServices
    {
        private readonly IDynamoDBContext _context;
        public OrderServices(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            try
            {
                var order = await _context.LoadAsync<Order>(orderId) ?? throw new OrderException($"Order with OrderId {orderId} not found.");
                return order;
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new OrderException($"Error getting Order with id {orderId} failed: {ex.ToString()}");
                }

                throw new OrderException($"Error getting Order with id {orderId} failed: {ex.Message}");
            }
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            try
            {
                await _context.SaveAsync(order);
                var response = await _context.LoadAsync<Order>(order.OrderId) ?? throw new OrderException($"Order with id {order.OrderId} not found.");
                return response;
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
                var order = await _context.LoadAsync<Order>(orderRequest.OrderId) ?? throw new OrderException($"Order with OrderId {orderRequest.OrderId} not found.");
                await _context.SaveAsync(orderRequest);
                return orderRequest;
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if(ex.ToString() != null)
                {
                    throw new OrderException($"Error updating Order with id {orderRequest.OrderId} failed: {ex.ToString()}");
                }

                throw new OrderException($"Error updating Order with id {orderRequest.OrderId} failed: {ex.Message}");
            }
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            try
            {
                var order = await _context.LoadAsync<Order>(orderId) ?? throw new OrderException($"Order with OrderId {orderId} not found.");
                await _context.DeleteAsync(order);
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
                var queryConfig = new QueryOperationConfig
                {
                    IndexName = "CustomerName-index",
                    KeyExpression = new Expression
                    {
                        ExpressionStatement = "CustomerName = :v_customerName",
                        ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                        {
                            { ":v_customerName", customerName }
                        }
                    }
                };

                var search = _context.FromQueryAsync<Order>(queryConfig);
                var orders = await search.GetRemainingAsync();

                return orders ?? [];
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
                var orders = await _context.ScanAsync<Order>(new List<ScanCondition>()).GetRemainingAsync();
                if (orders == null) return new List<Order>();
                return orders;

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
