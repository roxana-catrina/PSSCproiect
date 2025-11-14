using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Invoice;

namespace Proiect.Domain.Models.Events;

public static class InvoiceGeneratedEvent
{
    public interface IInvoiceGeneratedEvent { }
    
    public record InvoiceGeneratedSucceededEvent : IInvoiceGeneratedEvent
    {
        internal InvoiceGeneratedSucceededEvent(
            string invoiceNumber,
            OrderNumber orderNumber,
            string customerName,
            Price totalAmount,
            Price vatAmount,
            Price totalWithVat,
            DateTime generatedAt)
        {
            InvoiceNumber = invoiceNumber;
            OrderNumber = orderNumber;
            CustomerName = customerName;
            TotalAmount = totalAmount;
            VatAmount = vatAmount;
            TotalWithVat = totalWithVat;
            GeneratedAt = generatedAt;
        }
        
        public string InvoiceNumber { get; }
        public OrderNumber OrderNumber { get; }
        public string CustomerName { get; }
        public Price TotalAmount { get; }
        public Price VatAmount { get; }
        public Price TotalWithVat { get; }
        public DateTime GeneratedAt { get; }
    }
    
    public record InvoiceGeneratedFailedEvent : IInvoiceGeneratedEvent
    {
        internal InvoiceGeneratedFailedEvent(IEnumerable<string> reasons)
        {
            Reasons = reasons;
        }
        
        public IEnumerable<string> Reasons { get; }
    }
    
    public static IInvoiceGeneratedEvent ToEvent(this IInvoice invoice) => invoice switch
    {
        GeneratedInvoice generated => new InvoiceGeneratedSucceededEvent(
            generated.InvoiceNumber,
            generated.OrderNumber,
            generated.CustomerName,
            generated.TotalAmount,
            generated.VatAmount,
            generated.TotalWithVat,
            generated.GeneratedAt),
        InvalidInvoice invalid => new InvoiceGeneratedFailedEvent(invalid.Reasons),
        UnvalidatedInvoice _ => new InvoiceGeneratedFailedEvent(new[] { "Invoice was not validated" }),
        ValidatedInvoice _ => new InvoiceGeneratedFailedEvent(new[] { "Invoice was not generated" }),
        _ => new InvoiceGeneratedFailedEvent(new[] { $"Unexpected state: {invoice.GetType().Name}" })
    };
}
