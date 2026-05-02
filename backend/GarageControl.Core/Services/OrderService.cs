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
using GarageControl.Shared.Enums;

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

        public async Task<List<OrderListVM>> GetOrdersAsync(string workshopId, bool? isArchived = null)
        {
            var activeOrders = new List<OrderListVM>();
            var archivedOrders = new List<OrderListVM>();

            if (isArchived == null || isArchived == false)
            {
                var rawData = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.Car.Owner.WorkshopId == workshopId && !o.IsArchived)
                    .Select(o => new
                    {
                        o.Id,
                        o.CarId,
                        CarMakeName = o.Car.Model.CarMake.Name,
                        CarModelName = o.Car.Model.Name,
                        o.Car.RegistrationNumber,
                        o.Car.Owner.Name,
                        o.Kilometers,
                        o.IsArchived
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
                    IsArchived = o.IsArchived
                }).ToList();
            }

            if (isArchived == null || isArchived == true)
            {
                var rawArchived = await _context.OrderSnapshots
                    .AsNoTracking()
                    .Where(o => o.WorkshopId == workshopId)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderId,
                        o.CarName,
                        o.CarRegistrationNumber,
                        o.ClientName,
                        o.Kilometers
                    })
                    .ToListAsync();

                archivedOrders = rawArchived.Select(o => new OrderListVM
                {
                    Id = o.OrderId,
                    CarId = "",
                    CarName = o.CarName,
                    CarRegistrationNumber = o.CarRegistrationNumber,
                    ClientName = o.ClientName,
                    Kilometers = o.Kilometers,
                    IsArchived = true
                }).ToList();
            }

            return activeOrders.Concat(archivedOrders).ToList();
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
                    IsArchived = o.IsArchived
                })
                .FirstOrDefaultAsync();

            if (activeOrder != null && !activeOrder.IsArchived) return activeOrder;

            return await _context.OrderSnapshots
                .AsNoTracking()
                .Where(o => o.OrderId == id && o.WorkshopId == workshopId)
                .Select(o => new OrderDetailsVM
                {
                    Id = o.OrderId,
                    CarId = "",
                    CarName = o.CarName,
                    CarRegistrationNumber = o.CarRegistrationNumber,
                    ClientName = o.ClientName,
                    Kilometers = o.Kilometers,
                    IsArchived = true
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

            if (car == null) throw new System.Exception("Car not found or access denied.");

            if (model.Kilometers < 0)
                throw new System.ArgumentException("Kilometers cannot be negative.");

            if (model.Kilometers < car.Kilometers)
                throw new System.ArgumentException($"Kilometers cannot be decreased. Current car value is {car.Kilometers}.");

            var order = new Order
            {
                CarId = model.CarId,
                Kilometers = model.Kilometers,
                IsArchived = false
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
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null || order.Car.Owner.WorkshopId != workshopId)
            {
                return new MethodResponseVM(false, "Order not found.");
            }
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

            if (model.IsArchived && !order.IsArchived)
            {
                // Check if all jobs are done
                if (order.Jobs.Any(j => j.Status != JobStatus.Done))
                {
                    return new MethodResponseVM(false, "Cannot archive order with incomplete jobs.");
                }

                // Create OrderSnapshot
                var orderSnapshot = new OrderSnapshot
                {
                    OrderId = order.Id,
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
                    var jobSnapshot = new JobSnapshot
                    {
                        JobId = job.Id,
                        OrderSnapshotId = orderSnapshot.Id,
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
                        jobSnapshot.JobPartSnapshots.Add(new JobPartSnapshot
                        {
                            JobPartId = job.OrderId + "_" + jp.PartId,
                            JobSnapshotId = jobSnapshot.Id,
                            PartId = jp.PartId,
                            PartName = jp.Part.Name,
                            UsedQuantity = jp.UsedQuantity,
                            Price = jp.Price
                        });
                    }

                    orderSnapshot.JobSnapshots.Add(jobSnapshot);
                }

                _context.OrderSnapshots.Add(orderSnapshot);
                order.IsArchived = true;
                changes.Add(new ActivityPropertyChange("status", "open", "archived"));
            }
            else
            {
                order.Kilometers = model.Kilometers;
                order.IsArchived = model.IsArchived;
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
            var snapshotInvoice = await _context.OrderSnapshots
                .AsNoTracking()
                .Where(o => o.OrderId == id)
                .AsSplitQuery()
                .Select(o => new OrderInvoiceVM
                {
                    OrderId = o.OrderId,
                    WorkshopName = o.WorkshopName,
                    WorkshopAddress = o.WorkshopAddress,
                    WorkshopPhone = o.WorkshopPhone,
                    WorkshopEmail = o.WorkshopEmail,
                    WorkshopRegistrationNumber = o.WorkshopRegistrationNumber,
                    CarName = o.CarName,
                    CarRegistrationNumber = o.CarRegistrationNumber,
                    ClientName = o.ClientName,
                    Kilometers = o.Kilometers,
                    Jobs = o.JobSnapshots.Select(j => new JobInvoiceVM
                    {
                        JobTypeName = j.JobTypeName,
                        Description = j.Description ?? "",
                        MechanicName = j.MechanicName,
                        LaborCost = j.LaborCost,
                        Parts = j.JobPartSnapshots.Select(jp => new JobPartDetailsVM
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

            if (snapshotInvoice != null) return snapshotInvoice;

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

            var snapshot = await _context.OrderSnapshots
                .Include(o => o.JobSnapshots)
                    .ThenInclude(j => j.JobPartSnapshots)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.WorkshopId == workshopId);

            if (snapshot != null)
            {
                _context.OrderSnapshots.Remove(snapshot);
                await _context.SaveChangesAsync();

                await _activityLogger.LogOrderDeletedAsync(userId, workshopId, $"{snapshot.CarName} ({snapshot.CarRegistrationNumber})");

                return new MethodResponseVM(true, "Archived order snapshot deleted successfully.");
            }

            return new MethodResponseVM(false, "Order not found or access denied.");
        }

        public async Task<string> GenerateInvoiceAsync(string orderId, string workshopId)
        {
            // Ensure table exists (bypass broken migration history)
            await _context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Invoices]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Invoices] (
                        [Id] int NOT NULL IDENTITY,
                        [OrderId] nvarchar(max) NOT NULL,
                        [WorkshopId] nvarchar(max) NOT NULL,
                        [GeneratedAt] datetime2 NOT NULL,
                        [InvoiceNumber] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id])
                    );
                END
            ");

            var invoice = new Invoice
            {
                OrderId = orderId,
                WorkshopId = workshopId,
                GeneratedAt = DateTime.UtcNow,
                InvoiceNumber = "PENDING" // Temporary placeholder
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync(); // This will populate invoice.Id

            // Now format the number
            var year = DateTime.UtcNow.Year;
            invoice.InvoiceNumber = $"INV-{year}-{invoice.Id:D6}";
            
            await _context.SaveChangesAsync();
            return invoice.InvoiceNumber;
        }
    }
}
