namespace Proiect.Domain.Models.ValueObjects;

/// <summary>
/// Represents a price with amount and currency
/// </summary>
public record Price
{
    /// <summary>
    /// Gets the price amount
    /// </summary>
    public decimal Amount { get; }
    
    /// <summary>
    /// Gets the currency code
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Private constructor for Price
    /// </summary>
    /// <param name="amount">The price amount</param>
    /// <param name="currency">The currency code (default: RON)</param>
    /// <exception cref="InvalidPriceException">Thrown when the price is invalid</exception>
    private Price(decimal amount, string currency = "RON")
    {
        if (!IsValid(amount, currency))
            throw new InvalidPriceException("Price amount cannot be negative and currency cannot be empty");

        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Validates the price components
    /// </summary>
    /// <param name="amount">The price amount</param>
    /// <param name="currency">The currency code</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool IsValid(decimal amount, string currency)
    {
        return amount >= 0 && !string.IsNullOrWhiteSpace(currency);
    }

    /// <summary>
    /// Tries to parse price components into a Price
    /// </summary>
    /// <param name="amount">The price amount</param>
    /// <param name="currency">The currency code</param>
    /// <param name="price">The resulting Price if successful, null otherwise</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryParse(decimal amount, string currency, out Price? price)
    {
        price = null;
        
        if (!IsValid(amount, currency))
            return false;
        
        price = new Price(amount, currency);
        return true;
    }

    /// <summary>
    /// Adds two prices with the same currency
    /// </summary>
    /// <param name="other">The other price to add</param>
    /// <returns>A new Price with the sum of amounts</returns>
    /// <exception cref="InvalidOperationException">Thrown when currencies don't match</exception>
    public Price Add(Price other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add prices with different currencies");
        
        return new Price(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Multiplies the price by a quantity
    /// </summary>
    /// <param name="quantity">The quantity multiplier</param>
    /// <returns>A new Price with the multiplied amount</returns>
    public Price Multiply(int quantity)
    {
        return new Price(Amount * quantity, Currency);
    }

    /// <summary>
    /// Returns the string representation of the price
    /// </summary>
    /// <returns>The formatted price as string</returns>
    public override string ToString() => $"{Amount:F2} {Currency}";
}

/// <summary>
/// Exception thrown when a price is invalid
/// </summary>
public class InvalidPriceException : Exception
{
    public InvalidPriceException(string message) : base(message) { }
}
