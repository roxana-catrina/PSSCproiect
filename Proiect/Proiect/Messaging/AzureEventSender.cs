using Proiect.Messaging.Events;
using Proiect.Messaging.Events.ServiceBus;
using Azure.Messaging.ServiceBus;

namespace Proiect.Messaging;

/// <summary>
/// Implementation of IEventSender using Azure Service Bus
/// Wraps ServiceBusTopicEventSender for compatibility
/// </summary>
public class AzureEventSender : IEventSender
{
    private readonly ServiceBusTopicEventSender _sender;

    public AzureEventSender(ServiceBusClient serviceBusClient)
    {
        _sender = new ServiceBusTopicEventSender(serviceBusClient);
    }

    public async Task SendAsync<T>(string topicName, T @event)
    {
        await _sender.SendAsync(topicName, @event);
    }
}
