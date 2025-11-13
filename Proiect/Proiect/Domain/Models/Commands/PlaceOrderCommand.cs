namespace Proiect.Domain.Models.Commands;

using Proiect.Domain.Models.ValueObjects;

public class PlaceOrderCommand
{
    public PlaceOrderCommand(string customerName, string customerEmail, DeliveryAddress deliveryAddress, IReadOnlyCollection<OrderItem> items)
    {
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        DeliveryAddress = deliveryAddress;
        Items = items;
    }

    public string CustomerName { get; }
    public string CustomerEmail { get; }
    public DeliveryAddress DeliveryAddress { get; }
    public IReadOnlyCollection<OrderItem> Items { get; }
}

public class OrderItem
{
    public OrderItem(string productId, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public string ProductId { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
}
