using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageControl.Core.Contracts;
using GarageControl.Core.Services.Helpers;

namespace GarageControl.Core.Services
{
    public class FolderService : IFolderService
    {
        private readonly GarageControlDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IDeficitService _deficitService;
        private readonly PartActivityLogger _activityLogger;

        public FolderService(GarageControlDbContext context, IActivityLogService activityLogService, IInventoryService inventoryService, IDeficitService deficitService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _deficitService = deficitService;
            _activityLogger = new PartActivityLogger(activityLogService);
        }

        public async Task<FolderContentVM> GetFolderContentAsync(string workshopId, string? folderId)
        {
            var result = new FolderContentVM { CurrentFolderId = folderId };

            if (!string.IsNullOrEmpty(folderId))
            {
                var currentFolder = await _context.PartsFolders
                    .FirstOrDefaultAsync(f => f.Id == folderId && f.WorkshopId == workshopId);

                if (currentFolder != null)
                {
                    result.CurrentFolderName = currentFolder.Name;
                    result.ParentFolderId = currentFolder.ParentId;
                }
            }

            var foldersQuery = _context.PartsFolders.Where(f => f.WorkshopId == workshopId);
            var partsQuery = _context.Parts.Where(p => p.WorkshopId == workshopId);

            if (string.IsNullOrEmpty(folderId))
            {
                foldersQuery = foldersQuery.Where(f => f.ParentId == null);
                partsQuery = partsQuery.Where(p => p.ParentId == null);
            }
            else
            {
                foldersQuery = foldersQuery.Where(f => f.ParentId == folderId);
                partsQuery = partsQuery.Where(p => p.ParentId == folderId);
            }

            result.SubFolders = await foldersQuery
                .Select(f => new PartsFolderVM 
                { 
                    Id = f.Id, 
                    Name = f.Name, 
                    ParentId = f.ParentId,
                    LowerDeficitSeverityCount = f.LowerDeficitSeverityCount,
                    HigherDeficitSeverityCount = f.HigherDeficitSeverityCount
                })
                .ToListAsync();

            var parts = await partsQuery.ToListAsync();
            var partIds = parts.Select(p => p.Id).ToList();
            var partsToSendDict = await _inventoryService.GetPartsToSendAsync(workshopId, partIds);

            result.Parts = parts.Select(p => new PartVM
            {
                Id = p.Id,
                Name = p.Name,
                PartNumber = p.PartNumber,
                Price = p.Price,
                Quantity = p.Quantity,
                AvailabilityBalance = p.AvailabilityBalance,
                PartsToSend = partsToSendDict.GetValueOrDefault(p.Id),
                MinimumQuantity = p.MinimumQuantity,
                ParentId = p.ParentId,
                DeficitStatus = p.DeficitStatus
            }).ToList();

            return result;
        }

        public async Task<PartsFolderVM> CreateFolderAsync(string userId, string workshopId, CreateFolderVM model)
        {
            var folder = new PartsFolder
            {
                Name = model.Name,
                ParentId = model.ParentId,
                WorkshopId = workshopId
            };

            _context.PartsFolders.Add(folder);
            await _context.SaveChangesAsync();

            await _activityLogger.LogFolderCreatedAsync(userId, workshopId, folder.Name);

            return new PartsFolderVM { Id = folder.Id, Name = folder.Name, ParentId = folder.ParentId };
        }

        public async Task RenameFolderAsync(string userId, string workshopId, string folderId, string newName)
        {
            var folder = await _context.PartsFolders
                .FirstOrDefaultAsync(f => f.Id == folderId && f.WorkshopId == workshopId);

            if (folder == null) throw new ArgumentException("Folder not found");

            string oldName = folder.Name;
            folder.Name = newName;
            await _context.SaveChangesAsync();

            await _activityLogger.LogFolderRenamedAsync(userId, workshopId, oldName, newName);
        }

        public async Task DeleteFolderAsync(string userId, string workshopId, string folderId)
        {
            var folder = await _context.PartsFolders
                .Include(f => f.FolderChildren)
                .Include(f => f.PartsChildren)
                .FirstOrDefaultAsync(f => f.Id == folderId && f.WorkshopId == workshopId);

            if (folder == null) return;

            string folderName = folder.Name;
            string? parentId = folder.ParentId;
            
            await DeleteFolderRecursive(folder);
            await _context.SaveChangesAsync();
            
            // Recalculate parent folder's deficit counts
            if (!string.IsNullOrEmpty(parentId))
            {
                await _deficitService.RecalculateFolderDeficitCountsAsync(parentId);
            }

            await _activityLogger.LogFolderDeletedAsync(userId, workshopId, folderName);
        }

        private async Task DeleteFolderRecursive(PartsFolder folder)
        {
            var subFolders = await _context.PartsFolders.Where(f => f.ParentId == folder.Id).ToListAsync();
            foreach (var sub in subFolders)
            {
                await DeleteFolderRecursive(sub);
            }

            var parts = await _context.Parts.Where(p => p.ParentId == folder.Id).ToListAsync();
            _context.Parts.RemoveRange(parts);
            _context.PartsFolders.Remove(folder);
        }

        public async Task MoveFolderAsync(string userId, string workshopId, string folderId, string? newParentId)
        {
            var folder = await _context.PartsFolders.FirstOrDefaultAsync(f => f.Id == folderId && f.WorkshopId == workshopId);
            if (folder == null) throw new ArgumentException("Folder not found");
            if (folder.Id == newParentId) throw new ArgumentException("Cannot move folder into itself");

            var oldParentId = folder.ParentId;
            if (oldParentId == newParentId) return;

            if (newParentId != null)
            {
                var parent = await _context.PartsFolders.FirstOrDefaultAsync(f => f.Id == newParentId && f.WorkshopId == workshopId);
                if (parent == null) throw new ArgumentException("Target folder not found");
            }

            string oldParentName = await GetFolderNameOrBaseAsync(oldParentId);
            string newParentName = await GetFolderNameOrBaseAsync(newParentId);

            folder.ParentId = newParentId;
            await _context.SaveChangesAsync();
            
            // Recalculate deficit counts for old and new parents
            if (!string.IsNullOrEmpty(oldParentId))
            {
                await _deficitService.RecalculateFolderDeficitCountsAsync(oldParentId);
            }
            if (!string.IsNullOrEmpty(newParentId))
            {
                await _deficitService.RecalculateFolderDeficitCountsAsync(newParentId);
            }

            await _activityLogger.LogFolderMovedAsync(userId, workshopId, folder.Name, oldParentName, newParentName);
        }

        private async Task<string> GetFolderNameOrBaseAsync(string? folderId)
        {
            if (folderId == null) return "base";
            var folder = await _context.PartsFolders.FindAsync(folderId);
            return folder?.Name ?? "base";
        }
    }
}
