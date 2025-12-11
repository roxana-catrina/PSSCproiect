using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Package;

namespace Proiect.Domain.Models.Events;

public static class PackageDeliveredEvent
{
    public interface IPackageDeliveredEvent { }
    
    public record PackageDeliveredSucceededEvent : IPackageDeliveredEvent
    {
        internal PackageDeliveredSucceededEvent(
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
    
    public record PackageDeliveredFailedEvent : IPackageDeliveredEvent
    {
        internal PackageDeliveredFailedEvent(IEnumerable<string> reasons)
        {
            Reasons = reasons;
        }
        
        public IEnumerable<string> Reasons { get; }
    }
    
    public static IPackageDeliveredEvent ToEvent(this IPackage package) => package switch
    {
        DeliveredPackage delivered => new PackageDeliveredSucceededEvent(
            delivered.OrderNumber,
            delivered.TrackingNumber,
            delivered.DeliveredAt,
            delivered.RecipientName),
        InvalidPackage invalid => new PackageDeliveredFailedEvent(invalid.Reasons),
        UnvalidatedPackage _ => new PackageDeliveredFailedEvent(new[] { "Package was not validated" }),
        ValidatedPackage _ => new PackageDeliveredFailedEvent(new[] { "Package was not prepared" }),
        PreparedPackage _ => new PackageDeliveredFailedEvent(new[] { "Package was not shipped" }),
        ShippedPackage _ => new PackageDeliveredFailedEvent(new[] { "Package was not delivered" }),
        _ => new PackageDeliveredFailedEvent(new[] { $"Unexpected state: {package.GetType().Name}" })
    };
}
