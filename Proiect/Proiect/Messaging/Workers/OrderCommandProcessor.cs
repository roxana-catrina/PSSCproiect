using Azure.Messaging.ServiceBus;
using Proiect.Data.Services;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using System.Text.Json;

namespace Proiect.Messaging.Workers;

/// <summary>
/// Background service that processes order commands from the orders-commands queue
/// </summary>
public class OrderCommandProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderCommandProcessor> _logger;

    public OrderCommandProcessor(
        IServiceProvider serviceProvider,
        ServiceBusClient serviceBusClient,
        IConfiguration configuration,
        ILogger<OrderCommandProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _serviceBusClient = serviceBusClient;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = _configuration["ServiceBus:Queues:OrderCommands"];
        var processor = _serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });

        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        await processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("OrderCommandProcessor started processing messages from queue {QueueName}", queueName);

        // Keep the processor running until cancellation is requested
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            _logger.LogInformation("Received message: {MessageBody}", body);

            var command = JsonSerializer.Deserialize<PlaceOrderCommand>(body);
            if (command == null)
            {
                _logger.LogWarning("Failed to deserialize PlaceOrderCommand from message");
                await args.DeadLetterMessageAsync(args.Message, "DeserializationError", "Could not deserialize message body");
                return;
            }

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var orchestrationService = scope.ServiceProvider.GetRequiredService<IWorkflowOrchestrationService>();
            var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

            // Process the order using the existing orchestration service
            var result = await orchestrationService.ProcessOrderAsync(command);

            // Publish event to order-events topic based on result
            var topicName = _configuration["ServiceBus:Topics:OrderEvents"];
            await messagePublisher.PublishEventAsync(topicName!, result, args.Message.CorrelationId);

            // Complete the message
            await args.CompleteMessageAsync(args.Message);
            _logger.LogInformation("Successfully processed order command with CorrelationId {CorrelationId}", args.Message.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {MessageId}", args.Message.MessageId);
            // Don't complete the message - it will be retried
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error in message processing: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }
}
