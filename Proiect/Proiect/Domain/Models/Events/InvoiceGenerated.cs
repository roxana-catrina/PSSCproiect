namespace Proiect.Domain.Models.Events;

public record InvoiceGenerated(
    string InvoiceId,
    string OrderId,
    string InvoiceNumber,
    decimal TotalAmount,
    decimal VATAmount,
    DateTime GeneratedAt
);

