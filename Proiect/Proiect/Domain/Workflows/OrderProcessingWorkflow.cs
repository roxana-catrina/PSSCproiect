using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Operations;
using static Proiect.Domain.Models.Entities.Order;

namespace Proiect.Domain.Workflows;

/// <summary>
/// Workflow for processing customer orders
/// Orchestrates validation, stock checking, and order confirmation
/// </summary>
public class OrderProcessingWorkflow
{
    /// <summary>
    /// Executes the order processing workflow
    /// </summary>
    /// <param name="command">The place order command with raw input data</param>
    /// <param name="checkStockAvailability">Dependency to check if product stock is available</param>
    /// <param name="generateOrderNumber">Dependency to generate unique order number</param>
    /// <returns>Order placed event (success or failure)</returns>
    public OrderPlacedEvent.IOrderPlacedEvent Execute(
        PlaceOrderCommand command,
        Func<string, int, bool> checkStockAvailability,
        Func<string> generateOrderNumber)
    {
        // Step 1: Create unvalidated order from command
        var unvalidatedItems = command.Items
            .Select(i => new UnvalidatedOrderItem(i.ProductName, i.Quantity, i.UnitPrice))
            .ToList();
        
        var unvalidated = new UnvalidatedOrder(
            command.CustomerName,
            command.CustomerEmail,
            command.DeliveryStreet,
            command.DeliveryCity,
            command.DeliveryPostalCode,
            command.DeliveryCountry,
            unvalidatedItems);
        
        // Step 2: Chain operations to transform order through states
        IOrder result = new ValidateStockOperation(checkStockAvailability).Transform(unvalidated);
        result = new NotifyCustomerOperation(generateOrderNumber).Transform(result);
        
        // Step 3: Convert final state to event
        return result.ToEvent();
    }
}
