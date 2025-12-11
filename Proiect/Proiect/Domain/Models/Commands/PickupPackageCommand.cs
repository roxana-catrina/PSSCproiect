namespace Proiect.Domain.Models.Commands;

public record PickupPackageCommand(
    string OrderNumber,
    string DeliveryStreet,
    string DeliveryCity,
    string DeliveryPostalCode,
    string DeliveryCountry);
