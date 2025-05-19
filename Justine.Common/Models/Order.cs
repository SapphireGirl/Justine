using Amazon.DynamoDBv2.DataModel;

namespace Justine.Common.Models
{
    [DynamoDBTable("Orders")]
    public class Order
    {
        [DynamoDBHashKey]
        public int OrderId { get; set; }
        
        [DynamoDBRangeKey]
        public string CustomerName { get; set; } // Required
        
        [DynamoDBProperty]
        public int BasketId { get; set; } // Required
        
        [DynamoDBProperty]
        public DateTime OrderDate { get; set; }
    }
}
