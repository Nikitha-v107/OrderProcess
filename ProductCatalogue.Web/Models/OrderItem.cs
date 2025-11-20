namespace ProductCatalogue.Web.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public required Order Order { get; set; }
        public int ProductId { get; set; }
        public required Product Product { get; set; } // Assuming you have a Product model
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
