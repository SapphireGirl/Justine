using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Moq;
using NUnit.Framework;
using Justine.Common.Services;

namespace Justine.Common.Tests
{
    [TestFixture]
    public class AdminServicesTests
    {
        // Helper to build a DescribeTableResponse with given status
        private static DescribeTableResponse DescribeResponse(TableStatus status) =>
            new DescribeTableResponse
            {
                Table = new TableDescription
                {
                    TableStatus = status,
                    TableName = "Products"
                }
            };

        [Test]
        public async Task CreateTableAsync_TableAlreadyExists_StillInvokesSeed_WhenSeedTrue()
        {
            // Arrange
            var dynamo = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);

            // Describe returns ACTIVE (table exists)
            dynamo
                .Setup(d => d.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DescribeResponse(TableStatus.ACTIVE));

            // BatchWrite for seeding should be called (seed true)
            dynamo
                .Setup(d => d.BatchWriteItemAsync(It.IsAny<BatchWriteItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BatchWriteItemResponse { UnprocessedItems = new Dictionary<string, List<WriteRequest>>() });

            var svc = new AdminServices(dynamo.Object);

            // Act
            var result = await svc.CreateTableAsync("Products", "ProductId", ScalarAttributeType.N, "Name", ScalarAttributeType.S, seed: true);

            // Assert
            Assert.That(result, Is.True);
            dynamo.Verify(d => d.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            dynamo.Verify(d => d.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            dynamo.Verify(d => d.BatchWriteItemAsync(It.IsAny<BatchWriteItemRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task CreateTableAsync_TableMissing_CreatesAndSeeds()
        {
            // Arrange
            var dynamo = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);

            // First Describe: table missing -> throw ResourceNotFoundException
            // Next Describe (polling after create): return ACTIVE
            dynamo
                .SetupSequence(d => d.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ResourceNotFoundException("Not found"))
                .ReturnsAsync(DescribeResponse(TableStatus.ACTIVE));

            // CreateTable should be invoked and return a response
            dynamo
                .Setup(d => d.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateTableResponse
                {
                    TableDescription = new TableDescription { TableName = "Products" }
                });

            // BatchWrite for seed
            dynamo
                .Setup(d => d.BatchWriteItemAsync(It.IsAny<BatchWriteItemRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BatchWriteItemResponse { UnprocessedItems = new Dictionary<string, List<WriteRequest>>() });

            var svc = new AdminServices(dynamo.Object);

            // Act
            var result = await svc.CreateTableAsync("Products", "ProductId", ScalarAttributeType.N, "Name", ScalarAttributeType.S, seed: true);

            // Assert
            Assert.That(result, Is.True);
            dynamo.Verify(d => d.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
            dynamo.Verify(d => d.CreateTableAsync(It.Is<CreateTableRequest>(r => r.TableName == "Products"), It.IsAny<CancellationToken>()), Times.Once);
            dynamo.Verify(d => d.BatchWriteItemAsync(It.IsAny<BatchWriteItemRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteTableAsync_TableNotFound_ReturnsFalse()
        {
            // Arrange
            var dynamo = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);

            // Describe throws ResourceNotFoundException -> DeleteTableAsync should return false
            dynamo
                .Setup(d => d.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ResourceNotFoundException("Not found"));

            var svc = new AdminServices(dynamo.Object);

            // Act
            var result = await svc.DeleteTableAsync("NonExistentTable");

            // Assert
            Assert.That(result, Is.False);
            dynamo.Verify(d => d.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            dynamo.Verify(d => d.DeleteTableAsync(It.IsAny<DeleteTableRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task DeleteTableAsync_TableExists_DeletesAndReturnsTrue()
        {
            // Arrange
            var dynamo = new Mock<IAmazonDynamoDB>(MockBehavior.Strict);

            // First Describe: exists; subsequent polling Describe will return DELETING then throw ResourceNotFoundException indicating deletion complete
            dynamo
                .SetupSequence(d => d.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DescribeResponse(TableStatus.ACTIVE)) // initial exists check
                .ReturnsAsync(new DescribeTableResponse { Table = new TableDescription { TableStatus = TableStatus.DELETING } }) // after delete maybe DELETING
                .ThrowsAsync(new ResourceNotFoundException("Not found")); // finally gone

            dynamo
                .Setup(d => d.DeleteTableAsync(It.IsAny<DeleteTableRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteTableResponse());

            var svc = new AdminServices(dynamo.Object);

            // Act
            var result = await svc.DeleteTableAsync("Products");

            // Assert
            Assert.That(result, Is.True);
            dynamo.Verify(d => d.DeleteTableAsync(It.Is<DeleteTableRequest>(r => r.TableName == "Products"), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}