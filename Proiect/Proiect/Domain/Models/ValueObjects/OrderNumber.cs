namespace Proiect.Domain.Models.ValueObjects;

/// <summary>
/// Represents an order number for order identification
/// </summary>
public record OrderNumber
{
    /// <summary>
    /// Gets the order number value
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Private constructor for OrderNumber
    /// </summary>
    /// <param name="value">The order number string value</param>
    /// <exception cref="InvalidOrderNumberException">Thrown when the order number value is invalid</exception>
    private OrderNumber(string value)
    {
        if (!IsValid(value))
            throw new InvalidOrderNumberException("Order number cannot be empty or whitespace");
        
        Value = value;
    }

    /// <summary>
    /// Validates the order number value
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool IsValid(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Tries to parse a string into an OrderNumber
    /// </summary>
    /// <param name="value">The string value to parse</param>
    /// <param name="orderNumber">The resulting OrderNumber if successful, null otherwise</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryParse(string value, out OrderNumber? orderNumber)
    {
        orderNumber = null;
        
        if (!IsValid(value))
            return false;
        
        orderNumber = new OrderNumber(value);
        return true;
    }

    /// <summary>
    /// Generates a new OrderNumber with a unique identifier
    /// </summary>
    /// <returns>A new OrderNumber instance</returns>
    public static OrderNumber Generate()
    {
        return new OrderNumber($"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");
    }

    /// <summary>
    /// Returns the string representation of the order number
    /// </summary>
    /// <returns>The order number value as string</returns>
    public override string ToString() => Value;
}

/// <summary>
/// Exception thrown when an order number value is invalid
/// </summary>
public class InvalidOrderNumberException : Exception
{
    public InvalidOrderNumberException(string message) : base(message) { }
}
