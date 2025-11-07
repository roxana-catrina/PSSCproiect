namespace Proiect.Domain.Models.Events;

public record PackageDelivered(
    string PackageId,
    string OrderId,
    string AWB,
    DateTime DeliveredAt,
    string ReceivedBy
);

