using Justine.Common.Models;

namespace Justine.Common.Services
{
    public interface IAdminServices
    {
        Task CreateProductTableAsync();
        Task CreateBasketTableAsync();
        Task CreateOrderTableAsync();
        //Task<bool> PopulateProductTableAsync();
        //Task<bool> PopulateBasketTableAsync();
        //Task<bool> PopulateOrderTableAsync();
        // Cleanup 
        Task<bool> DeleteProductTableAsync();
        Task<bool> DeleteBasketTableAsync();
        Task<bool> DeleteOrderTableAsync();
        Task<bool> DeleteAllLambdasAsync();
    }
}