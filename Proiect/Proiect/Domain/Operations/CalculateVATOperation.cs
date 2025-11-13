namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Operation to calculate VAT breakdown for invoices
/// Single responsibility: VAT calculation and breakdown
/// </summary>
public class CalculateVATOperation : InvoiceOperation<(decimal SubTotal, decimal VATAmount, decimal Total)>
{
    private const decimal DefaultVatRate = 0.19m; // 19% VAT
    private readonly decimal _vatRate;

    /// <summary>
    /// Constructor for dependency injection with custom VAT rate
    /// </summary>
    /// <param name="vatRate">Optional custom VAT rate (defaults to 19%)</param>
    public CalculateVATOperation(decimal? vatRate = null)
    {
        _vatRate = vatRate ?? DefaultVatRate;
    }

    /// <summary>
    /// Calculates VAT breakdown for an UnvalidatedInvoice
    /// </summary>
    protected override (decimal SubTotal, decimal VATAmount, decimal Total) OnUnvalidated(UnvalidatedInvoice invoice)
    {
        ValidateInvoice(invoice);
        var vatAmount = CalculateVAT(invoice.TotalAmount, _vatRate);
        var total = invoice.TotalAmount + vatAmount;
        return (invoice.TotalAmount, vatAmount, total);
    }

    /// <summary>
    /// Calculates VAT breakdown for an UnpaidInvoice
    /// </summary>
    protected override (decimal SubTotal, decimal VATAmount, decimal Total) OnUnpaid(UnpaidInvoice invoice)
    {
        ValidateInvoice(invoice);
        return CalculateBreakdown(invoice.SubTotal, invoice.VatAmount, invoice.TotalAmount);
    }

    /// <summary>
    /// Calculates VAT breakdown for a PaidInvoice
    /// </summary>
    protected override (decimal SubTotal, decimal VATAmount, decimal Total) OnPaid(PaidInvoice invoice)
    {
        ValidateInvoice(invoice);
        return CalculateBreakdown(invoice.SubTotal, invoice.VatAmount, invoice.TotalAmount);
    }

    /// <summary>
    /// Calculates VAT breakdown for an InvalidInvoice
    /// </summary>
    protected override (decimal SubTotal, decimal VATAmount, decimal Total) OnInvalid(InvalidInvoice invoice)
    {
        ValidateInvoice(invoice);
        return CalculateBreakdown(invoice.SubTotal, invoice.VatAmount, 0);
    }

    /// <summary>
    /// Private helper method to calculate breakdown from existing values
    /// </summary>
    private static (decimal SubTotal, decimal VATAmount, decimal Total) CalculateBreakdown(decimal subTotal, decimal vatAmount, decimal total)
    {
        return (subTotal, vatAmount, total);
    }

    /// <summary>
    /// Private helper method to validate invoice is not null
    /// </summary>
    private static void ValidateInvoice(IInvoice invoice)
    {
        if (invoice == null)
            throw new ArgumentNullException(nameof(invoice));
    }

    /// <summary>
    /// Static helper to calculate VAT amount from a subtotal
    /// </summary>
    public static decimal CalculateVATAmount(decimal subTotal, decimal? vatRate = null)
    {
        var rate = vatRate ?? DefaultVatRate;
        return CalculateVAT(subTotal, rate);
    }

    /// <summary>
    /// Static helper to get VAT breakdown from a subtotal
    /// </summary>
    public static (decimal SubTotal, decimal VATAmount, decimal Total) GetVATBreakdown(decimal subTotal, decimal? vatRate = null)
    {
        var rate = vatRate ?? DefaultVatRate;
        var vatAmount = CalculateVAT(subTotal, rate);
        var total = subTotal + vatAmount;
        
        return (subTotal, vatAmount, total);
    }

    /// <summary>
    /// Private helper method for VAT calculation
    /// </summary>
    private static decimal CalculateVAT(decimal amount, decimal rate)
    {
        return amount * rate;
    }
}
