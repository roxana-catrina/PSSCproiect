namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Operation to validate and create an unpaid invoice from unvalidated invoice
/// Single responsibility: Invoice validation and creation
/// </summary>
public class ValidateInvoiceOperation : InvoiceOperation
{
    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    public ValidateInvoiceOperation()
    {
    }

    /// <summary>
    /// Validates and converts UnvalidatedInvoice to UnpaidInvoice
    /// </summary>
    protected override IInvoice OnUnvalidated(UnvalidatedInvoice invoice)
    {
        ValidateInvoice(invoice);
        
        // Create invoice items (in real scenario, this would come from invoice data)
        var items = invoice.Items;
        
        return new UnpaidInvoice(
            invoice.OrderId,
            invoice.CustomerName,
            invoice.CustomerEmail,
            items
        );
    }

    /// <summary>
    /// Private helper method to validate invoice
    /// </summary>
    private static void ValidateInvoice(UnvalidatedInvoice invoice)
    {
        if (invoice == null)
            throw new ArgumentNullException(nameof(invoice));
        
        if (string.IsNullOrWhiteSpace(invoice.OrderId))
            throw new ArgumentException("OrderId cannot be empty", nameof(invoice));
        
        if (string.IsNullOrWhiteSpace(invoice.CustomerName))
            throw new ArgumentException("CustomerName cannot be empty", nameof(invoice));
        
        if (string.IsNullOrWhiteSpace(invoice.CustomerEmail))
            throw new ArgumentException("CustomerEmail cannot be empty", nameof(invoice));
        
        if (invoice.TotalAmount <= 0)
            throw new ArgumentException("TotalAmount must be greater than zero", nameof(invoice));
    }
}

