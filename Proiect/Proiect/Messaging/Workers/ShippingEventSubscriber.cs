using Azure.Messaging.ServiceBus;
using Proiect.Data.Services;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using System.Text.Json;

namespace Proiect.Messaging.Workers;

/// <summary>
/// Background service that subscribes to order-events and initiates shipping
/// </summary>
public class ShippingEventSubscriber : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ShippingEventSubscriber> _logger;

    public ShippingEventSubscriber(
        IServiceProvider serviceProvider,
        ServiceBusClient serviceBusClient,
        IConfiguration configuration,
        ILogger<ShippingEventSubscriber> logger)
    {
        _serviceProvider = serviceProvider;
        _serviceBusClient = serviceBusClient;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topicName = _configuration["ServiceBus:Topics:OrderEvents"];
        var subscriptionName = "shipping-subscription";
        
        var processor = _serviceBusClient.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });

        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        await processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("ShippingEventSubscriber started listening to topic {TopicName}, subscription {SubscriptionName}", 
            topicName, subscriptionName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            _logger.LogInformation("Shipping received event: {MessageBody}", body);

            var eventType = args.Message.Subject;

            if (eventType == "OrderPlacedSucceededEvent")
            {
                var succeededEvent = JsonSerializer.Deserialize<OrderPlacedEvent.OrderPlacedSucceededEvent>(body);
                if (succeededEvent != null)
                {
                    // Create shipping command from order event
                    var shippingCommand = new PickupPackageCommand(
                        succeededEvent.OrderNumber.Value,
                        succeededEvent.DeliveryStreet,
                        succeededEvent.DeliveryCity,
                        succeededEvent.DeliveryPostalCode,
                        succeededEvent.DeliveryCountry
                    );

                    using var scope = _serviceProvider.CreateScope();
                    var orchestrationService = scope.ServiceProvider.GetRequiredService<IWorkflowOrchestrationService>();
                    var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

                    // Process shipping
                    var result = await orchestrationService.ProcessShippingAsync(shippingCommand);

                    // Publish package event to package-events topic
                    var packageTopicName = _configuration["ServiceBus:Topics:PackageEvents"];
                    await messagePublisher.PublishEventAsync(packageTopicName!, result, args.Message.CorrelationId);

                    _logger.LogInformation("Shipping processed for order {OrderNumber}", succeededEvent.OrderNumber.Value);
                }
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing shipping event: {MessageId}", args.Message.MessageId);
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error in shipping event processing: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }
}
