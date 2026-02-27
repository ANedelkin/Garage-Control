using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Shared;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageControl.Core.Models;

namespace GarageControl.Core.Services.Jobs
{
    public class JobService : IJobService
    {
        private readonly GarageControlDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IAuthService _authService;
        private readonly JobActivityLogger _activityLogger;

        public JobService(
            GarageControlDbContext context,
            IInventoryService inventoryService,
            IAuthService authService,
            IActivityLogService activityLogService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _authService = authService;
            _activityLogger = new JobActivityLogger(activityLogService);
        }

        // ==============================
        // QUERIES (unchanged)
        // ==============================
        public async Task<MethodResponseVM> CreateJobAsync(
    string userId,
    string orderId,
    string workshopId,
    CreateJobVM model)
        {
            var order = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Owner)
                .Include(o => o.Car.Model.CarMake)
                .Include(o => o.Car.Model)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.Car.Owner.WorkshopId == workshopId);

            if (order == null)
                return new MethodResponseVM(false, "Order not found or access denied.");

            var car = order.Car;
            string carInfo = $"{car.Model.CarMake.Name} {car.Model.Name} ({car.RegistrationNumber})";

            var jobType = await _context.JobTypes.FindAsync(model.JobTypeId);
            var worker = await _context.Workers.FindAsync(model.WorkerId);

            if (jobType == null || worker == null)
                return new MethodResponseVM(false, "Invalid job type or worker.");

            var job = new Job
            {
                OrderId = order.Id,
                JobTypeId = model.JobTypeId,
                WorkerId = model.WorkerId,
                Status = model.Status,
                Description = model.Description,
                LaborCost = model.LaborCost,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                JobParts = new List<JobPart>()
            };

            var userAccesses = await _authService.GetUserAccess(userId);

            var (changes, affectedPartIds) =
                await ApplyPartsChangesAsync(job, model.Parts, workshopId, userId, userAccesses);

            _context.Jobs.Add(job);

            await _context.SaveChangesAsync();

            // Inventory recalculation handles notifications automatically now
            foreach (var partId in affectedPartIds)
            {
                await _inventoryService.RecalculateAvailabilityBalanceAsync(workshopId, partId);
            }

            await _activityLogger.LogJobCreatedAsync(
                userId,
                workshopId,
                jobType.Name,
                carInfo,
                changes);

