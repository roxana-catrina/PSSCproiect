namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Operation to send package notifications to customers based on package state
/// Single responsibility: Package notification sending
/// </summary>
public class NotifyPackageOperation : PackageOperation<Task>
{
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="notificationService">Optional notification service (uses console if null)</param>
    public NotifyPackageOperation(INotificationService? notificationService = null)
    {
        _notificationService = notificationService ?? new ConsoleNotificationService();
    }

    /// <summary>
    /// No notification for unvalidated packages
    /// </summary>
    protected override Task OnUnvalidated(UnvalidatedPackage package)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends shipping notification for a PreparedPackage
    /// </summary>
    protected override Task OnPrepared(PreparedPackage package)
    {
        ValidatePackage(package);
        return SendShippingNotificationAsync(package.OrderId, package.AWB.Value);
    }

    /// <summary>
    /// Sends in-transit notification for an InTransitPackage
    /// </summary>
    protected override Task OnInTransit(InTransitPackage package)
    {
        ValidatePackage(package);
        return SendInTransitNotificationAsync(package.OrderId, package.AWB.Value);
    }

    /// <summary>
    /// Sends delivery confirmation for a DeliveredPackage
    /// </summary>
    protected override Task OnDelivered(DeliveredPackage package)
    {
        ValidatePackage(package);
        return SendDeliveryConfirmationAsync(package.OrderId);
    }

    /// <summary>
    /// No notification for returned packages
    /// </summary>
    protected override Task OnReturned(ReturnedPackage package)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// No notification for invalid packages
    /// </summary>
    protected override Task OnInvalid(InvalidPackage package)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Private helper method to validate package is not null
    /// </summary>
    private static void ValidatePackage(IPackage package)
    {
        if (package == null)
            throw new ArgumentNullException(nameof(package));
    }

    /// <summary>
    /// Private helper method to send shipping notification
    /// </summary>
    private async Task SendShippingNotificationAsync(string orderId, string awb)
    {
        await _notificationService.SendAsync("customer", $"Package for order {orderId} prepared. AWB: {awb}");
    }

    /// <summary>
    /// Private helper method to send in-transit notification
    /// </summary>
    private async Task SendInTransitNotificationAsync(string orderId, string awb)
    {
        await _notificationService.SendAsync("customer", $"Package for order {orderId} is in transit. AWB: {awb}");
    }

    /// <summary>
    /// Private helper method to send delivery confirmation
    /// </summary>
    private async Task SendDeliveryConfirmationAsync(string orderId)
    {
        await _notificationService.SendAsync("customer", $"Package for order {orderId} has been delivered");
    }
}
