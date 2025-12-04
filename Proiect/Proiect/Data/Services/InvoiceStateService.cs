using Microsoft.EntityFrameworkCore;
using Proiect.Data.Models;
using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Invoice;

namespace Proiect.Data.Services
{
    public interface IInvoiceStateService
    {
        Task<IInvoice?> LoadInvoiceAsync(string invoiceNumber);
        Task SaveInvoiceAsync(IInvoice invoice);
    }

    public class InvoiceStateService : IInvoiceStateService
    {
        private readonly ApplicationDbContext _context;

        public InvoiceStateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IInvoice?> LoadInvoiceAsync(string invoiceNumber)
        {
            var dbInvoice = await _context.Invoices
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);

            if (dbInvoice == null) return null;

            // Use TryParse to create Value Objects since constructors are internal
            if (!OrderNumber.TryParse(dbInvoice.Order.OrderNumber, out var orderNum) || orderNum == null)
                return null;

            return dbInvoice.Status switch
            {
                "Generated" => new GeneratedInvoice(
                    dbInvoice.InvoiceNumber,
                    orderNum,
                    dbInvoice.Order.CustomerName,
                    new Price(dbInvoice.SubtotalAmount),
                    new Price(dbInvoice.VatAmount),
                    new Price(dbInvoice.TotalWithVat),
                    dbInvoice.GeneratedAt ?? DateTime.UtcNow),
                
                "Validated" => new ValidatedInvoice(
                    orderNum,
                    dbInvoice.Order.CustomerName,
                    new Price(dbInvoice.SubtotalAmount)),
                
                _ => null
            };
        }

        public async Task SaveInvoiceAsync(IInvoice invoice)
        {
            switch (invoice)
            {
                case GeneratedInvoice generated:
                    await SaveGeneratedInvoiceAsync(generated);
                    break;
                case ValidatedInvoice validated:
                    await SaveValidatedInvoiceAsync(validated);
                    break;
            }
        }

        private async Task SaveGeneratedInvoiceAsync(GeneratedInvoice generated)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == generated.OrderNumber.Value);

            if (order == null) return;

            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceNumber == generated.InvoiceNumber);

            if (existingInvoice != null)
            {
                existingInvoice.Status = "Generated";
                existingInvoice.GeneratedAt = generated.GeneratedAt;
                existingInvoice.VatAmount = generated.VatAmount.Value;
                existingInvoice.TotalWithVat = generated.TotalWithVat.Value;
            }
            else
            {
                var newInvoice = new Invoice
                {
                    InvoiceNumber = generated.InvoiceNumber,
                    OrderId = order.Id,
                    SubtotalAmount = generated.TotalAmount.Value,
                    VatAmount = generated.VatAmount.Value,
                    TotalWithVat = generated.TotalWithVat.Value,
                    Status = "Generated",
                    GeneratedAt = generated.GeneratedAt
                };
                _context.Invoices.Add(newInvoice);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SaveValidatedInvoiceAsync(ValidatedInvoice validated)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == validated.OrderNumber.Value);

            if (order == null) return;

            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.OrderId == order.Id);

            if (existingInvoice == null)
            {
                var newInvoice = new Invoice
                {
                    InvoiceNumber = string.Empty, // Will be set when generated
                    OrderId = order.Id,
                    SubtotalAmount = validated.TotalAmount.Value,
                    VatAmount = 0,
                    TotalWithVat = 0,
                    Status = "Validated"
                };
                _context.Invoices.Add(newInvoice);
                await _context.SaveChangesAsync();
            }
        }
    }
}
