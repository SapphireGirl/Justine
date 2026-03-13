using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Moq;
using Justine.Common.Models;
using Justine.Common.Services;
using System.Net;

namespace Justine.Common.Tests.ServicesTests
{
    [TestFixture]
    public class ProductServicesTests
    {
        private List<Product> _testData = null!;
        private Mock<IAmazonDynamoDB> _mockAmazonDynamoDB = null!;
        private Product expectedProduct = null!;

        [SetUp]
        public void Setup()
        {
            _testData = new List<Product>
            {
                new Product { ProductId = 1, Name = "Product1", Description = "Description1", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                new Product { ProductId = 2, Name = "Product2", Description = "Description2", Price = 20.0M, ImageUrl = "url2", Quantity = 2 },
                new Product { ProductId = 3, Name = "Product3", Description = "Description3", Price = 30.0M, ImageUrl = "url3", Quantity = 3 }
            };

            expectedProduct = new Product
            {
                ProductId = 1,
                Name = "Product1",
                Description = "Description1",
                Price = 10.0M,
                ImageUrl = "url1",
                Quantity = 1
            };

            _mockAmazonDynamoDB = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);
        }

        [TearDown]
        public void Teardown()
        {
            _mockAmazonDynamoDB = null!;
            _testData = null!;
        }

        [Test]
        public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                { "ProductId", new AttributeValue { N = expectedProduct.ProductId.ToString() } },
                { "Name", new AttributeValue { S = expectedProduct.Name } },
                { "Description", new AttributeValue { S = expectedProduct.Description } },
                { "Price", new AttributeValue { N = expectedProduct.Price.ToString() } },
                { "ImageUrl", new AttributeValue { S = expectedProduct.ImageUrl } },
                { "Quantity", new AttributeValue { N = expectedProduct.Quantity.ToString() } },
                { "CreatedAt", new AttributeValue { S = expectedProduct.CreatedAt?.ToString("o") ?? string.Empty } },
                { "UpdatedAt", new AttributeValue { S = expectedProduct.UpdatedAt?.ToString("o") ?? string.Empty } }
            };

            _mockAmazonDynamoDB
                .Setup(x => x.GetItemAsync(It.Is<GetItemRequest>(r => r.Key["ProductId"].N == "1"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetItemResponse { Item = item, HttpStatusCode = HttpStatusCode.OK });

            var productServices = new ProductServices(_mockAmazonDynamoDB.Object);

            // Act
            var result = await productServices.GetProductByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ProductId, Is.EqualTo(expectedProduct.ProductId));
            Assert.That(result.Name, Is.EqualTo(expectedProduct.Name));

            _mockAmazonDynamoDB.VerifyAll();
        }

        [Test]
        public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            _mockAmazonDynamoDB
                .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetItemResponse { Item = new Dictionary<string, AttributeValue>(), HttpStatusCode = HttpStatusCode.OK });

            var productServices = new ProductServices(_mockAmazonDynamoDB.Object);

            // Act
            var result = await productServices.GetProductByIdAsync(999);

            // Assert
            Assert.That(result, Is.Null);

            _mockAmazonDynamoDB.VerifyAll();
        }

        [Test]
        public async Task AddProductAsync_ShouldSaveProduct()
        {
            // Arrange
            var newProduct = new Product
            {
                ProductId = 4,
                Name = "Product4",
                Description = "Description4",
                Price = 40.0M,
                ImageUrl = "url4",
                Quantity = 4
            };

            _mockAmazonDynamoDB
                .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutItemResponse { HttpStatusCode = HttpStatusCode.OK });

            var productServices = new ProductServices(_mockAmazonDynamoDB.Object);

            // Act
            var result = await productServices.AddProductAsync(newProduct);

            // Assert
            Assert.That(result, Is.True);
            _mockAmazonDynamoDB.Verify(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange
            var items = _testData.Select(p => new Dictionary<string, AttributeValue>
            {
                { "ProductId", new AttributeValue { N = p.ProductId.ToString() } },
                { "Name", new AttributeValue { S = p.Name } },
                { "Description", new AttributeValue { S = p.Description } },
                { "Price", new AttributeValue { N = p.Price.ToString() } },
                { "ImageUrl", new AttributeValue { S = p.ImageUrl } },
                { "Quantity", new AttributeValue { N = p.Quantity.ToString() } },
                { "CreatedAt", new AttributeValue { S = p.CreatedAt?.ToString("o") ?? string.Empty } },
                { "UpdatedAt", new AttributeValue { S = p.UpdatedAt?.ToString("o") ?? string.Empty } }
            }).ToList();

            _mockAmazonDynamoDB
                .Setup(x => x.ScanAsync(It.IsAny<ScanRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ScanResponse { Items = items, HttpStatusCode = HttpStatusCode.OK });

            var productServices = new ProductServices(_mockAmazonDynamoDB.Object);

            // Act
            var result = await productServices.GetAllProductsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(_testData.Count));

            _mockAmazonDynamoDB.VerifyAll();
        }

        [Test]
        public async Task DeleteProductAsync_ShouldDeleteProduct_WhenProductExists()
        {
            // Arrange
            var idToDelete = _testData.Last().ProductId;

            _mockAmazonDynamoDB
                .Setup(x => x.DeleteItemAsync(It.Is<DeleteItemRequest>(r => r.Key["ProductId"].N == idToDelete.ToString()), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteItemResponse { HttpStatusCode = HttpStatusCode.OK });

            var productServices = new ProductServices(_mockAmazonDynamoDB.Object);

            // Act
            var result = await productServices.DeleteProductAsync(idToDelete);

            // Assert
            Assert.That(result, Is.True);
            _mockAmazonDynamoDB.Verify(x => x.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task UpdateProductAsync_ShouldUpdateProduct()
        {
            // Arrange
            Product updatedProduct = new Product
            {
                ProductId = 1,
                Name = "UpdatedProduct",
                Description = "UpdatedDescription",
                Price = 15.0M,
                ImageUrl = "updatedUrl",
                Quantity = 2
            };

            _mockAmazonDynamoDB
                .Setup(x => x.PutItemAsync(It.Is<PutItemRequest>(req => req.Item["ProductId"].N == updatedProduct.ProductId.ToString()), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutItemResponse { HttpStatusCode = HttpStatusCode.OK });

            var productServices = new ProductServices(_mockAmazonDynamoDB.Object);

            // Act
            var result = await productServices.UpdateProductAsync(updatedProduct);

            // Assert
            Assert.That(result.ProductId, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("UpdatedProduct"));
            Assert.That(result.Description, Is.EqualTo("UpdatedDescription"));
            Assert.That(result.Price, Is.EqualTo(15.0M));
            Assert.That(result.ImageUrl, Is.EqualTo("updatedUrl"));
            Assert.That(result.Quantity, Is.EqualTo(2));

            _mockAmazonDynamoDB.VerifyAll();
        }
    }
}
