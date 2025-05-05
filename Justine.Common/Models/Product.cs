using Amazon.DynamoDBv2.DataModel;
using System;


namespace Justine.Common.Models
{
    [DynamoDBTable("Products")]
    public class Product 
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; }
        [DynamoDBProperty]
        public string? Name { get; set; }
        [DynamoDBProperty] 
        public string? Description { get; set; }
        [DynamoDBProperty]
        public decimal Price { get; set; }
        [DynamoDBProperty]
        public string? ImageUrl { get; set; }
        [DynamoDBProperty] 
        public int CategoryId { get; set; }
        [DynamoDBProperty] 
        public DateTime? CreatedAt { get; set; }
        [DynamoDBProperty] 
        public DateTime? UpdatedAt { get; set; }
       
    }
}



