namespace Proiect.Domain.Models.Entities;

public interface IInvoice { }

public record InvoiceItem(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal VatRate
)
{
    public decimal SubTotal => Quantity * UnitPrice;
    public decimal VatAmount => SubTotal * VatRate;
    public decimal Total => SubTotal + VatAmount;
}

public record UnpaidInvoice(
    string Id,
    string InvoiceNumber,
    string OrderId,
    string CustomerName,
    string CustomerEmail,
    decimal SubTotal,
    decimal VatAmount,
    decimal TotalAmount,
    DateTime GeneratedAt
) : IInvoice
{
    public IReadOnlyCollection<InvoiceItem> Items { get; init; } = new List<InvoiceItem>().AsReadOnly();
    
    internal UnpaidInvoice(string orderId, string customerName, string customerEmail, IReadOnlyCollection<InvoiceItem> items)
        : this(
            Guid.NewGuid().ToString(),
            $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            orderId,
            customerName,
            customerEmail,
            items.Sum(i => i.SubTotal),
            items.Sum(i => i.VatAmount),
            items.Sum(i => i.Total),
            DateTime.UtcNow
        )
    {
        Items = items;
    }
}

public record PaidInvoice(
    string Id,
    string InvoiceNumber,
    string OrderId,
    string CustomerName,
    string CustomerEmail,
    decimal SubTotal,
    decimal VatAmount,
    decimal TotalAmount,
    DateTime GeneratedAt,
    DateTime PaidAt
) : IInvoice
{
    public IReadOnlyCollection<InvoiceItem> Items { get; init; } = new List<InvoiceItem>().AsReadOnly();
    
    internal PaidInvoice(UnpaidInvoice unpaidInvoice)
        : this(
            unpaidInvoice.Id,
            unpaidInvoice.InvoiceNumber,
            unpaidInvoice.OrderId,
            unpaidInvoice.CustomerName,
            unpaidInvoice.CustomerEmail,
            unpaidInvoice.SubTotal,
            unpaidInvoice.VatAmount,
            unpaidInvoice.TotalAmount,
            unpaidInvoice.GeneratedAt,
            DateTime.UtcNow
        )
    {
        Items = unpaidInvoice.Items;
    }
}

public record InvalidInvoice(
    string OrderId,
    string CustomerName,
    string CustomerEmail,
    decimal SubTotal,
    decimal VatAmount,
    IEnumerable<string> Reasons
) : IInvoice
{
    public IReadOnlyCollection<InvoiceItem> Items { get; init; } = new List<InvoiceItem>().AsReadOnly();
    
    internal InvalidInvoice(string orderId, string customerName, string customerEmail, IReadOnlyCollection<InvoiceItem> items, params string[] reasons)
        : this(orderId, customerName, customerEmail, items.Sum(i => i.SubTotal), items.Sum(i => i.VatAmount), reasons.ToList().AsReadOnly())
    {
        Items = items;
    }
}
