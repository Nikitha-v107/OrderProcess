using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace ProductCatalogue.Web.Services
{
    public class OrderCreatedMessage
    {
        public int OrderId { get; set; }
        public required string CustomerEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        // Add other relevant order details
    }

    public class OrderServiceBusSender
    {
        private readonly ServiceBusSender _sender;

        public OrderServiceBusSender(ServiceBusClient client, string? queueName)
        {
            _sender = client.CreateSender(queueName ?? string.Empty);
        }

        public async Task SendOrderCreatedMessageAsync(OrderCreatedMessage message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(jsonMessage);
            await _sender.SendMessageAsync(serviceBusMessage);
        }
    }
}
