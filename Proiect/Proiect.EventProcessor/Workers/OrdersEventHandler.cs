using Proiect.Data.Services;
using Proiect.Domain.Models.Commands;
using Proiect.Domain.Workflows;
using static Proiect.Domain.Models.Events.OrderPlacedEvent;
using Proiect.Domain.Models.Events;
using Proiect.Messaging.Events;
using Proiect.Messaging.Events.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Proiect.EventProcessor.Workers;

/// <summary>
/// Event handler for processing order commands
/// Executes OrderProcessingWorkflow and publishes events
/// </summary>
internal class OrdersEventHandler : AbstractEventHandler<PlaceOrderCommand>
{
    private readonly IServiceProvider _serviceProvider;

    public OrdersEventHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override string[] EventTypes => new[] { typeof(PlaceOrderCommand).Name };

    protected override async Task<EventProcessingResult> OnHandleAsync(PlaceOrderCommand command)
    {
        using var scope = _serviceProvider.CreateScope();
        var workflow = scope.ServiceProvider.GetRequiredService<OrderProcessingWorkflow>();
        var orderStateService = scope.ServiceProvider.GetRequiredService<IOrderStateService>();
        var eventSender = scope.ServiceProvider.GetRequiredService<IEventSender>();

        // Execute workflow
        var workflowResult = workflow.Execute(
            command,
            checkStockAvailability: (productName, quantity) =>
                orderStateService.CheckStockAvailabilityAsync(productName, quantity).Result,
            generateOrderNumber: () => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}"
        );

        // Handle result and publish event
        if (workflowResult is OrderPlacedSucceededEvent successEvent)
        {
            await eventSender.SendAsync("order-events", new OrderPlacedDto
            {
                OrderNumber = successEvent.OrderNumber.Value,
                CustomerName = successEvent.CustomerName,
                CustomerEmail = successEvent.CustomerEmail,
                TotalAmount = successEvent.TotalAmount.Value,
                PlacedAt = successEvent.PlacedAt,
                DeliveryAddress = new DeliveryAddressDto
                {
                    Street = successEvent.DeliveryStreet,
                    City = successEvent.DeliveryCity,
                    PostalCode = successEvent.DeliveryPostalCode,
                    Country = successEvent.DeliveryCountry
                }
            });

            Console.WriteLine($"Order {successEvent.OrderNumber.Value} processed successfully");
            return EventProcessingResult.Completed;
        }
        else if (workflowResult is OrderPlacedFailedEvent failedEvent)
        {
            Console.WriteLine($"Order processing failed: {string.Join(", ", failedEvent.Reasons)}");
            return EventProcessingResult.Failed;
        }

        return EventProcessingResult.Completed;
    }
}
