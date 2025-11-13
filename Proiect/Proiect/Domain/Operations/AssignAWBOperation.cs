namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.ValueObjects;
using Proiect.Domain.Models.Entities;

/// <summary>
/// Operation to assign or retrieve AWB (Air Waybill) from a package
/// Single responsibility: AWB assignment and retrieval
/// </summary>
public class AssignAWBOperation : PackageOperation<AWB>
{
    /// <summary>
    /// Constructor for dependency injection (currently no dependencies needed)
    /// </summary>
    public AssignAWBOperation()
    {
    }

    /// <summary>
    /// Retrieves AWB from a PreparedPackage
    /// </summary>
    protected override AWB OnPrepared(PreparedPackage package)
    {
        ValidatePackage(package);
        return package.AWB;
    }

    /// <summary>
    /// Retrieves AWB from an InTransitPackage
    /// </summary>
    protected override AWB OnInTransit(InTransitPackage package)
    {
        ValidatePackage(package);
        return package.AWB;
    }

    /// <summary>
    /// Retrieves AWB from a DeliveredPackage
    /// </summary>
    protected override AWB OnDelivered(DeliveredPackage package)
    {
        ValidatePackage(package);
        return package.AWB;
    }

    /// <summary>
    /// Retrieves AWB from a ReturnedPackage
    /// </summary>
    protected override AWB OnReturned(ReturnedPackage package)
    {
        ValidatePackage(package);
        return package.AWB;
    }

    /// <summary>
    /// Private helper method to validate package is not null
    /// </summary>
    private static void ValidatePackage(IPackage package)
    {
        if (package == null)
            throw new ArgumentNullException(nameof(package));
    }

    /// <summary>
    /// Generates a new AWB for package creation
    /// </summary>
    public static AWB GenerateNewAWB()
    {
        return AWB.Generate();
    }
}
