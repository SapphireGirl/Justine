using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Moq;
using Justine.Common.Models;
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
                               new Product { ProductId = 1, Name = "Product1", Description = "Description1", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                               new Product { ProductId = 2, Name = "Product2", Description = "Description2", Price = 20.0M, ImageUrl = "url2", Quantity = 2 }
                    }
                },
                new Basket { BasketId = 2, CustomerName = "Jane",
                    Products = new List<Product> {
                               new Product { ProductId = 3, Name = "Product3", Description = "Description3", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                               new Product { ProductId = 4, Name = "Product4", Description = "Description4", Price = 20.0M, ImageUrl = "url2", Quantity = 2 }
                    }
                },
                new Basket { 
                    BasketId = 3, 
                    CustomerName = "Justine",
                    Products = new List<Product> {
                               new Product { ProductId = 5, Name = "Product5", Description = "Description5", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                               new Product { ProductId = 6, Name = "Product6", Description = "Description6", Price = 20.0M, ImageUrl = "url2", Quantity = 2 }
                    }
                }           
            };

            expectedBasket = new Basket
            {
                BasketId = 1,
                CustomerName = "Joe",
                Products = new List<Product>
                {
                    new Product { ProductId = 1, Name = "Product1", Description = "Description1", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                    new Product { ProductId = 2, Name = "Product2", Description = "Description2", Price = 20.0M, ImageUrl = "url2", Quantity = 2 }
                }
            };
            // Provide dummy AWS credentials
            var mockCredentials = new Mock<BasicAWSCredentials>("fakeAccessKey", "fakeSecretKey");

            // Create a mock AmazonDynamoDBConfig with a dummy ServiceURL
            var mockConfig = new Mock<AmazonDynamoDBConfig>("http://localhost");

            mockCredentials.Setup(x => x.GetCredentials()).Returns(mockCredentials.Object.GetCredentials());

            _mockDynamoDbContext = new Mock<IDynamoDBContext>();

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
        {
            // Clean up any resources if needed
            _mockDynamoDbContext = null;
            _testData = null;
        }

        [Test]
        public void GetBasketByIdAsync_ShouldReturnBasket_WhenBasketExists()
        {
            // Arrange
            _mockDynamoDbContext.Setup(x => x.LoadAsync<Basket>(1, default))
                .ReturnsAsync(expectedBasket);
            // Act
            // We have a basket with Id 1 in our test data
            var basketServices = new BasketServices(_mockDynamoDbContext.Object);
            var result = basketServices.GetBasketByIdAsync(1).Result;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BasketId, Is.EqualTo(expectedBasket.BasketId));
            Assert.That(result.CustomerName, Is.EqualTo(expectedBasket.CustomerName));
            Assert.That(result.Products.Count, Is.EqualTo(expectedBasket.Products.Count));
        }

        [Test]
        public void GetBasketByIdAsync_ShouldReturnNull_WhenBasketDoesNotExists()
        {
            // Arrange
            // we do not have a basket with Id 8 in our test data

            var basketServices = new BasketServices(_mockDynamoDbContext.Object);

            // Act
            // We do not have a basket with Id 8 in our test data
            var result = basketServices.GetBasketByIdAsync(8).Result;

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAllBasketsAsync_ShouldReturnAllBaskets()
        {
            // Arrange
            _mockDynamoDbContext = new Mock<IDynamoDBContext>();

            _mockDynamoDbContext
                .Setup(x => x.ScanAsync<Basket>(It.IsAny<List<ScanCondition>>()))
                .Returns(new MockAsyncSearch<Basket>(_testData));

            // Act
            var basketServices = new BasketServices(_mockDynamoDbContext.Object);
            var result = await basketServices.GetAllBasketsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(_testData.Count));
        }
        
        [Test]
        public async Task AddBasketAsync_ShouldReturnBasket_WhenBasketIsSaved()
        {
            // Arrange

            var newBasket = new Basket
            {
                CustomerName = "Justine",
                Products = new List<Product>
                {
                    new Product { ProductId = 4, Name = "Product4", Description = "Description4", Price = 10.00M, ImageUrl = "url1", Quantity = 1 },
                    new Product { ProductId = 5, Name = "Product5", Description = "Description5", Price = 20.00M, ImageUrl = "url2", Quantity = 2 }
                }
            };

            var expectedTotalPrice = newBasket.Products.Sum(item => item.Price * item.Quantity);

            newBasket.BasketId = 4; // Set the Id after saving
            // Mock LoadAsync to return the same basket that was "saved"
            _mockDynamoDbContext
                .Setup(x => x.SaveAsync(newBasket, default))
                .Returns(Task.CompletedTask);

            _mockDynamoDbContext
                .Setup(x => x.LoadAsync<Basket>(It.IsAny<int>(), default))
                .ReturnsAsync(newBasket);

            // Act
            // SUT is BasketServices
            var basketServices = new BasketServices(_mockDynamoDbContext.Object);
            var result = await basketServices.AddBasketAsync(newBasket);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Products.Count, Is.EqualTo(2));
            Assert.That(result.CustomerName, Is.EqualTo("Justine"));
            Assert.That(result.BasketId, Is.EqualTo(4));
            Assert.That(result.Products[0].Name, Is.EqualTo("Product4"));
            Assert.That(result.Products[1].Name, Is.EqualTo("Product5"));
            Assert.That(result.Products[0].Description, Is.EqualTo("Description4"));
            Assert.That(result.Products[1].Description, Is.EqualTo("Description5"));
            Assert.That(result.Products[0].Price, Is.EqualTo(10.0M));
            Assert.That(result.Products[1].Price, Is.EqualTo(20.0M));
            Assert.That(result.TotalPrice, Is.EqualTo(expectedTotalPrice));
        }

        [Test]
        public async Task UpdateBasketAsync_ShouldUpdateBasket()
        {
            // Arrange

            Basket updatedBasket = new Basket
            {
                BasketId = 1,
                CustomerName = "Joe",
                Products = new List<Product>
                {
                    new Product { ProductId = 1, Name = "Product1", Description = "UpdatedDescription1", Price = 10.0M, ImageUrl = "url1", Quantity = 2 },
                    new Product { ProductId = 2, Name = "Product2", Description = "UpdatedDescription2", Price = 20.0M, ImageUrl = "url2", Quantity = 3 }
                }
            };

            _mockDynamoDbContext
                .Setup(x => x.LoadAsync<Basket>(It.IsAny<int>(), default))
                .ReturnsAsync(updatedBasket);

            _mockDynamoDbContext
                .Setup(x => x.SaveAsync(It.IsAny<Basket>(), default))
                .Returns(Task.CompletedTask);

            // Act
            // SUT is BasketServices
            var basketServices = new BasketServices(_mockDynamoDbContext.Object);
            var result = await basketServices.UpdateBasketAsync(updatedBasket);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BasketId, Is.EqualTo(updatedBasket.BasketId));
            Assert.That(result.CustomerName, Is.EqualTo(updatedBasket.CustomerName));
            Assert.That(result.Products.Count, Is.EqualTo(updatedBasket.Products.Count));
            Assert.That(result.Products[0].Description, Is.EqualTo("UpdatedDescription1"));
            Assert.That(result.Products[1].Description, Is.EqualTo("UpdatedDescription2"));
        }

        [Test]
        public async Task DeleteBasket_DeletesBasket()
        {             
            // Arrange
            Basket basketToDelete = new Basket
            {
                BasketId = 1,
                CustomerName = "Joe",
                Products = new List<Product>
                {
                    new Product { ProductId = 1, Name = "Product1", Description = "Description1", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                    new Product { ProductId = 2, Name = "Product2", Description = "Description2", Price = 20.0M, ImageUrl = "url2", Quantity = 2 }
                }
            };

            _mockDynamoDbContext
                .Setup(x => x.LoadAsync<Basket>(It.IsAny<int>(), default))
                .ReturnsAsync(basketToDelete);

            _mockDynamoDbContext.Setup(Setup =>
                           Setup.DeleteAsync(It.IsAny<Basket>(), default))
                .Returns(Task.CompletedTask);

            var basketServices = new BasketServices(_mockDynamoDbContext.Object);

            // Act
            var result = await basketServices.DeleteBasketAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }
        
        [Test]
        public async Task GetBasketByCustomerName_ReturnsCustomerBaskets()
        {
            // Arrange

            // Act
            // sut is OrderServices
            var basketService = new BasketServices(_mockDynamoDbContext.Object);
            var result = await basketService.GetUsersBasketsByNameAsync("Justine");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }
}
    
