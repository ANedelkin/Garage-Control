using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Shared.Enums;

namespace GarageControl.Core.Services.Jobs
{
    public class JobActivityLogger
    {
        private readonly IActivityLogService _activityLogService;

        public JobActivityLogger(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        // Logs changes in job properties (type, worker, status, etc.)
        public async Task<List<string>> TrackJobPropertiesAsync(
            string userId,
            string workshopId,
            Job oldJob,
            UpdateJobViewModel model,
            string newJobTypeName,
            string newWorkerName)
        {
            var changes = new List<string>();

            string FormatPrice(decimal p) => p.ToString("0.00");

            if (oldJob.JobTypeId != model.JobTypeId)
                changes.Add($"type from <b>{oldJob.JobType.Name}</b> to <b>{newJobTypeName}</b>");

            if (oldJob.WorkerId != model.WorkerId)
                changes.Add($"mechanic from <b>{oldJob.Worker.Name}</b> to <b>{newWorkerName}</b>");

            if (oldJob.Status != model.Status)
                changes.Add($"status from <b>{oldJob.Status}</b> to <b>{model.Status}</b>");

            if (oldJob.LaborCost != model.LaborCost)
                changes.Add($"labor cost from <b>{FormatPrice(oldJob.LaborCost)}</b> to <b>{FormatPrice(model.LaborCost)}</b>");

            if (oldJob.StartTime != model.StartTime || oldJob.EndTime != model.EndTime)
                changes.Add($"Updated interval from {oldJob.StartTime:HH:mm}-{oldJob.EndTime:HH:mm} to {model.StartTime:HH:mm}-{model.EndTime:HH:mm}");

            if (oldJob.Description != model.Description)
                changes.Add("updated description");

            if (changes.Count > 0)
            {
                string carName = $"{oldJob.Order.Car.Model.CarMake.Name} {oldJob.Order.Car.Model.Name} ({oldJob.Order.Car.RegistrationNumber})";
                string orderLink = $"<a href='/orders' class='log-link target-link'>order for {carName}</a>";
                string actionHtml = $"updated job '{oldJob.JobType.Name}' for {orderLink}: {string.Join(", ", changes)}";

                await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
            }

            return changes;
        }

        // Logs changes in parts and adjusts stock
        public async Task<List<string>> TrackPartsChangesAsync(
            Job job,
            List<CreateJobPartViewModel> updatedParts,
            JobStatus oldStatus,
            IInventoryService inventoryService,
            string workshopId)
        {
            var changes = new List<string>();
            var partIdsInModel = updatedParts.Select(p => p.PartId).ToList();
            var partsToRemove = job.JobParts.Where(jp => !partIdsInModel.Contains(jp.PartId)).ToList();

            foreach (var jp in partsToRemove)
            {
                if (jp.Part != null)
                {
                    if (oldStatus == JobStatus.AwaitingParts)
                        jp.Part.AvailabilityBalance += jp.Quantity;
                    else
                    {
                        jp.Part.Quantity += jp.Quantity;
                        jp.Part.AvailabilityBalance += jp.Quantity;
                    }
                }
                changes.Add($"removed part '<b>{jp.Part?.Name}</b>'");
                job.JobParts.Remove(jp);
            }

            foreach (var partModel in updatedParts)
            {
                var existingJobPart = job.JobParts.FirstOrDefault(jp => jp.PartId == partModel.PartId);
                var part = job.JobParts.FirstOrDefault(jp => jp.PartId == partModel.PartId)?.Part;

                if (part == null) continue;

                if (existingJobPart != null)
                {
                    if (existingJobPart.Quantity != partModel.Quantity)
                    {
                        int diff = partModel.Quantity - existingJobPart.Quantity;
                        if (oldStatus == JobStatus.AwaitingParts)
                            part.AvailabilityBalance -= diff;
                        else
                        {
                            part.Quantity -= diff;
                            part.AvailabilityBalance -= diff;
                        }
                        changes.Add($"changed quantity of '<b>{part.Name}</b>' from <b>{existingJobPart.Quantity}</b> to <b>{partModel.Quantity}</b>");
                        existingJobPart.Quantity = partModel.Quantity;
                    }
                }
                else
                {
                    job.JobParts.Add(new JobPart
                    {
                        PartId = part.Id,
                        Quantity = partModel.Quantity,
                        Price = part.Price,
                        Part = part
                    });
                    changes.Add($"added part '<b>{part.Name}</b>'");

                    if (oldStatus == JobStatus.AwaitingParts)
                        part.AvailabilityBalance -= partModel.Quantity;
                    else
                    {
                        part.Quantity -= partModel.Quantity;
                        part.AvailabilityBalance -= partModel.Quantity;
                    }
                }

                await inventoryService.CheckLowStockAsync(workshopId, part);
            }

            return changes;
        }

        public async Task LogJobCreatedAsync(
            string userId,
            string workshopId,
            string jobTypeName,
            Order order,
            List<string> changes)
        {
            string carName = $"{order.Car.Model.CarMake.Name} {order.Car.Model.Name} ({order.Car.RegistrationNumber})";
            string orderLink = $"<a href='/orders' class='log-link target-link'>order for {carName}</a>";
            string actionHtml = $"created job '{jobTypeName}' for {orderLink}";
            if (changes.Count > 0)
                actionHtml += $": {string.Join(", ", changes)}";

            await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
        }
    }
}
