using Amazon.Lambda.Core;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Justine.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Justine.LambdaWebApi.Controllers
{
    [ApiController]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminServices _adminServices;

        public AdminController(IAdminServices adminServices)
        {
            _adminServices = adminServices;
        }

        [HttpPost("CreateProductTableAsync")]
        public async Task<IActionResult> CreateProductTableAsync()
        {
            try
            {
                await _adminServices.CreateProductTableAsync();
                return Ok("Product Table Created");
            }
            catch (Exception ex)
            {
                var msg = $"ERROR creating Product table: {ex.Message}";
                LambdaLogger.Log(msg);
                return StatusCode(500, msg);
            }
        }

        [HttpPost("CreateBasketTableAsync")]
        public async Task<IActionResult> CreateBasketTableAsync()
        {
            try
            {
                await _adminServices.CreateBasketTableAsync();
                return Ok("Basket Table Created");
            }
            catch (Exception ex)
            {
                var msg = $"ERROR creating Basket table: {ex.Message}";
                LambdaLogger.Log(msg);
                return StatusCode(500, msg);
            }
        }

        [HttpPost("CreateOrderTableAsync")]
        public async Task<IActionResult> CreateOrderTableAsync()
        {
            try
            {
                await _adminServices.CreateOrderTableAsync();
                return Ok("Basket Table Created");

            }
            catch (Exception ex)
            {
                var msg = $"ERROR creating Order table: {ex.Message}";
                LambdaLogger.Log(msg);
                return StatusCode(500, msg);
            }
        }

        [HttpDelete("DeleteProductTableAsync")]
        public async Task<IActionResult> DeleteProductTableAsync()
        {
            try
            {
                var result = await _adminServices.DeleteProductTableAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                var msg = $"ERROR deleting Product table: {ex.Message}";
                LambdaLogger.Log(msg);
                return StatusCode(500, msg);
            }
        }

        [HttpDelete("DeleteBasketTableAsync")]
        public async Task<IActionResult> DeleteBasketTableAsync()
        {
            try
            {
                var result = await _adminServices.DeleteBasketTableAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                var msg = $"ERROR deleting Basket table: {ex.Message}";
                LambdaLogger.Log(msg);
                return StatusCode(500, msg);
            }
        }

        [HttpDelete("DeleteOrderTableAsync")]
        public async Task<IActionResult> DeleteOrderTableAsync()
        {
            try
            {
                var result = await _adminServices.DeleteOrderTableAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                var msg = $"ERROR deleting Order table: {ex.Message}";
                LambdaLogger.Log(msg);
                return StatusCode(500, msg);
            }
        }
    }
}
