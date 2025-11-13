namespace Proiect.Domain.Models.Entities;

using Proiect.Domain.Models.ValueObjects;

public interface IPackage { }

public record PackageItem(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal Weight
);

public record PreparedPackage(
    string Id,
    string OrderId,
    AWB AWB,
    DeliveryAddress DeliveryAddress,
    DateTime PreparedAt
) : IPackage
{
    public IReadOnlyCollection<PackageItem> Items { get; init; } = new List<PackageItem>().AsReadOnly();
    
    internal PreparedPackage(string orderId, DeliveryAddress deliveryAddress, IReadOnlyCollection<PackageItem> items)
        : this(
            Guid.NewGuid().ToString(),
            orderId,
            AWB.Generate(),
            deliveryAddress,
            DateTime.UtcNow
        )
    {
        Items = items;
    }
}

public record InTransitPackage(
    string Id,
    string OrderId,
    AWB AWB,
    DeliveryAddress DeliveryAddress,
    DateTime PreparedAt,
    DateTime PickedUpAt
) : IPackage
{
    public IReadOnlyCollection<PackageItem> Items { get; init; } = new List<PackageItem>().AsReadOnly();
    
    internal InTransitPackage(PreparedPackage preparedPackage)
        : this(
            preparedPackage.Id,
            preparedPackage.OrderId,
            preparedPackage.AWB,
            preparedPackage.DeliveryAddress,
            preparedPackage.PreparedAt,
            DateTime.UtcNow
        )
    {
        Items = preparedPackage.Items;
    }
}

public record DeliveredPackage(
    string Id,
    string OrderId,
    AWB AWB,
    DeliveryAddress DeliveryAddress,
    DateTime PreparedAt,
    DateTime PickedUpAt,
    DateTime DeliveredAt,
    string ReceivedBy
) : IPackage
{
    public IReadOnlyCollection<PackageItem> Items { get; init; } = new List<PackageItem>().AsReadOnly();
    
    internal DeliveredPackage(InTransitPackage inTransitPackage, string receivedBy)
        : this(
            inTransitPackage.Id,
            inTransitPackage.OrderId,
            inTransitPackage.AWB,
            inTransitPackage.DeliveryAddress,
            inTransitPackage.PreparedAt,
            inTransitPackage.PickedUpAt,
            DateTime.UtcNow,
            receivedBy
        )
    {
        Items = inTransitPackage.Items;
    }
}

public record ReturnedPackage(
    string Id,
    string OrderId,
    AWB AWB,
    DeliveryAddress DeliveryAddress,
    DateTime PreparedAt,
    DateTime PickedUpAt,
    DateTime ReturnedAt,
    string ReturnReason
) : IPackage
{
    public IReadOnlyCollection<PackageItem> Items { get; init; } = new List<PackageItem>().AsReadOnly();
    
    internal ReturnedPackage(InTransitPackage inTransitPackage, string returnReason)
        : this(
            inTransitPackage.Id,
            inTransitPackage.OrderId,
            inTransitPackage.AWB,
            inTransitPackage.DeliveryAddress,
            inTransitPackage.PreparedAt,
            inTransitPackage.PickedUpAt,
            DateTime.UtcNow,
            returnReason
        )
    {
        Items = inTransitPackage.Items;
    }
}

public record InvalidPackage(
    string OrderId,
    DeliveryAddress DeliveryAddress,
    IEnumerable<string> Reasons
) : IPackage
{
    public IReadOnlyCollection<PackageItem> Items { get; init; } = new List<PackageItem>().AsReadOnly();
    
    internal InvalidPackage(string orderId, DeliveryAddress deliveryAddress, IReadOnlyCollection<PackageItem> items, params string[] reasons)
        : this(orderId, deliveryAddress, reasons.ToList().AsReadOnly())
    {
        Items = items;
    }
}
