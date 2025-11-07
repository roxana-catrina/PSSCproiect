namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;
using Proiect.Domain.Exceptions;

public class ValidateStockOperation
{
    public static bool Execute(List<Product> products, Dictionary<string, int> requestedQuantities)
    {
        foreach (var kvp in requestedQuantities)
        {
            var product = products.FirstOrDefault(p => p.Id == kvp.Key);
            
            if (product == null)
                throw new InsufficientStockException($"Product with ID {kvp.Key} not found");
            
            if (!product.IsAvailable)
                throw new InsufficientStockException($"Product {product.Name} is not available");
            
            if (product.StockQuantity < kvp.Value)
                throw new InsufficientStockException($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {kvp.Value}");
        }
        
        return true;
    }
}

