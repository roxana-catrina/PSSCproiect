namespace Proiect.Domain.Operations;

public class CalculateVATOperation
{
    private const decimal DefaultVATRate = 0.19m; // 19% VAT

    public static decimal Execute(decimal amount, decimal? vatRate = null)
    {
        var rate = vatRate ?? DefaultVATRate;
        return amount * rate;
    }

    public static (decimal SubTotal, decimal VATAmount, decimal Total) CalculateBreakdown(decimal subTotal, decimal? vatRate = null)
    {
        var rate = vatRate ?? DefaultVATRate;
        var vatAmount = subTotal * rate;
        var total = subTotal + vatAmount;
        
        return (subTotal, vatAmount, total);
    }
}

