using static Proiect.Domain.Models.Entities.Order;

namespace Proiect.Domain.Operations;

public abstract class OrderOperation : DomainOperation<IOrder, object, IOrder>
{
    internal IOrder Transform(IOrder order) => Transform(order, null);
    
    public override IOrder Transform(IOrder order, object? state) => order switch
    {
        UnvalidatedOrder unvalidated => OnUnvalidated(unvalidated),
        ValidatedOrder validated => OnValidated(validated),
        ConfirmedOrder confirmed => OnConfirmed(confirmed),
        PaidOrder paid => OnPaid(paid),
        InvalidOrder invalid => OnInvalid(invalid),
        _ => order
    };
    
    protected virtual IOrder OnUnvalidated(UnvalidatedOrder order) => order;
    protected virtual IOrder OnValidated(ValidatedOrder order) => order;
    protected virtual IOrder OnConfirmed(ConfirmedOrder order) => order;
    protected virtual IOrder OnPaid(PaidOrder order) => order;
    protected virtual IOrder OnInvalid(InvalidOrder order) => order;
}
