namespace Proiect.Domain.Models.Commands;

public class PickupPackageCommand
{
    public PickupPackageCommand(string orderId, string awb, DateTime pickupDate)
    {
        OrderId = orderId;
        AWB = awb;
        PickupDate = pickupDate;
    }

    public string OrderId { get; }
    public string AWB { get; }
    public DateTime PickupDate { get; }
}
