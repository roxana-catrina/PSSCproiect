using System;
using System.Text.Json;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Proiect.Messaging.Events.Models;

namespace Proiect.Messaging.Events;

/// <summary>
/// Abstract base class for event handlers that provides deserialization logic
/// </summary>
/// <typeparam name="TEvent">The type of event to handle</typeparam>
public abstract class AbstractEventHandler<TEvent> : IEventHandler where TEvent : notnull
{
    public abstract string[] EventTypes { get; }

    public Task<EventProcessingResult> HandleAsync(CloudEvent cloudEvent)
    {
        TEvent eventData = DeserializeEvent(cloudEvent);
        return OnHandleAsync(eventData);
    }

    protected abstract Task<EventProcessingResult> OnHandleAsync(TEvent eventData);

    private TEvent DeserializeEvent(CloudEvent cloudEvent)
    {
        if (cloudEvent.Data is not null)
        {
            var json = ((JsonElement)cloudEvent.Data).GetRawText();
            var input = JsonSerializer.Deserialize<TEvent>(json);
            if (input is not null)
            {
                return input;
            }
            throw new NullReferenceException($"Deserializing event generated null value. {json}");
        }
        throw new NullReferenceException("CloudEvent Data cannot be null");
    }
}
