using System.Globalization;
using GarageControl.Core.Contracts;
using GarageControl.Core.Services.Helpers;
using GarageControl.Core.ViewModels;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageControl.Core.Models;

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

        public async Task<List<PartVM>> GetAllPartsAsync(string workshopId)
        {
            var parts = await _context.Parts.Where(p => p.WorkshopId == workshopId).ToListAsync();
            var result = new List<PartVM>();
            foreach (var p in parts)
            {
                var toSend = await _inventoryService.GetPartsToSendAsync(p.Id);
                result.Add(ToPartVM(p, toSend));
            }
            return result;
        }

        public async Task<PartVM> GetPartByIdAsync(string partId, string workshopId)
        {
            var part = await _context.Parts.FirstOrDefaultAsync(p => p.Id == partId && p.WorkshopId == workshopId);
            if (part == null) return null!;

            var toSend = await _inventoryService.GetPartsToSendAsync(part.Id);
            return ToPartVM(part, toSend);
        }

        public async Task<List<PartVM>> GetPartsByFolderAsync(string? folderId, string workshopId)
        {
            var parts = await _context.Parts
                .Where(p => p.WorkshopId == workshopId && p.ParentId == folderId)
                .ToListAsync();

            var result = new List<PartVM>();
            foreach (var p in parts)
            {
                var toSend = await _inventoryService.GetPartsToSendAsync(p.Id);
                result.Add(ToPartVM(p, toSend));
            }
            return result;
        }

        public async Task<PartVM> CreatePartAsync(string userId, string workshopId, CreatePartVM model)
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
            await _activityLogger.LogPartCreatedAsync(userId, workshopId, part.Id, part.Name);

            return ToPartVM(part, 0);
        }

        public async Task<PartWithPathVM?> GetPartWithPathAsync(string partId, string workshopId)
        {
            var part = await _context.Parts.FirstOrDefaultAsync(p => p.Id == partId && p.WorkshopId == workshopId);
            if (part == null) return null;

            var toSend = await _inventoryService.GetPartsToSendAsync(part.Id);
            var result = new PartWithPathVM
            {
                Id = part.Id,
                Name = part.Name,
                PartNumber = part.PartNumber,
                Price = part.Price,
                Quantity = part.Quantity,
                AvailabilityBalance = part.AvailabilityBalance,
                PartsToSend = toSend,
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

        public async Task<PartVM> EditPartAsync(string userId, string workshopId, string partId, UpdatePartVM model)
        {
            var part = await _context.Parts.FirstOrDefaultAsync(p => p.Id == partId && p.WorkshopId == workshopId);
            if (part == null) throw new ArgumentException("Part not found");

            var changes = TrackPartChanges(part, model);

            part.Name = model.Name;
            part.PartNumber = model.PartNumber;
            part.Price = model.Price;
            part.Quantity = model.Quantity;
            part.MinimumQuantity = model.MinimumQuantity;

            await _context.SaveChangesAsync();
            
            // Recalculate availability in case Quantity (stockpile) was changed
            await _inventoryService.RecalculateAvailabilityBalanceAsync(workshopId, part.Id);
            
            await _activityLogger.LogPartUpdatedAsync(userId, workshopId, part.Id, part.Name, changes);

            var toSend = await _inventoryService.GetPartsToSendAsync(part.Id);
            return ToPartVM(part, toSend);
        }

        private List<ActivityPropertyChange> TrackPartChanges(Part part, UpdatePartVM model)
        {
            var changes = new List<ActivityPropertyChange>();
            string FormatPrice(decimal p) => p.ToString("0.00", CultureInfo.InvariantCulture);
            bool NumbersEqual(double n1, double n2) => Math.Abs(n1 - n2) < 0.0001;
            bool PricesEqual(decimal n1, decimal n2) => n1 == n2;

            if (part.Name != model.Name) changes.Add(new ActivityPropertyChange("name", part.Name, model.Name));
            if (part.PartNumber != model.PartNumber) changes.Add(new ActivityPropertyChange("part number", part.PartNumber, model.PartNumber));
            if (!PricesEqual(part.Price, model.Price)) changes.Add(new ActivityPropertyChange("price", FormatPrice(part.Price), FormatPrice(model.Price)));
            if (!NumbersEqual(part.Quantity, model.Quantity)) changes.Add(new ActivityPropertyChange("quantity", part.Quantity.ToString(), model.Quantity.ToString()));
            if (!NumbersEqual(part.MinimumQuantity, model.MinimumQuantity)) changes.Add(new ActivityPropertyChange("minimum quantity", part.MinimumQuantity.ToString(), model.MinimumQuantity.ToString()));

            return changes;
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

            await _activityLogger.LogPartMovedAsync(userId, workshopId, part.Id, part.Name, oldParentName, newParentName);
        }

        // ---------------- HELPERS ----------------

        private PartVM ToPartVM(Part part, double partsToSend)
        {
            return new PartVM
            {
                Id = part.Id,
                Name = part.Name,
                PartNumber = part.PartNumber,
                Price = part.Price,
                Quantity = part.Quantity,
                AvailabilityBalance = part.AvailabilityBalance,
                PartsToSend = partsToSend,
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
