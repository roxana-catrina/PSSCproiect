namespace Proiect.Domain.Models.Commands;

public record GenerateInvoiceCommand(
    string OrderNumber,
    string CustomerName,
    string TotalAmount);
