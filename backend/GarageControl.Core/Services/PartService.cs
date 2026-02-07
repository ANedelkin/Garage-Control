using System.Globalization;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Core.Services
{
    public class PartService : IPartService
    {
        private readonly GarageControlDbContext _context;
        private readonly IActivityLogService _activityLogService;
        private readonly INotificationService _notificationService;

        public PartService(GarageControlDbContext context, IActivityLogService activityLogService, INotificationService notificationService)
        {
            _context = context;
            _activityLogService = activityLogService;
            _notificationService = notificationService;
        }

        public async Task<FolderContentViewModel> GetFolderContentAsync(string workshopId, string? folderId)
        {
            var result = new FolderContentViewModel
            {
                CurrentFolderId = folderId
            };

            if (!string.IsNullOrEmpty(folderId))
            {
                var currentFolder = await _context.PartsFolders
                    .FirstOrDefaultAsync(f => f.Id == folderId && f.WorkshopId == workshopId);
                
                if (currentFolder == null)
                {
                    // Or throw exception
                    return result; 
                }
                result.CurrentFolderName = currentFolder.Name;
                result.ParentFolderId = currentFolder.ParentId;
            }

            var foldersQuery = _context.PartsFolders
                .Where(f => f.WorkshopId == workshopId);
            
            var partsQuery = _context.Parts
                .Where(p => p.WorkshopId == workshopId);

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
                .Select(f => new PartsFolderViewModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    ParentId = f.ParentId
                })
                .ToListAsync();

            var parts = await partsQuery.ToListAsync();
            var partViewModels = new List<PartViewModel>();
            
            foreach (var p in parts)
            {
                var partsReserved = await GetPartsReservedAsync(p.Id);
                partViewModels.Add(new PartViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    PartNumber = p.PartNumber,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    AvailabilityBalance = p.AvailabilityBalance,
                    PartsReserved = partsReserved,
                    MinimumQuantity = p.MinimumQuantity,
                    ParentId = p.ParentId
                });
            }
            
            result.Parts = partViewModels;

            return result;
        }

        public async Task<List<PartViewModel>> GetAllPartsAsync(string workshopId)
        {
            var parts = await _context.Parts
                .Where(p => p.WorkshopId == workshopId)
                .ToListAsync();
            
            var result = new List<PartViewModel>();
            foreach (var p in parts)
            {
                var partsReserved = await GetPartsReservedAsync(p.Id);
                result.Add(new PartViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    PartNumber = p.PartNumber,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    AvailabilityBalance = p.AvailabilityBalance,
                    PartsReserved = partsReserved,
                    MinimumQuantity = p.MinimumQuantity,
                    ParentId = p.ParentId
                });
            }
            
            return result;
        }

        public async Task<PartViewModel> CreatePartAsync(string userId, string workshopId, CreatePartViewModel model)
        {
            var part = new Part
            {
                Name = model.Name,
                PartNumber = model.PartNumber,
                Price = model.Price,
                Quantity = model.Quantity,
                MinimumQuantity = model.MinimumQuantity,
                ParentId = model.ParentId,
                WorkshopId = workshopId,
                AvailabilityBalance = model.Quantity
            };

            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            // If new part is already below minimum, send notification
            if (part.AvailabilityBalance < part.MinimumQuantity)
            {
                await _notificationService.SendStockNotificationAsync(workshopId, part.Id, part.Name, part.AvailabilityBalance, part.MinimumQuantity);
            }

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"created part <a href='/parts?partId={part.Id}' class='log-link target-link'>{part.Name}</a>");

            return new PartViewModel
            {
                Id = part.Id,
                Name = part.Name,
                PartNumber = part.PartNumber,
                Price = part.Price,
                Quantity = part.Quantity,
                AvailabilityBalance = part.AvailabilityBalance,
                PartsReserved = 0,
                MinimumQuantity = part.MinimumQuantity,
                ParentId = part.ParentId
            };
        }

        public async Task<PartWithPathViewModel?> GetPartAsync(string workshopId, string partId)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == partId && p.WorkshopId == workshopId);
            
            if (part == null) return null;

            var partsReserved = await GetPartsReservedAsync(part.Id);
            var result = new PartWithPathViewModel
            {
                Id = part.Id,
                Name = part.Name,
                PartNumber = part.PartNumber,
                Price = part.Price,
                Quantity = part.Quantity,
                AvailabilityBalance = part.AvailabilityBalance,
                PartsReserved = partsReserved,
                MinimumQuantity = part.MinimumQuantity,
                ParentId = part.ParentId,
                Path = new List<string>()
            };

            // Calculate path
            var currentParentId = part.ParentId;
            while (!string.IsNullOrEmpty(currentParentId))
            {
                result.Path.Insert(0, currentParentId);
                var parent = await _context.PartsFolders
                    .FirstOrDefaultAsync(f => f.Id == currentParentId);
                currentParentId = parent?.ParentId;
            }

            return result;
        }

        public async Task EditPartAsync(string userId, string workshopId, UpdatePartViewModel model)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == model.Id && p.WorkshopId == workshopId);

            if (part == null) throw new ArgumentException("Part not found");

            var changes = new List<string>();
            string FormatPrice(decimal p) => p.ToString("0.00", CultureInfo.InvariantCulture);
            bool NumbersEqual(decimal? n1, decimal? n2) => (n1 ?? 0) == (n2 ?? 0);

            void TrackChange(string fieldName, object? oldValue, object? newValue)
            {
                if (oldValue is decimal oldNum && newValue is decimal newNum)
                {
                    if (!NumbersEqual(oldNum, newNum))
                    {
                        changes.Add($"{fieldName} from <b>{FormatPrice(oldNum)}</b> to <b>{FormatPrice(newNum)}</b>");
                    }
                    return;
                }

                string oldStr = oldValue?.ToString() ?? "";
                string newStr = newValue?.ToString() ?? "";
                if (oldStr != newStr)
                {
                    string oldDisp = string.IsNullOrEmpty(oldStr) ? "[empty]" : oldStr;
                    string newDisp = string.IsNullOrEmpty(newStr) ? "[empty]" : newStr;
                    
                    if (oldDisp.Length > 100 || newDisp.Length > 100)
                    {
                        changes.Add(fieldName);
                    }
                    else
                    {
                        changes.Add($"{fieldName} from <b>{oldDisp}</b> to <b>{newDisp}</b>");
                    }
                }
            }

            TrackChange("name", part.Name, model.Name);
            TrackChange("part number", part.PartNumber, model.PartNumber);
            TrackChange("price", part.Price, model.Price);
            TrackChange("quantity", (decimal)part.Quantity, (decimal)model.Quantity);
            TrackChange("minimum quantity", (decimal)part.MinimumQuantity, (decimal)model.MinimumQuantity);

            part.Name = model.Name;
            part.PartNumber = model.PartNumber;
            part.Price = model.Price;
            part.Quantity = model.Quantity;
            part.MinimumQuantity = model.MinimumQuantity;

            await _context.SaveChangesAsync();

            // Recalculate availability balance for this part and notify if needed
            await RecalculateAvailabilityBalanceAsync(workshopId, part.Id);

            if (changes.Count > 0)
            {
                string partLink = $"<a href='/parts?partId={part.Id}' class='log-link target-link'>{part.Name}</a>";
                string actionHtml;

                if (changes.Count == 1 && changes[0].Contains("from"))
                {
                    actionHtml = $"changed {changes[0]} of part {partLink}";
                }
                else if (changes.All(c => !c.Contains("from")))
                {
                    actionHtml = $"updated details of part {partLink}";
                }
                else
                {
                    actionHtml = $"updated part {partLink}: {string.Join(", ", changes)}";
                }

                await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
            }
        }

        public async Task DeletePartAsync(string userId, string workshopId, string partId)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == partId && p.WorkshopId == workshopId);

            if (part != null)
            {
                string partName = part.Name;
                _context.Parts.Remove(part);
                await _context.SaveChangesAsync();

                await _activityLogService.LogActionAsync(
                    userId,
                    workshopId,
                    $"deleted part <b>{partName}</b>");
            }
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

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"created group of parts <b>{folder.Name}</b>");

            return new PartsFolderViewModel
            {
                Id = folder.Id,
                Name = folder.Name,
                ParentId = folder.ParentId
            };
        }

        public async Task RenameFolderAsync(string userId, string workshopId, string folderId, string newName)
        {
             var folder = await _context.PartsFolders
                .FirstOrDefaultAsync(f => f.Id == folderId && f.WorkshopId == workshopId);

            if (folder == null) throw new ArgumentException("Folder not found");

            string oldName = folder.Name;
            folder.Name = newName;
            await _context.SaveChangesAsync();

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"renamed group of parts <b>{oldName}</b> to <b>{newName}</b>");
        }

        public async Task DeleteFolderAsync(string userId, string workshopId, string folderId)
        {
            // Recursive delete
            var folder = await _context.PartsFolders
                .Include(f => f.FolderChildren)
                .Include(f => f.PartsChildren)
                .FirstOrDefaultAsync(f => f.Id == folderId && f.WorkshopId == workshopId);
            
            if (folder == null) return;

            // Load all descendants recursively
            // Since EF Core doesn't support recursive delete configuration easily without loading,
            // or setting up cascade delete in DB. For creating logic here, I will load children manually if needed.
            // But waiting, deleting a folder should delete its content.
            // Let's implement a recursive helper or use loading.
            
            string folderName = folder.Name;
            await DeleteFolderRecursive(folder);
            await _context.SaveChangesAsync();

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"deleted group of parts <b>{folderName}</b>");
        }

        private async Task DeleteFolderRecursive(PartsFolder folder)
        {
            // Load children if not loaded (though we included direct children)
            // We need to load children of children.
            // This can be expensive.
            
            // Alternative: Fetch all folders and parts for garage, build tree in memory, delete relevant.
            // Or just iterate.
            
            // For now, let's explicitely load children of children to delete them.
            // Or simpler: Rely on Cascade Delete if configured in DB context.
            // Let's check DB Context or assume we need to do it manually.
            // To be safe, manual recursion.
            
            var subFolders = await _context.PartsFolders
                .Where(f => f.ParentId == folder.Id)
                .ToListAsync();

            foreach (var sub in subFolders)
            {
                await DeleteFolderRecursive(sub);
            }

            var parts = await _context.Parts
                .Where(p => p.ParentId == folder.Id)
                .ToListAsync();
            
            _context.Parts.RemoveRange(parts);
            _context.PartsFolders.Remove(folder);
            _context.PartsFolders.Remove(folder);
        }

        public async Task MovePartAsync(string userId, string workshopId, string partId, string? newParentId)
        {
            var part = await _context.Parts.FirstOrDefaultAsync(p => p.Id == partId && p.WorkshopId == workshopId);
            if (part == null) throw new ArgumentException("Part not found");

            var oldParentId = part.ParentId;
            if (oldParentId == newParentId) return;

            if (newParentId != null)
            {
                var parent = await _context.PartsFolders.FirstOrDefaultAsync(f => f.Id == newParentId && f.WorkshopId == workshopId);
                if (parent == null) throw new ArgumentException("Target folder not found");
            }

            string oldParentName = "base";
            if (oldParentId != null)
            {
                var oldFolder = await _context.PartsFolders.FindAsync(oldParentId);
                oldParentName = oldFolder?.Name ?? "base";
            }

            string newParentName = "base";
            if (newParentId != null)
            {
                var newFolder = await _context.PartsFolders.FindAsync(newParentId);
                newParentName = newFolder?.Name ?? "base";
            }

            part.ParentId = newParentId;
            await _context.SaveChangesAsync();

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"moved part <a href='/parts?partId={part.Id}' class='log-link target-link'>{part.Name}</a> from <b>{oldParentName}</b> to <b>{newParentName}</b>");
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

            string oldParentName = "base";
            if (oldParentId != null)
            {
                var oldFolder = await _context.PartsFolders.FindAsync(oldParentId);
                oldParentName = oldFolder?.Name ?? "base";
            }

            string newParentName = "base";
            if (newParentId != null)
            {
                var newFolder = await _context.PartsFolders.FindAsync(newParentId);
                newParentName = newFolder?.Name ?? "base";
            }

            folder.ParentId = newParentId;
            await _context.SaveChangesAsync();

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"moved group of parts <b>{folder.Name}</b> from <b>{oldParentName}</b> to <b>{newParentName}</b>");
        }

        public async Task RecalculateAvailabilityBalanceAsync(string workshopId, string? partId = null)
        {
            var partsQuery = _context.Parts.Where(p => p.WorkshopId == workshopId);
            if (!string.IsNullOrEmpty(partId))
            {
                partsQuery = partsQuery.Where(p => p.Id == partId);
            }

            var parts = await partsQuery.ToListAsync();

            foreach (var part in parts)
            {
                var awaitingQuantity = await _context.JobParts
                    .Where(jp => jp.PartId == part.Id && jp.Job.Status == Shared.Enums.JobStatus.AwaitingParts)
                    .SumAsync(jp => jp.Quantity);

                part.AvailabilityBalance = part.Quantity - awaitingQuantity;

                if (part.AvailabilityBalance < part.MinimumQuantity)
                {
                    await _notificationService.SendStockNotificationAsync(workshopId, part.Id, part.Name, part.AvailabilityBalance, part.MinimumQuantity);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<int> GetPartsReservedAsync(string partId)
        {
            return await _context.JobParts
                .Where(jp => jp.PartId == partId && jp.Job.Status == Shared.Enums.JobStatus.AwaitingParts)
                .SumAsync(jp => jp.Quantity);
        }
    }
}
