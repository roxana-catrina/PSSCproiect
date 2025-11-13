namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Operation to validate and prepare a package from unvalidated package
/// Single responsibility: Package validation and preparation
/// </summary>
public class PreparePackageOperation : PackageOperation
{
    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    public PreparePackageOperation()
    {
    }

    /// <summary>
    /// Validates and converts UnvalidatedPackage to PreparedPackage
    /// </summary>
    protected override IPackage OnUnvalidated(UnvalidatedPackage package)
    {
        ValidatePackage(package);
        
        return new PreparedPackage(
            package.OrderId,
            package.DeliveryAddress,
            package.Items
        );
    }

    /// <summary>
    /// Private helper method to validate package
    /// </summary>
    private static void ValidatePackage(UnvalidatedPackage package)
    {
        if (package == null)
            throw new ArgumentNullException(nameof(package));
        
        if (string.IsNullOrWhiteSpace(package.OrderId))
            throw new ArgumentException("OrderId cannot be empty", nameof(package));
        
        if (package.DeliveryAddress == null)
            throw new ArgumentException("DeliveryAddress cannot be null", nameof(package));
        
        if (package.Items == null || !package.Items.Any())
            throw new ArgumentException("Items cannot be empty", nameof(package));
    }
}

