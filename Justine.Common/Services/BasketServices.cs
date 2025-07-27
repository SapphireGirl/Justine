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
                if (basket == null)
                {
                    throw new BasketException($"Basket with BasketId {basketId} not found.");
                }

                return basket;
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new BasketException($"Error getting Basket with BasketId {basketId} failed: {ex.ToString()}");
                }

                throw new BasketException($"Error getting Basket with BasketId {basketId} failed: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Basket>> GetAllBasketsAsync()
        {
            try
            {
                var baskets = await _context.ScanAsync<Basket>(new List<ScanCondition>()).GetRemainingAsync();
                return baskets ?? new List<Basket>();
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new BasketException($"Error getting all Baskets: {ex.ToString()}");
                }

                throw new BasketException($"Error getting all Baskets: {ex.ToString()}");
            }
        }

        public async Task<Basket> AddBasketAsync(Basket basket)
        {
            try
            {
                await _context.SaveAsync<Basket>(basket);
                var response = await _context.LoadAsync<Basket>(basket.BasketId);

                if (response == null)
                {
                    throw new BasketException($"Failed to retrieve the added Basket with BasketId {basket.BasketId}.");
                }

                return response;
            }
            catch (Exception ex)
            {
                var basketJson = JsonConvert.SerializeObject(basket);
                if (ex.ToString() != null)
                {
                    throw new BasketException($"Error adding Basket {basketJson} \n ERROR: {ex.ToString()}");
                }

                throw new BasketException($"Error adding Basket {basketJson} \n ERROR: {ex.Message}");
            }
        }

        public async Task<Basket> UpdateBasketAsync(Basket basketRequest)
        {
            try
            {
                var basket = await _context.LoadAsync<Basket>(basketRequest.BasketId);
                if (basket == null)
                {
                    throw new BasketException($"Basket with BasketId {basketRequest.BasketId} not found.");
                }

                await _context.SaveAsync(basketRequest);
                return basketRequest;
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new BasketException($"Error updating Basket with BasketId {basketRequest.BasketId} failed: {ex.ToString()}");
                }

                throw new BasketException($"Error updating Basket with BasketId {basketRequest.BasketId} failed: {ex.Message}");
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
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new BasketException($"Error deleting Basket with BasketId {basketId}: {ex.ToString()}");
                }

                throw new BasketException($"Error deleting Basket with BasketId {basketId}: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Basket>> GetUsersBasketsByNameAsync(string customerName)
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

                var search = _context.FromQueryAsync<Basket>(queryConfig);
                var baskets = await search.GetRemainingAsync();

                return baskets ?? new List<Basket>();
            }
            catch (Exception ex)
            {
                // To get the inner exception and stack trace for more detailed error information
                if (ex.ToString() != null)
                {
                    throw new BasketException($"Error getting all Baskets with customer Name: {customerName} {ex.ToString()}");
                }

                throw new BasketException($"Error getting all Baskets with customer Name: {customerName} {ex.Message}");
            }
        }
    }
}
