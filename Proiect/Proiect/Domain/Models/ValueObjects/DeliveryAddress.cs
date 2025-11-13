namespace Proiect.Domain.Models.ValueObjects;

/// <summary>
/// Represents a delivery address for order shipping
/// </summary>
public record DeliveryAddress
{
    /// <summary>
    /// Gets the street address
    /// </summary>
    public string Street { get; }
    
    /// <summary>
    /// Gets the city name
    /// </summary>
    public string City { get; }
    
    /// <summary>
    /// Gets the postal code
    /// </summary>
    public string PostalCode { get; }
    
    /// <summary>
    /// Gets the country name
    /// </summary>
    public string Country { get; }

    /// <summary>
    /// Private constructor for DeliveryAddress
    /// </summary>
    /// <param name="street">The street address</param>
    /// <param name="city">The city name</param>
    /// <param name="postalCode">The postal code</param>
    /// <param name="country">The country name</param>
    /// <exception cref="InvalidDeliveryAddressException">Thrown when any address component is invalid</exception>
    private DeliveryAddress(string street, string city, string postalCode, string country)
    {
        if (!IsValid(street, city, postalCode, country))
            throw new InvalidDeliveryAddressException("All address fields must be non-empty");

        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    /// <summary>
    /// Validates the delivery address components
    /// </summary>
    /// <param name="street">The street address</param>
    /// <param name="city">The city name</param>
    /// <param name="postalCode">The postal code</param>
    /// <param name="country">The country name</param>
    /// <returns>True if all components are valid, false otherwise</returns>
    private static bool IsValid(string street, string city, string postalCode, string country)
    {
        return !string.IsNullOrWhiteSpace(street) &&
               !string.IsNullOrWhiteSpace(city) &&
               !string.IsNullOrWhiteSpace(postalCode) &&
               !string.IsNullOrWhiteSpace(country);
    }

    /// <summary>
    /// Tries to parse address components into a DeliveryAddress
    /// </summary>
    /// <param name="street">The street address</param>
    /// <param name="city">The city name</param>
    /// <param name="postalCode">The postal code</param>
    /// <param name="country">The country name</param>
    /// <param name="address">The resulting DeliveryAddress if successful, null otherwise</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryParse(string street, string city, string postalCode, string country, out DeliveryAddress? address)
    {
        address = null;
        
        if (!IsValid(street, city, postalCode, country))
            return false;
        
        address = new DeliveryAddress(street, city, postalCode, country);
        return true;
    }

    /// <summary>
    /// Returns the string representation of the delivery address
    /// </summary>
    /// <returns>The formatted address as string</returns>
    public override string ToString() => $"{Street}, {City}, {PostalCode}, {Country}";
}

/// <summary>
/// Exception thrown when a delivery address is invalid
/// </summary>
public class InvalidDeliveryAddressException : Exception
{
    public InvalidDeliveryAddressException(string message) : base(message) { }
}
