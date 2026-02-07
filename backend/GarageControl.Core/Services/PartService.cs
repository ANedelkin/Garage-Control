using System.Globalization;
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
    public class PartService : IPartService
    {
        private readonly GarageControlDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly PartActivityLogger _activityLogger;

        public PartService(
            GarageControlDbContext context,
            IActivityLogService activityLogService,
            IInventoryService inventoryService)
        {
            _context = context;
            _activityLogger = new PartActivityLogger(activityLogService);
            _inventoryService = inventoryService;
        }
        
        // ---------------- PART OPERATIONS ----------------

        public async Task<List<PartViewModel>> GetAllPartsAsync(string workshopId)
        {
            var parts = await _context.Parts.Where(p => p.WorkshopId == workshopId).ToListAsync();
            var result = new List<PartViewModel>();
            foreach (var p in parts)
            {
                var reserved = await _inventoryService.GetPartsReservedAsync(p.Id);
                result.Add(ToPartViewModel(p, reserved));
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

            await _inventoryService.CheckLowStockAsync(workshopId, part);
            await _activityLogger.LogPartCreatedAsync(userId, workshopId, part);

            return ToPartViewModel(part, 0);
        }

        public async Task<PartWithPathViewModel?> GetPartAsync(string workshopId, string partId)
        {
            var part = await _context.Parts.FirstOrDefaultAsync(p => p.Id == partId && p.WorkshopId == workshopId);
            if (part == null) return null;

            var reserved = await _inventoryService.GetPartsReservedAsync(part.Id);
            var result = new PartWithPathViewModel
            {
                Id = part.Id,
                Name = part.Name,
                PartNumber = part.PartNumber,
                Price = part.Price,
                Quantity = part.Quantity,
                AvailabilityBalance = part.AvailabilityBalance,
                PartsReserved = reserved,
                MinimumQuantity = part.MinimumQuantity,
                ParentId = part.ParentId,
                Path = new List<string>()
            };

            var currentParentId = part.ParentId;
            while (!string.IsNullOrEmpty(currentParentId))
            {
                result.Path.Insert(0, currentParentId);
                var parent = await _context.PartsFolders.FirstOrDefaultAsync(f => f.Id == currentParentId);
                currentParentId = parent?.ParentId;
            }

            return result;
        }

        public async Task EditPartAsync(string userId, string workshopId, UpdatePartViewModel model)
        {
            var part = await _context.Parts.FirstOrDefaultAsync(p => p.Id == model.Id && p.WorkshopId == workshopId);
            if (part == null) throw new ArgumentException("Part not found");

            var changes = _activityLogger.TrackChanges(part, model);

            part.Name = model.Name;
            part.PartNumber = model.PartNumber;
            part.Price = model.Price;
            part.Quantity = model.Quantity;
            part.MinimumQuantity = model.MinimumQuantity;

            await _context.SaveChangesAsync();
            await _inventoryService.CheckLowStockAsync(workshopId, part);
            await _activityLogger.LogPartUpdatedAsync(userId, workshopId, part, changes);
        }

        public async Task DeletePartAsync(string userId, string workshopId, string partId)
        {
            var part = await _context.Parts.FirstOrDefaultAsync(p => p.Id == partId && p.WorkshopId == workshopId);
            if (part == null) return;

            string partName = part.Name;
            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();

            await _activityLogger.LogPartDeletedAsync(userId, workshopId, partName);
        }

        // ---------------- MOVE OPERATIONS ----------------

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

            string oldParentName = await GetFolderNameOrBaseAsync(oldParentId);
            string newParentName = await GetFolderNameOrBaseAsync(newParentId);

            part.ParentId = newParentId;
            await _context.SaveChangesAsync();

            await _activityLogger.LogPartMovedAsync(userId, workshopId, part, oldParentName, newParentName);
        }

        // ---------------- HELPERS ----------------

        private PartViewModel ToPartViewModel(Part part, int partsReserved)
        {
            return new PartViewModel
            {
                Id = part.Id,
                Name = part.Name,
                PartNumber = part.PartNumber,
                Price = part.Price,
                Quantity = part.Quantity,
                AvailabilityBalance = part.AvailabilityBalance,
                PartsReserved = partsReserved,
                MinimumQuantity = part.MinimumQuantity,
                ParentId = part.ParentId
            };
        }

        private async Task<string> GetFolderNameOrBaseAsync(string? folderId)
        {
            if (folderId == null) return "base";
            var folder = await _context.PartsFolders.FindAsync(folderId);
            return folder?.Name ?? "base";
        }
    }
}
