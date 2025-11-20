using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProductCatalogue.Web.Pages
{
    public class OrderConfirmationModel : PageModel
    {
        public int OrderId { get; set; }

        public void OnGet(int orderId)
        {
            OrderId = orderId;
        }
    }
}
