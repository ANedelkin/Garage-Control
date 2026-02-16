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

        /// <summary>
        /// Calculates deficit status based on availability balance and minimum quantity.
        /// HigherSeverity: Negative availability balance OR stockpiled parts under minimum
        /// LowerSeverity: Availability balance under minimum (but >= 0)
        /// NoDeficit: All other cases
        /// </summary>
        public DeficitStatus CalculatePartDeficitStatus(Part part)
        {
            // HigherSeverity: Negative availability balance OR stockpiled parts under minimum
            if (part.AvailabilityBalance < 0 || part.Quantity < part.MinimumQuantity)
            {
                return DeficitStatus.HigherSeverity;
            }

            // LowerSeverity: AvailabilityBalance is under minimum but >= 0
            if (part.AvailabilityBalance < part.MinimumQuantity)
            {
                return DeficitStatus.LowerSeverity;
            }

            // NoDeficit: All good
            return DeficitStatus.NoDeficit;
        }

        /// <summary>
        /// Updates a part's deficit status and propagates changes up the folder hierarchy.
        /// </summary>
        public async Task UpdatePartDeficitStatusAsync(string workshopId, Part part)
        {
            var newStatus = CalculatePartDeficitStatus(part);
            var oldStatus = part.DeficitStatus;

            if (newStatus == oldStatus)
                return; // No change needed

            part.DeficitStatus = newStatus;
            await _context.SaveChangesAsync();

            // Propagate changes up to ancestors
            if (!string.IsNullOrEmpty(part.ParentId))
            {
                await PropagateDeficitChangeToAncestorsAsync(part.ParentId, oldStatus, newStatus);
            }
        }

        /// <summary>
        /// Recalculates all deficit statuses for parts in a workshop.
        /// </summary>
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

            // Recalculate all folder deficit counts
            var folders = await _context.PartsFolders
                .Where(f => f.WorkshopId == workshopId)
                .ToListAsync();

            foreach (var folder in folders)
            {
                await RecalculateFolderDeficitCountsAsync(folder.Id);
            }
        }

        /// <summary>
        /// Recalculates deficit counts for a folder based on its children and propagates upward.
        /// </summary>
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

            // Count direct part children by deficit status
            foreach (var part in folder.PartsChildren ?? new HashSet<Part>())
            {
                if (part.DeficitStatus == DeficitStatus.LowerSeverity)
                    lowerCount++;
                else if (part.DeficitStatus == DeficitStatus.HigherSeverity)
                    higherCount++;
            }

            // Count deficit statuses from folder children
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

            // Propagate upward if counts changed
            if ((oldLowerCount != lowerCount || oldHigherCount != higherCount) && !string.IsNullOrEmpty(folder.ParentId))
            {
                await RecalculateFolderDeficitCountsAsync(folder.ParentId);
            }
        }

        /// <summary>
        /// Propagates deficit status changes from a part up through its folder ancestors.
        /// </summary>
        private async Task PropagateDeficitChangeToAncestorsAsync(string folderId, DeficitStatus oldStatus, DeficitStatus newStatus)
        {
            var folder = await _context.PartsFolders.FirstOrDefaultAsync(f => f.Id == folderId);
            if (folder == null)
                return;

            // Update counts based on old and new status
            if (oldStatus == DeficitStatus.LowerSeverity)
                folder.LowerDeficitSeverityCount--;
            else if (oldStatus == DeficitStatus.HigherSeverity)
                folder.HigherDeficitSeverityCount--;

            if (newStatus == DeficitStatus.LowerSeverity)
                folder.LowerDeficitSeverityCount++;
            else if (newStatus == DeficitStatus.HigherSeverity)
                folder.HigherDeficitSeverityCount++;

            await _context.SaveChangesAsync();

            // Continue propagating upward
            if (!string.IsNullOrEmpty(folder.ParentId))
            {
                await PropagateDeficitChangeToAncestorsAsync(folder.ParentId, oldStatus, newStatus);
            }
        }
    }
}
