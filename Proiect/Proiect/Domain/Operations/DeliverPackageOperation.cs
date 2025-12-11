using static Proiect.Domain.Models.Entities.Package;

namespace Proiect.Domain.Operations;

internal sealed class DeliverPackageOperation : PackageOperation
{
    private readonly Func<string, string> _getRecipientName;
    
    internal DeliverPackageOperation(Func<string, string> getRecipientName)
    {
        _getRecipientName = getRecipientName;
    }
    
    protected override IPackage OnShipped(ShippedPackage package)
    {
        // Get recipient name from delivery address or order
        var recipientName = _getRecipientName(package.OrderNumber.Value);
        
        if (string.IsNullOrWhiteSpace(recipientName))
        {
            return new InvalidPackage(new[] { "Failed to identify recipient for delivery" });
        }
        
        return new DeliveredPackage(
            package.OrderNumber,
            package.TrackingNumber,
            DateTime.Now,
            recipientName);
    }
}

