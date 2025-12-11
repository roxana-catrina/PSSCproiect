using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Package;

namespace Proiect.Domain.Models.Events;

public static class PackageShippedEvent
{
    public interface IPackageShippedEvent { }
    
    public record PackageShippedSucceededEvent : IPackageShippedEvent
    {
        internal PackageShippedSucceededEvent(
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
    
    public record PackageShippedFailedEvent : IPackageShippedEvent
    {
        internal PackageShippedFailedEvent(IEnumerable<string> reasons)
        {
            Reasons = reasons;
        }
        
        public IEnumerable<string> Reasons { get; }
    }
    
    public static IPackageShippedEvent ToEvent(this IPackage package) => package switch
    {
        ShippedPackage shipped => new PackageShippedSucceededEvent(
            shipped.OrderNumber,
            shipped.DeliveryAddress,
            shipped.TrackingNumber,
            shipped.ShippedAt),
        InvalidPackage invalid => new PackageShippedFailedEvent(invalid.Reasons),
        UnvalidatedPackage _ => new PackageShippedFailedEvent(new[] { "Package was not validated" }),
        ValidatedPackage _ => new PackageShippedFailedEvent(new[] { "Package was not prepared" }),
        PreparedPackage _ => new PackageShippedFailedEvent(new[] { "Package was not shipped" }),
        DeliveredPackage _ => new PackageShippedFailedEvent(new[] { "Package state is beyond shipped (already delivered)" }),
        _ => new PackageShippedFailedEvent(new[] { $"Unexpected state: {package.GetType().Name}" })
    };
}

