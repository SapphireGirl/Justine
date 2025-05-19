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
    public class OrderServicesTests
    {
        private List<Order> _testData;
        private Mock<IDynamoDBContext> _mockDynamoDbContext;
        private Order expectedOrder;

        [SetUp]
        public void Setup()
        {
            _testData = new List<Order>
            {
                // OrderId, CustomerName, OrderId
                new Order
                {
                    OrderId = 1, 
                    CustomerName = "Joe", 
                    BasketId = 1 
                },
                new Order
                {
                    OrderId = 2, 
                    CustomerName = "Jane", 
                    BasketId = 2 },
                new Order
                { 
                    OrderId = 3, 
                    CustomerName = "Justine", 
                    BasketId = 3 
                }
            };

            expectedOrder = new Order
            {
                OrderId = 1,
                CustomerName = "Joe",
                BasketId = 1
            };

            // Provide dummy AWS credentials
            var mockCredentials = new Mock<BasicAWSCredentials>("fakeAccessKey", "fakeSecretKey");

            // Create a mock AmazonDynamoDBConfig with a dummy ServiceURL
            var mockConfig = new Mock<AmazonDynamoDBConfig>("http://localhost");

            mockCredentials.Setup(x => x.GetCredentials()).Returns(mockCredentials.Object.GetCredentials());

           _mockDynamoDbContext = new Mock<IDynamoDBContext>();

            // Mock QueryAsync to simulate GSI behavior
            _mockDynamoDbContext
                .Setup(x => x.FromQueryAsync<Order>(
                    It.IsAny<QueryOperationConfig>() // Accepts the query configuration
                ))
                .Returns((QueryOperationConfig config) =>
                {
                    // Simulate filtering by CustomerName (GSI behavior)
                    var customerName = config.KeyExpression.ExpressionAttributeValues[":v_customerName"].AsString();
                    var filteredData = _testData.Where(order => order.CustomerName == customerName).ToList();

                    return new MockAsyncSearch<Order>(filteredData);
                });
        }

        [TearDown] public void Teardown() 
        {
            // Clean up any resources if needed
            _mockDynamoDbContext = null;
            _testData = null;
        }

        [Test]
        public async Task GetOrderByIdAsync_ShouldReturnOrder_WhenOrderExists()
        {
            // Arrange
            _mockDynamoDbContext
                .Setup(x => x.LoadAsync<Order>(It.IsAny<int>(), default))
                .ReturnsAsync(expectedOrder);
            // Act
            // We have a Order with Id 1 in our test data
            var orderServices = new OrderServices(_mockDynamoDbContext.Object);
            var result = await orderServices.GetOrderByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.OrderId, Is.EqualTo(expectedOrder.OrderId));
            Assert.That(result.OrderId, Is.EqualTo(expectedOrder.OrderId));
            Assert.That(result.CustomerName, Is.EqualTo(expectedOrder.CustomerName));
        }

        [Test]
        public async Task GetOrderByIdAsync_ShouldReturnNull_WhenOrderDoesNotExists()
        {
            // Arrange

            // Act
            // SUT is OrderServices
            // We do not have a Order with Id 8 in our test data
            var orderServices = new OrderServices(_mockDynamoDbContext.Object);
            var result = await orderServices.GetOrderByIdAsync(8);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddOrderAsync_ShouldReturnOrder_WhenOrderIsSaved()
        {
            // Arrange


            var newOrder = new Order
            {
               // No OrderId 
                CustomerName = "Justine",
                BasketId = 4
            };

            _mockDynamoDbContext
                .Setup(x => x.SaveAsync(newOrder, default))
                .Returns(Task.CompletedTask);

            newOrder.OrderId = 4; // Set the OrderId after saving
            _mockDynamoDbContext
                .Setup(x => x.LoadAsync<Order>(newOrder.OrderId, default))
                .ReturnsAsync(newOrder);

            // Act
            // sut is OrderServices
            var orderServices = new OrderServices(_mockDynamoDbContext.Object);
            var result = await orderServices.AddOrderAsync(newOrder);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CustomerName, Is.EqualTo("Justine"));
            Assert.That(result.OrderId, Is.EqualTo(4));
            Assert.That(result.BasketId, Is.EqualTo(4));
        }

        [Test]
        public async Task UpdateOrderAsync_ShouldUpdateOrder()
        {
            // Arrange

            Order updatedOrder = new Order
            {
                OrderId = 1,
                CustomerName = "Joe",
                BasketId = 1
            };

            _mockDynamoDbContext
                .Setup(x => x.LoadAsync<Order>(It.IsAny<int>(), default))
                .ReturnsAsync(updatedOrder);

            _mockDynamoDbContext
                .Setup(x => x.SaveAsync(It.IsAny<Order>(), default))
                .Returns(Task.CompletedTask);

            // Act
            // SUT is OrderServices
            var OrderServices = new OrderServices(_mockDynamoDbContext.Object);
            var result = await OrderServices.UpdateOrderAsync(updatedOrder);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.OrderId, Is.EqualTo(updatedOrder.OrderId));
            Assert.That(result.BasketId, Is.EqualTo(updatedOrder.BasketId));
            Assert.That(result.CustomerName, Is.EqualTo(updatedOrder.CustomerName));
        }

        [Test]
        public async Task DeleteOrder_DeletesOrder()
        {
            // Arrange
            Order OrderToDelete = new Order
            {
                OrderId = 1,
                CustomerName = "Joe",
                BasketId = 4
            };

            _mockDynamoDbContext
                .Setup(x => x.LoadAsync<Order>(It.IsAny<int>(), default))
                .ReturnsAsync(OrderToDelete);

            _mockDynamoDbContext.Setup(Setup =>
                           Setup.DeleteAsync(It.IsAny<Order>(), default))
                .Returns(Task.CompletedTask);

            // Act
            var OrderServices = new OrderServices(_mockDynamoDbContext.Object);
            var result = await OrderServices.DeleteOrderAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task GetAllOrdersAsync_ShouldReturnAllOrders()
        {
            // Arrange
            var mockAsyncSearch = new Mock<AsyncSearch<Order>>(_testData);

            mockAsyncSearch.Setup(search => search.GetRemainingAsync(It.IsAny<CancellationToken>()))
                           .ReturnsAsync(_testData);

            _mockDynamoDbContext
                .Setup(x => x.ScanAsync<Order>(It.IsAny<List<ScanCondition>>()))
                .Returns(new MockAsyncSearch<Order>(_testData));


            // Act
            var orderServices = new OrderServices(_mockDynamoDbContext.Object);
            var result = await orderServices.GetAllOrdersAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(_testData.Count));
        }

        [Test]
        public async Task GetOrdersByCustomerName_ReturnsCustomerOrders()
        {
            // Arrange


            // Act
            // sut is OrderServices
            var orderService = new OrderServices(_mockDynamoDbContext.Object);
            var result = await orderService.GetOrdersByCustomer("Justine");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }
}
