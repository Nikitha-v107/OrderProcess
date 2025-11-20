using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProductCatalogue.Web.Data;   // Adjust namespace if different
using ProductCatalogue.Web.Models; // Where Product model exists

namespace ProductCatalogue.Web.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly AppDBContext _context;

        public IndexModel(AppDBContext context)
        {
            _context = context;
        }

        public IList<Product> Products { get; set; } = new List<Product>();

        public async Task OnGetAsync()
        {
            Products = await _context.Products.AsNoTracking().ToListAsync();
        }
    }
}
