namespace Proiect.Domain.Workflows;

using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Entities;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Models.ValueObjects;
using Proiect.Domain.Operations;

/// <summary>
/// Workflow for shipping operations
/// Orchestrates package preparation, pickup, and delivery
/// </summary>
public class ShippingWorkflow
{
    /// <summary>
    /// Executes the shipping workflow
    /// </summary>
    /// <param name="command">The pickup package command</param>
    /// <param name="deliveryAddress">Delivery address for the package</param>
    /// <param name="items">Package items</param>
    /// <param name="preparePackageOp">Operation to prepare package</param>
    /// <param name="notifyPackageOp">Operation to notify customer</param>
    /// <param name="pickupPackageOp">Operation to mark package as picked up</param>
    /// <param name="deliverPackageOp">Optional operation to deliver package</param>
    /// <returns>Package prepared event and optional delivered event</returns>
    public async Task<PackagePrepared> Execute(
        PickupPackageCommand command,
        DeliveryAddress deliveryAddress,
        IReadOnlyCollection<PackageItem> items,
        PreparePackageOperation preparePackageOp,
        NotifyPackageOperation notifyPackageOp,
        PickupPackageOperation pickupPackageOp,
        DeliverPackageOperation? deliverPackageOp = null)
    {
        // Create Unvalidated entity at the beginning
        IPackage result = new UnvalidatedPackage(
            command.OrderId,
            deliveryAddress,
            items
        );

        // Chain operations: result = op.Transform(result)
        result = preparePackageOp.Transform(result);

        // Notify customer after package preparation
        await notifyPackageOp.Transform(result);

        // Mark package as picked up (in transit)
        result = pickupPackageOp.Transform(result);

        // Optionally deliver the package
        if (deliverPackageOp != null)
        {
            result = deliverPackageOp.Transform(result);
            await notifyPackageOp.Transform(result);
        }

        // Convert to event using ToEvent()
        if (result is PreparedPackage preparedPackage)
        {
            return preparedPackage.ToEvent();
        }
        
        if (result is InTransitPackage inTransitPackage)
        {
            // Return the prepared event from the original prepared state
            // In real scenario, we would track the original prepared package
            return new PackagePrepared(
                inTransitPackage.Id,
                inTransitPackage.OrderId,
                inTransitPackage.AWB.Value,
                inTransitPackage.PreparedAt
            );
        }
        
        if (result is DeliveredPackage deliveredPackage)
        {
            return new PackagePrepared(
                deliveredPackage.Id,
                deliveredPackage.OrderId,
                deliveredPackage.AWB.Value,
                deliveredPackage.PreparedAt
            );
        }

        // This should never happen if workflow is correct
        throw new InvalidOperationException("Workflow did not produce a valid package state");
    }
}
