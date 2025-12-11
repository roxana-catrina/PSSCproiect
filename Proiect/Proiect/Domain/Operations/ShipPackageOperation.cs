using static Proiect.Domain.Models.Entities.Package;

namespace Proiect.Domain.Operations;

internal sealed class ShipPackageOperation : PackageOperation
{
    private readonly Func<string, bool> _notifyCourier;
    
    internal ShipPackageOperation(Func<string, bool> notifyCourier)
    {
        _notifyCourier = notifyCourier;
    }
    
    protected override IPackage OnPrepared(PreparedPackage package)
    {
        // Notify courier service
        if (!_notifyCourier(package.TrackingNumber.Value))
        {
            return new InvalidPackage(new[] { "Failed to notify courier service" });
        }
        
        return new ShippedPackage(
            package.OrderNumber,
            package.DeliveryAddress,
            package.TrackingNumber,
            DateTime.Now);
    }
}
