namespace Proiect.Domain.Models.Events
{
  public record PackagePrepared(
      string PackageId,
      string OrderId,
      string AWB,
      DateTime PreparedAt
  );
}

