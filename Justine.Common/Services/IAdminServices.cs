using Amazon.DynamoDBv2;

namespace Justine.Common.Services
{
    public interface IAdminServices
    {
        Task CreateProductTableAsync();
        Task CreateBasketTableAsync();
        Task CreateOrderTableAsync();

        Task<bool> CreateTableAsync(string tableName,
                                    string primaryKeyName,
                                    ScalarAttributeType primaryKeyType,
                                    string? sortKeyName = null,
                                    ScalarAttributeType? sortKeyType = null,
                                    bool seed = false);

        Task SeedProductsAsync();
        Task SeedBasketsAsync();
        Task SeedOrderAsync();
        Task<bool> DeleteTableAsync(string tableName);
        Task<bool> DeleteProductTableAsync();
        Task<bool> DeleteBasketTableAsync();
        Task<bool> DeleteOrderTableAsync();
    }
}