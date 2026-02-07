using GarageControl.Core.Contracts;
using GarageControl.Core.Services.Helpers;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GarageControl.Core.Services
{
    public class FolderService : IFolderService
    {
        private readonly GarageControlDbContext _context;
        private readonly PartActivityLogger _activityLogger;

        public FolderService(GarageControlDbContext context, IActivityLogService activityLogService)
        {
            _context = context;
            _activityLogger = new PartActivityLogger(activityLogService);
        }

        public async Task<FolderContentViewModel> GetFolderContentAsync(string workshopId, string? folderId)
        {
            var result = new FolderContentViewModel { CurrentFolderId = folderId };

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
                .Select(f => new PartsFolderViewModel { Id = f.Id, Name = f.Name, ParentId = f.ParentId })
                .ToListAsync();

            result.Parts = new List<PartViewModel>();
            foreach (var p in await partsQuery.ToListAsync())
            {
                result.Parts.Add(new PartViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    PartNumber = p.PartNumber,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    AvailabilityBalance = p.AvailabilityBalance,
                    MinimumQuantity = p.MinimumQuantity,
                    ParentId = p.ParentId
                });
            }

            return result;
        }

        public async Task<PartsFolderViewModel> CreateFolderAsync(string userId, string workshopId, CreateFolderViewModel model)
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

            return new PartsFolderViewModel { Id = folder.Id, Name = folder.Name, ParentId = folder.ParentId };
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
            await DeleteFolderRecursive(folder);
            await _context.SaveChangesAsync();

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
