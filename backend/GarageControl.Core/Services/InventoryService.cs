using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Core.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly GarageControlDbContext _context;
        private readonly INotificationService _notification;

        public InventoryService(
            GarageControlDbContext context,
            INotificationService notification)
        {
            _context = context;
            _notification = notification;
        }

        // No async needed because no awaits
        public void SendParts(Part part, int quantity)
        {
            if (part.Quantity < quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for part '{part.Name}'");

            part.Quantity -= quantity;
            part.AvailabilityBalance -= quantity;
        }

        // No async needed because no awaits
        public void ReturnParts(Part part, int quantity)
        {
            part.Quantity += quantity;
            part.AvailabilityBalance += quantity;
        }

        public async Task<double> GetPartsToSendAsync(string partId)
        {
            // Sum of (planned - sent)
            return await _context.JobParts
                .Where(jp =>
                    jp.PartId == partId &&
                    jp.Job.Status != JobStatus.Done &&
                    jp.PlannedQuantity > jp.SentQuantity)
                .SumAsync(jp => jp.PlannedQuantity - jp.SentQuantity);
        }
        public async Task CheckLowStockAsync(string workshopId, Part part)
        {
            if (part.AvailabilityBalance < part.MinimumQuantity)
            {
                await _notification.SendStockNotificationAsync(
                    workshopId,
                    part.Id,
                    part.Name,
                    part.AvailabilityBalance,
                    part.MinimumQuantity);
            }
        }
        public async Task RecalculateAvailabilityBalanceAsync(
            string workshopId,
            string? partId = null)
        {
            var partsQuery = _context.Parts
                .Where(p => p.WorkshopId == workshopId);

            if (!string.IsNullOrEmpty(partId))
                partsQuery = partsQuery.Where(p => p.Id == partId);

            var parts = await partsQuery.ToListAsync();

            if (!parts.Any())
                return;

            // Prevent N+1 queries by fetching all outstanding quantities at once
            var outstandingDict = await _context.JobParts
                .Where(jp =>
                    jp.Job.Order.Car.Owner.WorkshopId == workshopId &&
                    jp.Job.Status != JobStatus.Done &&
                    jp.PlannedQuantity > jp.SentQuantity)
                .GroupBy(jp => jp.PartId)
                .Select(g => new
                {
                    PartId = g.Key,
                    Outstanding = g.Sum(x => x.PlannedQuantity - x.SentQuantity)
                })
                .ToDictionaryAsync(x => x.PartId, x => x.Outstanding);

            foreach (var part in parts)
            {
                var oldBalance = part.AvailabilityBalance;

                var outstandingQty =
                    outstandingDict.GetValueOrDefault(part.Id);

                var newBalance = part.Quantity - outstandingQty;

                part.AvailabilityBalance = newBalance;

                // Notify ONLY when crossing threshold (avoid spam)
                bool wasLow = oldBalance < part.MinimumQuantity;
                bool isLow = newBalance < part.MinimumQuantity;

                if (!wasLow && isLow)
                {
                    await _notification.SendStockNotificationAsync(
                        workshopId,
                        part.Id,
                        part.Name,
                        newBalance,
                        part.MinimumQuantity);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Part?> GetPartByIdAsync(string partId)
        {
            return await _context.Parts.FindAsync(partId);
        }
    }
}
