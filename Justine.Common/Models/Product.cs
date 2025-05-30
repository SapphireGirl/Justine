﻿using Amazon.DynamoDBv2.DataModel;

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
        public string Name { get; set; }  // Required

        // This is unnecessary, because the Attribute is also named the same as the property:
        // If it wasn't you would use it like this[DynamoDBProperty("Authors")]
        // public List<string> BookAuthors { get; set; }
        // Leaving for now
        [DynamoDBProperty] 
        public string? Description { get; set; }
        [DynamoDBProperty]
        public decimal Price { get; set; } // Required
        [DynamoDBProperty]
        public string? ImageUrl { get; set; }
        [DynamoDBProperty] 
        public int Quantity { get; set; }
        [DynamoDBProperty] 
        public DateTime? CreatedAt { get; set; }
        [DynamoDBProperty] 
        public DateTime? UpdatedAt { get; set; }
    }
}



