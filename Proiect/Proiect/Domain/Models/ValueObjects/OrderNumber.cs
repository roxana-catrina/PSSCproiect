namespace Proiect.Domain.Models.ValueObjects;

public record OrderNumber
{
    public string Value { get; init; }

    public OrderNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Order number cannot be empty", nameof(value));
        
        Value = value;
    }

    public static OrderNumber Generate()
    {
        return new OrderNumber($"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");
    }

    public override string ToString() => Value;
}

