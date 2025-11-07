namespace Proiect.Domain.Models.Entities;

using Proiect.Domain.Models.ValueObjects;

public class Product
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Price Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsAvailable { get; private set; }

    public Product(string name, string description, Price price, int stockQuantity)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        IsAvailable = true;
    }

    public void UpdateStock(int quantity)
    {
        StockQuantity += quantity;
        if (StockQuantity < 0)
            throw new InvalidOperationException("Stock quantity cannot be negative");
    }

    public void ReserveStock(int quantity)
    {
        if (quantity > StockQuantity)
            throw new InvalidOperationException("Insufficient stock");
        
        StockQuantity -= quantity;
    }

    public void Deactivate()
    {
        IsAvailable = false;
    }

    public void Activate()
    {
        IsAvailable = true;
    }
}

