using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GarageControl.Core.Services.Jobs
{
    public class JobService : IJobService
    {
        private readonly GarageControlDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly JobActivityLogger _activityLogger;

        public JobService(
            GarageControlDbContext context,
            IInventoryService inventoryService,
            IActivityLogService activityLogService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _activityLogger = new JobActivityLogger(activityLogService);
        }

        // --- QUERIES ---
        public async Task<List<JobToDoViewModel>> GetMyJobsAsync(string userId, string workshopId)
        {
            return await _context.Jobs
                .AsNoTracking()
                .Include(j => j.JobType)
                .Include(j => j.Order)
                    .ThenInclude(o => o.Car)
                        .ThenInclude(c => c.Owner)
                .Include(j => j.Order.Car.Model.CarMake)
                .Include(j => j.Order.Car.Model)
                .Include(j => j.Worker)
                .Where(j => j.Worker.UserId == userId && j.Order.Car.Owner.WorkshopId == workshopId)
                .OrderBy(j => j.StartTime)
                .Select(j => new JobToDoViewModel
                {
                    Id = j.Id,
                    TypeName = j.JobType.Name,
                    Description = j.Description ?? "",
                    Status = j.Status,
                    StartTime = j.StartTime,
                    EndTime = j.EndTime,
                    OrderId = j.OrderId,
                    CarName = j.Order.Car.Model.CarMake.Name + " " + j.Order.Car.Model.Name,
                    CarRegistrationNumber = j.Order.Car.RegistrationNumber,
                    ClientName = j.Order.Car.Owner.Name
                })
                .ToListAsync();
        }

        public async Task<JobDetailsViewModel?> GetJobByIdAsync(string jobId, string workshopId)
        {
            return await _context.Jobs
                .AsNoTracking()
                .Where(j => j.Id == jobId && j.Order.Car.Owner.WorkshopId == workshopId)
                .Select(j => new JobDetailsViewModel
                {
                    Id = j.Id,
                    JobTypeId = j.JobTypeId,
                    WorkerId = j.WorkerId,
                    Description = j.Description ?? "",
                    Status = j.Status,
                    LaborCost = j.LaborCost,
                    StartTime = j.StartTime,
                    EndTime = j.EndTime,
                    Parts = j.JobParts.Select(jp => new JobPartDetailsViewModel
                    {
                        PartId = jp.PartId,
                        PartName = jp.Part.Name,
                        Quantity = jp.Quantity,
                        Price = jp.Price
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        // --- JOB UPDATE ---
        public async Task<MethodResponse> UpdateJobAsync(string userId, string jobId, string workshopId, UpdateJobViewModel model)
        {
            var job = await _context.Jobs
                .Include(j => j.JobType)
                .Include(j => j.Worker)
                .Include(j => j.Order)
                    .ThenInclude(o => o.Car)
                        .ThenInclude(c => c.Owner)
                .Include(j => j.Order.Car.Model.CarMake)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Order.Car.Owner.WorkshopId == workshopId);

            if (job == null) throw new Exception("Job not found or access denied.");

            var oldStatus = job.Status;

            // resolve JobType and Worker names
            var jobType = await _context.JobTypes.FindAsync(model.JobTypeId);
            var worker = await _context.Workers.FindAsync(model.WorkerId);
            if (jobType == null || worker == null) throw new Exception("Invalid job type or worker");

            // track parts changes (also updates stock)
            var partsChanges = await _activityLogger.TrackPartsChangesAsync(job, model.Parts, oldStatus, _inventoryService, workshopId);

            // apply new values
            job.JobTypeId = model.JobTypeId;
            job.WorkerId = model.WorkerId;
            job.Description = model.Description;
            job.Status = model.Status;
            job.LaborCost = model.LaborCost;
            job.StartTime = model.StartTime;
            job.EndTime = model.EndTime;

            await _context.SaveChangesAsync();

            // track property changes
            var propertyChanges = await _activityLogger.TrackJobPropertiesAsync(
                userId,
                workshopId,
                job,
                model,
                jobType.Name,
                worker.Name
            );

            return new MethodResponse(true, "Job updated successfully");
        }

        // --- JOB CREATION ---
        public async Task<MethodResponse> CreateJobAsync(string userId, string orderId, string workshopId, CreateJobViewModel model)
        {
            var order = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Owner)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.Car.Owner.WorkshopId == workshopId);

            if (order == null) return new MethodResponse(false, "Order not found or access denied.");

            var jobType = await _context.JobTypes.FindAsync(model.JobTypeId);
            var worker = await _context.Workers.FindAsync(model.WorkerId);
            if (jobType == null || worker == null) return new MethodResponse(false, "Invalid job type or worker.");

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

            // --- Track parts and apply stock ---
            var changes = await _activityLogger.TrackPartsChangesAsync(job, model.Parts, model.Status, _inventoryService, workshopId);

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            // --- Log creation ---
            await _activityLogger.LogJobCreatedAsync(userId, workshopId, jobType.Name, order, changes);

            return new MethodResponse(true, "Job created successfully", job.Id);
        }
    }
}
