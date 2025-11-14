using System.Text.RegularExpressions;
using Proiect.Domain.Exceptions;

namespace Proiect.Domain.Models.ValueObjects;

/// <summary>
/// Value object representing a unique order number
/// Format: ORD-YYYYMMDD-XXXX (e.g., ORD-20251114-0001)
/// </summary>
public record OrderNumber
{
    private static readonly Regex ValidPattern = new(@"^ORD-\d{8}-\d{4}$", RegexOptions.Compiled);
    public string Value { get; }
    
    private OrderNumber(string value)
    {
        if (IsValid(value))
            Value = value;
        else
            throw new InvalidOrderNumberException($"Invalid order number format: {value}. Expected format: ORD-YYYYMMDD-XXXX");
    }
    
    private static bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
            
        if (!ValidPattern.IsMatch(value))
            return false;
            
        // Extract date part and validate
        var datePart = value.Substring(4, 8);
        return DateTime.TryParseExact(datePart, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out _);
    }
    
    public static bool TryParse(string input, out OrderNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input))
            return false;
            
        if (!IsValid(input))
            return false;
            
        try
        {
            result = new OrderNumber(input);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public override string ToString() => Value;
}

/// <summary>
/// Exception thrown when an order number value is invalid
/// </summary>
public class InvalidOrderNumberException : Exception
{
    public InvalidOrderNumberException(string message) : base(message) { }
}
