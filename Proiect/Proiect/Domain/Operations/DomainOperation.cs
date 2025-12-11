namespace Proiect.Domain.Operations;

/// <summary>
/// Base class for all domain operations that transform entities through their lifecycle
/// </summary>
public abstract class DomainOperation<TEntity, TState, TResult>
    where TEntity : notnull
    where TState : class
{
    public abstract TResult Transform(TEntity entity, TState? state);
}

