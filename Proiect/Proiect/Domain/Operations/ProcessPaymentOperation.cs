namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Operation to process payment and convert unpaid invoice to paid invoice
/// Single responsibility: Payment processing
/// </summary>
public class ProcessPaymentOperation : InvoiceOperation
{
    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    public ProcessPaymentOperation()
    {
    }

    /// <summary>
    /// Processes payment for UnpaidInvoice and converts to PaidInvoice
    /// </summary>
    protected override IInvoice OnUnpaid(UnpaidInvoice invoice)
    {
        ValidateInvoice(invoice);
        
        // In a real scenario, this would interact with a payment gateway
        // For now, we just create a PaidInvoice
        return new PaidInvoice(invoice);
    }

    /// <summary>
    /// Private helper method to validate invoice
    /// </summary>
    private static void ValidateInvoice(UnpaidInvoice invoice)
    {
        if (invoice == null)
            throw new ArgumentNullException(nameof(invoice));
    }
}
