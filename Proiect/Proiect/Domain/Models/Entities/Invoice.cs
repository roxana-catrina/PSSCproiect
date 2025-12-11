using Proiect.Domain.Models.ValueObjects;

namespace Proiect.Domain.Models.Entities;

public static class Invoice
{
    public interface IInvoice { }
    
    public record UnvalidatedInvoice(
        string OrderNumber,
        string CustomerName,
        string TotalAmount) : IInvoice;
    
    public record ValidatedInvoice : IInvoice
    {
        internal ValidatedInvoice(
            OrderNumber orderNumber,
            string customerName,
            Price totalAmount)
        {
            OrderNumber = orderNumber;
            CustomerName = customerName;
            TotalAmount = totalAmount;
        }
        
        public OrderNumber OrderNumber { get; }
        public string CustomerName { get; }
        public Price TotalAmount { get; }
    }
    
    public record GeneratedInvoice : IInvoice
    {
        internal GeneratedInvoice(
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
    
    public record InvalidInvoice : IInvoice
    {
        internal InvalidInvoice(IEnumerable<string> reasons)
        {
            Reasons = reasons;
        }
        
        public IEnumerable<string> Reasons { get; }
    }
}
