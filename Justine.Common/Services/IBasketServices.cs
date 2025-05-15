using Justine.Common.Models;

namespace Justine.Common.Services
{
    public interface IBasketServices
    {
        Task<Basket> GetBasketByIdAsync(int basketId);
        Task<IEnumerable<Basket>> GetAllBasketsAsync();
        Task<Basket> AddBasketAsync(Basket basket);
        Task<Basket> UpdateBasketAsync(Basket basket);
        Task<bool> DeleteBasketAsync(int basketId);
        Task<IEnumerable<string>> GetUsersBasketsByName(int userId, string userName);
    }
}