using Amazon.DynamoDBv2.DataModel;

namespace Justine.Common.Models
{
    [DynamoDBTable("Baskets")]
    public class Basket
    {
        // This is the partition key
        [DynamoDBHashKey]
        public int BasketId { get; set; } // Required

        [DynamoDBRangeKey]
        public string CustomerName { get; set; }

        [DynamoDBProperty("Products")]
        public List<Product> Products { get; set; } 

        [DynamoDBProperty]
        public decimal TotalPrice => Products.Sum(item => item.Price * item.Quantity);
        
        [DynamoDBProperty]
        public DateTime? CreatedAt { get; set; }
        
        [DynamoDBProperty]
        public DateTime? UpdatedAt { get; set; }
        //public Basket(int Basket_BasketId, string Basket_CustomerName)
        //{
        //    BasketId = Basket_BasketId;
        //    CustomerName = Basket_CustomerName;
        //}
    }
}
