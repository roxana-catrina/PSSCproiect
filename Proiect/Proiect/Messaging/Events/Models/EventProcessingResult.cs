namespace Proiect.Messaging.Events.Models;

/// <summary>
/// Result of event processing
/// </summary>
public enum EventProcessingResult
{
    Completed,
    Failed,
    Retry
}

