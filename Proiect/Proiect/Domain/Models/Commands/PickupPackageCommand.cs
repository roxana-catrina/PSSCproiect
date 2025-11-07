namespace Proiect.Domain.Models.Commands;

public record PickupPackageCommand(
    string OrderId,
    string AWB,
    DateTime PickupDate
);

