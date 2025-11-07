namespace Proiect.Domain.Models.Commands;

public record GenerateInvoiceCommand(
    string OrderId,
    decimal TotalAmount,
    string CustomerName,
    string CustomerEmail
);

