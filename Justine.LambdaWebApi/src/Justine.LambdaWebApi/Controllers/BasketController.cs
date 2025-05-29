using Amazon.Lambda.Core;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Justine.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Justine.LambdaWebApi.Controllers
{
    [ApiController]
    [Route("baskets")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketServices _basketServices;
        public BasketController(IBasketServices basketServices)
        {
            _basketServices = basketServices;
        }

        // GET /baskets
        [HttpGet]
        public async Task<IEnumerable<Basket>> GetAllBasketsAsync()
        {
            try
            {
                return await _basketServices.GetAllBasketsAsync();
            }
            catch(Exception ex)
            {
                var msg = $"ERROR: GetAllBasketsAsync retrieving all baskets Error: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new Exception(msg);
            }
        }

        // GET /baskets/{id}
        [HttpGet("{id}")]
        public async Task<Basket> GetBasketByIdAsync(int basketId)
        {
            try
            {
                LambdaLogger.Log($"GetBasketByIdAsync: Id: {basketId}");
                return await _basketServices.GetBasketByIdAsync(basketId);
            }
            catch (Exception ex)
            {
                var msg = $"ERROR retrieving basket with id {basketId}: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new BasketException(msg);
            }
        }

        // POST /baskets
        [HttpPost]
        public async Task<Basket> AddBasketAsync([FromBody] Basket basket)
        {
            try
            {

                var newBasket = await _basketServices.AddBasketAsync(basket);
                LambdaLogger.Log($"AddBasketAsync: Success: Id: {basket.BasketId}");
                return newBasket;
            }
            catch (Exception ex)
            {
                var basketJson = JsonConvert.SerializeObject(basket);
                var msg = $"ERROR: AddBasketAsync Adding Basket {basketJson}: Error: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new BasketException(msg);
            }
        }

        // DELETE /baskets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBasketAsync(int basketId)
        {
            try
            {
                var isDeleted = await _basketServices.DeleteBasketAsync(basketId);
                if (!isDeleted)
                {
                    throw new BasketException($"Basket with id {basketId} not found or could not be deleted.");
                }
                else
                {
                    LambdaLogger.Log($"DeleteBasketAsync: Id: {basketId} successfully deleted");
                    return Ok(new { Success = true });
                }
            }
            catch (Exception ex)
            {
                var msg = $"ERROR DeleteBasketAsync deleting Basket: Id: {basketId} \n Error: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new BasketException(msg);
            }
        }

        // PUT /baskets/{id}
        [HttpPut("{id}")]
        public async Task<Basket> UpdateBasketAsync([FromBody] Basket basket)
        {
            try
            {
                var updatedBasket = await _basketServices.UpdateBasketAsync(basket);
                if (updatedBasket == null)
                {
                    throw new BasketException($"Basket with id {basket.BasketId} not found.");
                }
                else
                {
                    var updatedBasketJson = JsonConvert.SerializeObject(updatedBasket);
                    LambdaLogger.Log($"UpdateBasketAsync: success Basket: {updatedBasketJson}");
                    return updatedBasket;
                }
                
            }
            catch (Exception ex)
            {
                var basketJson = JsonConvert.SerializeObject(basket);
                var msg = $"ERROR: UpdateBasketAsync updating Basket {basketJson} \n ERROR: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new BasketException(msg);
            }
        }

        // GET /baskets/user/{name}
        [HttpGet("user/{name}")]
        public async Task<IEnumerable<Basket>> GetUsersBasketsByNameAsync(string name)
        {
            try
            {
                var baskets = await _basketServices.GetUsersBasketsByNameAsync(name);
                var basketsJson = JsonConvert.SerializeObject(baskets);

                LambdaLogger.Log($"GetUsersBasketsByNameAsync: Name: {name} : {basketsJson}");
                return baskets;
            }
            catch (Exception ex)
            {
                var msg = $"ERROR: GetUsersBasketsByName: Error retrieving baskets for user: {name}: Error: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new BasketException(msg);
            }
        }
    }
}
