using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ProductCatalogue.Function;

var host = new HostBuilder()
    // Register the Azure Functions isolated worker
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, configBuilder) =>
    {
        configBuilder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices(services =>
    {
        // Register custom services for dependency injection
        services.AddSingleton<OrderProcessorService>();
    })
    .Build();

host.Run();
