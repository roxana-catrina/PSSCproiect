using static Proiect.Domain.Models.Entities.Package;

namespace Proiect.Domain.Operations;

public abstract class PackageOperation : DomainOperation<IPackage, object, IPackage>
{
    internal IPackage Transform(IPackage package) => Transform(package, null);
    
    public override IPackage Transform(IPackage package, object? state) => package switch
    {
        UnvalidatedPackage unvalidated => OnUnvalidated(unvalidated),
        ValidatedPackage validated => OnValidated(validated),
        PreparedPackage prepared => OnPrepared(prepared),
        ShippedPackage shipped => OnShipped(shipped),
        DeliveredPackage delivered => OnDelivered(delivered),
        InvalidPackage invalid => OnInvalid(invalid),
        _ => package
    };
    
    protected virtual IPackage OnUnvalidated(UnvalidatedPackage package) => package;
    protected virtual IPackage OnValidated(ValidatedPackage package) => package;
    protected virtual IPackage OnPrepared(PreparedPackage package) => package;
    protected virtual IPackage OnShipped(ShippedPackage package) => package;
    protected virtual IPackage OnDelivered(DeliveredPackage package) => package;
    protected virtual IPackage OnInvalid(InvalidPackage package) => package;
}
