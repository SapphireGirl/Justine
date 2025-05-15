using System.Collections.Generic;
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
    public class OrderServicesTests
    {
        private List<Order> _testData;
        private Mock<IDynamoDBContext> _mockDynamoDbContext;

        [SetUp]
        public void Setup()
        {
            _testData = new List<Order>
            {
                // OrderId, CustomerName, BasketId
                new Order(1, "Joe", 1),
                new Order(2, "Jane", 2),
                new Order(3, "Justine", 3)
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
        { }

        [Test]
        public void GetNextIdAsyncReturns4()
        {
            // Arrange

            var orderService = new OrderServices(_mockDynamoDbContext.Object);

            // Act
            // sut is OrderServices
            var result = orderService.GetNextIdAsync("Justine").Result;

            // Assert
            Assert.That(result, Is.EqualTo(4));
        }

        [Test]
        public void GetOrdersByCustomerReturnsOrders()
        {
            // Arrange
            // sut is OrderServices
            var orderService = new OrderServices(_mockDynamoDbContext.Object);

            // Act
            var result = orderService.GetOrdersByCustomer("Justine").Result;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }

    public class MockAsyncSearch<T> : AsyncSearch<T>
    {
        private readonly List<T> _data;

        public MockAsyncSearch(IEnumerable<T> data)
        {
            _data = data.ToList();
        }

        public override Task<List<T>> GetRemainingAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_data);
        }
    }
}
