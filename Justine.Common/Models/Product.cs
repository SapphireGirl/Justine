using Amazon.DynamoDBv2.DataModel;

namespace Justine.Common.Models
{
    [DynamoDBTable("Products")]
    public class Product
    {
        // This is the partition key
        [DynamoDBHashKey]
        public int ProductId { get; set; } // Required

        // Maps a class property to the sort key of the table's primary key
        [DynamoDBRangeKey]
        private string _name = string.Empty; // Initialize to a non-null default value

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Name cannot be null or empty because it is a DynamoDBRangeKey.");
                }
                _name = value;
            }
        }

        [DynamoDBProperty]
        public string? Description { get; set; } // Nullable, optional

        [DynamoDBProperty]
        public decimal Price { get; set; } // Required

        [DynamoDBProperty]
        public string? ImageUrl { get; set; } // Nullable, optional

        [DynamoDBProperty]
        public int Quantity { get; set; } // Required

        [DynamoDBProperty]
        public DateTime? CreatedAt { get; set; } // Nullable, optional

        [DynamoDBProperty]
        public DateTime? UpdatedAt { get; set; } // Nullable, optional
    }
}
