namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Operation to mark a package as in transit
/// Single responsibility: Package pickup/transit marking
/// </summary>
public class PickupPackageOperation : PackageOperation
{
    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    public PickupPackageOperation()
    {
    }

    /// <summary>
    /// Marks PreparedPackage as InTransit after pickup
    /// </summary>
    protected override IPackage OnPrepared(PreparedPackage package)
    {
        ValidatePackage(package);
        
        return new InTransitPackage(package);
    }

    /// <summary>
    /// Private helper method to validate package
    /// </summary>
    private static void ValidatePackage(PreparedPackage package)
    {
        if (package == null)
            throw new ArgumentNullException(nameof(package));
    }
}
