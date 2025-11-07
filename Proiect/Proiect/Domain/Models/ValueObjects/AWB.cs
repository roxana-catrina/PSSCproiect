namespace Proiect.Domain.Models.ValueObjects;

public record AWB
{
    public string Value { get; init; }

    public AWB(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("AWB cannot be empty", nameof(value));
        
        Value = value;
    }

    public static AWB Generate()
    {
        return new AWB($"AWB{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(100000, 999999)}");
    }

    public override string ToString() => Value;
}

