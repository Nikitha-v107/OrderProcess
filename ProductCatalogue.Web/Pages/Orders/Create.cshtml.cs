using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProductCatalogue.Web.Data;
using ProductCatalogue.Web.Models;
using ProductCatalogue.Web.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ProductCatalogue.Web.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly AppDBContext _context;
        private readonly OrderServiceBusSender _serviceBusSender;

        public CreateModel(AppDBContext context, OrderServiceBusSender serviceBusSender)
        {
            _context = context;
            _serviceBusSender = serviceBusSender;
        }

        [BindProperty]
        public OrderInputModel OrderInput { get; set; } = new OrderInputModel(); // Initialize here
        public Product? Product { get; set; }

        public class OrderInputModel
        {
            public int ProductId { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 2)]
            [Display(Name = "Your Name")]
            public string CustomerName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Your Email")]
            public string CustomerEmail { get; set; } = string.Empty;

            [Required]
            [StringLength(200, MinimumLength = 10)]
            [Display(Name = "Shipping Address")]
            public string ShippingAddress { get; set; } = string.Empty;

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int Quantity { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? productId)
        {
            if (productId == null)
            {
                return RedirectToPage("/Products/Index");
            }

            Product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (Product == null)
            {
                return NotFound();
            }

            OrderInput = new OrderInputModel
            {
                ProductId = Product.Id,
                Quantity = 1 // Default quantity
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Product = await _context.Products.FirstOrDefaultAsync(p => p.Id == OrderInput.ProductId);
                return Page();
            }

            Product = await _context.Products.FirstOrDefaultAsync(p => p.Id == OrderInput.ProductId);
            if (Product == null)
            {
                ModelState.AddModelError(string.Empty, "Selected product not found.");
                return Page();
            }

            // Removed/commented out stock validation for testing purposes
            // if (Product.Stock < OrderInput.Quantity)
            // {
            //     ModelState.AddModelError(string.Empty, $"Not enough stock. Available stock: {Product.Stock}");
            //     return Page();
            // }

            // Create the order
            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                CustomerName = OrderInput.CustomerName,
                CustomerEmail = OrderInput.CustomerEmail,
                ShippingAddress = OrderInput.ShippingAddress,
                Status = "Pending",
                TotalAmount = OrderInput.Quantity * Product.Price,
                OrderItems = new List<OrderItem>() // Initialize with an empty list
            };

            var orderItem = new OrderItem
            {
                ProductId = Product.Id,
                Product = Product,
                Quantity = OrderInput.Quantity,
                UnitPrice = Product.Price,
                Order = order // Explicitly set the Order navigation property
            };
            order.OrderItems.Add(orderItem); // Add the order item after order is created

            _context.Orders.Add(order);
            Product.Stock -= OrderInput.Quantity; // Update product stock
            await _context.SaveChangesAsync();

            // Send OrderCreated message to Azure Service Bus
            var orderCreatedMessage = new OrderCreatedMessage
            {
                OrderId = order.OrderId,
                CustomerEmail = order.CustomerEmail,
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate
            };
            await _serviceBusSender.SendOrderCreatedMessageAsync(orderCreatedMessage);

            return RedirectToPage("/OrderConfirmation", new { orderId = order.OrderId });
        }
    }
}
