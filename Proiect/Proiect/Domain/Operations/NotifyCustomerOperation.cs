namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Operation to send notifications to customers based on order state
/// Single responsibility: Customer notification sending
/// </summary>
public class NotifyCustomerOperation : OrderOperation<Task>
{
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="notificationService">Optional notification service (uses console if null)</param>
    public NotifyCustomerOperation(INotificationService? notificationService = null)
    {
        _notificationService = notificationService ?? new ConsoleNotificationService();
    }

    /// <summary>
    /// No notification for unvalidated orders
    /// </summary>
    protected override Task OnUnvalidated(UnvalidatedOrder order)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends notification for a Pending order (no notification needed at this stage)
    /// </summary>
    protected override Task OnPending(PendingOrder order)
    {
        ValidateOrder(order);
        // No notification for pending orders
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends order confirmation notification for a Confirmed order
    /// </summary>
    protected override Task OnConfirmed(ConfirmedOrder order)
    {
        ValidateOrder(order);
        return SendOrderConfirmationAsync(order.CustomerEmail, order.OrderNumber.Value);
    }

    /// <summary>
    /// Sends payment confirmation notification for a Paid order
    /// </summary>
    protected override Task OnPaid(PaidOrder order)
    {
        ValidateOrder(order);
        return SendPaymentConfirmationAsync(order.CustomerEmail, order.OrderNumber.Value);
    }

    /// <summary>
    /// Sends shipping notification for a Shipped order
    /// </summary>
    protected override Task OnShipped(ShippedOrder order)
    {
        ValidateOrder(order);
        return SendShippingNotificationAsync(order.CustomerEmail, order.OrderNumber.Value);
    }

    /// <summary>
    /// Sends delivery confirmation notification for a Delivered order
    /// </summary>
    protected override Task OnDelivered(DeliveredOrder order)
    {
        ValidateOrder(order);
        return SendDeliveryConfirmationAsync(order.CustomerEmail, order.OrderNumber.Value);
    }

    /// <summary>
    /// No notification for cancelled orders
    /// </summary>
    protected override Task OnCancelled(CancelledOrder order)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// No notification for invalid orders
    /// </summary>
    protected override Task OnInvalid(InvalidOrder order)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Private helper method to validate order is not null
    /// </summary>
    private static void ValidateOrder(IOrder order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
    }

    /// <summary>
    /// Private helper method to send order confirmation
    /// </summary>
    private async Task SendOrderConfirmationAsync(string customerEmail, string orderNumber)
    {
        await _notificationService.SendAsync(customerEmail, $"Order confirmation sent for order {orderNumber}");
    }

    /// <summary>
    /// Private helper method to send payment confirmation
    /// </summary>
    private async Task SendPaymentConfirmationAsync(string customerEmail, string orderNumber)
    {
        await _notificationService.SendAsync(customerEmail, $"Payment confirmed for order {orderNumber}");
    }

    /// <summary>
    /// Private helper method to send shipping notification
    /// </summary>
    private async Task SendShippingNotificationAsync(string customerEmail, string orderNumber)
    {
        await _notificationService.SendAsync(customerEmail, $"Order {orderNumber} has been shipped");
    }

    /// <summary>
    /// Private helper method to send delivery confirmation
    /// </summary>
    private async Task SendDeliveryConfirmationAsync(string customerEmail, string orderNumber)
    {
        await _notificationService.SendAsync(customerEmail, $"Order {orderNumber} has been delivered");
    }
}

/// <summary>
/// Interface for notification service
/// </summary>
public interface INotificationService
{
    Task SendAsync(string recipient, string message);
}

/// <summary>
/// Console implementation of notification service
/// </summary>
internal class ConsoleNotificationService : INotificationService
{
    public async Task SendAsync(string recipient, string message)
    {
        await Task.Delay(100);
        Console.WriteLine($"Notification sent to {recipient}: {message}");
    }
}
