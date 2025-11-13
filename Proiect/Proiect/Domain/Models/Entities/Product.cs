namespace Proiect.Domain.Models.Entities;

using Proiect.Domain.Models.ValueObjects;

public interface IProduct { }

public record ProductVariant(
    string VariantId,
    string Size,
    string Color,
    int StockQuantity
);

public record ActiveProduct(
    string Id,
    string Name,
    string Description,
    Price Price,
    int StockQuantity
) : IProduct
{
    public IReadOnlyCollection<ProductVariant> Variants { get; init; } = new List<ProductVariant>().AsReadOnly();
    
    internal ActiveProduct(string name, string description, Price price, int stockQuantity, IReadOnlyCollection<ProductVariant> variants)
        : this(
            Guid.NewGuid().ToString(),
            name,
            description,
            price,
            stockQuantity
        )
    {
        Variants = variants;
    }
    
    internal ActiveProduct WithStockQuantity(int newStockQuantity)
        => this with { StockQuantity = newStockQuantity };
}

public record InactiveProduct(
    string Id,
    string Name,
    string Description,
    Price Price,
    int StockQuantity,
    DateTime DeactivatedAt,
    string DeactivationReason
) : IProduct
{
    public IReadOnlyCollection<ProductVariant> Variants { get; init; } = new List<ProductVariant>().AsReadOnly();
    
    internal InactiveProduct(ActiveProduct activeProduct, string deactivationReason)
        : this(
            activeProduct.Id,
            activeProduct.Name,
            activeProduct.Description,
            activeProduct.Price,
            activeProduct.StockQuantity,
            DateTime.UtcNow,
            deactivationReason
        )
    {
        Variants = activeProduct.Variants;
    }
}

public record OutOfStockProduct(
    string Id,
    string Name,
    string Description,
    Price Price,
    DateTime OutOfStockSince
) : IProduct
{
    public IReadOnlyCollection<ProductVariant> Variants { get; init; } = new List<ProductVariant>().AsReadOnly();
    
    internal OutOfStockProduct(ActiveProduct activeProduct)
        : this(
            activeProduct.Id,
            activeProduct.Name,
            activeProduct.Description,
            activeProduct.Price,
            DateTime.UtcNow
        )
    {
        Variants = activeProduct.Variants;
    }
}

public record InvalidProduct(
    string Name,
    string Description,
    Price Price,
    int StockQuantity,
    IEnumerable<string> Reasons
) : IProduct
{
    public IReadOnlyCollection<ProductVariant> Variants { get; init; } = new List<ProductVariant>().AsReadOnly();
    
    internal InvalidProduct(string name, string description, Price price, int stockQuantity, IReadOnlyCollection<ProductVariant> variants, params string[] reasons)
        : this(name, description, price, stockQuantity, reasons.ToList().AsReadOnly())
    {
        Variants = variants;
    }
}
