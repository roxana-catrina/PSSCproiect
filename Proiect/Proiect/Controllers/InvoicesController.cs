using Microsoft.AspNetCore.Mvc;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Workflows;
using Proiect.Domain.Models.Events;

namespace Proiect.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    [HttpPost]
    public IActionResult GenerateInvoice([FromBody] GenerateInvoiceRequest request)
    {
        try
        {
            var command = new GenerateInvoiceCommand(
                request.OrderNumber,
                request.CustomerName,
                request.TotalAmount
            );

            var workflow = new BillingWorkflow();
            
            // Mock dependencies
            decimal vatRate = 0.19m; // 19% VAT
            Func<string> generateInvoiceNumber = () => $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999):D4}";
            
            var invoiceEvent = workflow.Execute(command, vatRate, generateInvoiceNumber);

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
                    message = "Invoice generated successfully"
                }),
                InvoiceGeneratedEvent.InvoiceGeneratedFailedEvent failure => BadRequest(new
                {
                    success = false,
                    errors = failure.Reasons,
                    message = "Failed to generate invoice"
                }),
                _ => StatusCode(500, new { success = false, message = "Unexpected error" })
            };
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
        }
    }
}

public record GenerateInvoiceRequest(string OrderNumber, string CustomerName, string TotalAmount);
