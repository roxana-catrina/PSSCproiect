using Microsoft.AspNetCore.Mvc;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Workflows;
using Proiect.Messaging.Events;
using static Proiect.Domain.Models.Events.InvoiceGeneratedEvent;

namespace Proiect.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly ILogger<InvoicesController> _logger;
    private readonly BillingWorkflow _billingWorkflow;
    private readonly IEventSender _eventSender;

    public InvoicesController(
        ILogger<InvoicesController> logger,
        BillingWorkflow billingWorkflow,
        IEventSender eventSender)
    {
        _logger = logger;
        _billingWorkflow = billingWorkflow;
        _eventSender = eventSender;
    }

    [HttpPost]
    public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceRequest request)
    {
        try
        {
            var command = new GenerateInvoiceCommand(
                request.OrderNumber,
                request.CustomerName,
                request.TotalAmount
            );

            // Execute workflow with dependencies
            IInvoiceGeneratedEvent workflowResult = _billingWorkflow.Execute(
                command,
                vatRate: 0.19m, // 19% VAT
                generateInvoiceNumber: () => $"INV-{Guid.NewGuid().ToString()[..8].ToUpper()}"
            );

            // Handle workflow result
            IActionResult response = workflowResult switch
            {
                InvoiceGeneratedSucceededEvent @event => await PublishEvent(@event),
                InvoiceGeneratedFailedEvent @event => BadRequest(@event.Reasons),
                _ => throw new NotImplementedException()
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while generating the invoice",
                error = ex.Message
            });
        }
    }

    private async Task<IActionResult> PublishEvent(InvoiceGeneratedSucceededEvent successEvent)
    {
        await _eventSender.SendAsync("invoice-events", new InvoiceGeneratedDto
        {
            InvoiceNumber = successEvent.InvoiceNumber,
            OrderNumber = successEvent.OrderNumber.Value,
            CustomerName = successEvent.CustomerName,
            TotalAmount = successEvent.TotalAmount.Value,
            VatAmount = successEvent.VatAmount.Value,
            TotalWithVat = successEvent.TotalWithVat.Value,
            GeneratedAt = successEvent.GeneratedAt
        });

        return Ok();
    }
}

public record GenerateInvoiceRequest(
    string OrderNumber,
    string CustomerName,
    string TotalAmount
);
