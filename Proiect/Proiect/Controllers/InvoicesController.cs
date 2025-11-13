/*amespace Proiect.Controllers;

using Microsoft.AspNetCore.Mvc;
using Proiect.DTOs;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Workflows;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceRequest request)
    {
        try
        {
            var command = new GenerateInvoiceCommand(
                request.OrderId,
                request.TotalAmount,
                request.CustomerName,
                request.CustomerEmail
            );

            var workflow = new BillingWorkflow();
            var (invoiceEvent, paymentEvent) = await workflow.ExecuteAsync(command, request.PaymentMethod);

            return Ok(new
            {
                success = true,
                invoice = new
                {
                    invoiceId = invoiceEvent.InvoiceId,
                    invoiceNumber = invoiceEvent.InvoiceNumber,
                    orderId = invoiceEvent.OrderId,
                    totalAmount = invoiceEvent.TotalAmount,
                    vatAmount = invoiceEvent.VATAmount,
                    generatedAt = invoiceEvent.GeneratedAt
                },
                payment = new
                {
                    paymentId = paymentEvent.PaymentId,
                    amount = paymentEvent.Amount,
                    paymentMethod = paymentEvent.PaymentMethod,
                    recordedAt = paymentEvent.RecordedAt
                },
                message = "Invoice generated and payment processed successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while generating the invoice", error = ex.Message });
        }
    }
}

*/