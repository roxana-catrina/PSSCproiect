using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Order;

namespace Proiect.Domain.Operations;

internal sealed class ValidateStockOperation : OrderOperation
{
    private readonly Func<string, int, bool> _checkStockAvailability;
    
    internal ValidateStockOperation(Func<string, int, bool> checkStockAvailability)
    {
        _checkStockAvailability = checkStockAvailability;
    }
    
    protected override IOrder OnUnvalidated(UnvalidatedOrder order)
    {
        var errors = new List<string>();
        
        // Validate customer info
        if (string.IsNullOrWhiteSpace(order.CustomerName))
            errors.Add("Customer name is required");
        
        if (string.IsNullOrWhiteSpace(order.CustomerEmail) || !order.CustomerEmail.Contains("@"))
            errors.Add("Valid customer email is required");
        
        // Validate delivery address
        if (!DeliveryAddress.TryParse(
            order.DeliveryStreet,
            order.DeliveryCity,
            order.DeliveryPostalCode,
            order.DeliveryCountry,
            out var deliveryAddress))
        {
            errors.Add("Invalid delivery address");
        }
        
        // Validate items
        var validatedItems = new List<ValidatedOrderItem>();
        if (!order.Items.Any())
        {
            errors.Add("Order must contain at least one item");
        }
        else
        {
            foreach (var item in order.Items)
            {
                if (string.IsNullOrWhiteSpace(item.ProductName))
                {
                    errors.Add($"Product name is required");
                    continue;
                }
                
                if (!int.TryParse(item.Quantity, out var quantity) || quantity <= 0)
                {
                    errors.Add($"Invalid quantity for {item.ProductName}");
                    continue;
                }
                
                if (!Price.TryParse(item.UnitPrice, out var price) || price == null)
                {
                    errors.Add($"Invalid price for {item.ProductName}");
                    continue;
                }
                
                // Check stock availability
                if (!_checkStockAvailability(item.ProductName, quantity))
                {
                    errors.Add($"Insufficient stock for {item.ProductName}");
                    continue;
                }
                
                validatedItems.Add(new ValidatedOrderItem(item.ProductName, quantity, price));
            }
        }
        
        if (errors.Any())
            return new InvalidOrder(errors);
        
        return new ValidatedOrder(
            order.CustomerName,
            order.CustomerEmail,
            deliveryAddress!,
            validatedItems);
    }
}
