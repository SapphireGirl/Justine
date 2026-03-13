using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Justine.Common.Services;
using Justine.LambdaWebApi.Controllers;
using Amazon.DynamoDBv2;

namespace Justine.LambdaWebApi.Tests
{
    [TestFixture]
    public class AdminControllerTests
    {
        private Mock<IAdminServices> _adminMock = null!;
        private Mock<IAmazonDynamoDB> _dynamoMock = null!;
        private AdminController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _adminMock = new Mock<IAdminServices>(MockBehavior.Strict);
            _dynamoMock = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
            _controller = new AdminController(_adminMock.Object, _dynamoMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _adminMock.VerifyAll();
            _dynamoMock.VerifyAll();
        }

        [Test]
        public async Task CreateProductTableAsync_WhenAdminServiceThrows_ResourceNotFound_Returns404()
        {
            // Arrange
            var ex = new ResourceNotFoundException("table not found");
            _adminMock.Setup(s => s.CreateProductTableAsync()).ThrowsAsync(ex);

            // Act
            var result = await _controller.CreateProductTableAsync();

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var objectResult = (NotFoundObjectResult)result;
            Assert.AreEqual((int)HttpStatusCode.NotFound, objectResult.StatusCode);
            StringAssert.Contains("Requested resource not found", objectResult.Value?.ToString() ?? "");
        }

        [Test]
        public async Task CreateProductTableAsync_WhenAdminServiceThrows_AmazonServiceUnavailable_Returns503()
        {
            // Arrange
            var awsEx = new AmazonServiceException("service unavailable")
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                ErrorCode = "ThrottlingException"
            };
            _adminMock.Setup(s => s.CreateProductTableAsync()).ThrowsAsync(awsEx);

            // Act
            var result = await _controller.CreateProductTableAsync();

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var obj = (ObjectResult)result;
            Assert.AreEqual((int)HttpStatusCode.ServiceUnavailable, obj.StatusCode);
            StringAssert.Contains("AWS service unavailable", obj.Value?.ToString() ?? "");
        }

        [Test]
        public async Task CreateProductTableAsync_WhenAdminServiceThrows_ArgumentException_Returns400()
        {
            // Arrange
            var argEx = new ArgumentException("invalid request");
            _adminMock.Setup(s => s.CreateProductTableAsync()).ThrowsAsync(argEx);

            // Act
            var result = await _controller.CreateProductTableAsync();

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual((int)HttpStatusCode.BadRequest, bad.StatusCode);
            StringAssert.Contains("Invalid request", bad.Value?.ToString() ?? "");
        }

        [Test]
        public async Task CreateProductTableAsync_WhenAdminServiceThrows_GenericException_Returns500()
        {
            // Arrange
            var ex = new Exception("boom");
            _adminMock.Setup(s => s.CreateProductTableAsync()).ThrowsAsync(ex);

            // Act
            var result = await _controller.CreateProductTableAsync();

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var obj = (ObjectResult)result;
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, obj.StatusCode);
            StringAssert.Contains("An internal error occurred", obj.Value?.ToString() ?? "");
        }

        // Additional test demonstrating legacy CreateTableAsync mapping
        [Test]
        public async Task CreateTableAsync_LegacyEndpoint_WhenServiceThrows_ArgumentException_Returns400()
        {
            // Arrange
            var argEx = new ArgumentException("bad args");
            _adminMock.Setup(s => s.CreateTableAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ScalarAttributeType>(),
                It.IsAny<string?>(),
                It.IsAny<ScalarAttributeType?>(),
                It.IsAny<bool>()))
                .ThrowsAsync(argEx);

            // Act
            var result = await _controller.CreateTableAsync("Products", "ProductId", "Name");

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
    }
}