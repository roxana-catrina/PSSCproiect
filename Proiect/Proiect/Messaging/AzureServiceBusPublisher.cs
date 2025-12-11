using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Proiect.Messaging;

/// <summary>
/// Azure Service Bus implementation for message publishing
/// </summary>
public class AzureServiceBusPublisher : IMessagePublisher
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<AzureServiceBusPublisher> _logger;

    public AzureServiceBusPublisher(ServiceBusClient serviceBusClient, ILogger<AzureServiceBusPublisher> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
    }

    public async Task SendCommandAsync<T>(string queueName, T command, string? correlationId = null) where T : class
    {
        await using var sender = _serviceBusClient.CreateSender(queueName);
        
        var messageBody = JsonSerializer.Serialize(command);
        var message = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            Subject = typeof(T).Name
        };

        if (!string.IsNullOrEmpty(correlationId))
        {
            message.CorrelationId = correlationId;
        }

        await sender.SendMessageAsync(message);
        _logger.LogInformation("Command {CommandType} sent to queue {QueueName} with CorrelationId {CorrelationId}", 
            typeof(T).Name, queueName, correlationId);
    }

    public async Task PublishEventAsync<T>(string topicName, T @event, string? correlationId = null) where T : class
    {
        await using var sender = _serviceBusClient.CreateSender(topicName);
        
        var messageBody = JsonSerializer.Serialize(@event);
        var message = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            Subject = typeof(T).Name
        };

        if (!string.IsNullOrEmpty(correlationId))
        {
            message.CorrelationId = correlationId;
        }

        await sender.SendMessageAsync(message);
        _logger.LogInformation("Event {EventType} published to topic {TopicName} with CorrelationId {CorrelationId}", 
            typeof(T).Name, topicName, correlationId);
    }
}

