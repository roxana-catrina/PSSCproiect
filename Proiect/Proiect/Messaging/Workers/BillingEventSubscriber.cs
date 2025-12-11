using Azure.Messaging.ServiceBus;
using Proiect.Data.Services;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using System.Text.Json;

namespace Proiect.Messaging.Workers;

/// <summary>
/// Background service that subscribes to package-events and initiates billing
/// </summary>
public class BillingEventSubscriber : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BillingEventSubscriber> _logger;

    public BillingEventSubscriber(
        IServiceProvider serviceProvider,
        ServiceBusClient serviceBusClient,
        IConfiguration configuration,
        ILogger<BillingEventSubscriber> logger)
    {
        _serviceProvider = serviceProvider;
        _serviceBusClient = serviceBusClient;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topicName = _configuration["ServiceBus:Topics:PackageEvents"];
        var subscriptionName = "billing-subscription";
        
        var processor = _serviceBusClient.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });

        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        await processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("BillingEventSubscriber started listening to topic {TopicName}, subscription {SubscriptionName}", 
            topicName, subscriptionName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            _logger.LogInformation("Billing received event: {MessageBody}", body);

            var eventType = args.Message.Subject;

            if (eventType == "PackageDeliveredSucceededEvent")
            {
                var succeededEvent = JsonSerializer.Deserialize<PackageDeliveredEvent.PackageDeliveredSucceededEvent>(body);
                if (succeededEvent != null)
                {
                    // Create invoice command from package delivered event
                    var invoiceCommand = new GenerateInvoiceCommand(
                        succeededEvent.OrderNumber.Value,
                        succeededEvent.RecipientName,
                        "0" // TotalAmount will be retrieved from order in workflow
                    );

                    using var scope = _serviceProvider.CreateScope();
                    var orchestrationService = scope.ServiceProvider.GetRequiredService<IWorkflowOrchestrationService>();
                    var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

                    // Process billing
                    var result = await orchestrationService.GenerateInvoiceAsync(invoiceCommand);

                    // Publish invoice event to invoice-events topic
                    var invoiceTopicName = _configuration["ServiceBus:Topics:InvoiceEvents"];
                    await messagePublisher.PublishEventAsync(invoiceTopicName!, result, args.Message.CorrelationId);

                    _logger.LogInformation("Billing processed for order {OrderNumber}", succeededEvent.OrderNumber.Value);
                }
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing billing event: {MessageId}", args.Message.MessageId);
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error in billing event processing: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }
}
