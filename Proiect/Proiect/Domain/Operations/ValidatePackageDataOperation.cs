using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Package;

namespace Proiect.Domain.Operations;

internal sealed class ValidatePackageDataOperation : PackageOperation
{
    protected override IPackage OnUnvalidated(UnvalidatedPackage package)
    {
        var errors = new List<string>();
        
        // Validate order number
        if (!OrderNumber.TryParse(package.OrderNumber, out var orderNumber))
        {
            errors.Add("Invalid order number format");
        }
        
        // Validate delivery address
        if (!DeliveryAddress.TryParse(
            package.DeliveryStreet,
            package.DeliveryCity,
            package.DeliveryPostalCode,
            package.DeliveryCountry,
            out var deliveryAddress))
        {
            errors.Add("Invalid delivery address");
        }
        
        if (errors.Any())
            return new InvalidPackage(errors);
        
        return new ValidatedPackage(orderNumber!, deliveryAddress!);
    }
}

