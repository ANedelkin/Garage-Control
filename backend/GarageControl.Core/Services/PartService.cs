using GarageControl.Core.ViewModels.Parts;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Core.Services
{
    public class PartService : IPartService
    {
        private readonly GarageControlDbContext _context;

        public PartService(GarageControlDbContext context)
        {
            _context = context;
        }

        public async Task<FolderContentViewModel> GetFolderContentAsync(string garageId, string? folderId)
        {
            var result = new FolderContentViewModel
            {
                CurrentFolderId = folderId
            };

            if (!string.IsNullOrEmpty(folderId))
            {
                var currentFolder = await _context.PartsFolders
                    .FirstOrDefaultAsync(f => f.Id == folderId && f.CarServiceId == garageId);
                
                if (currentFolder == null)
                {
                    // Or throw exception
                    return result; 
                }
                result.CurrentFolderName = currentFolder.Name;
                result.ParentFolderId = currentFolder.ParentId;
            }

            var foldersQuery = _context.PartsFolders
                .Where(f => f.CarServiceId == garageId);
            
            var partsQuery = _context.Parts
                .Where(p => p.CarServiceId == garageId);

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

            result.Parts = await partsQuery
                .Select(p => new PartViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    PartNumber = p.PartNumber,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    ParentId = p.ParentId
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<PartViewModel>> GetAllPartsAsync(string garageId)
        {
            return await _context.Parts
                .Where(p => p.CarServiceId == garageId)
                .Select(p => new PartViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    PartNumber = p.PartNumber,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    ParentId = p.ParentId
                })
                .ToListAsync();
        }

        public async Task<PartViewModel> CreatePartAsync(string garageId, CreatePartViewModel model)
        {
            var part = new Part
            {
                Name = model.Name,
                PartNumber = model.PartNumber,
                Price = model.Price,
                Quantity = model.Quantity,
                ParentId = model.ParentId,
                CarServiceId = garageId
            };

            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            return new PartViewModel
            {
                Id = part.Id,
                Name = part.Name,
                PartNumber = part.PartNumber,
                Price = part.Price,
                Quantity = part.Quantity,
                ParentId = part.ParentId
            };
        }

        public async Task EditPartAsync(string garageId, UpdatePartViewModel model)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == model.Id && p.CarServiceId == garageId);

            if (part == null) throw new ArgumentException("Part not found");

            part.Name = model.Name;
            part.PartNumber = model.PartNumber;
            part.Price = model.Price;
            part.Quantity = model.Quantity;

            await _context.SaveChangesAsync();
        }

        public async Task DeletePartAsync(string garageId, string partId)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == partId && p.CarServiceId == garageId);

            if (part != null)
            {
                _context.Parts.Remove(part);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<PartsFolderViewModel> CreateFolderAsync(string garageId, CreateFolderViewModel model)
        {
            var folder = new PartsFolder
            {
                Name = model.Name,
                ParentId = model.ParentId,
                CarServiceId = garageId
            };

            _context.PartsFolders.Add(folder);
            await _context.SaveChangesAsync();

            return new PartsFolderViewModel
            {
                Id = folder.Id,
                Name = folder.Name,
                ParentId = folder.ParentId
            };
        }

        public async Task RenameFolderAsync(string garageId, string folderId, string newName)
        {
             var folder = await _context.PartsFolders
                .FirstOrDefaultAsync(f => f.Id == folderId && f.CarServiceId == garageId);

            if (folder == null) throw new ArgumentException("Folder not found");

            folder.Name = newName;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFolderAsync(string garageId, string folderId)
        {
            // Recursive delete
            var folder = await _context.PartsFolders
                .Include(f => f.FolderChildren)
                .Include(f => f.PartsChildren)
                .FirstOrDefaultAsync(f => f.Id == folderId && f.CarServiceId == garageId);
            
            if (folder == null) return;

            // Load all descendants recursively
            // Since EF Core doesn't support recursive delete configuration easily without loading,
            // or setting up cascade delete in DB. For creating logic here, I will load children manually if needed.
            // But waiting, deleting a folder should delete its content.
            // Let's implement a recursive helper or use loading.
            
            await DeleteFolderRecursive(folder);
            await _context.SaveChangesAsync();
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
        }
    }
}
