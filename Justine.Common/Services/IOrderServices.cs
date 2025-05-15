using Justine.Common.Models;

namespace Justine.Common.Services
{
    public interface IOrderServices
    {
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<Order> AddOrderAsync(Order order);
        Task<Order?> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByCustomer(string customer);

    }
}