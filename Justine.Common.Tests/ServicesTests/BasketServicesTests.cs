using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Moq;
using Justine.Common.Models;
using Justine.Common.Services;
using Newtonsoft.Json;
using Justine.Common.Exceptions;

namespace Justine.Common.Tests.ServicesTests
{
    [TestFixture]
    public class BasketServicesTests
    {
        private List<Basket> _testData;
        private Mock<IAmazonDynamoDB> _mockDynamoDbClient;
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

            // Mock IAmazonDynamoDB
            _mockDynamoDbClient = new Mock<IAmazonDynamoDB>();

            // Setup GetItemAsync used for GetBasketByIdAsync and AddBasketAsync/load
            _mockDynamoDbClient
                .Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetItemRequest req, CancellationToken ct) =>
                {
                    if (req.Key != null && req.Key.TryGetValue("BasketId", out var idAttr) && int.TryParse(idAttr.N, out var id))
                    {
                        var found = _testData.FirstOrDefault(b => b.BasketId == id);
                        return new GetItemResponse { Item = found != null ? ConvertBasketToAttributeMap(found) : new Dictionary<string, AttributeValue>() };
                    }

                    return new GetItemResponse { Item = new Dictionary<string, AttributeValue>() };
                });

            // Setup PutItemAsync used for AddBasketAsync/UpdateBasketAsync
            _mockDynamoDbClient
                .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutItemResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

            // Setup DeleteItemAsync used for DeleteBasketAsync
            _mockDynamoDbClient
                .Setup(x => x.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteItemResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

            // Setup ScanAsync used for GetAllBasketsAsync
            _mockDynamoDbClient
                .Setup(x => x.ScanAsync(It.IsAny<ScanRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ScanRequest req, CancellationToken ct) =>
                {
                    var items = _testData.Select(b => ConvertBasketToAttributeMap(b)).ToList();
                    return new ScanResponse { Items = items };
                });

            // Setup QueryAsync used for GetUsersBasketsByNameAsync (GSI)
            _mockDynamoDbClient
                .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((QueryRequest req, CancellationToken ct) =>
                {
                    // Attempt to get the first expression attribute value and use its S value as customer name
                    var customerName = req.ExpressionAttributeValues?.Values.FirstOrDefault()?.S;
                    var filtered = _testData.Where(b => b.CustomerName == customerName).Select(b => ConvertBasketToAttributeMap(b)).ToList();
                    return new QueryResponse { Items = filtered };
                });
        }

        [TearDown]
        public void Teardown()
        {
            _mockDynamoDbClient = null;
            _testData = null;
        }

        [Test]
        public void GetBasketByIdAsync_ShouldReturnBasket_WhenBasketExists()
        {
            // Arrange
            var basketServices = new BasketServices(_mockDynamoDbClient.Object);

            // Act
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
            var basketServices = new BasketServices(_mockDynamoDbClient.Object);

            // Act
            var basketId = 8;

            var ex = Assert.ThrowsAsync<BasketException>(async () =>
                await basketServices.GetBasketByIdAsync(basketId));
        }

        [Test]
        public async Task GetAllBasketsAsync_ShouldReturnAllBaskets()
        {
            // Act
            var basketServices = new BasketServices(_mockDynamoDbClient.Object);
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
                    new Product { ProductId = 7, Name = "Product4", Description = "Description4", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                    new Product { ProductId = 8, Name = "Product5", Description = "Description5", Price = 20.0M, ImageUrl = "url2", Quantity = 2 }
                }
            };

            // Simulate that after saving the basket gets BasketId = 4 and will be returned by GetItemAsync
            newBasket.BasketId = 4;
            _testData.Add(newBasket);

            // Act
            var basketServices = new BasketServices(_mockDynamoDbClient.Object);
            var result = await basketServices.AddBasketAsync(newBasket);

            // Assert
            Assert.That(result, Is.EqualTo(true));
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

            // Ensure the data source returns the updated basket on GetItemAsync
            var existingIndex = _testData.FindIndex(b => b.BasketId == updatedBasket.BasketId);
            if (existingIndex >= 0) _testData[existingIndex] = updatedBasket;
            else _testData.Add(updatedBasket);

            var basketServices = new BasketServices(_mockDynamoDbClient.Object);
            var result = await basketServices.UpdateBasketAsync(updatedBasket);

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

            // Ensure the data contains the basket
            if (!_testData.Any(b => b.BasketId == basketToDelete.BasketId))
                _testData.Add(basketToDelete);

            var basketServices = new BasketServices(_mockDynamoDbClient.Object);

            // Act
            var result = await basketServices.DeleteBasketAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task GetBasketByCustomerName_ReturnsCustomerBaskets()
        {
            // Act
            var basketService = new BasketServices(_mockDynamoDbClient.Object);
            var result = await basketService.GetUsersBasketsByNameAsync("Justine");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        // Helper to convert Basket -> DynamoDB attribute map (keeps Products as JSON string)
        private static Dictionary<string, AttributeValue> ConvertBasketToAttributeMap(Basket b)
        {
            var map = new Dictionary<string, AttributeValue>
            {
                { "BasketId", new AttributeValue { N = b.BasketId.ToString() } },
                { "CustomerName", new AttributeValue { S = b.CustomerName ?? string.Empty } },
                { "Products", new AttributeValue { S = JsonConvert.SerializeObject(b.Products ?? new List<Product>()) } },
                { "TotalPrice", new AttributeValue { N = (b.Products?.Sum(p => p.Price * p.Quantity) ?? 0M).ToString() } },
                { "CreatedAt", new AttributeValue { S = b.CreatedAt?.ToString("o") ?? string.Empty } },
                { "UpdatedAt", new AttributeValue { S = b.UpdatedAt?.ToString("o") ?? string.Empty } }
            };
            return map;
        }
    }
}