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
        // Calculate VAT and round to 2 decimal places
        var vatAmountValue = Math.Round(invoice.TotalAmount.Value * _vatRate, 2, MidpointRounding.AwayFromZero);
        var vatAmount = new Price(vatAmountValue);
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