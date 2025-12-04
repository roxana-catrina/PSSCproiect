using Microsoft.AspNetCore.Mvc;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using Proiect.Data.Services;

namespace Proiect.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IWorkflowOrchestrationService _orchestrationService;

    public InvoicesController(IWorkflowOrchestrationService orchestrationService)
    {
        _orchestrationService = orchestrationService;
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

            // Folosește serviciul de orchestrare care încarcă/salvează starea din/în baza de date
            var invoiceEvent = await _orchestrationService.GenerateInvoiceAsync(command);

            return invoiceEvent switch
            {
                InvoiceGeneratedEvent.InvoiceGeneratedSucceededEvent success => Ok(new
                {
                    success = true,
                    invoice = new
                    {
                        invoiceNumber = success.InvoiceNumber,
                        orderNumber = success.OrderNumber.Value,
                        customerName = success.CustomerName,
                        totalAmount = success.TotalAmount.Value,
                        vatAmount = success.VatAmount.Value,
                        totalWithVat = success.TotalWithVat.Value,
                        generatedAt = success.GeneratedAt
                    },
                    message = "Invoice generated successfully and saved to database"
                }),

                InvoiceGeneratedEvent.InvoiceGeneratedFailedEvent failure => BadRequest(new
                {
                    success = false,
                    errors = failure.Reasons,
                    message = "Invoice generation failed"
                }),

                _ => StatusCode(500, new { success = false, message = "Unknown error occurred" })
            };
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while generating the invoice",
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
