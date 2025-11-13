namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.ValueObjects;
using Proiect.Domain.Models.Entities;

public class AssignAWBOperation
{
    public static AWB Execute(IPackage package)
    {
        if (package == null)
            throw new ArgumentNullException(nameof(package));
        
        return package switch
        {
            PreparedPackage p => p.AWB,
            InTransitPackage p => p.AWB,
            DeliveredPackage p => p.AWB,
            ReturnedPackage p => p.AWB,
            _ => throw new InvalidOperationException("Cannot assign AWB to invalid package")
        };
    }

    public static AWB GenerateNewAWB()
    {
        return AWB.Generate();
    }
}
