using Proiect.Domain.Exceptions;

namespace Proiect.Domain.Models.ValueObjects;

/// <summary>
/// Value object representing a monetary price
/// Must be positive and have maximum 2 decimal places
/// </summary>
public record Price
{
    public decimal Value { get; }
    
    internal Price(decimal value)
    {
        if (IsValid(value))
            Value = value;
        else
            throw new InvalidPriceException($"Invalid price: {value}. Price must be positive and have maximum 2 decimal places.");
    }
    
    private static bool IsValid(decimal value)
    {
        if (value <= 0)
            return false;
            
        // Check for maximum 2 decimal places
        var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
        return decimalPlaces <= 2;
    }
    
    public static bool TryParse(string input, out Price? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input))
            return false;
            
        if (!decimal.TryParse(input, out var decimalValue))
            return false;
            
        if (!IsValid(decimalValue))
            return false;
            
        try
        {
            result = new Price(decimalValue);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public static Price Zero => new Price(0.01m);
    
    public static Price operator +(Price a, Price b) => new Price(a.Value + b.Value);
    public static Price operator *(Price price, int quantity) => new Price(price.Value * quantity);
    
    public override string ToString() => Value.ToString("F2");
}

/// <summary>
/// Exception thrown when a price is invalid
/// </summary>
public class InvalidPriceException : Exception
{
    public InvalidPriceException(string message) : base(message) { }
}
