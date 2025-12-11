using Proiect.Domain.Models.ValueObjects;

namespace Proiect.Domain.Models.Entities;

public static class Order
{
    public interface IOrder { }
    
    public record UnvalidatedOrder(
        string CustomerName,
        string CustomerEmail,
        string DeliveryStreet,
        string DeliveryCity,
        string DeliveryPostalCode,
        string DeliveryCountry,
        IReadOnlyList<UnvalidatedOrderItem> Items) : IOrder;
    
    public record UnvalidatedOrderItem(string ProductName, string Quantity, string UnitPrice);
    
    public record ValidatedOrder : IOrder
    {
        internal ValidatedOrder(
            string customerName,
            string customerEmail,
            DeliveryAddress deliveryAddress,
            IReadOnlyCollection<ValidatedOrderItem> items)
        {
            CustomerName = customerName;
            CustomerEmail = customerEmail;
            DeliveryAddress = deliveryAddress;
            Items = items;
        }
        
        public string CustomerName { get; }
        public string CustomerEmail { get; }
        public DeliveryAddress DeliveryAddress { get; }
        public IReadOnlyCollection<ValidatedOrderItem> Items { get; }
    }
    
    public record ValidatedOrderItem(string ProductName, int Quantity, Price UnitPrice);
    
    public record ConfirmedOrder : IOrder
    {
        internal ConfirmedOrder(
            OrderNumber orderNumber,
            string customerName,
            string customerEmail,
            DeliveryAddress deliveryAddress,
            IReadOnlyCollection<ValidatedOrderItem> items,
            Price totalAmount,
            DateTime confirmedAt)
        {
            OrderNumber = orderNumber;
            CustomerName = customerName;
            CustomerEmail = customerEmail;
            DeliveryAddress = deliveryAddress;
            Items = items;
            TotalAmount = totalAmount;
            ConfirmedAt = confirmedAt;
        }
        
        public OrderNumber OrderNumber { get; }
        public string CustomerName { get; }
        public string CustomerEmail { get; }
        public DeliveryAddress DeliveryAddress { get; }
        public IReadOnlyCollection<ValidatedOrderItem> Items { get; }
        public Price TotalAmount { get; }
        public DateTime ConfirmedAt { get; }
    }
    
    public record PaidOrder : IOrder
    {
        internal PaidOrder(
            OrderNumber orderNumber,
            string customerName,
            DeliveryAddress deliveryAddress,
            IReadOnlyCollection<ValidatedOrderItem> items,
            Price totalAmount,
            DateTime paidAt)
        {
            OrderNumber = orderNumber;
            CustomerName = customerName;
            DeliveryAddress = deliveryAddress;
            Items = items;
            TotalAmount = totalAmount;
            PaidAt = paidAt;
        }
        
        public OrderNumber OrderNumber { get; }
        public string CustomerName { get; }
        public DeliveryAddress DeliveryAddress { get; }
        public IReadOnlyCollection<ValidatedOrderItem> Items { get; }
        public Price TotalAmount { get; }
        public DateTime PaidAt { get; }
    }
    
    public record InvalidOrder : IOrder
    {
        internal InvalidOrder(IEnumerable<string> reasons)
        {
            Reasons = reasons;
        }
        
        public IEnumerable<string> Reasons { get; }
    }
}
