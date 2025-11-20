using Microsoft.EntityFrameworkCore;
using ProductCatalogue.Web.Data;
using ProductCatalogue.Web.Services;
using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core (SQL connection)
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register ServiceBusClient using AMQP WebSockets
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("ServiceBusConnection");
    return new ServiceBusClient(connectionString, new ServiceBusClientOptions
    {
        TransportType = ServiceBusTransportType.AmqpWebSockets
    });
});

// Register OrderServiceBusSender with the configured ServiceBusClient
builder.Services.AddSingleton(sp =>
{
    var serviceBusClient = sp.GetRequiredService<ServiceBusClient>();
    var queueName = builder.Configuration["ServiceBusQueueName"];
    return new OrderServiceBusSender(serviceBusClient, queueName);
});

builder.Services.AddRazorPages();
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
