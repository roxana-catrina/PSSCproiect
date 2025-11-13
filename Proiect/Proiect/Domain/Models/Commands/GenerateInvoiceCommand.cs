namespace Proiect.Domain.Models.Commands;

public class GenerateInvoiceCommand
{
    public GenerateInvoiceCommand(string orderId, decimal totalAmount, string customerName, string customerEmail)
    {
        OrderId = orderId;
        TotalAmount = totalAmount;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
    }

    public string OrderId { get; }
    public decimal TotalAmount { get; }
    public string CustomerName { get; }
    public string CustomerEmail { get; }
}
