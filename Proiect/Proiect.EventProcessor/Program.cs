using Proiect.Messaging.Events;
using Proiect.Messaging.Events.ServiceBus;
using Proiect.EventProcessor.Workers;
using Proiect.Data;
using Proiect.Data.Services;
using Proiect.Domain.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Proiect.EventProcessor;

internal class Program
{
    private static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Load local configuration with secrets (not committed to Git)
                config.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Azure Service Bus
                services.AddAzureClients(builder =>
                {
                    builder.AddServiceBusClient(hostContext.Configuration.GetConnectionString("ServiceBus"));
                });

                // Database
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")));

                // State Services
                services.AddTransient<IOrderStateService, OrderStateService>();
                services.AddTransient<IInvoiceStateService, InvoiceStateService>();
                services.AddTransient<IPackageStateService, PackageStateService>();

                // Workflows
                services.AddTransient<OrderProcessingWorkflow>();
                services.AddTransient<BillingWorkflow>();
                services.AddTransient<ShippingWorkflow>();

                // Event Infrastructure
                services.AddSingleton<IEventSender, ServiceBusTopicEventSender>();
                services.AddSingleton<IEventListener, ServiceBusTopicEventListener>();
                
                // Event Handlers - Listen to events (DTOs) published by the API
                services.AddSingleton<IEventHandler, OrderPlacedEventHandler>();
                services.AddSingleton<IEventHandler, InvoiceGeneratedEventHandler>();
                services.AddSingleton<IEventHandler, PackageDeliveredEventHandler>();

                // Hosted Service
                services.AddHostedService<Worker>();
            });
}
