using System.ComponentModel.DataAnnotations;

namespace ProductCatalogue.Web.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public required string Name { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}
