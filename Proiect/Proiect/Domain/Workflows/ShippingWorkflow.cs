namespace Proiect.Domain.Workflows;

using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Entities;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Models.ValueObjects;
using Proiect.Domain.Operations;

public class ShippingWorkflow
{
    public async Task<(PackagePrepared PreparedEvent, PackageDelivered? DeliveredEvent)> ExecuteAsync(
        PickupPackageCommand command,
        DeliveryAddress deliveryAddress,
        string customerEmail,
        bool simulateDelivery = false)
    {
        // Create empty package items list (should be populated from order items in real scenario)
        var packageItems = new List<PackageItem>().AsReadOnly();
        
        var preparedPackage = new PreparedPackage(command.OrderId, deliveryAddress, packageItems);
        var awb = AssignAWBOperation.Execute(preparedPackage);

        await NotifyCustomerOperation.SendShippingNotification(customerEmail, awb.Value);

        var preparedEvent = new PackagePrepared(
            preparedPackage.Id,
            preparedPackage.OrderId,
            preparedPackage.AWB.Value,
            preparedPackage.PreparedAt
        );

        var inTransitPackage = new InTransitPackage(preparedPackage);

        PackageDelivered? deliveredEvent = null;
        if (simulateDelivery)
        {
            await Task.Delay(200);
            var deliveredPackage = new DeliveredPackage(inTransitPackage, "Customer");
            
            await NotifyCustomerOperation.SendDeliveryConfirmation(customerEmail, command.OrderId);
            
            deliveredEvent = new PackageDelivered(
                deliveredPackage.Id,
                deliveredPackage.OrderId,
                deliveredPackage.AWB.Value,
                deliveredPackage.DeliveredAt,
                deliveredPackage.ReceivedBy
            );
        }

        return (preparedEvent, deliveredEvent);
    }
}
