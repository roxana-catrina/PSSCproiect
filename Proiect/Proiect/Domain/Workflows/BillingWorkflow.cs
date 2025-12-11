using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Operations;
using static Proiect.Domain.Models.Entities.Invoice;

namespace Proiect.Domain.Workflows;

/// <summary>
/// Workflow for generating invoices
/// Orchestrates invoice validation, VAT calculation, and invoice generation
/// </summary>
public class BillingWorkflow
{
    /// <summary>
    /// Executes the billing workflow
    /// </summary>
    /// <param name="command">The generate invoice command with raw input data</param>
    /// <param name="vatRate">The VAT rate to apply (e.g., 0.19 for 19%)</param>
    /// <param name="generateInvoiceNumber">Dependency to generate unique invoice number</param>
    /// <returns>Invoice generated event (success or failure)</returns>
    public InvoiceGeneratedEvent.IInvoiceGeneratedEvent Execute(
        GenerateInvoiceCommand command,
        decimal vatRate,
        Func<string> generateInvoiceNumber)
    {
        // Step 1: Create unvalidated invoice from command
        var unvalidated = new UnvalidatedInvoice(
            command.OrderNumber,
            command.CustomerName,
            command.TotalAmount);
        
        // Step 2: Chain operations to transform invoice through states
        IInvoice result = unvalidated;
        result = new ValidateInvoiceDataOperation().Transform(result);
        result = new CalculateVATOperation(vatRate, generateInvoiceNumber).Transform(result);
        
        // Step 3: Convert final state to event
        return result.ToEvent();
    }
}
