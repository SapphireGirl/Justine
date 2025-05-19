using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Newtonsoft.Json;

namespace Justine.Common.Services
{
    public class BasketServices : IBasketServices
    {
        private readonly IDynamoDBContext _context;
        private const string TableName = "Baskets";
        public BasketServices(IDynamoDBContext context) 
        {
            _context = context;
        }
        public async Task<Basket> GetBasketByIdAsync(int basketId)
        {
            try
            {
                var basket = await _context.LoadAsync<Basket>(basketId);
                if (basket == null) return null;

                return basket;
            }
            catch (Exception ex)
            {
                throw new BasketException($"Error getting Basket with basketId {basketId} failed: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Basket>> GetAllBasketsAsync()
        {
            try
            {
                var baskets = await _context.ScanAsync<Basket>(new List<ScanCondition>()).GetRemainingAsync();
                if (baskets == null) return new List<Basket>();
                return baskets;

            }
            catch (Exception ex)
            {
                throw new BasketException($"Error getting all Products: {ex.Message}", ex);
            }
        }

        public async Task<Basket> AddBasketAsync(Basket basket)
        {
            try
            {

                //The SaveAsync method creates a new Item if the primary key does not already exist. 
                //If it exists, it will overwrite the existing item with the new item values.
                await _context.SaveAsync<Basket>(basket);

                var response = await _context.LoadAsync<Basket>(basket.BasketId);

                return response;
            }
            catch (Exception ex)
            {
                var basketJson = JsonConvert.SerializeObject(basket);
                throw new BasketException($"Error adding Product {basketJson} \n ERROR: {ex.Message}", ex);
            }
        }

        public async Task<Basket> UpdateBasketAsync(Basket basketRequest)
        {
            try
            {
                var basket = await _context.LoadAsync<Basket>(basketRequest.BasketId);
                if (basket == null) return null;
                await _context.SaveAsync(basketRequest);
                return basket;
            }
            catch (Exception ex)
            {
                throw new BasketException($"Error updating Product with id {basketRequest.BasketId} failed: {ex.Message}", ex);
            }
        }
        
        public async Task<bool> DeleteBasketAsync(int basketId)
        {
            try
            {
                var basket = await _context.LoadAsync<Basket>(basketId);
                if (basket == null)
                {
                    throw new BasketException($"Basket with BasketId {basketId} not found.");
                }

                await _context.DeleteAsync(basket);
                return true;
            }
            catch (Exception ex)
            {
                throw new BasketException($"Error deleting Basket with BasketId  {basketId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Basket>> GetUsersBasketsByName(string customerName)
        {
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
                    }
                };

                // Execute the query
                var search = _context.FromQueryAsync<Basket>(queryConfig);
                var baskets = await search.GetRemainingAsync();

                return baskets;

            }
            catch (Exception ex)
            {
                throw new BasketException($"Error getting all Baskets with customer Name: {customerName} {ex.Message}", ex);
            }
        }
    }
}
