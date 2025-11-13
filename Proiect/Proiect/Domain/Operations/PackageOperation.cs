namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Base class for package operations that transform package states
/// </summary>
public abstract class PackageOperation
{
    /// <summary>
    /// Transforms a package through its lifecycle states
    /// </summary>
    /// <param name="package">The package to transform</param>
    /// <returns>The transformed package</returns>
    public IPackage Transform(IPackage package)
    {
        return package switch
        {
            UnvalidatedPackage p => OnUnvalidated(p),
            PreparedPackage p => OnPrepared(p),
            InTransitPackage p => OnInTransit(p),
            DeliveredPackage p => OnDelivered(p),
            ReturnedPackage p => OnReturned(p),
            InvalidPackage p => OnInvalid(p),
            _ => throw new InvalidOperationException($"Unexpected package state: {package.GetType().Name}")
        };
    }

    /// <summary>
    /// Handles a package in the Unvalidated state. Default is identity (returns same object).
    /// </summary>
    protected virtual IPackage OnUnvalidated(UnvalidatedPackage package) => package;

    /// <summary>
    /// Handles a package in the Prepared state. Default is identity (returns same object).
    /// </summary>
    protected virtual IPackage OnPrepared(PreparedPackage package) => package;

    /// <summary>
    /// Handles a package in the InTransit state. Default is identity (returns same object).
    /// </summary>
    protected virtual IPackage OnInTransit(InTransitPackage package) => package;

    /// <summary>
    /// Handles a package in the Delivered state. Default is identity (returns same object).
    /// </summary>
    protected virtual IPackage OnDelivered(DeliveredPackage package) => package;

    /// <summary>
    /// Handles a package in the Returned state. Default is identity (returns same object).
    /// </summary>
    protected virtual IPackage OnReturned(ReturnedPackage package) => package;

    /// <summary>
    /// Handles a package in the Invalid state. Default is identity (returns same object).
    /// </summary>
    protected virtual IPackage OnInvalid(InvalidPackage package) => package;
}

/// <summary>
/// Base class for package operations that transform package states and return a specific result
/// </summary>
/// <typeparam name="TResult">The result type of the operation</typeparam>
public abstract class PackageOperation<TResult>
{
    /// <summary>
    /// Transforms a package through its lifecycle states and returns a result
    /// </summary>
    /// <param name="package">The package to transform</param>
    /// <returns>The result of the transformation</returns>
    public TResult Transform(IPackage package)
    {
        return package switch
        {
            UnvalidatedPackage p => OnUnvalidated(p),
            PreparedPackage p => OnPrepared(p),
            InTransitPackage p => OnInTransit(p),
            DeliveredPackage p => OnDelivered(p),
            ReturnedPackage p => OnReturned(p),
            InvalidPackage p => OnInvalid(p),
            _ => throw new InvalidOperationException($"Unexpected package state: {package.GetType().Name}")
        };
    }

    /// <summary>
    /// Handles a package in the Unvalidated state and returns a result
    /// </summary>
    protected abstract TResult OnUnvalidated(UnvalidatedPackage package);

    /// <summary>
    /// Handles a package in the Prepared state and returns a result
    /// </summary>
    protected abstract TResult OnPrepared(PreparedPackage package);

    /// <summary>
    /// Handles a package in the InTransit state and returns a result
    /// </summary>
    protected abstract TResult OnInTransit(InTransitPackage package);

    /// <summary>
    /// Handles a package in the Delivered state and returns a result
    /// </summary>
    protected abstract TResult OnDelivered(DeliveredPackage package);

    /// <summary>
    /// Handles a package in the Returned state and returns a result
    /// </summary>
    protected abstract TResult OnReturned(ReturnedPackage package);

    /// <summary>
    /// Handles a package in the Invalid state and returns a result
    /// </summary>
    protected abstract TResult OnInvalid(InvalidPackage package);
}
