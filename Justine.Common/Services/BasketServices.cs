using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
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
                // Get the latest OrderId + 1
                var latestOrderId = await GetNextIdAsync(basket.CustomerName);
                basket.BasketId = latestOrderId;

                // get Products on basket

                List<Product> products = new List<Product>();
                Dictionary<string, AttributeValue> productAttribute = new Dictionary<string, AttributeValue>();
                List<AttributeValue> productAttributes = new List<AttributeValue>();
                foreach (var product in basket.Products)
                {
                    var productToAdd = await _context.LoadAsync<Product>(product.Id);
                    
                    if (productToAdd != null)
                    {
                        products.Add(productToAdd);

                        productAttributes.Add(new AttributeValue { S = productToAdd.Name.ToString() });
                        productAttributes.Add(new AttributeValue { S = productToAdd.Description.ToString() });
                        productAttributes.Add(new AttributeValue { N = productToAdd.Price.ToString() });
                        productAttributes.Add(new AttributeValue { S = productToAdd.ImageUrl.ToString() });
                        productAttributes.Add(new AttributeValue { N = productToAdd.Quantity.ToString() });
                        productAttributes.Add(new AttributeValue { S = productToAdd.CreatedAt.ToString() });
                        productAttributes.Add(new AttributeValue { S = productToAdd.UpdatedAt.ToString() });

                        //productAttribute.Add("Name", new AttributeValue { S = product.Name });
                        //productAttribute.Add("Description", new AttributeValue { S = product.Description });
                        //productAttribute.Add("Price", new AttributeValue { N = product.Price.ToString() });
                        //productAttribute.Add("ImageUrl", new AttributeValue { S = product.ImageUrl });
                        //productAttribute.Add("Quantity", new AttributeValue { N = product.Quantity.ToString() });
                        //productAttribute.Add("CreatedAt", new AttributeValue { S = DateTime.UtcNow.ToString("o") });
                        //productAttribute.Add("UpdatedAt", new AttributeValue { S = DateTime.UtcNow.ToString("o") });
                    }
                }

                //var products = await _context.FromQueryAsync<Product>(new List<ScanCondition>()).GetRemainingAsync();
                // set up item
                var item = new Dictionary<string, AttributeValue>
                {
                    ["BasketId"] = new AttributeValue { N = latestOrderId.ToString() },
                    ["CustomerName"] = new AttributeValue { S = basket.CustomerName },
                    ["TotalPrice"] = new AttributeValue { N = basket.TotalPrice.ToString() },
                    ["CreatedAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") },
                    ["UpdatedAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") },
                    ["Products"] = new AttributeValue
                    {
                        L = productAttributes // Add the list of products as a string set
                    },
                };

                var request = new PutItemRequest
                {
                    TableName = TableName,
                    Item = item
                };

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

        public async Task<IEnumerable<string>> GetUsersBasketsByName(int userId, string userName)
        {
            var baskets = await _context.QueryAsync<Basket>(
                userName,
                QueryOperator.BeginsWith,
                new object[] { userName }
            ).GetRemainingAsync();

            List<string> users = new List<string>();

            if (baskets == null || baskets.Count == 0)
            {
                return Enumerable.Empty<string>();
            }

            foreach (var basket in baskets)
            {
                if (basket.CustomerName == userName)
                {
                    users.Add(basket.CustomerName);
                }
            }

            return users;
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
                var search = _context.FromQueryAsync<Basket>(queryConfig);
                var orders = await search.GetRemainingAsync();

                // Get the latest OrderId or default to 0 if no items exist
                var latestBasketId = orders.FirstOrDefault()?.BasketId ?? 0;

                return latestBasketId + 1;
            }
            catch (Exception ex)
            {
                throw new BasketException($"Error getting next BasketId for customer {customerName}: {ex.Message}", ex);
            }
        }

    }
}
