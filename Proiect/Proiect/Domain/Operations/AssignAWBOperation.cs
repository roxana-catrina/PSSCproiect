using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Package;

namespace Proiect.Domain.Operations;

internal sealed class AssignAWBOperation : PackageOperation
{
    private readonly Func<string> _generateAWB;
    
    internal AssignAWBOperation(Func<string> generateAWB)
    {
        _generateAWB = generateAWB;
    }
    
    protected override IPackage OnValidated(ValidatedPackage package)
    {
        // Generate AWB tracking number
        var awbString = _generateAWB();
        
        if (!AWB.TryParse(awbString, out var awb) || awb == null)
        {
            return new InvalidPackage(new[] { "Failed to generate valid AWB tracking number" });
        }
        
        return new PreparedPackage(
            package.OrderNumber,
            package.DeliveryAddress,
            awb,
            DateTime.Now);
    }
}
