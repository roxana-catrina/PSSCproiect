using Microsoft.EntityFrameworkCore;
using Proiect.Data.Models;
using Proiect.Domain.Models.ValueObjects;
using static Proiect.Domain.Models.Entities.Package;

namespace Proiect.Data.Services
{
    public interface IPackageStateService
    {
        Task<IPackage?> LoadPackageAsync(string orderNumber);
        Task SavePackageAsync(IPackage package);
        Task SavePackageFromEventAsync(string orderNumber, string trackingNumber, DateTime deliveredAt, string recipientName);
    }

    public class PackageStateService : IPackageStateService
    {
        private readonly ApplicationDbContext _context;

        public PackageStateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IPackage?> LoadPackageAsync(string orderNumber)
        {
            var dbPackage = await _context.Packages
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Order.OrderNumber == orderNumber);

            if (dbPackage == null) return null;

            // Use TryParse to create Value Objects since constructors are internal
            if (!OrderNumber.TryParse(dbPackage.Order.OrderNumber, out var orderNum) || orderNum == null)
                return null;

            if (!DeliveryAddress.TryParse(dbPackage.Order.DeliveryStreet, dbPackage.Order.DeliveryCity,
                dbPackage.Order.DeliveryPostalCode, dbPackage.Order.DeliveryCountry, out var deliveryAddress) || deliveryAddress == null)
                return null;

            return dbPackage.Status switch
            {
                "Delivered" when AWB.TryParse(dbPackage.AWBNumber ?? string.Empty, out var awb1) && awb1 != null => 
                    new DeliveredPackage(
                        orderNum,
                        awb1,
                        dbPackage.DeliveredAt ?? DateTime.UtcNow,
                        dbPackage.Order.CustomerName),
                
                "Shipped" when AWB.TryParse(dbPackage.AWBNumber ?? string.Empty, out var awb2) && awb2 != null =>
                    new ShippedPackage(
                        orderNum,
                        deliveryAddress,
                        awb2,
                        dbPackage.ShippedAt ?? DateTime.UtcNow),
                
                "Prepared" when AWB.TryParse(dbPackage.AWBNumber ?? string.Empty, out var awb3) && awb3 != null =>
                    new PreparedPackage(
                        orderNum,
                        deliveryAddress,
                        awb3,
                        dbPackage.CreatedAt),
                
                "Validated" => new ValidatedPackage(
                    orderNum,
                    deliveryAddress),
                
                _ => null
            };
        }

        public async Task SavePackageAsync(IPackage package)
        {
            switch (package)
            {
                case DeliveredPackage delivered:
                    await SaveDeliveredPackageAsync(delivered);
                    break;
                case ShippedPackage shipped:
                    await SaveShippedPackageAsync(shipped);
                    break;
                case PreparedPackage prepared:
                    await SavePreparedPackageAsync(prepared);
                    break;
                case ValidatedPackage validated:
                    await SaveValidatedPackageAsync(validated);
                    break;
            }
        }

        public async Task SavePackageFromEventAsync(string orderNumber, string trackingNumber, DateTime deliveredAt, string recipientName)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null) return;

            var existingPackage = await _context.Packages
                .FirstOrDefaultAsync(p => p.OrderId == order.Id);

            if (existingPackage != null)
            {
                existingPackage.Status = "Delivered";
                existingPackage.DeliveredAt = deliveredAt;
                existingPackage.AWBNumber = trackingNumber;
            }
            else
            {
                var newPackage = new Package
                {
                    OrderId = order.Id,
                    AWBNumber = trackingNumber,
                    Status = "Delivered",
                    DeliveredAt = deliveredAt
                };
                _context.Packages.Add(newPackage);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SaveDeliveredPackageAsync(DeliveredPackage delivered)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == delivered.OrderNumber.Value);

            if (order == null) return;

            var existingPackage = await _context.Packages
                .FirstOrDefaultAsync(p => p.OrderId == order.Id);

            if (existingPackage != null)
            {
                existingPackage.Status = "Delivered";
                existingPackage.DeliveredAt = delivered.DeliveredAt;
                existingPackage.AWBNumber = delivered.TrackingNumber.Value;
            }
            else
            {
                var newPackage = new Package
                {
                    OrderId = order.Id,
                    AWBNumber = delivered.TrackingNumber.Value,
                    Status = "Delivered",
                    DeliveredAt = delivered.DeliveredAt
                };
                _context.Packages.Add(newPackage);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SaveShippedPackageAsync(ShippedPackage shipped)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == shipped.OrderNumber.Value);

            if (order == null) return;

            var existingPackage = await _context.Packages
                .FirstOrDefaultAsync(p => p.OrderId == order.Id);

            if (existingPackage != null)
            {
                existingPackage.Status = "Shipped";
                existingPackage.ShippedAt = shipped.ShippedAt;
                existingPackage.AWBNumber = shipped.TrackingNumber.Value;
            }
            else
            {
                var newPackage = new Package
                {
                    OrderId = order.Id,
                    AWBNumber = shipped.TrackingNumber.Value,
                    Status = "Shipped",
                    ShippedAt = shipped.ShippedAt
                };
                _context.Packages.Add(newPackage);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SavePreparedPackageAsync(PreparedPackage prepared)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == prepared.OrderNumber.Value);

            if (order == null) return;

            var existingPackage = await _context.Packages
                .FirstOrDefaultAsync(p => p.OrderId == order.Id);

            if (existingPackage != null)
            {
                existingPackage.Status = "Prepared";
                existingPackage.AWBNumber = prepared.TrackingNumber.Value;
            }
            else
            {
                var newPackage = new Package
                {
                    OrderId = order.Id,
                    AWBNumber = prepared.TrackingNumber.Value,
                    Status = "Prepared"
                };
                _context.Packages.Add(newPackage);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SaveValidatedPackageAsync(ValidatedPackage validated)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == validated.OrderNumber.Value);

            if (order == null) return;

            var existingPackage = await _context.Packages
                .FirstOrDefaultAsync(p => p.OrderId == order.Id);

            if (existingPackage == null)
            {
                var newPackage = new Package
                {
                    OrderId = order.Id,
                    Status = "Validated"
                };
                _context.Packages.Add(newPackage);
                await _context.SaveChangesAsync();
            }
        }
    }
}
