using Amazon.Lambda.Core;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Justine.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Justine.LambdaWebApi.Controllers
{
    [ApiController]
    [Route("Products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductServices _productServices;
        public ProductController(IProductServices productServices)
        {
            _productServices = productServices;
        }

        // GET /Products
        [HttpGet]
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {
                return await _productServices.GetAllProductsAsync();
            }
            catch(Exception ex)
            {
                var msg = $"ERROR: GetAllProductsAsync retrieving all Products Error: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new Exception(msg);
            }
        }

        // GET /Products/{ProductId}
        [HttpGet("{ProductId}")]
        public async Task<Product> GetProductByIdAsync(int ProductId)
        {
            try
            {
                LambdaLogger.Log($"GetProductByIdAsync: Id: {ProductId}");
                var product = await _productServices.GetProductByIdAsync(ProductId);
                if (product == null)
                {
                    throw new ProductException($"Product with id {ProductId} not found.");
                }
                return product;
            }
            catch (Exception ex)
            {
                var msg = $"ERROR retrieving Product with id {ProductId}: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new ProductException(msg);
            }
        }

        // POST /Products
        [HttpPost]
        public async Task<Product> AddProductAsync([FromBody] Product product)
        {
            try
            {

                var newProduct = await _productServices.AddProductAsync(product);
                LambdaLogger.Log($"AddProductAsync: Success: ProductId: {product.ProductId}");
                return newProduct;
            }
            catch (Exception ex)
            {
                var productJson = JsonConvert.SerializeObject(product);
                var msg = $"ERROR: AddProductAsync Adding Product {productJson}: Error: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new ProductException(msg);
            }
        }

        // DELETE /Products/{ProductId}
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProductAsync(int productId)
        {
            try
            {
                var isDeleted = await _productServices.DeleteProductAsync(productId);
                if (!isDeleted)
                {
                    throw new ProductException($"Product with id {productId} not found or could not be deleted.");
                }
                else
                {
                    LambdaLogger.Log($"DeleteProductAsync: Id: {productId} successfully deleted");
                    return Ok(new { Success = true });
                }
            }
            catch (Exception ex)
            {
                var msg = $"ERROR DeleteProductAsync deleting Product: Id: {productId} \n Error: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new ProductException(msg);
            }
        }

        // PUT /Products/{productId}
        [HttpPut("{productId}")]
        public async Task<Product> UpdateProductAsync([FromBody] Product product)
        {
            try
            {
                var updatedProduct = await _productServices.UpdateProductAsync(product);
                if (updatedProduct == null)
                {
                    throw new ProductException($"Product with id {product.ProductId} not found.");
                }
                else
                {
                    var updatedProductJson = JsonConvert.SerializeObject(updatedProduct);
                    LambdaLogger.Log($"UpdateProductAsync: success Product: {updatedProductJson}");
                    return updatedProduct;
                }
                
            }
            catch (Exception ex)
            {
                var ProductJson = JsonConvert.SerializeObject(product);
                var msg = $"ERROR: UpdateProductAsync updating Product {ProductJson} \n ERROR: {ex.Message}";
                LambdaLogger.Log(msg);
                throw new ProductException(msg);
            }
        }

        // GET /Products/user/{name}
        //[HttpGet("user/{name}")]
        //public async Task<IEnumerable<Product>> GetUsersProductsByNameAsync(string name)
        //{
        //    try
        //    {
        //        var Products = await _productServices.GetUsersProductsByNameAsync(name);
        //        var ProductsJson = JsonConvert.SerializeObject(Products);

        //        LambdaLogger.Log($"GetUsersProductsByNameAsync: Name: {name} : {ProductsJson}");
        //        return Products;
        //    }
        //    catch (Exception ex)
        //    {
        //        var msg = $"ERROR: GetUsersProductsByName: Error retrieving Products for user: {name}: Error: {ex.Message}";
        //        LambdaLogger.Log(msg);
        //        throw new ProductException(msg);
        //    }
        //}
    }
}
