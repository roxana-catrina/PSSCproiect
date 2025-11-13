namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;
using Proiect.Domain.Exceptions;

/// <summary>
/// Operation to validate stock availability for orders
/// Single responsibility: Stock validation
/// </summary>
public class ValidateStockOperation : OrderOperation
{
    private readonly List<ActiveProduct> _products;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="products">List of available products</param>
    public ValidateStockOperation(List<ActiveProduct> products)
    {
        _products = products ?? throw new ArgumentNullException(nameof(products));
    }

    /// <summary>
    /// Validates stock for UnvalidatedOrder
    /// </summary>
    protected override IOrder OnUnvalidated(UnvalidatedOrder order)
    {
        ValidateOrder(order);
        ValidateStock(order.OrderLines);
        return order;
    }

    /// <summary>
    /// Private helper method to validate stock availability
    /// </summary>
    private void ValidateStock(IReadOnlyCollection<OrderLine> orderLines)
    {
        foreach (var line in orderLines)
        {
            var product = _products.FirstOrDefault(p => p.Id == line.ProductId);
            
            if (product == null)
                throw new InsufficientStockException($"Product with ID {line.ProductId} not found");
            
            if (product.StockQuantity < line.Quantity)
                throw new InsufficientStockException($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {line.Quantity}");
        }
    }

    /// <summary>
    /// Private helper method to validate order
    /// </summary>
    private static void ValidateOrder(UnvalidatedOrder order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
    }
}
