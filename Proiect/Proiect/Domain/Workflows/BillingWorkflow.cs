namespace Proiect.Domain.Workflows;

using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Entities;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Operations;

/// <summary>
/// Workflow for billing operations
/// Orchestrates invoice generation and payment processing
/// </summary>
public class BillingWorkflow
{
    /// <summary>
    /// Executes the billing workflow
    /// </summary>
    /// <param name="command">The generate invoice command</param>
    /// <param name="validateInvoiceOp">Operation to validate invoice</param>
    /// <param name="notifyInvoiceOp">Operation to notify customer about invoice</param>
    /// <param name="processPaymentOp">Operation to process payment</param>
    /// <param name="paymentMethod">Payment method to use</param>
    /// <returns>Invoice generated and payment recorded events</returns>
    public async Task<(InvoiceGenerated InvoiceEvent, PaymentRecorded PaymentEvent)> Execute(
        GenerateInvoiceCommand command,
        ValidateInvoiceOperation validateInvoiceOp,
        NotifyInvoiceOperation notifyInvoiceOp,
        ProcessPaymentOperation processPaymentOp,
        string paymentMethod = "CreditCard")
    {
        // Create Unvalidated entity at the beginning
        IInvoice result = new UnvalidatedInvoice(
            command.OrderId,
            command.CustomerName,
            command.CustomerEmail,
            command.TotalAmount
        );

        // Chain operations: result = op.Transform(result)
        result = validateInvoiceOp.Transform(result);

        // Notify customer after invoice generation
        await notifyInvoiceOp.Transform(result);

        // Process payment
        result = processPaymentOp.Transform(result);

        // Convert to events using ToEvent()
        if (result is PaidInvoice paidInvoice)
        {
            var invoiceEvent = paidInvoice.ToEvent();
            var paymentEvent = paidInvoice.ToPaymentEvent(paymentMethod);
            
            return (invoiceEvent, paymentEvent);
        }

        // This should never happen if workflow is correct
        throw new InvalidOperationException("Workflow did not produce a PaidInvoice");
    }
}
