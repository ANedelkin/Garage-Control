using GarageControl.Core.Contracts;
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
        private readonly IDeficitService _deficitService;

        public InventoryService(
            GarageControlDbContext context,
            INotificationService notification,
            IDeficitService deficitService)
        {
            _context = context;
            _notification = notification;
            _deficitService = deficitService;
        }

        public void SendParts(Part part, int quantity)
        {
            if (part.Quantity < quantity) throw new InvalidOperationException($"Insufficient stock for part '{part.Name}'");
            part.Quantity -= quantity;
            part.AvailabilityBalance -= quantity;
        }

        public void ReturnParts(Part part, int quantity)
        {
            part.Quantity += quantity;
        }

        public async Task<int> GetPartsToSendAsync(string partId)
        {
            return await _context.JobParts
                .Where(jp => jp.PartId == partId && jp.Job.Status != JobStatus.Done && jp.PlannedQuantity > jp.SentQuantity)
                .SumAsync(jp => jp.PlannedQuantity - jp.SentQuantity);
        }

        public async Task<Dictionary<string, int>> GetPartsToSendAsync(string workshopId, IEnumerable<string> partIds)
        {
            if (partIds == null || !partIds.Any()) return new Dictionary<string, int>();

            return await _context.JobParts
                .Where(jp => partIds.Contains(jp.PartId) && jp.Job.JobType.WorkshopId == workshopId && jp.Job.Status != JobStatus.Done && jp.PlannedQuantity > jp.SentQuantity)
                .GroupBy(jp => jp.PartId)
                .Select(g => new { PartId = g.Key, Outstanding = g.Sum(x => x.PlannedQuantity - x.SentQuantity) })
                .ToDictionaryAsync(x => x.PartId, x => x.Outstanding);
        }

        public async Task CheckLowStockAsync(string workshopId, Part part)
        {
            if (part.AvailabilityBalance < part.MinimumQuantity)
            {
                await _notification.SendStockNotificationAsync(workshopId, part.Id, part.Name, part.AvailabilityBalance, part.MinimumQuantity);
            }
        }

        public async Task RecalculateAvailabilityBalanceAsync(string workshopId, IEnumerable<string> partIds, int? oldMin = null)
        {
            var parts = await _context.Parts.Where(p => p.WorkshopId == workshopId && partIds.Contains(p.Id)).ToListAsync();
            if (!parts.Any()) return;

            var outstandingDict = await _context.JobParts
                .Where(jp => jp.Job.JobType.WorkshopId == workshopId
                             && jp.Job.Status != JobStatus.Done
                             && jp.PlannedQuantity > jp.SentQuantity
                             && partIds.Contains(jp.PartId))
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
                var outstandingQty = outstandingDict.GetValueOrDefault(part.Id);
                var newBalance = part.Quantity - outstandingQty;
                part.AvailabilityBalance = newBalance;

                int effectiveOldMin = oldMin ?? part.MinimumQuantity;
                bool wasLow = oldBalance < effectiveOldMin;
                bool isLow = newBalance < part.MinimumQuantity;

                if (!wasLow && isLow)
                {
                    await _notification.SendStockNotificationAsync(workshopId, part.Id, part.Name, newBalance, part.MinimumQuantity);
                }
                else if (wasLow && isLow && (oldBalance != newBalance || effectiveOldMin != part.MinimumQuantity))
                {
                    await _notification.SendStockNotificationAsync(workshopId, part.Id, part.Name, newBalance, part.MinimumQuantity);
                }
                else if (wasLow && !isLow)
                {
                    await _notification.RemoveStockNotificationAsync(workshopId, part.Id);
                }

                await _deficitService.UpdatePartDeficitStatusAsync(workshopId, part);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Part?> GetPartByIdAsync(string partId)
        {
            return await _context.Parts.FindAsync(partId);
        }
    }
}