using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Functions.Worker.Extensions.Storage.Blobs;
using Microsoft.Data.SqlClient;

namespace ProductCatalogue.Function
{
    public class OrderProcessorOutput
    {
        // Correct isolated-binding BlobOutput
        [BlobOutput("order-receipts/{OrderId}.txt", Connection = "AzureWebJobsStorage")]
        [Required]
        public string ReceiptContent { get; set; } = string.Empty;

        public int OrderId { get; set; }   // REQUIRED for blob path token
    }

    public class ProcessOrderFunction
    {
        private readonly ILogger<ProcessOrderFunction> _logger;
        private readonly OrderProcessorService _orderProcessorService;

        public ProcessOrderFunction(
            ILogger<ProcessOrderFunction> logger,
            OrderProcessorService orderProcessorService)
        {
            _logger = logger;
            _orderProcessorService = orderProcessorService;
        }

        [Function("ProcessOrderFunction")]
        public async Task<OrderProcessorOutput> RunAsync(
            [ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string queueMessage)
        {
            _logger.LogInformation($"Message received: {queueMessage}");

            OrderCreatedMessage? orderMessage =
                JsonSerializer.Deserialize<OrderCreatedMessage>(queueMessage);

            if (orderMessage == null)
            {
                _logger.LogError("Failed to deserialize order message.");
                return new OrderProcessorOutput
                {
                    ReceiptContent = "ERROR: Unable to deserialize order message.",
                    OrderId = 0
                };
            }

            try
            {
                await _orderProcessorService.UpdateOrderAndStock(
                    orderMessage.OrderId,
                    productId: 1,
                    quantity: 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process order.");
                return new OrderProcessorOutput
                {
                    ReceiptContent = $"ERROR processing order {orderMessage.OrderId}: {ex.Message}",
                    OrderId = orderMessage.OrderId
                };
            }

            string receipt = BuildReceipt(orderMessage);

            return new OrderProcessorOutput
            {
                ReceiptContent = receipt,
                OrderId = orderMessage.OrderId // CRITICAL REQUIRED FOR BLOB PATH
            };
        }

        private static string BuildReceipt(OrderCreatedMessage message)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Order Receipt - #{message.OrderId}");
            sb.AppendLine("------------------------------");
            sb.AppendLine($"Customer Email: {message.CustomerEmail}");
            sb.AppendLine($"Order Date: {message.OrderDate}");
            sb.AppendLine($"Total Amount: {message.TotalAmount:C}");
            sb.AppendLine("------------------------------");
            sb.AppendLine("Thank you for shopping with us!");
            return sb.ToString();
        }
    }
}
