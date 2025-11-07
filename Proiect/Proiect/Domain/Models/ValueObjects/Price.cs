namespace Proiect.Domain.Models.ValueObjects;

public record Price
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public Price(decimal amount, string currency = "RON")
    {
        if (amount < 0)
            throw new ArgumentException("Price cannot be negative", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    public Price Add(Price other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add prices with different currencies");
        
        return new Price(Amount + other.Amount, Currency);
    }

    public Price Multiply(int quantity)
    {
        return new Price(Amount * quantity, Currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}

