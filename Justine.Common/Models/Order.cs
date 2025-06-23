using Amazon.DynamoDBv2.DataModel;

namespace Justine.Common.Models
{
    [DynamoDBTable("Orders")]
    public class Order
    {
        [DynamoDBHashKey]
        public int OrderId { get; set; } // Required

        [DynamoDBRangeKey]
        private string _customerName = string.Empty; // Initialize to a non-null default value

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

        [DynamoDBProperty]
        public int BasketId { get; set; } // Required

        [DynamoDBProperty]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow; // Initialize to a default value
    }
}
