namespace Proiect.Domain.Models.Entities;

public class Invoice
{
    public string Id { get; private set; }
    public string InvoiceNumber { get; private set; }
    public string OrderId { get; private set; }
    public string CustomerName { get; private set; }
    public string CustomerEmail { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal VATAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public bool IsPaid { get; private set; }

    public Invoice(string orderId, string customerName, string customerEmail, decimal subTotal, decimal vatAmount)
    {
        Id = Guid.NewGuid().ToString();
        InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        OrderId = orderId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        SubTotal = subTotal;
        VATAmount = vatAmount;
        TotalAmount = subTotal + vatAmount;
        GeneratedAt = DateTime.UtcNow;
        IsPaid = false;
    }

    public void MarkAsPaid()
    {
        if (IsPaid)
            throw new InvalidOperationException("Invoice is already paid");
        
        IsPaid = true;
    }
}

