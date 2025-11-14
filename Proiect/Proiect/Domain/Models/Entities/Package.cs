using Proiect.Domain.Models.ValueObjects;

namespace Proiect.Domain.Models.Entities;

public static class Package
{
    public interface IPackage { }
    
    public record UnvalidatedPackage(
        string OrderNumber,
        string DeliveryStreet,
        string DeliveryCity,
        string DeliveryPostalCode,
        string DeliveryCountry) : IPackage;
    
    public record ValidatedPackage : IPackage
    {
        internal ValidatedPackage(
            OrderNumber orderNumber,
            DeliveryAddress deliveryAddress)
        {
            OrderNumber = orderNumber;
            DeliveryAddress = deliveryAddress;
        }
        
        public OrderNumber OrderNumber { get; }
        public DeliveryAddress DeliveryAddress { get; }
    }
    
    public record PreparedPackage : IPackage
    {
        internal PreparedPackage(
            OrderNumber orderNumber,
            DeliveryAddress deliveryAddress,
            AWB trackingNumber,
            DateTime preparedAt)
        {
            OrderNumber = orderNumber;
            DeliveryAddress = deliveryAddress;
            TrackingNumber = trackingNumber;
            PreparedAt = preparedAt;
        }
        
        public OrderNumber OrderNumber { get; }
        public DeliveryAddress DeliveryAddress { get; }
        public AWB TrackingNumber { get; }
        public DateTime PreparedAt { get; }
    }
    
    public record ShippedPackage : IPackage
    {
        internal ShippedPackage(
            OrderNumber orderNumber,
            DeliveryAddress deliveryAddress,
            AWB trackingNumber,
            DateTime shippedAt)
        {
            OrderNumber = orderNumber;
            DeliveryAddress = deliveryAddress;
            TrackingNumber = trackingNumber;
            ShippedAt = shippedAt;
        }
        
        public OrderNumber OrderNumber { get; }
        public DeliveryAddress DeliveryAddress { get; }
        public AWB TrackingNumber { get; }
        public DateTime ShippedAt { get; }
    }
    
    public record DeliveredPackage : IPackage
    {
        internal DeliveredPackage(
            OrderNumber orderNumber,
            AWB trackingNumber,
            DateTime deliveredAt,
            string recipientName)
        {
            OrderNumber = orderNumber;
            TrackingNumber = trackingNumber;
            DeliveredAt = deliveredAt;
            RecipientName = recipientName;
        }
        
        public OrderNumber OrderNumber { get; }
        public AWB TrackingNumber { get; }
        public DateTime DeliveredAt { get; }
        public string RecipientName { get; }
    }
    
    public record InvalidPackage : IPackage
    {
        internal InvalidPackage(IEnumerable<string> reasons)
        {
            Reasons = reasons;
        }
        
        public IEnumerable<string> Reasons { get; }
    }
}
