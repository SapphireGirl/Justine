using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Justine.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Justine.LambdaWebApi.Controllers
{
    [ApiController]
    [Route("Admin")]
    //[Route("api/[controller]")]
    [Authorize] // keep same auth pattern as other controllers (adjust as needed)
    public class AdminController : ControllerBase
    {
        private const string TableName = "Products";
        private readonly IAdminServices _adminServices;
        private readonly IAmazonDynamoDB _dynamo;

        public AdminController(IAdminServices adminServices, IAmazonDynamoDB dynamo)
        {
            _adminServices = adminServices;
            _dynamo = dynamo;
        }

        [HttpPost("CreateProductTableAsync")]
        public async Task<IActionResult> CreateProductTableAsync()
        {
            try
            {
                await _adminServices.CreateProductTableAsync();
                return Ok(new { success = true, message = "Product table ensured/created." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "CreateProductTableAsync");
            }
        }

        [HttpPost("CreateBasketTableAsync")]
        public async Task<IActionResult> CreateBasketTableAsync()
        {
            try
            {
                await _adminServices.CreateBasketTableAsync();
                return Ok(new { success = true, message = "Basket table ensured/created." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "CreateBasketTableAsync");
            }
        }

        [HttpPost("CreateOrderTableAsync")]
        public async Task<IActionResult> CreateOrderTableAsync()
        {
            try
            {
                await _adminServices.CreateOrderTableAsync();
                return Ok(new { success = true, message = "Order table ensured/created." });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "CreateOrderTableAsync");
            }
        }

        [HttpDelete("DeleteProductTableAsync")]
        public async Task<IActionResult> DeleteProductTableAsync()
        {
            try
            {
                var result = await _adminServices.DeleteProductTableAsync();
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "DeleteProductTableAsync");
            }
        }

        [HttpDelete("DeleteBasketTableAsync")]
        public async Task<IActionResult> DeleteBasketTableAsync()
        {
            try
            {
                var result = await _adminServices.DeleteBasketTableAsync();
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "DeleteBasketTableAsync");
            }
        }

        [HttpDelete("DeleteOrderTableAsync")]
        public async Task<IActionResult> DeleteOrderTableAsync()
        {
            try
            {
                var result = await _adminServices.DeleteOrderTableAsync();
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "DeleteOrderTableAsync");
            }
        }

        // Legacy method retained for compatibility — prefer CreateTable via IAdminServices.
        // If you keep this endpoint, consider replacing with the DTO-based CreateTable endpoint that delegates to IAdminServices.
        [HttpPost("CreateTableAsync")]
        public async Task<IActionResult> CreateTableAsync(string tableName, string primaryKeyColumnName, string sortKeyColumnName)
        {
            try
            {
                // Delegate to AdminServices for the heavy lifting where possible.
                // This controller method simply maps the incoming parameters into the service call.
                // For simplicity assume numeric PK and string SK for these helper endpoints.
                var created = await _adminServices.CreateTableAsync(
                    tableName,
                    primaryKeyColumnName,
                    ScalarAttributeType.N,
                    string.IsNullOrWhiteSpace(sortKeyColumnName) ? null : sortKeyColumnName,
                    string.IsNullOrWhiteSpace(sortKeyColumnName) ? (ScalarAttributeType?)null : ScalarAttributeType.S,
                    seed: string.Equals(tableName, "Products", StringComparison.OrdinalIgnoreCase)
                );

                return Ok(new { table = tableName, created });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "CreateTableAsync");
            }
        }

        // Centralized exception handling for controller actions.
        // Logs detailed exception server-side and returns a safe, mapped HTTP response to the client.
        private IActionResult HandleException(Exception ex, string operation)
        {
            // Log full exception server-side for diagnostics.
            try
            {
                LambdaLogger.Log($"AdminController.{operation} ERROR: {ex}");
            }
            catch
            {
                // Swallow logging errors to avoid masking original error.
            }

            // Specific mapping for DynamoDB "not found" cases
            if (ex is ResourceNotFoundException)
            {
                // Return 404 with a concise message (no stack traces or secrets)
                return NotFound(new { error = "Requested resource not found.", detail = ex.Message });
            }

            // AWS service level exceptions
            if (ex is AmazonServiceException awsEx)
            {
                // Throttling or service unavailable -> 503
                if (awsEx.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    string.Equals(awsEx.ErrorCode, "ThrottlingException", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode((int)HttpStatusCode.ServiceUnavailable,
                        new { error = "AWS service unavailable or throttled. Try again later." });
                }

                // Map other service errors to their HTTP status code if available
                var statusCode = (int)(awsEx.StatusCode != 0 ? awsEx.StatusCode : HttpStatusCode.InternalServerError);
                return StatusCode(statusCode, new { error = "AWS service error", detail = awsEx.Message });
            }

            // Argument/validation issues -> 400
            if (ex is ArgumentException || ex is ArgumentNullException)
            {
                return BadRequest(new { error = "Invalid request", detail = ex.Message });
            }

            // Default fallback: 500 Internal Server Error
            return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An internal error occurred.", detail = ex.Message });
        }
    }
}
