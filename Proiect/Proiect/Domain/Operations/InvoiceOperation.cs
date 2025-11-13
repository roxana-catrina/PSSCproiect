namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Base class for invoice operations that transform invoice states
/// </summary>
public abstract class InvoiceOperation
{
    /// <summary>
    /// Transforms an invoice through its lifecycle states
    /// </summary>
    /// <param name="invoice">The invoice to transform</param>
    /// <returns>The transformed invoice</returns>
    public IInvoice Transform(IInvoice invoice)
    {
        return invoice switch
        {
            UnvalidatedInvoice i => OnUnvalidated(i),
            UnpaidInvoice i => OnUnpaid(i),
            PaidInvoice i => OnPaid(i),
            InvalidInvoice i => OnInvalid(i),
            _ => throw new InvalidOperationException($"Unexpected invoice state: {invoice.GetType().Name}")
        };
    }

    /// <summary>
    /// Handles an invoice in the Unvalidated state. Default is identity (returns same object).
    /// </summary>
    protected virtual IInvoice OnUnvalidated(UnvalidatedInvoice invoice) => invoice;

    /// <summary>
    /// Handles an invoice in the Unpaid state. Default is identity (returns same object).
    /// </summary>
    protected virtual IInvoice OnUnpaid(UnpaidInvoice invoice) => invoice;

    /// <summary>
    /// Handles an invoice in the Paid state. Default is identity (returns same object).
    /// </summary>
    protected virtual IInvoice OnPaid(PaidInvoice invoice) => invoice;

    /// <summary>
    /// Handles an invoice in the Invalid state. Default is identity (returns same object).
    /// </summary>
    protected virtual IInvoice OnInvalid(InvalidInvoice invoice) => invoice;
}

/// <summary>
/// Base class for invoice operations that transform invoice states and return a specific result
/// </summary>
/// <typeparam name="TResult">The result type of the operation</typeparam>
public abstract class InvoiceOperation<TResult>
{
    /// <summary>
    /// Transforms an invoice through its lifecycle states and returns a result
    /// </summary>
    /// <param name="invoice">The invoice to transform</param>
    /// <returns>The result of the transformation</returns>
    public TResult Transform(IInvoice invoice)
    {
        return invoice switch
        {
            UnvalidatedInvoice i => OnUnvalidated(i),
            UnpaidInvoice i => OnUnpaid(i),
            PaidInvoice i => OnPaid(i),
            InvalidInvoice i => OnInvalid(i),
            _ => throw new InvalidOperationException($"Unexpected invoice state: {invoice.GetType().Name}")
        };
    }

    /// <summary>
    /// Handles an invoice in the Unvalidated state and returns a result
    /// </summary>
    protected abstract TResult OnUnvalidated(UnvalidatedInvoice invoice);

    /// <summary>
    /// Handles an invoice in the Unpaid state and returns a result
    /// </summary>
    protected abstract TResult OnUnpaid(UnpaidInvoice invoice);

    /// <summary>
    /// Handles an invoice in the Paid state and returns a result
    /// </summary>
    protected abstract TResult OnPaid(PaidInvoice invoice);

    /// <summary>
    /// Handles an invoice in the Invalid state and returns a result
    /// </summary>
    protected abstract TResult OnInvalid(InvalidInvoice invoice);
}
