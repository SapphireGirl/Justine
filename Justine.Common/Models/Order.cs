using Amazon.DynamoDBv2.DataModel;

namespace Justine.Common.Models
{
    [DynamoDBTable("Orders")]
    public class Order
    {
        [DynamoDBHashKey]
        public int OrderId { get; set; } // Required

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

        [DynamoDBProperty]
        public int BasketId { get; set; } // Required

        [DynamoDBProperty]
        public DateTime? CreatedAt { get; set; } // Nullable, optional

        [DynamoDBProperty]
        public DateTime? UpdatedAt { get; set; } // Nullable, optional
    }
}
