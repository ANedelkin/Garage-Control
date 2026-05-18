using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Core.Services
{
    public class DeficitService : IDeficitService
    {
        private readonly GarageControlDbContext _context;

        public DeficitService(GarageControlDbContext context)
        {
            _context = context;
        }
        public DeficitStatus CalculatePartDeficitStatus(Part part)
        {
            if (part.AvailabilityBalance < 0 || part.Quantity < part.MinimumQuantity)
            {
                return DeficitStatus.HigherSeverity;
            }

            if (part.AvailabilityBalance < part.MinimumQuantity)
            {
                return DeficitStatus.LowerSeverity;
            }

            return DeficitStatus.NoDeficit;
        }

        public async Task UpdatePartDeficitStatusAsync(string workshopId, Part part)
        {
            var newStatus = CalculatePartDeficitStatus(part);
            var oldStatus = part.DeficitStatus;

            if (newStatus == oldStatus)
                return; 

            part.DeficitStatus = newStatus;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(part.ParentId))
            {
                await PropagateDeficitChangeToAncestorsAsync(part.ParentId, oldStatus, newStatus);
            }
        }

        public async Task RecalculateAllDeficitStatusesAsync(string workshopId)
        {
            var parts = await _context.Parts
                .Where(p => p.WorkshopId == workshopId)
                .ToListAsync();

            foreach (var part in parts)
            {
                part.DeficitStatus = CalculatePartDeficitStatus(part);
            }

            await _context.SaveChangesAsync();

            var folders = await _context.PartsFolders
                .Where(f => f.WorkshopId == workshopId)
                .ToListAsync();

            foreach (var folder in folders)
            {
                await RecalculateFolderDeficitCountsAsync(folder.Id);
            }
        }

        public async Task RecalculateFolderDeficitCountsAsync(string folderId)
        {
            var folder = await _context.PartsFolders
                .Include(f => f.PartsChildren)
                .Include(f => f.FolderChildren)
                .FirstOrDefaultAsync(f => f.Id == folderId);

            if (folder == null)
                return;

            int lowerCount = 0;
            int higherCount = 0;

            foreach (var part in folder.PartsChildren ?? new HashSet<Part>())
            {
                if (part.DeficitStatus == DeficitStatus.LowerSeverity)
                    lowerCount++;
                else if (part.DeficitStatus == DeficitStatus.HigherSeverity)
                    higherCount++;
            }

            foreach (var childFolder in folder.FolderChildren ?? new HashSet<PartsFolder>())
            {
                lowerCount += childFolder.LowerDeficitSeverityCount;
                higherCount += childFolder.HigherDeficitSeverityCount;
            }

            var oldLowerCount = folder.LowerDeficitSeverityCount;
            var oldHigherCount = folder.HigherDeficitSeverityCount;

            folder.LowerDeficitSeverityCount = lowerCount;
            folder.HigherDeficitSeverityCount = higherCount;

            await _context.SaveChangesAsync();

            if ((oldLowerCount != lowerCount || oldHigherCount != higherCount) && !string.IsNullOrEmpty(folder.ParentId))
            {
                await RecalculateFolderDeficitCountsAsync(folder.ParentId);
            }
        }

        private async Task PropagateDeficitChangeToAncestorsAsync(string folderId, DeficitStatus oldStatus, DeficitStatus newStatus)
        {
            var folder = await _context.PartsFolders.FirstOrDefaultAsync(f => f.Id == folderId);
            if (folder == null)
                return;

            if (oldStatus == DeficitStatus.LowerSeverity)
                folder.LowerDeficitSeverityCount--;
            else if (oldStatus == DeficitStatus.HigherSeverity)
                folder.HigherDeficitSeverityCount--;

            if (newStatus == DeficitStatus.LowerSeverity)
                folder.LowerDeficitSeverityCount++;
            else if (newStatus == DeficitStatus.HigherSeverity)
                folder.HigherDeficitSeverityCount++;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(folder.ParentId))
            {
                await PropagateDeficitChangeToAncestorsAsync(folder.ParentId, oldStatus, newStatus);
            }
        }
    }
}
