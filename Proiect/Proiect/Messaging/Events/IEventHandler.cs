using System.Threading.Tasks;
using Azure.Messaging;
using CloudNative.CloudEvents;
using Proiect.Messaging.Events.Models;

namespace Proiect.Messaging.Events;

/// <summary>
/// Interface for handling specific event types
/// </summary>
public interface IEventHandler
{
    string[] EventTypes { get; }

    Task<EventProcessingResult> HandleAsync(CloudNative.CloudEvents.CloudEvent cloudEvent);
}

