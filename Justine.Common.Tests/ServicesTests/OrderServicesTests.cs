using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Justine.Common.Exceptions;
using Justine.Common.Models;
using Justine.Common.Services;
using Moq;

namespace Justine.Common.Tests.ServicesTests
{
    [TestFixture]
    public class OrderServicesTests
    {
        private List<Order> _testData;
        private Mock<IAmazonDynamoDB>? _mockDynamoDbClient;
        private Order expectedOrder;

        [SetUp]
        public void Setup()
        {
            _testData =
            [
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
            ];

            expectedOrder = new Order
            {
                OrderId = 1,
                CustomerName = "Joe",
                BasketId = 1
            };

            // Provide dummy AWS credentials
            var mockCredentials = new Mock<BasicAWSCredentials>("fakeAccessKey", "fakeSecretKey");

            // Setup PutItemAsync used for AddBasketAsync/UpdateBasketAsync
            _mockDynamoDbClient
                .Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutItemResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

            // Setup DeleteItemAsync used for DeleteBasketAsync
            _mockDynamoDbClient
                .Setup(x => x.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteItemResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

            // Setup ScanAsync used for GetAllBasketsAsync
            //_mockDynamoDbClient
            //    .Setup(x => x.ScanAsync(It.IsAny<ScanRequest>(), It.IsAny<CancellationToken>()))
            //    .ReturnsAsync((ScanRequest req, CancellationToken ct) =>
            //    {
            //        var items = _testData.Select(b => ConvertBasketToAttributeMap(b)).ToList();
            //        return new ScanResponse { Items = items };
            //    });

            // Setup QueryAsync used for GetUsersBasketsByNameAsync (GSI)
            //_mockDynamoDbClient
            //    .Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            //    .ReturnsAsync((QueryRequest req, CancellationToken ct) =>
            //    {
            //        // Attempt to get the first expression attribute value and use its S value as customer name
            //        var customerName = req.ExpressionAttributeValues?.Values.FirstOrDefault()?.S;
            //        var filtered = _testData.Where(b => b.CustomerName == customerName).Select(b => ConvertBasketToAttributeMap(b)).ToList();
            //        return new QueryResponse { Items = filtered };
            //    });
        }

        [TearDown] public void Teardown() 
        {
            // Clean up any resources if needed
            _mockDynamoDbClient = null;
            _testData = null;
        }

        [Test]
        public async Task GetOrderByIdAsync_ShouldReturnOrder_WhenOrderExists()
        {
            // Arrange
            //_mockDynamoDbClient
            //    .Setup(x => x.LoadAsync<Order>(It.IsAny<int>(), default))
            //    .ReturnsAsync(expectedOrder);
            // Act
            // We have a Order with Id 1 in our test data
            var orderServices = new OrderServices(_mockDynamoDbClient.Object);
            var result = await orderServices.GetOrderByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
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
            var orderServices = new OrderServices(_mockDynamoDbClient.Object);
            var orderId = 8;

            var ex = Assert.ThrowsAsync<OrderException>(async () =>
                await orderServices.GetOrderByIdAsync(orderId));
            Assert.That(ex.Message, Is.EqualTo("Error getting Order with id 8 failed: Order with OrderId 8 not found."));
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

            var request = new PutItemRequest
            {
                TableName = "Orders",
                Item = new Dictionary<string, AttributeValue>
                {
                    { "OrderId", new AttributeValue { N = "4" } },
                    { "CustomerName", new AttributeValue { S = "Justine" } },
                    { "BasketId", new AttributeValue { N = "4" } }
                }
            };

            newOrder.OrderId = 4; // Set the OrderId after saving
            //_mockDynamoDbClient
            //    .Setup(expression: x => x.PutItemAsync(request))
            //    .ReturnsAsync(new PutItemResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

            // Act
            // sut is OrderServices
            var orderServices = new OrderServices(_mockDynamoDbClient.Object);
            var result = await orderServices.AddOrderAsync(newOrder);

            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public async Task UpdateOrderAsync_ShouldUpdateOrder()
        {
            // Arrange

            Order updatedOrder = new()
            {
                OrderId = 1,
                CustomerName = "Joe",
                BasketId = 1
            };

            //_mockDynamoDbClient
            //    .Setup(x => x.LoadAsync<Order>(It.IsAny<int>(), default))
            //    .ReturnsAsync(updatedOrder);

            //_mockDynamoDbClient
            //    .Setup(x => x.SaveAsync(It.IsAny<Order>(), default))
            //    .Returns(Task.CompletedTask);

            // Act
            // SUT is OrderServices
            var OrderServices = new OrderServices(_mockDynamoDbClient.Object);
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
            Order OrderToDelete = new()
            {
                OrderId = 1,
                CustomerName = "Joe",
                BasketId = 4,
            };

            

            // Act
            var OrderServices = new OrderServices(_mockDynamoDbClient.Object);
            var result = await OrderServices.DeleteOrderAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task GetAllOrdersAsync_ShouldReturnAllOrders()
        {
            //// Arrange
            //var OrderServices = new OrderServices(_mockDynamoDbClient.Object);

            //mockAsyncSearch.Setup(search => search.GetRemainingAsync(It.IsAny<CancellationToken>()))
            //               .ReturnsAsync(_testData);

            //_mockDynamoDbClient
            //    .Setup(x => x.ScanAsync<Order>(It.IsAny<List<ScanCondition>>()))
            //    .Returns(new MockAsyncSearch<Order>(_testData));


            // Act
            var orderServices = new OrderServices(_mockDynamoDbClient.Object);
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
            var orderService = new OrderServices(_mockDynamoDbClient.Object);
            var result = await orderService.GetOrdersByCustomer("Justine");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }
}
