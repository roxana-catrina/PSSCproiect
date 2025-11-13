namespace Proiect.Domain.Models.ValueObjects;

/// <summary>
/// Represents an Air Waybill number for package tracking
/// </summary>
public record AWB
{
    /// <summary>
    /// Gets the AWB value
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Private constructor for AWB
    /// </summary>
    /// <param name="value">The AWB string value</param>
    /// <exception cref="InvalidAWBException">Thrown when the AWB value is invalid</exception>
    private AWB(string value)
    {
        if (!IsValid(value))
            throw new InvalidAWBException("AWB cannot be empty or whitespace");
        
        Value = value;
    }

    /// <summary>
    /// Validates the AWB value
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool IsValid(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Tries to parse a string into an AWB
    /// </summary>
    /// <param name="value">The string value to parse</param>
    /// <param name="awb">The resulting AWB if successful, null otherwise</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryParse(string value, out AWB? awb)
    {
        awb = null;
        
        if (!IsValid(value))
            return false;
        
        awb = new AWB(value);
        return true;
    }

    /// <summary>
    /// Generates a new AWB with a unique identifier
    /// </summary>
    /// <returns>A new AWB instance</returns>
    public static AWB Generate()
    {
        return new AWB($"AWB{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(100000, 999999)}");
    }

    /// <summary>
    /// Returns the string representation of the AWB
    /// </summary>
    /// <returns>The AWB value as string</returns>
    public override string ToString() => Value;
}

/// <summary>
/// Exception thrown when an AWB value is invalid
/// </summary>
public class InvalidAWBException : Exception
{
    public InvalidAWBException(string message) : base(message) { }
}
