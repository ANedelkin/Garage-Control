using System;
using System.Globalization;
using GarageControl.Core.Contracts;
using GarageControl.Core.Services.Helpers;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Parts;
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
        private readonly IDeficitService _deficitService;
        private readonly PartActivityLogger _activityLogger;

        public PartService(
            GarageControlDbContext context,
            IActivityLogService activityLogService,
            IInventoryService inventoryService,
            IDeficitService deficitService)
        {
            _context = context;
            _activityLogger = new PartActivityLogger(activityLogService);
            _inventoryService = inventoryService;
            _deficitService = deficitService;
        }

        // ---------------- PART OPERATIONS ----------------

        public async Task<List<PartVM>> GetAllPartsAsync(string workshopId)
        {
            return await _context.Parts
                .Where(p => p.WorkshopId == workshopId)
                .Select(p => new PartVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    PartNumber = p.PartNumber,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    AvailabilityBalance = p.AvailabilityBalance,
                    PartsToSend = p.JobParts
                        .Where(jp => jp.SentQuantity < jp.PlannedQuantity)
                        .Sum(jp => jp.PlannedQuantity - jp.SentQuantity),
                    MinimumQuantity = p.MinimumQuantity,
                    ParentId = p.ParentId,
                    DeficitStatus = (int)p.DeficitStatus
                })
                .ToListAsync();
        }

        public async Task<PartVM?> GetPartByIdAsync(string partId, string workshopId)
        {
            return await _context.Parts
                .Where(p => p.Id == partId && p.WorkshopId == workshopId)
                .Select(p => new PartVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    PartNumber = p.PartNumber,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    AvailabilityBalance = p.AvailabilityBalance,
                    PartsToSend = p.JobParts
                        .Where(jp => jp.SentQuantity < jp.PlannedQuantity)
                        .Sum(jp => jp.PlannedQuantity - jp.SentQuantity),
                    MinimumQuantity = p.MinimumQuantity,
                    ParentId = p.ParentId,
                    DeficitStatus = (int)p.DeficitStatus
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<PartVM>> GetPartsByFolderAsync(string? folderId, string workshopId)
        {
            return await _context.Parts
                .Where(p => p.WorkshopId == workshopId && p.ParentId == folderId)
                .Select(p => new PartVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    PartNumber = p.PartNumber,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    AvailabilityBalance = p.AvailabilityBalance,
                    PartsToSend = p.JobParts
                        .Where(jp => jp.SentQuantity < jp.PlannedQuantity)
                        .Sum(jp => jp.PlannedQuantity - jp.SentQuantity),
                    MinimumQuantity = p.MinimumQuantity,
                    ParentId = p.ParentId,
                    DeficitStatus = (int)p.DeficitStatus
                })
                .ToListAsync();
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

            part.DeficitStatus = _deficitService.CalculatePartDeficitStatus(part);

            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            await _activityLogger.LogPartCreatedAsync(
                userId,
                workshopId,
                part.Id,
                part.Name);

            if (!string.IsNullOrEmpty(part.ParentId))
                await _deficitService.RecalculateFolderDeficitCountsAsync(part.ParentId);

            return await GetPartByIdAsync(part.Id, workshopId)
                ?? throw new Exception("Created part not found");
        }

        public async Task<PartWithPathVM?> GetPartWithPathAsync(string partId, string workshopId)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == partId && p.WorkshopId == workshopId);

            if (part == null)
                return null;

            var result = new PartWithPathVM
            {
                Id = part.Id,
                Name = part.Name,
                PartNumber = part.PartNumber,
                Price = part.Price,
                Quantity = part.Quantity,
                AvailabilityBalance = part.AvailabilityBalance,
                PartsToSend = await _inventoryService.GetPartsToSendAsync(part.Id),
                MinimumQuantity = part.MinimumQuantity,
                ParentId = part.ParentId,
                Path = new List<string>()
            };

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

        public async Task<PartVM> EditPartAsync(string userId, string workshopId, string partId, UpdatePartVM model)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == partId && p.WorkshopId == workshopId);

            if (part == null)
                throw new ArgumentException("Part not found");

            var changes = TrackPartChanges(part, model);

            part.Name = model.Name;
            part.PartNumber = model.PartNumber;
            part.Price = model.Price;
            part.Quantity = model.Quantity;
            part.MinimumQuantity = model.MinimumQuantity;

            await _context.SaveChangesAsync();

            await _inventoryService.RecalculateAvailabilityBalanceAsync(workshopId, part.Id);
            await _deficitService.UpdatePartDeficitStatusAsync(workshopId, part);

            await _activityLogger.LogPartUpdatedAsync(
                userId,
                workshopId,
                part.Id,
                part.Name,
                changes);

            return await GetPartByIdAsync(part.Id, workshopId)
                ?? throw new Exception("Part not found after update");
        }

        private List<ActivityPropertyChange> TrackPartChanges(Part part, UpdatePartVM model)
        {
            var changes = new List<ActivityPropertyChange>();

            string FormatPrice(decimal p) =>
                p.ToString("0.00", CultureInfo.InvariantCulture);

            bool NumbersEqual(double n1, double n2) =>
                Math.Abs(n1 - n2) < 0.0001;

            if (part.Name != model.Name)
                changes.Add(new ActivityPropertyChange("name", part.Name, model.Name));

            if (part.PartNumber != model.PartNumber)
                changes.Add(new ActivityPropertyChange("part number", part.PartNumber, model.PartNumber));

            if (part.Price != model.Price)
                changes.Add(new ActivityPropertyChange("price", FormatPrice(part.Price), FormatPrice(model.Price)));

            if (!NumbersEqual(part.Quantity, model.Quantity))
                changes.Add(new ActivityPropertyChange("quantity", part.Quantity.ToString(), model.Quantity.ToString()));

            if (!NumbersEqual(part.MinimumQuantity, model.MinimumQuantity))
                changes.Add(new ActivityPropertyChange("minimum quantity", part.MinimumQuantity.ToString(), model.MinimumQuantity.ToString()));

            return changes;
        }

        public async Task DeletePartAsync(string userId, string workshopId, string partId)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == partId && p.WorkshopId == workshopId);

            if (part == null)
                return;

            string partName = part.Name;
            string? parentId = part.ParentId;

            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(parentId))
                await _deficitService.RecalculateFolderDeficitCountsAsync(parentId);

            await _activityLogger.LogPartDeletedAsync(userId, workshopId, partName);
        }

        public async Task MovePartAsync(string userId, string workshopId, string partId, string? newParentId)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == partId && p.WorkshopId == workshopId);

            if (part == null)
                throw new ArgumentException("Part not found");

            var oldParentId = part.ParentId;

            if (oldParentId == newParentId)
                return;

            if (newParentId != null)
            {
                var parent = await _context.PartsFolders
                    .FirstOrDefaultAsync(f => f.Id == newParentId && f.WorkshopId == workshopId);

                if (parent == null)
                    throw new ArgumentException("Target folder not found");
            }

            string oldParentName = await GetFolderNameOrBaseAsync(oldParentId);
            string newParentName = await GetFolderNameOrBaseAsync(newParentId);

            part.ParentId = newParentId;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(oldParentId))
                await _deficitService.RecalculateFolderDeficitCountsAsync(oldParentId);

            if (!string.IsNullOrEmpty(newParentId))
                await _deficitService.RecalculateFolderDeficitCountsAsync(newParentId);

            await _activityLogger.LogPartMovedAsync(
                userId,
                workshopId,
                part.Id,
                part.Name,
                oldParentName,
                newParentName);
        }

        private async Task<string> GetFolderNameOrBaseAsync(string? folderId)
        {
            if (folderId == null)
                return "base";

            var folder = await _context.PartsFolders.FindAsync(folderId);

            return folder?.Name ?? "base";
        }
    }
}
