using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Invoice;

namespace Proiect.Domain.Operations;

internal sealed class CalculateVATOperation : InvoiceOperation
{
    private readonly decimal _vatRate;
    private readonly Func<string> _generateInvoiceNumber;
    
    internal CalculateVATOperation(decimal vatRate, Func<string> generateInvoiceNumber)
    {
        _vatRate = vatRate;
        _generateInvoiceNumber = generateInvoiceNumber;
    }
    
    protected override IInvoice OnValidated(ValidatedInvoice invoice)
    {
        // Calculate VAT
        var vatAmount = new Price(invoice.TotalAmount.Value * _vatRate);
        var totalWithVat = invoice.TotalAmount + vatAmount;
        
        // Generate invoice number
        var invoiceNumber = _generateInvoiceNumber();
        
        return new GeneratedInvoice(
            invoiceNumber,
            invoice.OrderNumber,
            invoice.CustomerName,
            invoice.TotalAmount,
            vatAmount,
            totalWithVat,
            DateTime.Now);
    }
}
