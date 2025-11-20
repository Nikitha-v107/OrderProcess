using System;
using System.ComponentModel.DataAnnotations; // Add this using directive

namespace ProductCatalogue.Function
{
    public class OrderCreatedMessage
    {
        public int OrderId { get; set; }
        [Required] // Mark as required
        public string CustomerEmail { get; set; } = string.Empty; // Initialize to avoid nullability warning
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
