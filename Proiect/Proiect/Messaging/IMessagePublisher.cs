namespace Proiect.Messaging;

/// <summary>
/// Interface for publishing messages to queues and topics
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Sends a command message to a queue
    /// </summary>
    Task SendCommandAsync<T>(string queueName, T command, string? correlationId = null) where T : class;
    
    /// <summary>
    /// Publishes an event message to a topic
    /// </summary>
    Task PublishEventAsync<T>(string topicName, T @event, string? correlationId = null) where T : class;
}

