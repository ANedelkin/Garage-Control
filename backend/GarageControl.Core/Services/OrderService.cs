using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GarageControl.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly GarageControlDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly OrderActivityLogger _activityLogger;

        public OrderService(
            GarageControlDbContext context, 
            INotificationService notificationService,
            IActivityLogService activityLogService)
        {
            _context = context;
            _notificationService = notificationService;
            _activityLogger = new OrderActivityLogger(activityLogService);
        }
        public async Task<List<OrderListViewModel>> GetOrdersAsync(string workshopId, bool? isDone = null)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.Car.Owner.WorkshopId == workshopId);

            if (isDone.HasValue)
                query = query.Where(o => o.IsDone == isDone.Value);

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
                    Status = j.Status == Shared.Enums.JobStatus.AwaitingParts ? "awaitingparts" :
                             j.Status == Shared.Enums.JobStatus.Pending ? "pending" :
                             j.Status == Shared.Enums.JobStatus.InProgress ? "inprogress" : "finished",
                    MechanicName = j.MechanicName,
                    StartTime = j.StartTime,
                    EndTime = j.EndTime,
                    LaborCost = j.LaborCost,
                }).ToList()
            }).ToList();
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

        public async Task<object> CreateOrderAsync(string userId, string workshopId, CreateOrderViewModel model)
        {
            var car = await _context.Cars
                .Include(c => c.Owner)
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarMake)
                .FirstOrDefaultAsync(c => c.Id == model.CarId && c.Owner.WorkshopId == workshopId);

            if (car == null) throw new Exception("Car not found or access denied.");

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
                    Order = order,
                    JobTypeId = jobModel.JobTypeId,
                    WorkerId = jobModel.WorkerId,
                    Description = jobModel.Description,
                    Status = jobModel.Status,
                    LaborCost = jobModel.LaborCost,
                    StartTime = jobModel.StartTime,
                    EndTime = jobModel.EndTime,
                    JobParts = new List<JobPart>()
                };

                _context.Jobs.Add(job);

                foreach (var partModel in jobModel.Parts)
                {
                    var part = await _context.Parts.FindAsync(partModel.PartId);
                    if (part == null) continue;

                    var jobPart = new JobPart
                    {
                        Job = job,
                        PartId = part.Id,
                        Quantity = partModel.Quantity,
                        Price = part.Price
                    };
                    _context.JobParts.Add(jobPart);

                    if (job.Status == Shared.Enums.JobStatus.AwaitingParts)
                        part.AvailabilityBalance -= partModel.Quantity;
                    else
                    {
                        if (part.Quantity >= partModel.Quantity)
                        {
                            part.Quantity -= partModel.Quantity;
                            part.AvailabilityBalance -= partModel.Quantity;
                        }
                        else
                        {
                            return new { orderId = order.Id, success = false, message = $"Insufficient stock for part '{part.Name}'" };
                        }
                    }

                    if (part.AvailabilityBalance < part.MinimumQuantity)
                    {
                        await _notificationService.SendStockNotificationAsync(workshopId, part.Id, part.Name, part.AvailabilityBalance, part.MinimumQuantity);
                    }
                }
            }

            await _context.SaveChangesAsync();

            // --- log via the activity logger ---
            await _activityLogger.LogOrderCreatedAsync(userId, workshopId, order);

            return new { orderId = order.Id, message = "Order created successfully" };
        }

        public async Task<object> UpdateOrderAsync(string userId, string id, string workshopId, UpdateOrderViewModel model)
        {
            var order = await _context.Orders
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.JobParts)
                .Include(o => o.Car)
                    .ThenInclude(c => c.Owner)
                .FirstOrDefaultAsync(o => o.Id == id && o.Car.Owner.WorkshopId == workshopId);

            if (order == null)
                return new { success = false, message = "Order not found." };

            order.Kilometers = model.Kilometers;
            order.IsDone = model.IsDone;

            foreach (var jobModel in model.Jobs)
            {
                var existingJob = order.Jobs.FirstOrDefault(j => j.Id == jobModel.Id);
                if (existingJob != null)
                {
                    existingJob.Description = jobModel.Description;
                    existingJob.LaborCost = jobModel.LaborCost;
                    existingJob.Status = jobModel.Status;
                    existingJob.StartTime = jobModel.StartTime;
                    existingJob.EndTime = jobModel.EndTime;

                    foreach (var partModel in jobModel.Parts)
                    {
                        var existingJobPart = existingJob.JobParts.FirstOrDefault(jp => jp.PartId == partModel.PartId);
                        if (existingJobPart != null)
                        {
                            existingJobPart.Quantity = partModel.Quantity;
                        }
                        else
                        {
                            _context.JobParts.Add(new JobPart
                            {
                                JobId = existingJob.Id,
                                PartId = partModel.PartId,
                                Quantity = partModel.Quantity
                            });
                        }
                    }
                }
                else
                {
                    var newJob = new Job
                    {
                        OrderId = order.Id,
                        JobTypeId = jobModel.JobTypeId,
                        WorkerId = jobModel.WorkerId,
                        Description = jobModel.Description,
                        Status = jobModel.Status,
                        LaborCost = jobModel.LaborCost,
                        StartTime = jobModel.StartTime,
                        EndTime = jobModel.EndTime
                    };
                    _context.Jobs.Add(newJob);
                }
            }

            await _context.SaveChangesAsync();

            // --- log via the activity logger ---
            await _activityLogger.LogOrderUpdatedAsync(userId, workshopId, order);

            return new { orderId = order.Id, message = "Order updated successfully" };
        }
    }
}
