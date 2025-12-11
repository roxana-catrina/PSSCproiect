using Proiect.Data.Services;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Models.Events;
using Proiect.Domain.Workflows;
using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Order;
using static Proiect.Domain.Models.Entities.Invoice;
using static Proiect.Domain.Models.Entities.Package;

namespace Proiect.Data.Services
{
    public interface IWorkflowOrchestrationService
    {
        Task<OrderPlacedEvent.IOrderPlacedEvent> ProcessOrderAsync(PlaceOrderCommand command);
        Task<InvoiceGeneratedEvent.IInvoiceGeneratedEvent> GenerateInvoiceAsync(GenerateInvoiceCommand command);
        Task<PackageShippedEvent.IPackageShippedEvent> ProcessShippingAsync(PickupPackageCommand command);
    }

    public class WorkflowOrchestrationService : IWorkflowOrchestrationService
    {
        private readonly IOrderStateService _orderStateService;
        private readonly IInvoiceStateService _invoiceStateService;
        private readonly IPackageStateService _packageStateService;

        public WorkflowOrchestrationService(
            IOrderStateService orderStateService,
            IInvoiceStateService invoiceStateService,
            IPackageStateService packageStateService)
        {
            _orderStateService = orderStateService;
            _invoiceStateService = invoiceStateService;
            _packageStateService = packageStateService;
        }

        public async Task<OrderPlacedEvent.IOrderPlacedEvent> ProcessOrderAsync(PlaceOrderCommand command)
        {
            // 1. Încarcă starea existentă din baza de date (dacă există)
            var existingOrder = await _orderStateService.LoadOrderAsync(command.OrderNumber ?? "");

            // 2. Implementează funcțiile de verificare folosind baza de date
            Func<string, int, bool> checkStockAvailability = (productName, quantity) =>
            {
                return _orderStateService.CheckStockAvailabilityAsync(productName, quantity).Result;
            };

            // Execute workflow with DB-backed functions
            Func<string> generateOrderNumber = () => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

            // 3. Execută workflow-ul
            var workflow = new OrderProcessingWorkflow();
            var result = workflow.Execute(command, checkStockAvailability, generateOrderNumber);

            // 4. Salvează rezultatul în baza de date
            // We need to reconstruct the order from the event to save it
            if (result is OrderPlacedEvent.OrderPlacedSucceededEvent succeeded)
            {
                // Reconstruct ConfirmedOrder from event data
                if (DeliveryAddress.TryParse(command.DeliveryStreet, command.DeliveryCity, 
                    command.DeliveryPostalCode, command.DeliveryCountry, out var address) && address != null)
                {
                    var items = command.Items.Select(i => 
                    {
                        if (int.TryParse(i.Quantity, out var qty) && decimal.TryParse(i.UnitPrice, out var price))
                        {
                            return new ValidatedOrderItem(i.ProductName, qty, new Price(price));
                        }
                        return null;
                    }).Where(i => i != null).Cast<ValidatedOrderItem>().ToList();

                    var confirmedOrder = new ConfirmedOrder(
                        succeeded.OrderNumber,
                        succeeded.CustomerName,
                        succeeded.CustomerEmail,
                        address,
                        items,
                        succeeded.TotalAmount,
                        succeeded.PlacedAt);

                    await _orderStateService.SaveOrderAsync(confirmedOrder);
                    
                    // Actualizează stocul pentru produsele comandate
                    foreach (var item in items)
                    {
                        await _orderStateService.UpdateStockAsync(item.ProductName, item.Quantity);
                    }
                }
            }

            return result;
        }

        public async Task<InvoiceGeneratedEvent.IInvoiceGeneratedEvent> GenerateInvoiceAsync(GenerateInvoiceCommand command)
        {
            // 1. Încarcă starea comenzii din baza de date
            var existingOrder = await _orderStateService.LoadOrderAsync(command.OrderNumber);
            if (existingOrder == null)
            {
                return new InvoiceGeneratedEvent.InvoiceGeneratedFailedEvent(new[] { "Order not found in database" });
            }

            // 2. Verifică dacă factura există deja
            var existingInvoice = await _invoiceStateService.LoadInvoiceAsync($"INV-{command.OrderNumber}");

            // 3. Implementează funcțiile necesare
            decimal vatRate = 0.19m; // 19% VAT
            Func<string> generateInvoiceNumber = () => $"INV-{command.OrderNumber}-{DateTime.UtcNow:yyyyMMdd}";

            // 4. Execută workflow-ul de facturare
            var workflow = new BillingWorkflow();
            var result = workflow.Execute(command, vatRate, generateInvoiceNumber);

            // 5. Salvează rezultatul în baza de date
            if (result is InvoiceGeneratedEvent.InvoiceGeneratedSucceededEvent succeeded &&
                OrderNumber.TryParse(command.OrderNumber, out var orderNum) && orderNum != null &&
                decimal.TryParse(command.TotalAmount, out var totalAmt))
            {
                var generatedInvoice = new GeneratedInvoice(
                    succeeded.InvoiceNumber,
                    orderNum,
                    succeeded.CustomerName,
                    new Price(totalAmt),
                    succeeded.VatAmount,
                    succeeded.TotalWithVat,
                    succeeded.GeneratedAt);

                await _invoiceStateService.SaveInvoiceAsync(generatedInvoice);
            }

            return result;
        }

        public async Task<PackageShippedEvent.IPackageShippedEvent> ProcessShippingAsync(PickupPackageCommand command)
        {
            // 1. Încarcă starea comenzii din baza de date
            var existingOrder = await _orderStateService.LoadOrderAsync(command.OrderNumber);
            if (existingOrder == null)
            {
                return new PackageShippedEvent.PackageShippedFailedEvent(new[] { "Order not found in database" });
            }

            // 2. Încarcă starea coletului din baza de date (dacă există)
            var existingPackage = await _packageStateService.LoadPackageAsync(command.OrderNumber);

            // 3. Implementează funcțiile necesare
            // Generate AWB with correct format: 2 letters + 10 digits (e.g., RO2512111430)
            Func<string> generateAwb = () => 
            {
                var timestamp = DateTime.UtcNow.ToString("yyMMddHHmm"); // 10 digits: year(2) + month(2) + day(2) + hour(2) + minute(2)
                return $"RO{timestamp}";
            };
            Func<string, bool> notifyCourier = (awbNumber) =>
            {
                // Simulare notificare curier
                return !string.IsNullOrEmpty(awbNumber);
            };

            // 4. Execută workflow-ul de expediere (fără getRecipientName)
            var workflow = new ShippingWorkflow();
            var result = workflow.Execute(command, generateAwb, notifyCourier);

            // 5. Salvează rezultatul în baza de date
            if (result is PackageShippedEvent.PackageShippedSucceededEvent succeeded &&
                OrderNumber.TryParse(command.OrderNumber, out var orderNum) && orderNum != null)
            {
                var shippedPackage = new ShippedPackage(
                    orderNum,
                    succeeded.DeliveryAddress,
                    succeeded.TrackingNumber,
                    succeeded.ShippedAt);

                await _packageStateService.SavePackageAsync(shippedPackage);
            }

            return result;
        }
    }
}
