namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Operation to mark a package as delivered
/// Single responsibility: Package delivery marking
/// </summary>
public class DeliverPackageOperation : PackageOperation
{
    private readonly string _receivedBy;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="receivedBy">Name of person who received the package</param>
    public DeliverPackageOperation(string? receivedBy = null)
    {
        _receivedBy = receivedBy ?? "Customer";
    }

    /// <summary>
    /// Marks InTransitPackage as Delivered
    /// </summary>
    protected override IPackage OnInTransit(InTransitPackage package)
    {
        ValidatePackage(package);
        
        return new DeliveredPackage(package, _receivedBy);
    }

    /// <summary>
    /// Private helper method to validate package
    /// </summary>
    private static void ValidatePackage(InTransitPackage package)
    {
        if (package == null)
            throw new ArgumentNullException(nameof(package));
    }
}
