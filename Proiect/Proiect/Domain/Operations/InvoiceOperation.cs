using static Proiect.Domain.Models.Entities.Invoice;

namespace Proiect.Domain.Operations;

public abstract class InvoiceOperation : DomainOperation<IInvoice, object, IInvoice>
{
    internal IInvoice Transform(IInvoice invoice) => Transform(invoice, null);
    
    public override IInvoice Transform(IInvoice invoice, object? state) => invoice switch
    {
        UnvalidatedInvoice unvalidated => OnUnvalidated(unvalidated),
        ValidatedInvoice validated => OnValidated(validated),
        GeneratedInvoice generated => OnGenerated(generated),
        InvalidInvoice invalid => OnInvalid(invalid),
        _ => invoice
    };
    
    protected virtual IInvoice OnUnvalidated(UnvalidatedInvoice invoice) => invoice;
    protected virtual IInvoice OnValidated(ValidatedInvoice invoice) => invoice;
    protected virtual IInvoice OnGenerated(GeneratedInvoice invoice) => invoice;
    protected virtual IInvoice OnInvalid(InvalidInvoice invoice) => invoice;
}
