using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Shared;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageControl.Core.Models;

namespace GarageControl.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly GarageControlDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly OrderActivityLogger _activityLogger;
        private readonly IWorkshopService _workshopService;
        private readonly IInventoryService _inventoryService;
        private readonly IJobService _jobService;

        public OrderService(
            GarageControlDbContext context,
            INotificationService notificationService,
            IActivityLogService activityLogService,
            IWorkshopService workshopService,
            IInventoryService inventoryService,
            IJobService jobService)
        {
            _context = context;
            _notificationService = notificationService;
            _activityLogger = new OrderActivityLogger(activityLogService);
            _workshopService = workshopService;
            _inventoryService = inventoryService;
            _jobService = jobService;
        }
        public async Task<List<OrderListVM>> GetOrdersAsync(string workshopId, bool? isDone = null)
        {
            var activeOrders = new List<OrderListVM>();
            var completedOrders = new List<OrderListVM>();

            if (isDone == null || isDone == false)
            {
                var rawData = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.Car.Owner.WorkshopId == workshopId)
                    .Select(o => new
                    {
                        o.Id,
                        o.CarId,
                        CarMakeName = o.Car.Model.CarMake.Name,
                        CarModelName = o.Car.Model.Name,
                        o.Car.RegistrationNumber,
                        o.Car.Owner.Name,
                        o.Kilometers,
                        o.IsDone
                    })
                    .ToListAsync();

                activeOrders = rawData.Select(o => new OrderListVM
                {
                    Id = o.Id,
                    CarId = o.CarId,
                    CarName = $"{o.CarMakeName} {o.CarModelName}",
                    CarRegistrationNumber = o.RegistrationNumber,
                    ClientName = o.Name,
                    Kilometers = o.Kilometers,
                    IsDone = o.IsDone
                }).ToList();
            }

            if (isDone == null || isDone == true)
            {
                var rawCompleted = await _context.CompletedOrders
                    .AsNoTracking()
                    .Where(o => o.WorkshopId == workshopId)
                    .Select(o => new
                    {
                        o.Id,
                        CarName = o.CarName,
                        CarRegistrationNumber = o.CarRegistrationNumber,
                        ClientName = o.ClientName,
                        o.Kilometers
                    })
                    .ToListAsync();

                completedOrders = rawCompleted.Select(o => new OrderListVM
                {
                    Id = o.Id,
                    CarId = "", // We don't have the active car ID reference stored directly or it's not strictly needed for the list
                    CarName = o.CarName,
                    CarRegistrationNumber = o.CarRegistrationNumber,
                    ClientName = o.ClientName,
                    Kilometers = o.Kilometers,
                    IsDone = true
                }).ToList();
            }

            return activeOrders.Concat(completedOrders).ToList();
        }
        public async Task<OrderDetailsVM?> GetOrderByIdAsync(string id, string workshopId)
        {
            var activeOrder = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id && o.Car.Owner.WorkshopId == workshopId)
                .Select(o => new OrderDetailsVM
                {
                    Id = o.Id,
                    CarId = o.CarId,
                    CarName = o.Car.Model.CarMake.Name + " " + o.Car.Model.Name,
                    CarRegistrationNumber = o.Car.RegistrationNumber,
                    ClientName = o.Car.Owner.Name,
                    Kilometers = o.Kilometers,
                    IsDone = o.IsDone
                })
                .FirstOrDefaultAsync();

            if (activeOrder != null) return activeOrder;

            return await _context.CompletedOrders
                .AsNoTracking()
                .Where(o => o.Id == id && o.WorkshopId == workshopId)
                .Select(o => new OrderDetailsVM
                {
                    Id = o.Id,
                    CarId = "", // Not needed for completed
                    CarName = o.CarName,
                    CarRegistrationNumber = o.CarRegistrationNumber,
                    ClientName = o.ClientName,
                    Kilometers = o.Kilometers,
                    IsDone = true
                })
                .FirstOrDefaultAsync();
        }

        public async Task<object> CreateOrderAsync(string userId, string workshopId, CreateOrderVM model)
        {
            var car = await _context.Cars
                .Include(c => c.Owner)
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarMake)
                .FirstOrDefaultAsync(c => c.Id == model.CarId && c.Owner.WorkshopId == workshopId);

            if (car == null) throw new Exception("Car not found or access denied.");

            if (model.Kilometers < 0)
                throw new ArgumentException("Kilometers cannot be negative.");

            if (model.Kilometers < car.Kilometers)
                throw new ArgumentException($"Kilometers cannot be decreased. Current car value is {car.Kilometers}.");

            var order = new Order
            {
                CarId = model.CarId,
                Kilometers = model.Kilometers,
                IsDone = false
            };
            _context.Orders.Add(order);
            
            int oldKm = car.Kilometers;
            
            // Sync car kilometers
            car.Kilometers = model.Kilometers;
            
            await _context.SaveChangesAsync();

            // --- log via the activity logger ---
            var createChanges = new List<ActivityPropertyChange>();
            if (model.Kilometers > oldKm)
                createChanges.Add(new ActivityPropertyChange("odometer", oldKm.ToString(), model.Kilometers.ToString()));

            string carInfo = $"{car.Model.CarMake.Name} {car.Model.Name} ({car.RegistrationNumber})";
            await _activityLogger.LogOrderCreatedAsync(userId, workshopId, order.Id, carInfo, createChanges);

            return new MethodResponseVM(true, "Order created successfully", new { orderId = order.Id });
        }

        public async Task<object> UpdateOrderAsync(string userId, string id, string workshopId, UpdateOrderVM model)
        {
            var order = await _context.Orders
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.JobParts)
                        .ThenInclude(jp => jp.Part)
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.JobType)
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.Worker)
                .Include(o => o.Car)
                    .ThenInclude(c => c.Owner)
                        .ThenInclude(owner => owner.Workshop)
                .Include(o => o.Car.Model.CarMake)
                .Include(o => o.Car.Model)
                .AsSplitQuery()
                .FirstOrDefaultAsync(o => o.Id == id && o.Car.Owner.WorkshopId == workshopId);

            if (order == null)
                return new MethodResponseVM(false, "Order not found.");

            var changes = new List<ActivityPropertyChange>();

            if (model.Kilometers < 0)
                return new MethodResponseVM(false, "Kilometers cannot be negative.");

            if (model.Kilometers < order.Kilometers)
                return new MethodResponseVM(false, $"Kilometers cannot be decreased. Current value is {order.Kilometers}.");

            if (order.CarId != model.CarId)
            {
                var newCar = await _context.Cars
                    .Include(c => c.Model)
                        .ThenInclude(m => m.CarMake)
                    .FirstOrDefaultAsync(c => c.Id == model.CarId && c.Owner.WorkshopId == workshopId);
                
                if (newCar != null)
                {
                    changes.Add(new ActivityPropertyChange("car", 
                        $"{order.Car.Model.CarMake.Name} {order.Car.Model.Name} ({order.Car.RegistrationNumber})",
                        $"{newCar.Model.CarMake.Name} {newCar.Model.Name} ({newCar.RegistrationNumber})"));
                    
                    order.CarId = model.CarId;
                    // When changing car, we should also update the new car's kilometers if order's kilometers are higher
                    if (model.Kilometers > newCar.Kilometers)
                    {
                        newCar.Kilometers = model.Kilometers;
                    }
                }
            }

            if (order.Kilometers != model.Kilometers)
                changes.Add(new ActivityPropertyChange("odometer", order.Kilometers.ToString(), model.Kilometers.ToString()));
            if (model.IsDone && !order.IsDone)
            {
                // Migate to CompletedOrder
                var completedOrder = new CompletedOrder
                {
                    Id = order.Id,
                    WorkshopId = order.Car.Owner.WorkshopId,
                    CompletionDate = DateTime.UtcNow,
                    WorkshopName = order.Car.Owner.Workshop.Name,
                    WorkshopAddress = order.Car.Owner.Workshop.Address,
                    WorkshopPhone = order.Car.Owner.Workshop.PhoneNumber,
                    WorkshopEmail = order.Car.Owner.Workshop.Email ?? "",
                    WorkshopRegistrationNumber = order.Car.Owner.Workshop.RegistrationNumber ?? "",
                    CarName = $"{order.Car.Model.CarMake.Name} {order.Car.Model.Name}",
                    CarRegistrationNumber = order.Car.RegistrationNumber,
                    ClientName = order.Car.Owner.Name,
                    Kilometers = model.Kilometers
                };

                foreach (var job in order.Jobs)
                {
                    var completedJob = new CompletedJob
                    {
                        Id = job.Id,
                        CompletedOrderId = completedOrder.Id,
                        JobTypeId = job.JobTypeId,
                        WorkerId = job.WorkerId,
                        JobTypeName = job.JobType.Name,
                        Description = job.Description,
                        MechanicName = job.Worker.Name,
                        LaborCost = job.LaborCost,
                        StartTime = job.StartTime,
                        EndTime = job.EndTime
                    };

                    foreach (var jp in job.JobParts)
                    {
                        completedJob.CompletedJobParts.Add(new CompletedJobPart
                        {
                            Id = Guid.NewGuid().ToString(),
                            CompletedJobId = completedJob.Id,
                            PartId = jp.PartId,
                            PartName = jp.Part.Name,
                            UsedQuantity = jp.UsedQuantity,
                            Price = jp.Price
                        });
                    }

                    completedOrder.CompletedJobs.Add(completedJob);
                }

                _context.CompletedOrders.Add(completedOrder);

                // Use JobService to delete each job properly to satisfy FK restrict constraints
                foreach (var job in order.Jobs.ToList())
                {
                    await _jobService.DeleteJobAsync(userId, job.Id, workshopId, skipLogging: true);
                }

                _context.Orders.Remove(order);
                changes.Add(new ActivityPropertyChange("status", "open", "done"));
            }
            else
            {
                order.Kilometers = model.Kilometers;
                order.IsDone = model.IsDone;
            }

            // Sync car kilometers
            order.Car.Kilometers = model.Kilometers;
            
            await _context.SaveChangesAsync();

            // --- log via the activity logger ---
            string carInfo = $"{order.Car.Model.CarMake.Name} {order.Car.Model.Name} ({order.Car.RegistrationNumber})";
            await _activityLogger.LogOrderUpdatedAsync(userId, workshopId, order.Id, carInfo, changes);

            return new MethodResponseVM(true, "Order updated successfully", new { orderId = order.Id });
        }

        public async Task<OrderInvoiceVM?> GetOrderInvoiceByIdAsync(string id)
        {
            var completedInvoice = await _context.CompletedOrders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .AsSplitQuery()
                .Select(o => new OrderInvoiceVM
                {
                    OrderId = o.Id,
                    WorkshopName = o.WorkshopName,
                    WorkshopAddress = o.WorkshopAddress,
                    WorkshopPhone = o.WorkshopPhone,
                    WorkshopEmail = o.WorkshopEmail,
                    WorkshopRegistrationNumber = o.WorkshopRegistrationNumber,
                    CarName = o.CarName,
                    CarRegistrationNumber = o.CarRegistrationNumber,
                    ClientName = o.ClientName,
                    Kilometers = o.Kilometers,
                    Jobs = o.CompletedJobs.Select(j => new JobInvoiceVM
                    {
                        JobTypeName = j.JobTypeName,
                        Description = j.Description ?? "",
                        MechanicName = j.MechanicName,
                        LaborCost = j.LaborCost,
                        Parts = j.CompletedJobParts.Select(jp => new JobPartDetailsVM
                        {
                            PartId = jp.PartId ?? "",
                            PartName = jp.PartName,
                            PlannedQuantity = 0,
                            SentQuantity = 0,
                            UsedQuantity = jp.UsedQuantity,
                            RequestedQuantity = 0,
                            Price = jp.Price
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (completedInvoice != null) return completedInvoice;

            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .AsSplitQuery()
                .Select(o => new OrderInvoiceVM
                {
                    OrderId = o.Id,
                    WorkshopName = o.Car.Owner.Workshop.Name,
                    WorkshopAddress = o.Car.Owner.Workshop.Address,
                    WorkshopPhone = o.Car.Owner.Workshop.PhoneNumber,
                    WorkshopEmail = o.Car.Owner.Workshop.Email ?? "Not provided.",
                    WorkshopRegistrationNumber = o.Car.Owner.Workshop.RegistrationNumber ?? "Not provided.",
                    CarName = o.Car.Model.CarMake.Name + " " + o.Car.Model.Name,
                    CarRegistrationNumber = o.Car.RegistrationNumber,
                    ClientName = o.Car.Owner.Name,
                    Kilometers = o.Kilometers,
                    Jobs = o.Jobs.Select(j => new JobInvoiceVM
                    {
                        JobTypeName = j.JobType.Name,
                        Description = j.Description ?? "",
                        MechanicName = j.Worker.Name,
                        LaborCost = j.LaborCost,
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
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<MethodResponseVM> DeleteOrderAsync(string userId, string id, string workshopId)
        {
            var activeOrder = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Owner)
                .Include(o => o.Car.Model)
                    .ThenInclude(m => m.CarMake)
                .Include(o => o.Jobs)
                .AsSplitQuery()
                .FirstOrDefaultAsync(o => o.Id == id && o.Car.Owner.WorkshopId == workshopId);

            if (activeOrder != null)
            {
                // Use JobService to delete each job properly
                foreach (var job in activeOrder.Jobs.ToList())
                {
                    await _jobService.DeleteJobAsync(userId, job.Id, workshopId, skipLogging: true);
                }

                string carInfo = $"{activeOrder.Car.Model.CarMake.Name} {activeOrder.Car.Model.Name} ({activeOrder.Car.RegistrationNumber})";

                _context.Orders.Remove(activeOrder);
                await _context.SaveChangesAsync();

                await _activityLogger.LogOrderDeletedAsync(userId, workshopId, carInfo);

                return new MethodResponseVM(true, "Order deleted successfully.");
            }

            var completedOrder = await _context.CompletedOrders
                .Include(o => o.CompletedJobs)
                    .ThenInclude(j => j.CompletedJobParts)
                .FirstOrDefaultAsync(o => o.Id == id && o.WorkshopId == workshopId);

            if (completedOrder != null)
            {
                _context.CompletedOrders.Remove(completedOrder);
                await _context.SaveChangesAsync();

                await _activityLogger.LogOrderDeletedAsync(userId, workshopId, $"{completedOrder.CarName} ({completedOrder.CarRegistrationNumber})");

                return new MethodResponseVM(true, "Completed order deleted successfully.");
            }

            return new MethodResponseVM(false, "Order not found or access denied.");
        }
    }
}