            return new MethodResponseVM(true, "Job created successfully", job.Id);
        }

        public async Task<List<JobToDoVM>> GetMyJobsAsync(string userId, string workshopId)
        {
            return await _context.Jobs.Where(j => j.Worker.UserId == userId)
                                      .Select(j => new JobToDoVM
                                             {
                                                Id = j.Id,
                                                TypeName = j.JobType.Name,
                                                Description = j.Description ?? "",
                                                Status = j.Status == Shared.Enums.JobStatus.Pending ? "pending" :
                                                         j.Status == Shared.Enums.JobStatus.InProgress ? "inprogress" : "finished",
                                                StartTime = j.StartTime,
                                                CarName = j.Order.Car.Model.CarMake.Name + " " + j.Order.Car.Model.Name,
                                                CarRegistrationNumber = j.Order.Car.RegistrationNumber,
                                             })
                                      .OrderBy(j => j.StartTime)
                                      .ToListAsync();
        }


        public async Task<JobDetailsVM?> GetJobByIdAsync(string jobId, string workshopId)
        {
            return await _context.Jobs
                .AsNoTracking()
                .Where(j => j.Id == jobId && j.Order.Car.Owner.WorkshopId == workshopId)
                .Select(j => new JobDetailsVM
                {
                    Id = j.Id,
                    JobTypeId = j.JobTypeId,
                    WorkerId = j.WorkerId,
                    Description = j.Description ?? "",
                    Status = j.Status,
                    LaborCost = j.LaborCost,
                    StartTime = j.StartTime,
                    EndTime = j.EndTime,
                    OrderId = j.OrderId,
                    ClientName = j.Order.Car.Owner.Name,
                    CarName = j.Order.Car.Model.CarMake.Name + " " + j.Order.Car.Model.Name,
                    CarRegistrationNumber = j.Order.Car.RegistrationNumber,
                    Parts = j.JobParts.Select(jp => new JobPartDetailsVM
                    {
                        PartId = jp.PartId,
                        PartName = jp.Part.Name,
                        PlannedQuantity = jp.PlannedQuantity,
                        SentQuantity = jp.SentQuantity,
                        UsedQuantity = jp.UsedQuantity,
                        RequestedQuantity = jp.RequestedQuantity,
                        Price = jp.Price
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<JobListVM>> GetJobsByOrderIdAsync(string orderId, string workshopId)
        {
            return await _context.Jobs
                .AsNoTracking()
                .Where(j => j.OrderId == orderId && j.Order.Car.Owner.WorkshopId == workshopId)
                .Select(j => new JobListVM
                {
                    Id = j.Id,
                    Type = j.JobType.Name,
                    Description = j.Description ?? "",
                    Status = (int)j.Status == 0 ? "pending" : (int)j.Status == 1 ? "inprogress" : "finished",
                    MechanicName = j.Worker.Name,
                    StartTime = j.StartTime,
                    EndTime = j.EndTime,
                    LaborCost = j.LaborCost,
                    PartsCost = j.JobParts.Sum(jp => (decimal)jp.PlannedQuantity * jp.Price)
                })
                .ToListAsync();
        }

        // ==============================
        // JOB UPDATE (minor cleanup)
        // ==============================

        public async Task<MethodResponseVM> UpdateJobAsync(string userId, string jobId, string workshopId, UpdateJobVM model)
        {
            var job = await _context.Jobs
                .Include(j => j.JobType)
                .Include(j => j.Worker)
                .Include(j => j.Order)
                    .ThenInclude(o => o.Car)
                        .ThenInclude(c => c.Owner)
                .Include(j => j.Order.Car.Model.CarMake)
                .Include(j => j.Order.Car.Model)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Order.Car.Owner.WorkshopId == workshopId);

            if (job == null) throw new Exception("Job not found or access denied.");

            var car = job.Order.Car;
            string carInfo = $"{car.Model.CarMake.Name} {car.Model.Name} ({car.RegistrationNumber})";

            var jobType = await _context.JobTypes.FindAsync(model.JobTypeId);
            var worker = await _context.Workers.FindAsync(model.WorkerId);
            if (jobType == null || worker == null) throw new Exception("Invalid job type or worker");

            var propertyChanges = TrackPropertyChanges(job, model, jobType.Name, worker.Name);

            var userAccesses = await _authService.GetUserAccess(userId);

            var (partsChanges, affectedPartIds) =
                await ApplyPartsChangesAsync(job, model.Parts, workshopId, userId, userAccesses);

            job.JobTypeId = model.JobTypeId;
            job.WorkerId = model.WorkerId;
            job.Description = model.Description;

            var oldStatus = job.Status;
            job.Status = model.Status;

            if (oldStatus != model.Status && (oldStatus == JobStatus.Done || model.Status == JobStatus.Done))
            {
                foreach (var jp in job.JobParts)
                    affectedPartIds.Add(jp.PartId);
            }

            job.LaborCost = model.LaborCost;
            job.StartTime = model.StartTime;
            job.EndTime = model.EndTime;

            await _context.SaveChangesAsync();

            foreach (var partId in affectedPartIds)
                await _inventoryService.RecalculateAvailabilityBalanceAsync(workshopId, partId);

            await _activityLogger.LogJobUpdatedAsync(userId, workshopId, job.JobType.Name, carInfo, propertyChanges, partsChanges);

            return new MethodResponseVM(true, "Job updated successfully");
        }

        // ==============================
        // PART CHANGES (FIXED)
        // ==============================

        private async Task<(List<string> changes, HashSet<string> affectedPartIds)> ApplyPartsChangesAsync(
            Job job,
            List<CreateJobPartVM> updatedParts,
            string workshopId,
            string userId,
            List<string> userAccesses)
        {
            var changes = new List<string>();
            var affectedPartIds = new HashSet<string>();

            var partIdsInModel = updatedParts.Select(p => p.PartId).ToList();
            var partsToRemove = job.JobParts.Where(jp => !partIdsInModel.Contains(jp.PartId)).ToList();

            foreach (var jp in partsToRemove)
            {
                if (jp.Part != null)
                {
                    jp.Part.Quantity += jp.SentQuantity;
                    changes.Add(_activityLogger.FormatPartRemoved(jp.Part.Name));
                    affectedPartIds.Add(jp.PartId);
                }
                job.JobParts.Remove(jp);
            }

            foreach (var partModel in updatedParts)
            {
                var existingJobPart = job.JobParts.FirstOrDefault(jp => jp.PartId == partModel.PartId);
                var part = existingJobPart?.Part ?? await _inventoryService.GetPartByIdAsync(partModel.PartId);

                if (part == null) continue;

                bool hasStockAccess = userAccesses.Contains("Parts Stock");
                bool isAssignedWorker = job.Worker?.UserId == userId;

                if (existingJobPart != null)
                {
                    if (existingJobPart.PlannedQuantity != partModel.PlannedQuantity &&
                        (hasStockAccess || isAssignedWorker))
                        existingJobPart.PlannedQuantity = partModel.PlannedQuantity;

                    if (existingJobPart.SentQuantity != partModel.SentQuantity && hasStockAccess)
                    {
                        var diff = partModel.SentQuantity - existingJobPart.SentQuantity;
                        part.Quantity -= diff;
                        existingJobPart.SentQuantity = partModel.SentQuantity;
                    }

                    if (existingJobPart.UsedQuantity != partModel.UsedQuantity &&
                        (hasStockAccess || isAssignedWorker))
                        existingJobPart.UsedQuantity = partModel.UsedQuantity;

                    if (existingJobPart.RequestedQuantity != partModel.RequestedQuantity &&
                        (hasStockAccess || isAssignedWorker))
                        existingJobPart.RequestedQuantity = partModel.RequestedQuantity;
                }
                else
                {
                    int effectivePlanned = (hasStockAccess || isAssignedWorker) ? partModel.PlannedQuantity : 0;
                    int effectiveSent = hasStockAccess ? partModel.SentQuantity : 0;

                    job.JobParts.Add(new JobPart
                    {
                        PartId = part.Id,
                        PlannedQuantity = effectivePlanned,
                        SentQuantity = effectiveSent,
                        UsedQuantity = partModel.UsedQuantity,
                        RequestedQuantity = partModel.RequestedQuantity,
                        Price = part.Price,
                        Part = part
                    });

                    part.Quantity -= effectiveSent;
                    changes.Add(_activityLogger.FormatPartAdded(part.Name));
                }

                affectedPartIds.Add(part.Id);
            }

            return (changes, affectedPartIds);
        }
        private List<ActivityPropertyChange> TrackPropertyChanges(Job job, UpdateJobVM model, string newJobTypeName, string newWorkerName)
        {
            var changes = new List<ActivityPropertyChange>();
            string FormatPrice(decimal p) => p.ToString("0.00");

            if (job.JobTypeId != model.JobTypeId)
                changes.Add(new ActivityPropertyChange("type", job.JobType.Name, newJobTypeName));

            if (job.WorkerId != model.WorkerId)
                changes.Add(new ActivityPropertyChange("mechanic", job.Worker.Name, newWorkerName));

            if (job.Status != model.Status)
                changes.Add(new ActivityPropertyChange("status", job.Status.ToString(), model.Status.ToString()));

            if (job.LaborCost != model.LaborCost)
                changes.Add(new ActivityPropertyChange("labor cost", FormatPrice(job.LaborCost), FormatPrice(model.LaborCost)));

            if (job.StartTime != model.StartTime || job.EndTime != model.EndTime)
                changes.Add(new ActivityPropertyChange("interval", $"{job.StartTime:HH:mm}-{job.EndTime:HH:mm}", $"{model.StartTime:HH:mm}-{model.EndTime:HH:mm}"));

            if (job.Description != model.Description)
                changes.Add(new ActivityPropertyChange("description", "updated", ""));

            return changes;
        }
        public async Task<MethodResponseVM> DeleteJobAsync(string userId, string jobId, string workshopId)
        {
            var job = await _context.Jobs
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .Include(j => j.Order)
                    .ThenInclude(o => o.Car)
                        .ThenInclude(c => c.Owner)
                .Include(j => j.JobType)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Order.Car.Owner.WorkshopId == workshopId);

            if (job == null) return new MethodResponseVM(false, "Job not found or access denied.");

            var car = job.Order.Car;
            string carInfo = $"{car.Model?.CarMake?.Name} {car.Model?.Name} ({car.RegistrationNumber})";

            var userAccesses = await _authService.GetUserAccess(userId);

            // Revert parts
            var changes = new List<string>();
            var affectedPartIds = new HashSet<string>();
            
            foreach (var jp in job.JobParts.ToList())
            {
                if (jp.Part != null)
                {
                    jp.Part.Quantity += jp.SentQuantity;
                    affectedPartIds.Add(jp.PartId);
                }
                _context.JobParts.Remove(jp);
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            foreach (var partId in affectedPartIds)
            {
                await _inventoryService.RecalculateAvailabilityBalanceAsync(workshopId, partId);
            }

            return new MethodResponseVM(true, "Job deleted successfully");
        }
    }
}
