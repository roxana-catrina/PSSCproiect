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
    public async Task<IActionResult> GenerateInvoice(
        [FromBody] GenerateInvoiceRequest request,
        [FromServices] Proiect.Data.Services.IInvoiceStateService invoiceStateService)
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
                generateInvoiceNumber: () => $"INV-{request.OrderNumber}-{DateTime.UtcNow:yyyyMMdd}"
            );

            // Handle workflow result and save to database
            IActionResult response = workflowResult switch
            {
                InvoiceGeneratedSucceededEvent @event => await SaveAndPublishEvent(@event, invoiceStateService),
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

    private async Task<IActionResult> SaveAndPublishEvent(InvoiceGeneratedSucceededEvent successEvent, Proiect.Data.Services.IInvoiceStateService invoiceStateService)
    {
        try
        {
            // Save invoice to database
            var generatedInvoice = new Proiect.Domain.Models.Entities.Invoice.GeneratedInvoice(
                successEvent.InvoiceNumber,
                successEvent.OrderNumber,
                successEvent.CustomerName,
                successEvent.TotalAmount,
                successEvent.VatAmount,
                successEvent.TotalWithVat,
                successEvent.GeneratedAt
            );

            await invoiceStateService.SaveInvoiceAsync(generatedInvoice);
            _logger.LogInformation($"Invoice {successEvent.InvoiceNumber} saved to database");

            // Publish event to Service Bus
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

            _logger.LogInformation($"Invoice {successEvent.InvoiceNumber} event published");

            return Ok(new
            {
                success = true,
                invoiceNumber = successEvent.InvoiceNumber,
                orderNumber = successEvent.OrderNumber.Value,
                message = "Invoice generated successfully",
                totalWithVat = successEvent.TotalWithVat.Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving invoice to database");
            return StatusCode(500, new
            {
                success = false,
                message = "Invoice processed but failed to save",
                error = ex.Message
            });
        }
    }
}

public record GenerateInvoiceRequest(
    string OrderNumber,
    string CustomerName,
    string TotalAmount
);
