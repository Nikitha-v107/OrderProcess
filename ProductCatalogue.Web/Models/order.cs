using System;
using System.Collections.Generic;

namespace ProductCatalogue.Web.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public required string CustomerName { get; set; }
        public required string CustomerEmail { get; set; }
        public required string ShippingAddress { get; set; }
        public required string Status { get; set; } // e.g., "Pending", "Processing", "Shipped", "Delivered", "Cancelled"
        public decimal TotalAmount { get; set; }

        public required ICollection<OrderItem> OrderItems { get; set; }
    }
}
