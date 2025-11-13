namespace Proiect.Domain.Operations;

using Proiect.Domain.Models.Entities;

/// <summary>
/// Operation to send invoice notifications to customers based on invoice state
/// Single responsibility: Invoice notification sending
/// </summary>
public class NotifyInvoiceOperation : InvoiceOperation<Task>
{
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="notificationService">Optional notification service (uses console if null)</param>
    public NotifyInvoiceOperation(INotificationService? notificationService = null)
    {
        _notificationService = notificationService ?? new ConsoleNotificationService();
    }

    /// <summary>
    /// No notification for unvalidated invoices
    /// </summary>
    protected override Task OnUnvalidated(UnvalidatedInvoice invoice)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends invoice notification for an UnpaidInvoice
    /// </summary>
    protected override Task OnUnpaid(UnpaidInvoice invoice)
    {
        ValidateInvoice(invoice);
        return SendInvoiceNotificationAsync(invoice.CustomerEmail, invoice.InvoiceNumber);
    }

    /// <summary>
    /// Sends payment confirmation for a PaidInvoice
    /// </summary>
    protected override Task OnPaid(PaidInvoice invoice)
    {
        ValidateInvoice(invoice);
        return SendPaymentConfirmationAsync(invoice.CustomerEmail, invoice.InvoiceNumber);
    }

    /// <summary>
    /// No notification for invalid invoices
    /// </summary>
    protected override Task OnInvalid(InvalidInvoice invoice)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Private helper method to validate invoice is not null
    /// </summary>
    private static void ValidateInvoice(IInvoice invoice)
    {
        if (invoice == null)
            throw new ArgumentNullException(nameof(invoice));
    }

    /// <summary>
    /// Private helper method to send invoice notification
    /// </summary>
    private async Task SendInvoiceNotificationAsync(string customerEmail, string invoiceNumber)
    {
        await _notificationService.SendAsync(customerEmail, $"Invoice {invoiceNumber} has been generated");
    }

    /// <summary>
    /// Private helper method to send payment confirmation
    /// </summary>
    private async Task SendPaymentConfirmationAsync(string customerEmail, string invoiceNumber)
    {
        await _notificationService.SendAsync(customerEmail, $"Payment confirmed for invoice {invoiceNumber}");
    }
}

