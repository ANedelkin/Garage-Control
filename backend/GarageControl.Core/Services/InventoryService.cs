using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels.Orders;
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

        public InventoryService(GarageControlDbContext context, INotificationService notification)
        {
            _context = context;
            _notification = notification;
        }

        public async Task ApplyPartChangeAsync(Part part, int quantity, JobStatus status)
        {
            if (status == JobStatus.AwaitingParts)
            {
                part.AvailabilityBalance -= quantity;
            }
            else
            {
                if (part.Quantity < quantity)
                    throw new Exception($"Insufficient stock for part '{part.Name}'");

                part.Quantity -= quantity;
                part.AvailabilityBalance -= quantity;
            }
        }

        public async Task RevertPartChangeAsync(Part part, int quantity, JobStatus status)
        {
            if (status == JobStatus.AwaitingParts)
            {
                part.AvailabilityBalance += quantity;
            }
            else
            {
                part.Quantity += quantity;
                part.AvailabilityBalance += quantity;
            }
        }

        public async Task HandleStatusTransitionAsync(IEnumerable<JobPart> parts, JobStatus oldStatus, JobStatus newStatus)
        {
            if (oldStatus == JobStatus.AwaitingParts && newStatus != JobStatus.AwaitingParts)
            {
                foreach (var jp in parts)
                {
                    if (jp.Part.Quantity < jp.Quantity)
                        throw new Exception($"Insufficient stock for part '{jp.Part.Name}'");

                    jp.Part.Quantity -= jp.Quantity;
                }
            }
            else if (oldStatus != JobStatus.AwaitingParts && newStatus == JobStatus.AwaitingParts)
            {
                foreach (var jp in parts)
                {
                    jp.Part.Quantity += jp.Quantity;
                }
            }
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
        public async Task<int> GetPartsReservedAsync(string partId)
        {
            return await _context.JobParts
                .Where(jp => jp.PartId == partId && jp.Job.Status == JobStatus.AwaitingParts)
                .SumAsync(jp => jp.Quantity);
        }

        public async Task RecalculateAvailabilityBalanceAsync(string workshopId, string? partId = null)
        {
            var partsQuery = _context.Parts.Where(p => p.WorkshopId == workshopId);
            if (!string.IsNullOrEmpty(partId)) partsQuery = partsQuery.Where(p => p.Id == partId);

            var parts = await partsQuery.ToListAsync();
            foreach (var part in parts)
            {
                var reservedQty = await GetPartsReservedAsync(part.Id);
                part.AvailabilityBalance = part.Quantity - reservedQty;
                await CheckLowStockAsync(workshopId, part);
            }

            await _context.SaveChangesAsync();
        }
    }
}