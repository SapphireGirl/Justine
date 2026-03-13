using Amazon.DynamoDBv2.DataModel;

namespace Justine.Common.Models
{
    [DynamoDBTable("Baskets")]
    public class Basket
    {
        // This is the partition key
        [DynamoDBHashKey]
        public int BasketId { get; set; } // Required

        private string _customerName = string.Empty; // Initialize to a non-null default value

        [DynamoDBRangeKey]
        public string CustomerName
        {
            get => _customerName;

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("CustomerName cannot be null or empty because it is a DynamoDBRangeKey.");
                }
                _customerName = value;
            }
        }

        [DynamoDBProperty("Products")]
        public List<Product> Products { get; set; } = new List<Product>(); // Initialize to an empty list

        [DynamoDBProperty]
        public decimal TotalPrice => Products.Sum(item => item.Price * item.Quantity);

        [DynamoDBProperty]
        public DateTime? CreatedAt { get; set; }

        [DynamoDBProperty]
        public DateTime? UpdatedAt { get; set; }
    }
}
