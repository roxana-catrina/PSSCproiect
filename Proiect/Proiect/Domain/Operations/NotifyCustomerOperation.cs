namespace Proiect.Domain.Operations;

public class NotifyCustomerOperation
{
    public static async Task SendOrderConfirmation(string customerEmail, string orderNumber)
    {
        await Task.Delay(100);
        Console.WriteLine($"Order confirmation sent to {customerEmail} for order {orderNumber}");
    }

    public static async Task SendInvoice(string customerEmail, string invoiceNumber)
    {
        await Task.Delay(100);
        Console.WriteLine($"Invoice {invoiceNumber} sent to {customerEmail}");
    }

    public static async Task SendShippingNotification(string customerEmail, string awb)
    {
        await Task.Delay(100);
        Console.WriteLine($"Shipping notification sent to {customerEmail}. AWB: {awb}");
    }

    public static async Task SendDeliveryConfirmation(string customerEmail, string orderNumber)
    {
        await Task.Delay(100);
        Console.WriteLine($"Delivery confirmation sent to {customerEmail} for order {orderNumber}");
    }
}

