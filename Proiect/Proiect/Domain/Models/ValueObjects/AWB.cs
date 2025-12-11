using System.Text.RegularExpressions;
using Proiect.Domain.Exceptions;

namespace Proiect.Domain.Models.ValueObjects;

/// <summary>
/// Value object representing AWB (Air Waybill) tracking number
/// Format: 2 letters followed by 10 digits (e.g., RO1234567890)
/// </summary>
public record AWB
{
    private static readonly Regex ValidPattern = new(@"^[A-Z]{2}\d{10}$", RegexOptions.Compiled);
    public string Value { get; }
    
    private AWB(string value)
    {
        if (IsValid(value))
            Value = value.ToUpperInvariant();
        else
            throw new InvalidAWBException($"Invalid AWB format: {value}. Expected format: 2 letters followed by 10 digits (e.g., RO1234567890)");
    }
    
    private static bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
            
        return ValidPattern.IsMatch(value.ToUpperInvariant());
    }
    
    public static bool TryParse(string input, out AWB? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input))
            return false;
            
        if (!IsValid(input))
            return false;
            
        try
        {
            result = new AWB(input);
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
/// Exception thrown when an AWB value is invalid
/// </summary>
public class InvalidAWBException : Exception
{
    public InvalidAWBException(string message) : base(message) { }
}
