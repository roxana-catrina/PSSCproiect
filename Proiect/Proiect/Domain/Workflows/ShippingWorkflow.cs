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
        var package = new Package(command.OrderId, deliveryAddress);
        var awb = AssignAWBOperation.Execute(package);

        await NotifyCustomerOperation.SendShippingNotification(customerEmail, awb.Value);

        var preparedEvent = new PackagePrepared(
            package.Id,
            package.OrderId,
            package.AWB.Value,
            package.PreparedAt
        );

        package.MarkAsPickedUp();

        PackageDelivered? deliveredEvent = null;
        if (simulateDelivery)
        {
            await Task.Delay(200);
            package.MarkAsDelivered("Customer");
            
            await NotifyCustomerOperation.SendDeliveryConfirmation(customerEmail, command.OrderId);
            
            deliveredEvent = new PackageDelivered(
                package.Id,
                package.OrderId,
                package.AWB.Value,
                package.DeliveredAt!.Value,
                package.ReceivedBy!
            );
        }

        return (preparedEvent, deliveredEvent);
    }
}

