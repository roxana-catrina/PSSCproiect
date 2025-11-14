using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Order;

namespace Proiect.Domain.Models.Events;

public static class OrderPlacedEvent
{
    public interface IOrderPlacedEvent { }
    
    public record OrderPlacedSucceededEvent : IOrderPlacedEvent
    {
        internal OrderPlacedSucceededEvent(
            OrderNumber orderNumber,
            string customerName,
            string customerEmail,
            Price totalAmount,
            DateTime placedAt)
        {
            OrderNumber = orderNumber;
            CustomerName = customerName;
            CustomerEmail = customerEmail;
            TotalAmount = totalAmount;
            PlacedAt = placedAt;
        }
        
        public OrderNumber OrderNumber { get; }
        public string CustomerName { get; }
        public string CustomerEmail { get; }
        public Price TotalAmount { get; }
        public DateTime PlacedAt { get; }
    }
    
    public record OrderPlacedFailedEvent : IOrderPlacedEvent
    {
        internal OrderPlacedFailedEvent(IEnumerable<string> reasons)
        {
            Reasons = reasons;
        }
        
        public IEnumerable<string> Reasons { get; }
    }
    
    public static IOrderPlacedEvent ToEvent(this IOrder order) => order switch
    {
        ConfirmedOrder confirmed => new OrderPlacedSucceededEvent(
            confirmed.OrderNumber,
            confirmed.CustomerName,
            confirmed.CustomerEmail,
            confirmed.TotalAmount,
            confirmed.ConfirmedAt),
        InvalidOrder invalid => new OrderPlacedFailedEvent(invalid.Reasons),
        UnvalidatedOrder _ => new OrderPlacedFailedEvent(new[] { "Order was not validated" }),
        ValidatedOrder _ => new OrderPlacedFailedEvent(new[] { "Order was not confirmed" }),
        PaidOrder _ => new OrderPlacedFailedEvent(new[] { "Unexpected state: Order already paid" }),
        _ => new OrderPlacedFailedEvent(new[] { $"Unexpected state: {order.GetType().Name}" })
    };
}
