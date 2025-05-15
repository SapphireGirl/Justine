using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Order(int Order_OrderId, string Order_CustomerName, int Order_BasketId)
        {
            OrderId = Order_OrderId;
            CustomerName = Order_CustomerName;
            BasketId = Order_BasketId;
            OrderDate = DateTime.UtcNow;
        }

    }
}
