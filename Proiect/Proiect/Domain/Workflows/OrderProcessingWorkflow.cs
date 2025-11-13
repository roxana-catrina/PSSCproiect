namespace Proiect.Domain.Workflows;

using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Entities;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Operations;

/// <summary>
/// Workflow for order processing operations
/// Orchestrates order placement, validation, and confirmation
/// </summary>
public class OrderProcessingWorkflow
{
    /// <summary>
    /// Executes the order processing workflow
    /// </summary>
    /// <param name="command">The place order command</param>
    /// <param name="validateStockOp">Operation to validate stock</param>
    /// <param name="validateOrderOp">Operation to validate order</param>
    /// <param name="confirmOrderOp">Operation to confirm order</param>
    /// <param name="notifyCustomerOp">Operation to notify customer</param>
    /// <returns>Order placed event</returns>
    public async Task<OrderPlaced> Execute(
        PlaceOrderCommand command,
        ValidateStockOperation validateStockOp,
        ValidateOrderOperation validateOrderOp,
        ConfirmOrderOperation confirmOrderOp,
        NotifyCustomerOperation notifyCustomerOp)
    {
        // Create Unvalidated entity at the beginning
        var orderLines = command.Items.Select(item => new OrderLine(
            item.ProductId,
            string.Empty, // Product name will be filled during validation
            item.Quantity,
            item.UnitPrice
        )).ToList().AsReadOnly();

        IOrder result = new UnvalidatedOrder(
            command.CustomerName,
            command.CustomerEmail,
            command.DeliveryAddress,
            orderLines
        );

        // Chain operations: result = op.Transform(result)
        result = validateStockOp.Transform(result);
        result = validateOrderOp.Transform(result);
        result = confirmOrderOp.Transform(result);

        // Notify customer after confirmation
        await notifyCustomerOp.Transform(result);

        // Convert to event using ToEvent()
        if (result is ConfirmedOrder confirmedOrder)
        {
            return confirmedOrder.ToEvent();
        }

        // This should never happen if workflow is correct
        throw new InvalidOperationException("Workflow did not produce a ConfirmedOrder");
    }
}
