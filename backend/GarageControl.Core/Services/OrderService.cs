using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.Models;
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
        private readonly IWorkshopService _workshopService;
        private readonly IInventoryService _inventoryService;

        public OrderService(
            GarageControlDbContext context, 
            INotificationService notificationService,
            IActivityLogService activityLogService, 
            IWorkshopService workshopService,
            IInventoryService inventoryService)
        {
            _context = context;
            _notificationService = notificationService;
            _activityLogger = new OrderActivityLogger(activityLogService);
            _workshopService = workshopService;
            _inventoryService = inventoryService;
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
                    Status = j.Status == Shared.Enums.JobStatus.Pending ? "pending" :
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
                            PlannedQuantity = jp.PlannedQuantity,
                            SentQuantity = jp.SentQuantity,
                            UsedQuantity = jp.UsedQuantity,
                            RequestedQuantity = jp.RequestedQuantity,
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
                        PlannedQuantity = partModel.PlannedQuantity,
                        SentQuantity = partModel.SentQuantity,
                        UsedQuantity = partModel.UsedQuantity,
                        RequestedQuantity = partModel.RequestedQuantity,
                        Price = part.Price
                    };
                    _context.JobParts.Add(jobPart);

                    if (part.AvailabilityBalance >= partModel.PlannedQuantity)
                    {
                        part.AvailabilityBalance -= partModel.PlannedQuantity;
                    }
                    else
                    {
                        return new { orderId = order.Id, success = false, message = $"Insufficient availability for part '{part.Name}'" };
                    }

                    if (part.Quantity >= partModel.SentQuantity)
                    {
                        part.Quantity -= partModel.SentQuantity;
                    }
                    else
                    {
                         return new { orderId = order.Id, success = false, message = $"Insufficient stock for part '{part.Name}'" };
                    }

                    await _context.SaveChangesAsync();
                    await _inventoryService.RecalculateAvailabilityBalanceAsync(workshopId, part.Id);
                    
                    var updatedPart = await _context.Parts.FindAsync(part.Id);
                    if (updatedPart != null && updatedPart.AvailabilityBalance < updatedPart.MinimumQuantity)
                    {
                        await _notificationService.SendStockNotificationAsync(workshopId, part.Id, updatedPart.Name, updatedPart.AvailabilityBalance, updatedPart.MinimumQuantity);
                    }
                }
            }

            await _context.SaveChangesAsync();

            // --- log via the activity logger ---
            string carInfo = $"{car.Model.CarMake.Name} {car.Model.Name} ({car.RegistrationNumber})";
            await _activityLogger.LogOrderCreatedAsync(userId, workshopId, carInfo);

            return new { orderId = order.Id, message = "Order created successfully" };
        }

        public async Task<object> UpdateOrderAsync(string userId, string id, string workshopId, UpdateOrderViewModel model)
        {
            var order = await _context.Orders
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.JobParts)
                .Include(o => o.Car)
                    .ThenInclude(c => c.Owner)
                .Include(o => o.Car.Model.CarMake)
                .Include(o => o.Car.Model)
                .FirstOrDefaultAsync(o => o.Id == id && o.Car.Owner.WorkshopId == workshopId);

            if (order == null)
                return new { success = false, message = "Order not found." };

            var changes = new List<ActivityPropertyChange>();
            if (order.Kilometers != model.Kilometers)
                changes.Add(new ActivityPropertyChange("kilometers", order.Kilometers.ToString(), model.Kilometers.ToString()));
            if (order.IsDone != model.IsDone)
                changes.Add(new ActivityPropertyChange("status", order.IsDone ? "done" : "open", model.IsDone ? "done" : "open"));

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
                        var part = await _context.Parts.FindAsync(partModel.PartId);
                        if (part == null) continue;

                        var existingJobPart = existingJob.JobParts.FirstOrDefault(jp => jp.PartId == partModel.PartId);
                        if (existingJobPart != null)
                        {
                            var sentDelta = partModel.SentQuantity - existingJobPart.SentQuantity;

                            if (part.Quantity < sentDelta)
                                return new { success = false, message = $"Insufficient stock for part '{part.Name}'" };

                            part.Quantity -= sentDelta;

                            existingJobPart.PlannedQuantity = partModel.PlannedQuantity;
                            existingJobPart.SentQuantity = partModel.SentQuantity;
                            existingJobPart.UsedQuantity = partModel.UsedQuantity;
                            existingJobPart.RequestedQuantity = partModel.RequestedQuantity;
                            
                            await _context.SaveChangesAsync();
                            await _inventoryService.RecalculateAvailabilityBalanceAsync(workshopId, part.Id);
                        }
                        else
                        {
                            if (part.Quantity < partModel.SentQuantity)
                                return new { success = false, message = $"Insufficient stock for part '{part.Name}'" };

                            part.Quantity -= partModel.SentQuantity;

                            _context.JobParts.Add(new JobPart
                            {
                                JobId = existingJob.Id,
                                PartId = partModel.PartId,
                                PlannedQuantity = partModel.PlannedQuantity,
                                SentQuantity = partModel.SentQuantity,
                                UsedQuantity = partModel.UsedQuantity,
                                RequestedQuantity = partModel.RequestedQuantity,
                                Price = part.Price
                            });

                            await _context.SaveChangesAsync();
                            await _inventoryService.RecalculateAvailabilityBalanceAsync(workshopId, part.Id);
                        }
                    }

                    if (jobModel.Status != existingJob.Status)
                    {
                        existingJob.Status = jobModel.Status;
                        foreach (var jp in existingJob.JobParts)
                        {
                            await _inventoryService.RecalculateAvailabilityBalanceAsync(workshopId, jp.PartId);
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
            string carInfo = $"{order.Car.Model.CarMake.Name} {order.Car.Model.Name} ({order.Car.RegistrationNumber})";
            await _activityLogger.LogOrderUpdatedAsync(userId, workshopId, carInfo, changes);

            return new { orderId = order.Id, message = "Order updated successfully" };
        }

        public async Task<OrderInvoiceViewModel?> GetOrderInvoiceByIdAsync(string id)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new OrderInvoiceViewModel
                {
                    OrderId = o.Id,
                    WorkshopName = o.Car.Owner.Workshop.Name,
                    WorkshopAddress = o.Car.Owner.Workshop.Address,
                    WorkshopPhone = o.Car.Owner.Workshop.PhoneNumber,
                    WorkshopEmail = o.Car.Owner.Workshop.Email?? "Not provided.",
                    WorkshopRegistrationNumber = o.Car.Owner.Workshop.RegistrationNumber??"Not rovided",
                    CarName = o.Car.Model.CarMake.Name + " " + o.Car.Model.Name,
                    CarRegistrationNumber = o.Car.RegistrationNumber,
                    ClientName = o.Car.Owner.Name,
                    Kilometers = o.Kilometers,
                    Jobs = o.Jobs.Select(j => new JobInvoiceViewModel
                    {
                        JobTypeName = j.JobType.Name,
                        Description = j.Description ?? "",
                        MechanicName = j.Worker.Name,
                        LaborCost = j.LaborCost,
                        Parts = j.JobParts.Select(jp => new JobPartDetailsViewModel
                        {
                            PartId = jp.PartId,
                            PartName = jp.Part.Name,
                            PlannedQuantity = jp.PlannedQuantity,
                            SentQuantity = jp.SentQuantity,
                            UsedQuantity = jp.UsedQuantity,
                            RequestedQuantity = jp.RequestedQuantity,
                            Price = jp.Price
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }
    }
}
