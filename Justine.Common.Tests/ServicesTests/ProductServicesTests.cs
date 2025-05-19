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
    public class ProductServicesTests
    {
        private List<Product> _testData;
        private Mock<IDynamoDBContext> _mockDynamoDbContext;
        private Product expectedProduct;

        [SetUp]
        public void Setup()
        {
            _testData = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Description = "Description1", Price = 10.0M, ImageUrl = "url1", Quantity = 1 },
                new Product { Id = 2, Name = "Product2", Description = "Description2", Price = 20.0M, ImageUrl = "url2", Quantity = 2 },
                new Product { Id = 3, Name = "Product3", Description = "Description3", Price = 30.0M, ImageUrl = "url3", Quantity = 3 }
            };

            expectedProduct = new Product
            {
                Id = 1,
                Name = "Product1",
                Description = "Description1",
                Price = 10.0M,
                ImageUrl = "url1",
                Quantity = 1
            };

            // Provide dummy AWS credentials
            var mockCredentials = new Mock<BasicAWSCredentials>("fakeAccessKey", "fakeSecretKey");

            // Create a mock AmazonDynamoDBConfig with a dummy ServiceURL
            var mockConfig = new Mock<AmazonDynamoDBConfig>("http://localhost");

            mockCredentials.Setup(x => x.GetCredentials()).Returns(mockCredentials.Object.GetCredentials());

            var mockAsyncSearch = new Mock<AsyncSearch<Product>>(_testData);
            mockAsyncSearch.Setup(x => x.GetRemainingAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testData);

            _mockDynamoDbContext = new Mock<IDynamoDBContext>();

            _mockDynamoDbContext
                .Setup(x => x.ScanAsync<Product>(It.IsAny<List<ScanCondition>>(), It.IsAny<ScanConfig>()))
                .Returns(new MockAsyncSearch<Product>(_testData));

            _mockDynamoDbContext
                .Setup(x => x.SaveAsync(It.IsAny<Product>(), default))
                .Returns(Task.CompletedTask);

            // Mock QueryAsync to simulate GSI behavior
            _mockDynamoDbContext
                .Setup(x => x.FromQueryAsync<Product>(
                        It.IsAny<QueryOperationConfig>() // Accepts the query configuration
                ))
                .Returns((QueryOperationConfig config) =>
                {
                // Simulate filtering by CustomerName (GSI behavior)
                var Name = config.KeyExpression.ExpressionAttributeValues[":v_Name"].AsString();
                var filteredData = _testData.Where(product => product.Name == _testData[0].Name).ToList();
                return new MockAsyncSearch<Product>(_testData);
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
        public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            _mockDynamoDbContext.Setup(Setup => Setup.LoadAsync<Product>(1, default))
                .ReturnsAsync(expectedProduct);
            // Act
            var productServices = new ProductServices(_mockDynamoDbContext.Object);
            var result = await productServices.GetProductByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(expectedProduct.Id));
            Assert.That(result.Name, Is.EqualTo(expectedProduct.Name));
        }

        [Test]
        public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var productServices = new ProductServices(_mockDynamoDbContext.Object);

            // Act
            var result = await productServices.GetProductByIdAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddProductAsync_ShouldSaveProduct()
        {
            // Arrange
            var newProduct = new Product
            {
                //Id = 4,
                Name = "Product4",
                Description = "Description4",
                Price = 40.0M,
                ImageUrl = "url4",
                Quantity = 4
            };
            newProduct.Id = 4; // Set the Id after saving

            _mockDynamoDbContext
                .Setup(x => x.SaveAsync(newProduct, default))
                .Returns(Task.CompletedTask);

            _mockDynamoDbContext.Setup(x => x.LoadAsync<Product>(newProduct.Id, default))
                .ReturnsAsync(newProduct);

            // Act
            // SUT is ProductServices
            var productServices = new ProductServices(_mockDynamoDbContext.Object);
            var result = await productServices.AddProductAsync(newProduct);

            // Assert
            _mockDynamoDbContext.Verify(x => x.SaveAsync(newProduct, default), Times.Once);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(4));
            Assert.That(result.Name, Is.EqualTo(newProduct.Name));
            Assert.That(result.Description, Is.EqualTo(newProduct.Description));
            Assert.That(result.Price, Is.EqualTo(newProduct.Price));
            Assert.That(result.ImageUrl, Is.EqualTo(newProduct.ImageUrl));
        }

        [Test]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange
            var mockAsyncSearch = new Mock<AsyncSearch<Product>>(_testData);

            mockAsyncSearch.Setup(search => search.GetRemainingAsync(It.IsAny<CancellationToken>()))
                           .ReturnsAsync(_testData);

            _mockDynamoDbContext
                .Setup(x => x.ScanAsync<Product>(It.IsAny<List<ScanCondition>>()))
                .Returns(new MockAsyncSearch<Product>(_testData));


            // Act
            var productServices = new ProductServices(_mockDynamoDbContext.Object);
            var result = await productServices.GetAllProductsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(_testData.Count));
        }

        [Test]
        public async Task DeleteProductAsync_ShouldDeleteProduct_WhenProductExists()
        {
            // Arrange
            var productToDelete = _testData[_testData.Count - 1]; // Get the last product ID

            var countBeforeDelete = _testData.Count;

            _mockDynamoDbContext
                .Setup(x => x.LoadAsync<Product>(It.IsAny<int>(), default))
                .ReturnsAsync(productToDelete);

            _mockDynamoDbContext.Setup(Setup =>
                                      Setup.DeleteAsync(It.IsAny<Product>(), default));
            
            // Act
            var productServices = new ProductServices(_mockDynamoDbContext.Object);
            var result = await productServices.DeleteProductAsync(productToDelete.Id);

            // Assert
            _mockDynamoDbContext.Verify(x => x.DeleteAsync(It.IsAny<Product>(), default), Times.Once);
            Assert.That(result, Is.True);


        }

        [Test]
        public async Task UpdateProductAsync_ShouldUpdateProduct()
        {
            // Arrange
            Product updatedProduct = new Product
            {
                Id = 1,
                Name = "UpdatedProduct",
                Description = "UpdatedDescription",
                Price = 15.0M,
                ImageUrl = "updatedUrl",
                Quantity = 2
            };
            _mockDynamoDbContext
                .Setup(x => x.LoadAsync<Product>(updatedProduct.Id, default))
                .ReturnsAsync(updatedProduct);
            _mockDynamoDbContext.Setup(x => x.SaveAsync(updatedProduct, default))
                .Returns(Task.CompletedTask);
            // Act
            // Sut is ProductServices

            var productServices = new ProductServices(_mockDynamoDbContext.Object);
            var result = await productServices.UpdateProductAsync(updatedProduct);
            
            // Assert
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("UpdatedProduct"));
            Assert.That(result.Description, Is.EqualTo("UpdatedDescription"));
            Assert.That(result.Price, Is.EqualTo(15.0M));
            Assert.That(result.ImageUrl, Is.EqualTo("updatedUrl"));
            Assert.That(result.Quantity, Is.EqualTo(2));
        }
    }
}
