using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Operations;
using static Proiect.Domain.Models.Entities.Package;

namespace Proiect.Domain.Workflows;

/// <summary>
/// Workflow for shipping packages
/// Orchestrates package validation, AWB assignment, and courier notification
/// </summary>
public class ShippingWorkflow
{
    /// <summary>
    /// Executes the shipping workflow
    /// </summary>
    /// <param name="command">The pickup package command with raw input data</param>
    /// <param name="generateAwb">Dependency to generate AWB tracking number</param>
    /// <param name="notifyCourier">Dependency to notify courier service</param>
    /// <param name="getRecipientName">Dependency to get recipient name for delivery confirmation</param>
    /// <returns>Package delivered event (success or failure)</returns>
    public PackageDeliveredEvent.IPackageDeliveredEvent Execute(
        PickupPackageCommand command,
        Func<string> generateAwb,
        Func<string, bool> notifyCourier,
        Func<string, string> getRecipientName)
    {
        // Step 1: Create unvalidated package from command
        var unvalidated = new UnvalidatedPackage(
            command.OrderNumber,
            command.DeliveryStreet,
            command.DeliveryCity,
            command.DeliveryPostalCode,
            command.DeliveryCountry);
        
        // Step 2: Chain operations to transform package through states
        IPackage result = new ValidatePackageDataOperation().Transform(unvalidated);
        result = new AssignAWBOperation(generateAwb).Transform(result);
        result = new ShipPackageOperation(notifyCourier).Transform(result);
        result = new DeliverPackageOperation(getRecipientName).Transform(result);
        
        // Step 3: Convert final state to event
        return result.ToEvent();
    }
}
