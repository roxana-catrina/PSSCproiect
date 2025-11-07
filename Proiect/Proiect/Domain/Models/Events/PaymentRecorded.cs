namespace Proiect.Domain.Models.Events;

public record PaymentRecorded(
    string PaymentId,
    string InvoiceId,
    string OrderId,
    decimal Amount,
    string PaymentMethod,
    DateTime RecordedAt
);

