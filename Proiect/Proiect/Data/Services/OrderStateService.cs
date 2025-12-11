using Microsoft.EntityFrameworkCore;
using Proiect.Data.Models;
using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Order;

namespace Proiect.Data.Services
{
    public interface IOrderStateService
    {
        Task<IOrder?> LoadOrderAsync(string orderNumber);
        Task SaveOrderAsync(IOrder order);
        Task<bool> CheckProductExistsAsync(string productName);
        Task<bool> CheckStockAvailabilityAsync(string productName, int quantity);
        Task UpdateStockAsync(string productName, int quantityToDeduct);
        Task<OrderDto?> GetOrderByNumberAsync(string orderNumber);
    }

    public class OrderStateService : IOrderStateService
    {
        private readonly ApplicationDbContext _context;

        public OrderStateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IOrder?> LoadOrderAsync(string orderNumber)
        {
            var dbOrder = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (dbOrder == null) return null;

            // Use TryParse to create Value Objects since constructors are internal
            if (!OrderNumber.TryParse(dbOrder.OrderNumber, out var orderNum) || orderNum == null)
                return null;

            if (!DeliveryAddress.TryParse(dbOrder.DeliveryStreet, dbOrder.DeliveryCity, 
                dbOrder.DeliveryPostalCode, dbOrder.DeliveryCountry, out var address) || address == null)
                return null;

            return dbOrder.Status switch
            {
                "Confirmed" => new ConfirmedOrder(
                    orderNum,
                    dbOrder.CustomerName,
                    dbOrder.CustomerEmail,
                    address,
                    dbOrder.Items.Select(i => new ValidatedOrderItem(
                        i.Product.Name,
                        i.Quantity,
                        new Price(i.UnitPrice))).ToList(),
                    new Price(dbOrder.TotalAmount),
                    dbOrder.ConfirmedAt ?? DateTime.UtcNow),
                
                "Paid" => new PaidOrder(
                    orderNum,
                    dbOrder.CustomerName,
                    address,
                    dbOrder.Items.Select(i => new ValidatedOrderItem(
                        i.Product.Name,
                        i.Quantity,
                        new Price(i.UnitPrice))).ToList(),
                    new Price(dbOrder.TotalAmount),
                    dbOrder.PaidAt ?? DateTime.UtcNow),
                
                "Validated" => new ValidatedOrder(
                    dbOrder.CustomerName,
                    dbOrder.CustomerEmail,
                    address,
                    dbOrder.Items.Select(i => new ValidatedOrderItem(
                        i.Product.Name,
                        i.Quantity,
                        new Price(i.UnitPrice))).ToList()),
                
                _ => null
            };
        }

        public async Task SaveOrderAsync(IOrder order)
        {
            switch (order)
            {
                case ConfirmedOrder confirmed:
                    await SaveConfirmedOrderAsync(confirmed);
                    break;
                case PaidOrder paid:
                    await SavePaidOrderAsync(paid);
                    break;
                case ValidatedOrder validated:
                    await SaveValidatedOrderAsync(validated);
                    break;
            }
        }

        public async Task<bool> CheckProductExistsAsync(string productName)
        {
            return await _context.Products.AnyAsync(p => p.Name == productName);
        }

        public async Task<bool> CheckStockAvailabilityAsync(string productName, int quantity)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == productName);
            return product != null && product.StockQuantity >= quantity;
        }

        public async Task UpdateStockAsync(string productName, int quantityToDeduct)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == productName);
            if (product != null)
            {
                product.StockQuantity -= quantityToDeduct;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private async Task SaveConfirmedOrderAsync(ConfirmedOrder confirmed)
        {
            var existingOrder = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderNumber == confirmed.OrderNumber.Value);

            if (existingOrder != null)
            {
                existingOrder.Status = "Confirmed";
                existingOrder.ConfirmedAt = confirmed.ConfirmedAt;
                existingOrder.TotalAmount = confirmed.TotalAmount.Value;
            }
            else
            {
                var newOrder = new Order
                {
                    OrderNumber = confirmed.OrderNumber.Value,
                    CustomerName = confirmed.CustomerName,
                    CustomerEmail = confirmed.CustomerEmail,
                    DeliveryStreet = confirmed.DeliveryAddress.Street,
                    DeliveryCity = confirmed.DeliveryAddress.City,
                    DeliveryPostalCode = confirmed.DeliveryAddress.PostalCode,
                    DeliveryCountry = confirmed.DeliveryAddress.Country,
                    TotalAmount = confirmed.TotalAmount.Value,
                    Status = "Confirmed",
                    ConfirmedAt = confirmed.ConfirmedAt
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                // Add order items
                foreach (var item in confirmed.Items)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == item.ProductName);
                    if (product != null)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = newOrder.Id,
                            ProductId = product.Id,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice.Value,
                            LineTotal = item.Quantity * item.UnitPrice.Value
                        };
                        _context.OrderItems.Add(orderItem);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task SavePaidOrderAsync(PaidOrder paid)
        {
            var existingOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == paid.OrderNumber.Value);

            if (existingOrder != null)
            {
                existingOrder.Status = "Paid";
                existingOrder.PaidAt = paid.PaidAt;
                await _context.SaveChangesAsync();
            }
        }

        private Task SaveValidatedOrderAsync(ValidatedOrder validated)
        {
            // This would typically be used for saving intermediate state if needed
            // For now, we'll skip as validated orders usually transition quickly to confirmed
            return Task.CompletedTask;
        }

        public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null) return null;

            return new OrderDto
            {
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                DeliveryAddress = new DeliveryAddressDto
                {
                    Street = order.DeliveryStreet,
                    City = order.DeliveryCity,
                    PostalCode = order.DeliveryPostalCode,
                    Country = order.DeliveryCountry
                },
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                }).ToList(),
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ConfirmedAt = order.ConfirmedAt,
                PaidAt = order.PaidAt
            };
        }
    }

    public class OrderDto
    {
        public string OrderNumber { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public DeliveryAddressDto DeliveryAddress { get; set; } = null!;
        public List<OrderItemDto> Items { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class DeliveryAddressDto
    {
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!;
    }

    public class OrderItemDto
    {
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
