namespace Proiect.Domain.Exceptions;

public class InvalidDeliveryAddressException : Exception
{
    public InvalidDeliveryAddressException(string message) : base(message)
    {
    }
}


