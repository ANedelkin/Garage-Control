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
            if (part.Quantity < quantity)
                throw new Exception($"Insufficient stock for part '{part.Name}'");

            part.Quantity -= quantity;
            part.AvailabilityBalance -= quantity;
        }

        public async Task RevertPartChangeAsync(Part part, int quantity, JobStatus status)
        {
            part.Quantity += quantity;
            part.AvailabilityBalance += quantity;
        }

        public async Task HandleStatusTransitionAsync(IEnumerable<JobPart> parts, JobStatus oldStatus, JobStatus newStatus)
        {
            // Parts are now immediately subtracted from stock, no status transition needed
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
        public async Task<double> GetPartsToSendAsync(string partId)
        {
            // The user wants "sum of (planned - sent)" for the "Parts to send" field
            return await _context.JobParts
                .Where(jp => jp.PartId == partId && 
                             jp.Job.Status != JobStatus.Done &&
                             jp.PlannedQuantity > jp.SentQuantity)
                .SumAsync(jp => jp.PlannedQuantity - jp.SentQuantity);
        }

        public async Task RecalculateAvailabilityBalanceAsync(string workshopId, string? partId = null)
        {
            var partsQuery = _context.Parts.Where(p => p.WorkshopId == workshopId);
            if (!string.IsNullOrEmpty(partId)) partsQuery = partsQuery.Where(p => p.Id == partId);

            var parts = await partsQuery.ToListAsync();
            foreach (var part in parts)
            {
                var outstandingQty = await GetPartsToSendAsync(part.Id);
                part.AvailabilityBalance = part.Quantity - outstandingQty;
                await CheckLowStockAsync(workshopId, part);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Part?> GetPartByIdAsync(string partId)
        {
            return await _context.Parts.FindAsync(partId);
        }
    }
}