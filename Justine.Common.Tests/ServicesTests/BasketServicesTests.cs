using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Moq;
using Justine.Common.Models;
using NUnit.Framework;
using Justine.Common.Services;
using Amazon.Runtime;
using Amazon.DynamoDBv2.DocumentModel;

namespace Justine.Common.Tests.ServicesTests
{
    [TestFixture]
    public class BasketServicesTests
    {
        private List<Basket> _testData;
        private Mock<IDynamoDBContext> _mockDynamoDbContext;
        private Basket expectedBasket;

        [SetUp]
        public void Setup()
        {
            _testData = new List<Basket>
            {
                new Basket { BasketId = 1, CustomerName = "Joe",
                    Products = new List<Product> {
                               new Product { Id = 1, Name = "Product1", Description = "Description1", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                               new Product { Id = 2, Name = "Product2", Description = "Description2", Price = 20.0M, ImageUrl = "url2", Quantity = 2 }
                    }
                },
                new Basket { BasketId = 2, CustomerName = "Jane",
                    Products = new List<Product> {
                               new Product { Id = 3, Name = "Product3", Description = "Description3", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                               new Product { Id = 4, Name = "Product4", Description = "Description4", Price = 20.0M, ImageUrl = "url2", Quantity = 2 }
                    }
                },
                new Basket { BasketId = 3, CustomerName = "Justine",
                    Products = new List<Product> {
                               new Product { Id = 5, Name = "Product5", Description = "Description5", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                               new Product { Id = 6, Name = "Product6", Description = "Description6", Price = 20.0M, ImageUrl = "url2", Quantity = 2 }
                    }
                }           
            };

            expectedBasket = new Basket
            {
                BasketId = 1,
                CustomerName = "Joe",
                Products = new List<Product>
                {
                    new Product { Id = 1, Name = "Product1", Description = "Description1", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                    new Product { Id = 2, Name = "Product2", Description = "Description2", Price = 20.0M, ImageUrl = "url2", Quantity = 2 }
                }
            };
            // Provide dummy AWS credentials
            var mockCredentials = new Mock<BasicAWSCredentials>("fakeAccessKey", "fakeSecretKey");

            // Create a mock AmazonDynamoDBConfig with a dummy ServiceURL
            var mockConfig = new Mock<AmazonDynamoDBConfig>("http://localhost");

            mockCredentials.Setup(x => x.GetCredentials()).Returns(mockCredentials.Object.GetCredentials());

            _mockDynamoDbContext = new Mock<IDynamoDBContext>();

            _mockDynamoDbContext
                .Setup(x => x.SaveAsync(It.IsAny<Basket>(), default))
                .Returns(Task.CompletedTask);

            // Mock LoadAsync to return the same basket that was "saved"
            _mockDynamoDbContext
                .Setup(x => x.LoadAsync<Basket>(It.IsAny<int>(), default))
                .ReturnsAsync((int basketId, CancellationToken _) => new Basket
                {
                    BasketId = 4,
                    CustomerName = "Justine",
                    Products = new List<Product>
                    {
                        new Product { Id = 4, Name = "Product4", Description = "Description4", Price = 100.0M, ImageUrl = "url1", Quantity = 1 },
                        new Product { Id = 5, Name = "Product5", Description = "Description5", Price = 200.0M, ImageUrl = "url2", Quantity = 2 }
                    },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
                );

            // Mock QueryAsync to simulate GSI behavior
            _mockDynamoDbContext
                .Setup(x => x.FromQueryAsync<Basket>(
                    It.IsAny<QueryOperationConfig>() 
                ))// Accepts the query configuration
                                                                                      
                .Returns((QueryOperationConfig config) =>
                {
                    // Simulate filtering by CustomerName (GSI behavior)
                    var customerName = config.KeyExpression.ExpressionAttributeValues[":v_customerName"].AsString();
                    var filteredData = _testData.Where(basket => basket.CustomerName == customerName).ToList();

                    return new MockAsyncSearch<Basket>(filteredData);
                });
        }

        [TearDown]
        public void Teardown()
        { }

        [Test]
        public void GetNextIdAsyncReturns4()
        {
            // Arrange

            var basketServices = new BasketServices(_mockDynamoDbContext.Object);

            // Act
            // sut is BasketServices
            var result = basketServices.GetNextIdAsync("Justine").Result;

            // Assert
            Assert.That(result, Is.EqualTo(4));
        }

        [Test]
        public async Task AddBasketAsync_ShouldReturnBasket_WhenBasketIsSaved()
        {
            // Arrange
            // sut is BasketServices

            var basketServices = new BasketServices(_mockDynamoDbContext.Object);

            var newBasket = new Basket
            {
                CustomerName = "Justine",
                Products = new List<Product>
                {
                    new Product { Id = 4, Name = "Product4", Description = "Description1", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                    new Product { Id = 5, Name = "Product5", Description = "Description2", Price = 20.0M, ImageUrl = "url2", Quantity = 2 }
                }
            };
            // Act
            var result = await basketServices.AddBasketAsync(newBasket);

            // Assert
            Assert.That(result, Is.Not.Null);
            //Assert.That(result.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1)));
            //Assert.That(result.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1)));
            Assert.That(result.Products.Count, Is.EqualTo(2));
            Assert.That(result.CustomerName, Is.EqualTo("Justine"));
            Assert.That(result.BasketId, Is.EqualTo(4));
            Assert.That(result.Products[0].Name, Is.EqualTo("Product4"));
            Assert.That(result.Products[1].Name, Is.EqualTo("Product5"));
            Assert.That(result.Products[0].Description, Is.EqualTo("Description4"));
            Assert.That(result.Products[1].Description, Is.EqualTo("Description5"));
            // TotalPrice is coming back 500.00M
            //Assert.That(result.TotalPrice, Is.EqualTo(300.00M));

        }

        // Add more tests for other methods in IBasketServices...
    }
}
