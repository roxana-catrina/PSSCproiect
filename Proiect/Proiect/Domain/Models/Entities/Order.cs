namespace Proiect.Domain.Models.Entities;

using Proiect.Domain.Models.ValueObjects;

public class Order
{
    public string Id { get; private set; }
    public OrderNumber OrderNumber { get; private set; }
    public string CustomerName { get; private set; }
    public string CustomerEmail { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; }
    public List<OrderLine> OrderLines { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }

    public Order(string customerName, string customerEmail, DeliveryAddress deliveryAddress, List<OrderLine> orderLines)
    {
        Id = Guid.NewGuid().ToString();
        OrderNumber = OrderNumber.Generate();
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        DeliveryAddress = deliveryAddress;
        OrderLines = orderLines;
        Status = OrderStatus.Pending;
        TotalAmount = orderLines.Sum(ol => ol.TotalPrice);
        CreatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");
        
        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid()
    {
        Status = OrderStatus.Paid;
    }

    public void MarkAsShipped()
    {
        Status = OrderStatus.Shipped;
    }

    public void MarkAsDelivered()
    {
        Status = OrderStatus.Delivered;
    }
}

public class OrderLine
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}

