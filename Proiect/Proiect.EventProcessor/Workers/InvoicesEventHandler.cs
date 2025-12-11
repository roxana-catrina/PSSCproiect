using Proiect.Domain.Models.Commands;
using Proiect.Domain.Workflows;
using System.Text.Json;
using static Proiect.Domain.Models.Events.InvoiceGeneratedEvent;
using Proiect.Domain.Models.Events;
using Proiect.Messaging.Events;
using Proiect.Messaging.Events.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Event handler for processing invoice commands
/// Executes BillingWorkflow and publishes events
/// </summary>
internal class InvoicesEventHandler : AbstractEventHandler<GenerateInvoiceCommand>
{
    private readonly IServiceProvider _serviceProvider;

    public InvoicesEventHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override string[] EventTypes => new string[] { typeof(GenerateInvoiceCommand).Name };

    protected override async Task<EventProcessingResult> OnHandleAsync(GenerateInvoiceCommand command)
    {
        using var scope = _serviceProvider.CreateScope();
        var workflow = scope.ServiceProvider.GetRequiredService<BillingWorkflow>();
        var eventSender = scope.ServiceProvider.GetRequiredService<IEventSender>();

        // Execute workflow
        var workflowResult = workflow.Execute(
            command,
            vatRate: 0.19m,
            generateInvoiceNumber: () => $"INV-{Guid.NewGuid().ToString()[..8].ToUpper()}"
        );

        // Handle result and publish event
        if (workflowResult is InvoiceGeneratedSucceededEvent successEvent)
        {
            await eventSender.SendAsync("invoice-events", new InvoiceGeneratedDto
            {
                InvoiceNumber = successEvent.InvoiceNumber,
                OrderNumber = successEvent.OrderNumber.Value,
                CustomerName = successEvent.CustomerName,
                TotalAmount = successEvent.TotalAmount.Value,
                VatAmount = successEvent.VatAmount.Value,
                TotalWithVat = successEvent.TotalWithVat.Value,
                GeneratedAt = successEvent.GeneratedAt
            });

            Console.WriteLine($"Invoice {successEvent.InvoiceNumber} generated successfully");
            return EventProcessingResult.Completed;
        }
        else if (workflowResult is InvoiceGeneratedFailedEvent failedEvent)
        {
            Console.WriteLine($"Invoice generation failed: {string.Join(", ", failedEvent.Reasons)}");
            return EventProcessingResult.Failed;
        }

        return EventProcessingResult.Completed;
    }
}
