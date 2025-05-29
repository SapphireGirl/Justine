using Amazon.Lambda.Core;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Justine.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Justine.LambdaWebApi.Controllers
{
    [ApiController]
    [Route("Orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderServices _OrderServices;
        public OrderController(IOrderServices OrderServices)
        {
            _OrderServices = OrderServices;
        }

        // GET /Orders
        [HttpGet]
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            try
            {
                return await _OrderServices.GetAllOrdersAsync();
            }
            catch(Exception ex)
            {
                var msg = $"ERROR: GetAllOrdersAsync retrieving all Orders Error: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new Exception(msg);
            }
        }

        // GET /Orders/{id}
        [HttpGet("{id}")]
        public async Task<Order> GetOrderByIdAsync(int OrderId)
        {
            try
            {
                LambdaLogger.Log($"GetOrderByIdAsync: Id: {OrderId}");
                return await _OrderServices.GetOrderByIdAsync(OrderId);
            }
            catch (Exception ex)
            {
                var msg = $"ERROR retrieving Order with id {OrderId}: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new OrderException(msg);
            }
        }

        // POST /Orders
        [HttpPost]
        public async Task<Order> AddOrderAsync([FromBody] Order Order)
        {
            try
            {

                var newOrder = await _OrderServices.AddOrderAsync(Order);
                LambdaLogger.Log($"AddOrderAsync: Success: Id: {Order.OrderId}");
                return newOrder;
            }
            catch (Exception ex)
            {
                var OrderJson = JsonConvert.SerializeObject(Order);
                var msg = $"ERROR: AddOrderAsync Adding Order {OrderJson}: Error: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new OrderException(msg);
            }
        }

        // DELETE /Orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderAsync(int OrderId)
        {
            try
            {
                var isDeleted = await _OrderServices.DeleteOrderAsync(OrderId);
                if (!isDeleted)
                {
                    throw new OrderException($"Order with id {OrderId} not found or could not be deleted.");
                }
                else
                {
                    LambdaLogger.Log($"DeleteOrderAsync: Id: {OrderId} successfully deleted");
                    return Ok(new { Success = true });
                }
            }
            catch (Exception ex)
            {
                var msg = $"ERROR DeleteOrderAsync deleting Order: Id: {OrderId} \n Error: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new OrderException(msg);
            }
        }

        // PUT /Orders/{id}
        [HttpPut("{id}")]
        public async Task<Order> UpdateOrderAsync([FromBody] Order Order)
        {
            try
            {
                var updatedOrder = await _OrderServices.UpdateOrderAsync(Order);
                if (updatedOrder == null)
                {
                    throw new OrderException($"Order with id {Order.OrderId} not found.");
                }
                else
                {
                    var updatedOrderJson = JsonConvert.SerializeObject(updatedOrder);
                    LambdaLogger.Log($"UpdateOrderAsync: success Order: {updatedOrderJson}");
                    return updatedOrder;
                }
                
            }
            catch (Exception ex)
            {
                var OrderJson = JsonConvert.SerializeObject(Order);
                var msg = $"ERROR: UpdateOrderAsync updating Order {OrderJson} \n ERROR: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new OrderException(msg);
            }
        }

        // GET /Orders/user/{name}
        [HttpGet("user/{name}")]
        public async Task<IEnumerable<Order>> GetUsersOrdersByNameAsync(string name)
        {
            try
            {
                var Orders = await _OrderServices.GetOrdersByCustomer(name);
                var OrdersJson = JsonConvert.SerializeObject(Orders);

                LambdaLogger.Log($"GetOrdersByCustomer: Name: {name} : {OrdersJson}");
                return Orders;
            }
            catch (Exception ex)
            {
                var msg = $"ERROR: GetOrdersByCustomer: Error retrieving Orders for user: {name}: Error: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new OrderException(msg);
            }
        }
    }
}
