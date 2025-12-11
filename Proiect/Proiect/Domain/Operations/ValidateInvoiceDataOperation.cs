using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Invoice;

namespace Proiect.Domain.Operations;

internal sealed class ValidateInvoiceDataOperation : InvoiceOperation
{
    protected override IInvoice OnUnvalidated(UnvalidatedInvoice invoice)
    {
        var errors = new List<string>();
        
        // Validate order number
        if (!OrderNumber.TryParse(invoice.OrderNumber, out var orderNumber))
        {
            errors.Add("Invalid order number format");
        }
        
        // Validate customer name
        if (string.IsNullOrWhiteSpace(invoice.CustomerName))
        {
            errors.Add("Customer name is required");
        }
        
        // Validate total amount
        if (!Price.TryParse(invoice.TotalAmount, out var totalAmount))
        {
            errors.Add("Invalid total amount");
        }
        
        if (errors.Any())
            return new InvalidInvoice(errors);
        
        return new ValidatedInvoice(orderNumber!, invoice.CustomerName, totalAmount!);
    }
}

