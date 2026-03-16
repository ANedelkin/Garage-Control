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
                    o.IsDone
                })
                .ToListAsync();

            return rawData.Select(o => new OrderListVM
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
        public async Task<OrderDetailsVM?> GetOrderByIdAsync(string id, string workshopId)
        {
            return await _context.Orders
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
        }

        public async Task<object> CreateOrderAsync(string userId, string workshopId, CreateOrderVM model)
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
            
            // Sync car kilometers
            car.Kilometers = model.Kilometers;
            
            await _context.SaveChangesAsync();

            // --- log via the activity logger ---
            string carInfo = $"{car.Model.CarMake.Name} {car.Model.Name} ({car.RegistrationNumber})";
            await _activityLogger.LogOrderCreatedAsync(userId, workshopId, order.Id, carInfo);

            return new MethodResponseVM(true, "Order created successfully", new { orderId = order.Id });
        }

        public async Task<object> UpdateOrderAsync(string userId, string id, string workshopId, UpdateOrderVM model)
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
                return new MethodResponseVM(false, "Order not found.");

            var changes = new List<ActivityPropertyChange>();
            if (order.Kilometers != model.Kilometers)
                changes.Add(new ActivityPropertyChange("kilometers", order.Kilometers.ToString(), model.Kilometers.ToString()));
            if (order.IsDone != model.IsDone)
                changes.Add(new ActivityPropertyChange("status", order.IsDone ? "done" : "open", model.IsDone ? "done" : "open"));

            order.Kilometers = model.Kilometers;
            order.IsDone = model.IsDone;

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
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new OrderInvoiceVM
                {
                    OrderId = o.Id,
                    WorkshopName = o.Car.Owner.Workshop.Name,
                    WorkshopAddress = o.Car.Owner.Workshop.Address,
                    WorkshopPhone = o.Car.Owner.Workshop.PhoneNumber,
                    WorkshopEmail = o.Car.Owner.Workshop.Email ?? "Not provided.",
                    WorkshopRegistrationNumber = o.Car.Owner.Workshop.RegistrationNumber ?? "Not rovided",
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
            var order = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Owner)
                .Include(o => o.Jobs)
                .FirstOrDefaultAsync(o => o.Id == id && o.Car.Owner.WorkshopId == workshopId);

            if (order == null) return new MethodResponseVM(false, "Order not found or access denied.");

            // Use JobService to delete each job properly
            foreach (var job in order.Jobs.ToList())
            {
                await _jobService.DeleteJobAsync(userId, job.Id, workshopId);
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return new MethodResponseVM(true, "Order deleted successfully.");
        }
    }
}
