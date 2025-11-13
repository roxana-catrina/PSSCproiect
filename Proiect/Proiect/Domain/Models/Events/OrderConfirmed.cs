namespace Proiect.Domain.Models.Events
{
  public record OrderConfirmed(
      string OrderId,
      DateTime ConfirmedAt
  );
}

