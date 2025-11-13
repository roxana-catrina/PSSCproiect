namespace Proiect.Domain.Workflows;

using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Entities;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Operations;

public class BillingWorkflow
{
    public async Task<(InvoiceGenerated InvoiceEvent, PaymentRecorded PaymentEvent)> ExecuteAsync(
        GenerateInvoiceCommand command, 
        string paymentMethod = "CreditCard")
    {
        var vatAmount = CalculateVATOperation.Execute(command.TotalAmount);
        
        // Create invoice items from command
        var invoiceItems = new List<InvoiceItem>().AsReadOnly();

        var unpaidInvoice = new UnpaidInvoice(
            command.OrderId,
            command.CustomerName,
            command.CustomerEmail,
            invoiceItems
        );

        await NotifyCustomerOperation.SendInvoice(command.CustomerEmail, unpaidInvoice.InvoiceNumber);

        var invoiceEvent = new InvoiceGenerated(
            unpaidInvoice.Id,
            unpaidInvoice.OrderId,
            unpaidInvoice.InvoiceNumber,
            unpaidInvoice.TotalAmount,
            unpaidInvoice.VatAmount,
            unpaidInvoice.GeneratedAt
        );

        await Task.Delay(100);
        var paidInvoice = new PaidInvoice(unpaidInvoice);

        var paymentEvent = new PaymentRecorded(
            Guid.NewGuid().ToString(),
            paidInvoice.Id,
            paidInvoice.OrderId,
            paidInvoice.TotalAmount,
            paymentMethod,
            DateTime.UtcNow
        );

        return (invoiceEvent, paymentEvent);
    }
}
