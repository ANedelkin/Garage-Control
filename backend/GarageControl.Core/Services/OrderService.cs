using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GarageControl.Core.Services
{
    public interface IOrderService
    {
        Task<List<OrderListViewModel>> GetOrdersAsync(string workshopId, bool? isDone = null);
        Task<object> CreateOrderAsync(string userId, string workshopId, CreateOrderViewModel model);
        Task<OrderDetailsViewModel?> GetOrderByIdAsync(string id, string workshopId);
        Task<object> UpdateOrderAsync(string userId, string id, string workshopId, UpdateOrderViewModel model);
        Task<List<JobToDoViewModel>> GetMyJobsAsync(string userId, string workshopId);
        Task<JobDetailsViewModel?> GetJobByIdAsync(string jobId, string workshopId);
        Task CreateJobAsync(string userId, string orderId, string workshopId, CreateJobViewModel model);
        Task UpdateJobAsync(string userId, string jobId, string workshopId, UpdateJobViewModel model);
    }

    public class OrderService : IOrderService
    {
        private readonly GarageControlDbContext _context;
        private readonly IActivityLogService _activityLogService;

        public OrderService(GarageControlDbContext context, IActivityLogService activityLogService)
        {
            _context = context;
            _activityLogService = activityLogService;
        }

        public async Task<List<OrderListViewModel>> GetOrdersAsync(string workshopId, bool? isDone = null)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.Car.Owner.WorkshopId == workshopId);

            if (isDone.HasValue)
            {
                query = query.Where(o => o.IsDone == isDone.Value);
            }

            var rawData = await query
                .Select(o => new
                {
                    o.Id,
                    o.CarId,
                    CarMakeName = o.Car.Model.CarMake.Name,
                    CarModelName = o.Car.Model.Name,
                    o.Car.RegistrationNumber,
                    o.Car.Owner.Name,
                    o.Kilometers,
                    o.IsDone,
                    Jobs = o.Jobs.Select(j => new
                    {
                        j.Id,
                        TypeName = j.JobType.Name,
                        Description = j.Description ?? "",
                        j.Status,
                        MechanicName = j.Worker.Name,
                        j.StartTime,
                        j.EndTime,
                        j.LaborCost,
                    }).ToList()
                })
                .ToListAsync();

            return rawData.Select(o => new OrderListViewModel
            {
                Id = o.Id,
                CarId = o.CarId,
                CarName = $"{o.CarMakeName} {o.CarModelName}",
                CarRegistrationNumber = o.RegistrationNumber,
                ClientName = o.Name,
                Kilometers = o.Kilometers,
                IsDone = o.IsDone,
                Jobs = o.Jobs.Select(j => new JobListViewModel
                {
                    Id = j.Id,
                    Type = j.TypeName,
                    Description = j.Description,
                    Status = j.Status == Shared.Enums.JobStatus.Pending ? "pending" :
                             j.Status == Shared.Enums.JobStatus.InProgress ? "inprogress" : "finished",
                    MechanicName = j.MechanicName,
                    StartTime = j.StartTime,
                    EndTime = j.EndTime,
                    LaborCost = j.LaborCost,
                }).ToList()
            }).ToList();
        }

        public async Task<object> CreateOrderAsync(string userId, string workshopId, CreateOrderViewModel model)
        {
            var car = await _context.Cars
                .Include(c => c.Owner)
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarMake)
                .FirstOrDefaultAsync(c => c.Id == model.CarId && c.Owner.WorkshopId == workshopId);

            if (car == null)
            {
                throw new Exception("Car not found or access denied.");
            }

            var order = new Order
            {
                CarId = model.CarId,
                Kilometers = model.Kilometers,
                IsDone = false
            };

            _context.Orders.Add(order);

            foreach (var jobModel in model.Jobs)
            {
                var job = new Job
                {
                    OrderId = order.Id,
                    JobTypeId = jobModel.JobTypeId,
                    Description = jobModel.Description,
                    WorkerId = jobModel.WorkerId,
                    Status = jobModel.Status,
                    LaborCost = jobModel.LaborCost,
                    StartTime = jobModel.StartTime,
                    EndTime = jobModel.EndTime,
                };
                _context.Jobs.Add(job);
                
                foreach (var partModel in jobModel.Parts)
                {
                    var part = await _context.Parts.FindAsync(partModel.PartId);
                    if (part != null)
                    {
                        var jobPart = new JobPart
                        {
                            JobId = job.Id,
                            PartId = part.Id,
                            Quantity = partModel.Quantity,
                            Price = part.Price
                        };
                        _context.JobParts.Add(jobPart);

                        if (part.Quantity >= partModel.Quantity)
                        {
                            part.Quantity -= partModel.Quantity;
                        } 
                    }
                }
            }

            await _context.SaveChangesAsync();
            
            await _activityLogService.LogActionAsync(
                userId, 
                workshopId, 
                "created", 
                order.Id, 
                $"{car.Model.CarMake.Name} {car.Model.Name} ({car.RegistrationNumber})", 
                "Order");

            return new { orderId = order.Id, message = "Order created successfully" };
        }

        public async Task<OrderDetailsViewModel?> GetOrderByIdAsync(string id, string workshopId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id && o.Car.Owner.WorkshopId == workshopId)
                .Select(o => new OrderDetailsViewModel
                {
                    Id = o.Id,
                    CarId = o.CarId,
                    CarName = o.Car.Model.CarMake.Name + " " + o.Car.Model.Name,
                    CarRegistrationNumber = o.Car.RegistrationNumber,
                    ClientName = o.Car.Owner.Name,
                    Kilometers = o.Kilometers,
                    IsDone = o.IsDone,
                    Jobs = o.Jobs.Select(j => new JobDetailsViewModel
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
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<object> UpdateOrderAsync(string userId, string id, string workshopId, UpdateOrderViewModel model)
        {
            var order = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Owner)
                .Include(o => o.Car.Model.CarMake)
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.JobParts)
                        .ThenInclude(jp => jp.Part)
                .FirstOrDefaultAsync(o => o.Id == id && o.Car.Owner.WorkshopId == workshopId);

            if (order == null)
            {
                throw new Exception("Order not found or access denied.");
            }

            order.CarId = model.CarId;
            order.Kilometers = model.Kilometers;
            order.IsDone = model.IsDone;

            if (order.IsDone)
            {
                order.Car.Kilometers = order.Kilometers;
            }

            var jobIdsInModel = model.Jobs.Where(j => j.Id != null).Select(j => j.Id).ToList();
            var jobsToRemove = order.Jobs.Where(j => !jobIdsInModel.Contains(j.Id)).ToList();

            foreach (var job in jobsToRemove)
            {
                foreach (var jp in job.JobParts)
                {
                    if (jp.Part != null)
                    {
                        jp.Part.Quantity += jp.Quantity;
                    }
                }
                _context.Jobs.Remove(job);
            }

            foreach (var jobModel in model.Jobs)
            {
                Job? job;
                if (jobModel.Id != null)
                {
                    job = order.Jobs.FirstOrDefault(j => j.Id == jobModel.Id);
                    if (job == null) continue;
                }
                else
                {
                    job = new Job { OrderId = order.Id };
                    _context.Jobs.Add(job);
                }

                job.JobTypeId = jobModel.JobTypeId;
                job.WorkerId = jobModel.WorkerId;
                job.Description = jobModel.Description;
                job.Status = jobModel.Status;
                job.LaborCost = jobModel.LaborCost;
                job.StartTime = jobModel.StartTime;
                job.EndTime = jobModel.EndTime;

                var partIdsInModel = jobModel.Parts.Select(p => p.PartId).ToList();
                var partsToRemove = job.JobParts.Where(jp => !partIdsInModel.Contains(jp.PartId)).ToList();
                foreach (var jp in partsToRemove)
                {
                    if (jp.Part != null)
                    {
                        jp.Part.Quantity += jp.Quantity;
                    }
                    _context.JobParts.Remove(jp);
                }

                foreach (var partModel in jobModel.Parts)
                {
                    var existingJobPart = job.JobParts.FirstOrDefault(jp => jp.PartId == partModel.PartId);
                    var part = await _context.Parts.FindAsync(partModel.PartId);
                    if (part == null) continue;

                    if (existingJobPart != null)
                    {
                        int diff = partModel.Quantity - existingJobPart.Quantity;
                        if (part.Quantity >= diff)
                        {
                            part.Quantity -= diff;
                            existingJobPart.Quantity = partModel.Quantity;
                        }
                    }
                    else
                    {
                        var newJobPart = new JobPart
                        {
                            JobId = job.Id,
                            PartId = part.Id,
                            Quantity = partModel.Quantity,
                            Price = part.Price
                        };
                        _context.JobParts.Add(newJobPart);
                        if (part.Quantity >= partModel.Quantity)
                        {
                            part.Quantity -= partModel.Quantity;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            string actionText = order.IsDone ? "completed" : "updated details of";
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                actionText,
                order.Id,
                $"{order.Car.Model.CarMake.Name} {order.Car.Model.Name} ({order.Car.RegistrationNumber})",
                "Order");

            return new { orderId = order.Id, message = "Order updated successfully" };
        }

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

        public async Task CreateJobAsync(string userId, string orderId, string workshopId, CreateJobViewModel model)
        {
            var order = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Owner)
                .Include(o => o.Car.Model.CarMake)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.Car.Owner.WorkshopId == workshopId);

            if (order == null) throw new Exception("Order not found or access denied.");

            var jobType = await _context.JobTypes.FindAsync(model.JobTypeId);
            string jobTypeName = jobType?.Name ?? "Job";

            var job = new Job
            {
                OrderId = orderId,
                JobTypeId = model.JobTypeId,
                Description = model.Description,
                WorkerId = model.WorkerId,
                Status = model.Status,
                LaborCost = model.LaborCost,
                StartTime = model.StartTime,
                EndTime = model.EndTime
            };

            _context.Jobs.Add(job);
            
            foreach (var partModel in model.Parts)
            {
                var part = await _context.Parts.FindAsync(partModel.PartId);
                if (part != null)
                {
                    var jobPart = new JobPart
                    {
                        JobId = job.Id,
                        PartId = part.Id,
                        Quantity = partModel.Quantity,
                        Price = part.Price
                    };
                    _context.JobParts.Add(jobPart);

                    if (part.Quantity >= partModel.Quantity)
                    {
                        part.Quantity -= partModel.Quantity;
                    }
                }
            }

            await _context.SaveChangesAsync();

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"added job '{jobTypeName}' to",
                order.Id,
                $"{order.Car.Model.CarMake.Name} {order.Car.Model.Name} ({order.Car.RegistrationNumber})",
                "Order");
        }

        public async Task UpdateJobAsync(string userId, string jobId, string workshopId, UpdateJobViewModel model)
        {
            var job = await _context.Jobs
                .Include(j => j.Order)
                    .ThenInclude(o => o.Car)
                        .ThenInclude(c => c.Owner)
                .Include(j => j.Order.Car.Model.CarMake)
                .Include(j => j.JobParts)
                    .ThenInclude(jp => jp.Part)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.Order.Car.Owner.WorkshopId == workshopId);

            if (job == null) throw new Exception("Job not found or access denied.");

            var jobType = await _context.JobTypes.FindAsync(model.JobTypeId);
            string jobTypeName = jobType?.Name ?? "Job";

            job.JobTypeId = model.JobTypeId;
            job.WorkerId = model.WorkerId;
            job.Description = model.Description;
            job.Status = model.Status;
            job.LaborCost = model.LaborCost;
            job.StartTime = model.StartTime;
            job.EndTime = model.EndTime;

            // Parts sync
            var partIdsInModel = model.Parts.Select(p => p.PartId).ToList();
            var partsToRemove = job.JobParts.Where(jp => !partIdsInModel.Contains(jp.PartId)).ToList();
            
            foreach (var jp in partsToRemove)
            {
                if (jp.Part != null)
                {
                    jp.Part.Quantity += jp.Quantity;
                }
                _context.JobParts.Remove(jp);
            }

            foreach (var partModel in model.Parts)
            {
                var existingJobPart = job.JobParts.FirstOrDefault(jp => jp.PartId == partModel.PartId);
                var part = await _context.Parts.FindAsync(partModel.PartId);
                if (part == null) continue;

                if (existingJobPart != null)
                {
                    int diff = partModel.Quantity - existingJobPart.Quantity;
                    if (part.Quantity >= diff)
                    {
                        part.Quantity -= diff;
                        existingJobPart.Quantity = partModel.Quantity;
                    }
                }
                else
                {
                    var newJobPart = new JobPart
                    {
                        JobId = job.Id,
                        PartId = part.Id,
                        Quantity = partModel.Quantity,
                        Price = part.Price
                    };
                    _context.JobParts.Add(newJobPart);
                    if (part.Quantity >= partModel.Quantity)
                    {
                        part.Quantity -= partModel.Quantity;
                    }
                }
            }

            await _context.SaveChangesAsync();

            string statusName = model.Status.ToString();
            string actionText = model.Status == Shared.Enums.JobStatus.Done 
                ? $"finished job '{jobTypeName}' for" 
                : $"changed status of job '{jobTypeName}' to {statusName} for";

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                actionText,
                job.OrderId,
                $"{job.Order.Car.Model.CarMake.Name} {job.Order.Car.Model.Name} ({job.Order.Car.RegistrationNumber})",
                "Order");
        }
    }
}
