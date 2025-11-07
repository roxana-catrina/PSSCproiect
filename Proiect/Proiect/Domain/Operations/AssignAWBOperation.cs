namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.ValueObjects;
using Proiect.Domain.Models.Entities;

public class AssignAWBOperation
{
    public static AWB Execute(Package package)
    {
        if (package == null)
            throw new ArgumentNullException(nameof(package));
        
        return package.AWB;
    }

    public static AWB GenerateNewAWB()
    {
        return AWB.Generate();
    }
}

