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

        var invoice = new Invoice(
            command.OrderId,
            command.CustomerName,
            command.CustomerEmail,
            command.TotalAmount,
            vatAmount
        );

        await NotifyCustomerOperation.SendInvoice(command.CustomerEmail, invoice.InvoiceNumber);

        var invoiceEvent = new InvoiceGenerated(
            invoice.Id,
            invoice.OrderId,
            invoice.InvoiceNumber,
            invoice.TotalAmount,
            invoice.VATAmount,
            invoice.GeneratedAt
        );

        await Task.Delay(100);
        invoice.MarkAsPaid();

        var paymentEvent = new PaymentRecorded(
            Guid.NewGuid().ToString(),
            invoice.Id,
            invoice.OrderId,
            invoice.TotalAmount,
            paymentMethod,
            DateTime.UtcNow
        );

        return (invoiceEvent, paymentEvent);
    }
}

