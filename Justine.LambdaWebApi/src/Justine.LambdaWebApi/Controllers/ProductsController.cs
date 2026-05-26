using System.Linq;
using Amazon.Lambda.Core;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Amazon.DynamoDBv2.Model;
using Justine.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Justine.LambdaWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // require a valid Cognito JWT
    public class ProductsController : ControllerBase
    {
        private readonly IProductServices _productServices;
        public ProductsController(IProductServices productServices) => _productServices = productServices;

        // GET /api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProductsAsync()
        {
            try
            {
                //var products = await _productServices.GetAllProductsAsync();
                var products = await _productServices.GetAllMockProductsAsync();

                return Ok(products ?? Enumerable.Empty<Product>());
            }
            catch (ResourceNotFoundException resourceNotFoundException)
            {
                // Table missing — return 404 so client can show admin/seed UI
                var msg = $"Products table not found: {resourceNotFoundException.Message}";
                LambdaLogger.Log(msg);
                return NotFound(new { error = "Products table does not exist.", detail = resourceNotFoundException.Message });
            }
            catch (Exception ex)
            {
                var msg = $"ERROR: GetAllProductsAsync retrieving all Products Error: {ex.Message}";
                LambdaLogger.Log(msg);
                return StatusCode(500, new { error = "Unable to retrieve products." });
            }
        }

        // GET /api/products/{ProductId}
        [HttpGet("{ProductId}")]
        public async Task<ActionResult<Product>> GetProductByIdAsync(int ProductId)
        {
            try
            {
                LambdaLogger.Log($"GetProductByIdAsync: Id: {ProductId}");
                var product = await _productServices.GetProductByIdAsync(ProductId);
                if (product == null)
                {
                    return NotFound(new { error = $"Product with id {ProductId} not found." });
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                var msg = $"ERROR retrieving Product with id {ProductId}: {ex.Message}";
                LambdaLogger.Log(msg);
                return StatusCode(500, new { error = "Unable to retrieve product." });
            }
        }

        // POST /api/products
        [HttpPost]
        public async Task<ActionResult<Product>> AddProductAsync([FromBody] Product newProduct)
        {
            try
            {
                var product = await _productServices.AddProductAsync(newProduct);
                LambdaLogger.Log($"AddProductAsync: Success: ProductId: {newProduct.ProductId}");
                return CreatedAtAction(nameof(GetProductByIdAsync), new { ProductId = newProduct.ProductId }, newProduct);
            }
            catch (Exception ex)
            {
                var productJson = JsonConvert.SerializeObject(newProduct);
                var msg = $"ERROR: AddProductAsync Adding Product {productJson}: Error: {ex.Message}";
                LambdaLogger.Log(msg);
                return StatusCode(500, new { error = "Unable to add product." });
            }
        }

        // DELETE /api/products/{productId}
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProductAsync(int productId)
        {
            try
            {
                var isDeleted = await _productServices.DeleteProductAsync(productId);
                if (!isDeleted)
                {
                    return NotFound(new { error = $"Product with id {productId} not found or could not be deleted." });
                }
                LambdaLogger.Log($"DeleteProductAsync: Id: {productId} successfully deleted");
                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                var msg = $"ERROR DeleteProductAsync deleting Product: Id: {productId} \n Error: {ex.Message}";
                LambdaLogger.Log(msg);
                return StatusCode(500, new { error = "Unable to delete product." });
            }
        }

        // PUT /api/products/{productId}
        [HttpPut("{productId}")]
        public async Task<ActionResult<Product>> UpdateProductAsync([FromBody] Product product)
        {
            try
            {
                var updatedProduct = await _productServices.UpdateProductAsync(product);
                if (updatedProduct == null)
                {
                    return NotFound(new { error = $"Product with id {product.ProductId} not found." });
                }
                var updatedProductJson = JsonConvert.SerializeObject(updatedProduct);
                LambdaLogger.Log($"UpdateProductAsync: success Product: {updatedProductJson}");
                return Ok(updatedProduct);
            }
            catch (Exception ex)
            {
                var ProductJson = JsonConvert.SerializeObject(product);
                var msg = $"ERROR: UpdateProductAsync updating Product {ProductJson} \n ERROR: {ex.Message}";
                LambdaLogger.Log(msg);
                return StatusCode(500, new { error = "Unable to update product." });
            }
        }
    }
}