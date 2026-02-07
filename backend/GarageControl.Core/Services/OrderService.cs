using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.Models;
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
        Task<MethodResponse> CreateJobAsync(string userId, string orderId, string workshopId, CreateJobViewModel model);
        Task<MethodResponse> UpdateJobAsync(string userId, string jobId, string workshopId, UpdateJobViewModel model);
    }

    public class OrderService : IOrderService
    {
        private readonly GarageControlDbContext _context;
        private readonly IActivityLogService _activityLogService;
        private readonly INotificationService _notificationService;
        private readonly IPartService _partService;

        public OrderService(
            GarageControlDbContext context, 
            IActivityLogService activityLogService, 
            INotificationService notificationService,
            IPartService partService)
        {
            _context = context;
            _activityLogService = activityLogService;
            _notificationService = notificationService;
            _partService = partService;
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
                    Status = jobModel.Status, // Use the provided status
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

                        if (job.Status == Shared.Enums.JobStatus.AwaitingParts)
                        {
                            // When a job is created in AwaitingParts status, decrease AvailabilityBalance only
                            part.AvailabilityBalance -= partModel.Quantity;
                        }
                        else
                        {
                            // When a job is created in Pending/InProgress/Done status, decrease Quantity
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
            }

            await _context.SaveChangesAsync();
            
            string carName = $"{car.Model.CarMake.Name} {car.Model.Name} ({car.RegistrationNumber})";
            await _activityLogService.LogActionAsync(
                userId, 
                workshopId, 
                $"created <a href='/orders' class='log-link target-link'>order for {carName}</a>");

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
                    .ThenInclude(j => j.JobType)
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.JobParts)
                        .ThenInclude(jp => jp.Part)
                .FirstOrDefaultAsync(o => o.Id == id && o.Car.Owner.WorkshopId == workshopId);

            if (order == null)
            {
                throw new Exception("Order not found or access denied.");
            }

            var changes = new List<string>();
            string FormatQty(decimal q) => q.ToString("G29", CultureInfo.InvariantCulture);

            if (order.CarId != model.CarId)
            {
                var oldCar = order.Car;
                var newCar = await _context.Cars.Include(c => c.Model.CarMake).FirstOrDefaultAsync(c => c.Id == model.CarId);
                string oldCarDisp = $"{oldCar.Model.CarMake.Name} {oldCar.Model.Name} ({oldCar.RegistrationNumber})";
                string newCarDisp = newCar != null ? $"{newCar.Model.CarMake.Name} {newCar.Model.Name} ({newCar.RegistrationNumber})" : "Unknown";
                changes.Add($"car from <b>{oldCarDisp}</b> to <b>{newCarDisp}</b>");
            }

            if (order.Kilometers != model.Kilometers)
            {
                changes.Add($"kilometers from <b>{order.Kilometers}</b> to <b>{model.Kilometers}</b>");
            }
            if (order.IsDone != model.IsDone)
            {
                changes.Add($"status from <b>{(order.IsDone ? "Finished" : "Active")}</b> to <b>{(model.IsDone ? "Finished" : "Active")}</b>");
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
                        if (job.Status == Shared.Enums.JobStatus.AwaitingParts)
                        {
                            jp.Part.AvailabilityBalance += jp.Quantity;
                        }
                        else
                        {
                            jp.Part.Quantity += jp.Quantity;
                            jp.Part.AvailabilityBalance += jp.Quantity;
                        }
                    }
                }
                _context.Jobs.Remove(job);
                changes.Add($"removed job '<b>{job.JobType?.Name ?? "Job"}</b>'");
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
                    // New job - use the provided status
                    job = new Job { OrderId = order.Id, Status = jobModel.Status };
                    _context.Jobs.Add(job);
                    var jt = await _context.JobTypes.FindAsync(jobModel.JobTypeId);
                    changes.Add($"added job '<b>{jt?.Name ?? "Job"}</b>'");
                }

                // Internal job changes are not logged here in detail to avoid overwhelming the order log,
                // BUT the user specifically asked for job part changes and car changes.
                // Car change is above. Parts are below.
                
                var oldStatus = job.Status;
                var newStatus = jobModel.Status;

                // Update basic properties first
                job.JobTypeId = jobModel.JobTypeId;
                job.WorkerId = jobModel.WorkerId;
                job.Description = jobModel.Description;
                job.LaborCost = jobModel.LaborCost;
                job.StartTime = jobModel.StartTime;
                job.EndTime = jobModel.EndTime;

                var partIdsInModel = jobModel.Parts.Select(p => p.PartId).ToList();
                var partsToRemove = job.JobParts.Where(jp => !partIdsInModel.Contains(jp.PartId)).ToList();
                foreach (var jp in partsToRemove)
                {
                    if (jp.Part != null)
                    {
                        if (oldStatus == Shared.Enums.JobStatus.AwaitingParts)
                        {
                            // Removing from AwaitingParts job: increase AvailabilityBalance
                            jp.Part.AvailabilityBalance += jp.Quantity;
                        }
                        else
                        {
                            // Removing from non-AwaitingParts job: return to actual stock
                            jp.Part.Quantity += jp.Quantity;
                            jp.Part.AvailabilityBalance += jp.Quantity;
                        }
                    }
                    changes.Add($"removed part '<b>{jp.Part?.Name}</b>' from job '<b>{job.JobType?.Name}</b>'");
                    _context.JobParts.Remove(jp);
                }

                foreach (var partModel in jobModel.Parts)
                {
                    var existingJobPart = job.JobParts.FirstOrDefault(jp => jp.PartId == partModel.PartId);
                    var part = await _context.Parts.FindAsync(partModel.PartId);
                    if (part == null) continue;

                    if (existingJobPart != null)
                    {
                        if (existingJobPart.Quantity != partModel.Quantity)
                        {
                            changes.Add($"changed quantity of '<b>{part.Name}</b>' from <b>{FormatQty(existingJobPart.Quantity)}</b> to <b>{FormatQty(partModel.Quantity)}</b> in job '<b>{job.JobType?.Name}</b>'");
                            int diff = partModel.Quantity - existingJobPart.Quantity;
                            
                            if (oldStatus == Shared.Enums.JobStatus.AwaitingParts)
                            {
                                part.AvailabilityBalance -= diff;
                            }
                            else
                            {
                                if (part.Quantity >= diff)
                                {
                                    part.Quantity -= diff;
                                    part.AvailabilityBalance -= diff;
                                }
                                else
                                {
                                    return new MethodResponse(false, $"Insufficient stock for part '{part.Name}' in job '{job.JobType?.Name}'");
                                }
                            }
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
                        changes.Add($"added part '<b>{part.Name}</b>' to job '<b>{job.JobType?.Name}</b>'");
                        
                        if (oldStatus == Shared.Enums.JobStatus.AwaitingParts)
                        {
                            part.AvailabilityBalance -= partModel.Quantity;
                        }
                        else
                        {
                            if (part.Quantity >= partModel.Quantity)
                            {
                                part.Quantity -= partModel.Quantity;
                                part.AvailabilityBalance -= partModel.Quantity;
                            }
                            else
                            {
                                return new MethodResponse(false, $"Insufficient stock for part '{part.Name}' in job '{job.JobType?.Name}'");
                            }
                        }
                    }

                    if (part.AvailabilityBalance < part.MinimumQuantity)
                    {
                        await _notificationService.SendStockNotificationAsync(workshopId, part.Id, part.Name, part.AvailabilityBalance, part.MinimumQuantity);
                    }
                }

                // Handle status transition after all parts are processed
                if (oldStatus == Shared.Enums.JobStatus.AwaitingParts && newStatus != Shared.Enums.JobStatus.AwaitingParts)
                {
                    // Moving AWAY from AwaitingParts -> first update quantities, then verify stock
                    
                    // Step 1: Update the quantities for all parts
                    foreach (var jp in job.JobParts)
                    {
                        jp.Part.Quantity -= jp.Quantity;
                    }
                    
                    // Step 2: Verify that all parts have sufficient stock (should not go negative)
                    foreach (var jp in job.JobParts)
                    {
                        if (jp.Part.Quantity < 0)
                        {
                            // Revert the changes since we don't have enough stock
                            foreach (var jpRevert in job.JobParts)
                            {
                                jpRevert.Part.Quantity += jpRevert.Quantity;
                            }
                            return new MethodResponse(false, $"Insufficient stock for part '{jp.Part.Name}' in job '{job.JobType?.Name}'. Cannot transition to this status.");
                        }
                    }
                    // AvailabilityBalance stays the same because Quantity - (Awaiting-qty) = (Quantity-qty) - ((Awaiting-qty)-qty)
                }
                else if (oldStatus != Shared.Enums.JobStatus.AwaitingParts && newStatus == Shared.Enums.JobStatus.AwaitingParts)
                {
                    // Moving back TO AwaitingParts -> return quantity to stock
                    foreach (var jp in job.JobParts)
                    {
                        jp.Part.Quantity += jp.Quantity;
                        // AvailabilityBalance remains unchanged in the calculation
                    }
                }

                // Apply status change
                job.Status = newStatus;
            }

            await _context.SaveChangesAsync();

            if (changes.Count > 0)
            {
                string carName = $"{order.Car.Model.CarMake.Name} {order.Car.Model.Name} ({order.Car.RegistrationNumber})";
                string orderLink = $"<a href='/orders' class='log-link target-link'>order for {carName}</a>";
                string actionHtml;

                if (changes.Count == 1 && changes[0].Contains("from"))
                {
                    actionHtml = $"changed {changes[0]} of {orderLink}";
                }
                else if (changes.All(c => !c.Contains("from") && !c.Contains("added") && !c.Contains("removed")))
                {
                    actionHtml = $"updated details of {orderLink}";
                }
                else
                {
                    actionHtml = $"updated {orderLink}: {string.Join(", ", changes)}";
                }

                await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
            }

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

        public async Task<MethodResponse> CreateJobAsync(string userId, string orderId, string workshopId, CreateJobViewModel model)
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
                Status = model.Status, // Use the provided status
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

                    if (job.Status == Shared.Enums.JobStatus.AwaitingParts)
                    {
                        // When a job is created in AwaitingParts status, decrease AvailabilityBalance only
                        part.AvailabilityBalance -= partModel.Quantity;
                    }
                    else
                    {
                        // When a job is created in Pending/InProgress/Done status, decrease Quantity
                        if (part.Quantity >= partModel.Quantity)
                        {
                            part.Quantity -= partModel.Quantity;
                            part.AvailabilityBalance -= partModel.Quantity;
                        }
                        else
                        {
                            return new MethodResponse(false, $"Insufficient stock for part '{part.Name}'");
                        }
                    }

                    if (part.AvailabilityBalance < part.MinimumQuantity)
                    {
                        await _notificationService.SendStockNotificationAsync(workshopId, part.Id, part.Name, part.AvailabilityBalance, part.MinimumQuantity);
                    }
                }
            }

            await _context.SaveChangesAsync();

            string carName = $"{order.Car.Model.CarMake.Name} {order.Car.Model.Name} ({order.Car.RegistrationNumber})";
            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"added job '{jobTypeName}' to <a href='/orders' class='log-link target-link'>order for {carName}</a>");

            return new MethodResponse(true, "Job added successfully");
        }

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

            var changes = new List<string>();
            string oldJobTypeName = job.JobType.Name;

            string FormatPrice(decimal p) => p.ToString("0.00", CultureInfo.InvariantCulture);
            string FormatQty(decimal q) => q.ToString("G29", CultureInfo.InvariantCulture);
            bool NumbersEqual(decimal? n1, decimal? n2) => (n1 ?? 0) == (n2 ?? 0);
            
            if (job.JobTypeId != model.JobTypeId)
            {
                var newType = await _context.JobTypes.FindAsync(model.JobTypeId);
                changes.Add($"type from <b>{job.JobType.Name}</b> to <b>{newType?.Name}</b>");
            }
            if (job.WorkerId != model.WorkerId)
            {
                var newWorker = await _context.Workers.FindAsync(model.WorkerId);
                changes.Add($"mechanic from <b>{job.Worker.Name}</b> to <b>{newWorker?.Name}</b>");
            }
            if (job.Status != model.Status)
            {
                changes.Add($"status from <b>{job.Status}</b> to <b>{model.Status}</b>");
            }
            if (!NumbersEqual(job.LaborCost, model.LaborCost))
            {
                changes.Add($"labor cost from <b>{FormatPrice(job.LaborCost)}</b> to <b>{FormatPrice(model.LaborCost)}</b>");
            }
            
            bool timeChanged = job.StartTime != model.StartTime || job.EndTime != model.EndTime;
            if (timeChanged)
            {
                changes.Add($"Updated interval from {job.StartTime:HH:mm}-{job.EndTime:HH:mm} to {model.StartTime:HH:mm}-{model.EndTime:HH:mm}");
            }

            if (job.Description != model.Description)
            {
                changes.Add("updated description");
            }

            var oldStatus = job.Status;
            var newStatus = model.Status;

            if (oldStatus == Shared.Enums.JobStatus.AwaitingParts && newStatus != Shared.Enums.JobStatus.AwaitingParts)
            {
                foreach (var jp in job.JobParts)
                {
                    if (jp.Part.Quantity < jp.Quantity)
                    {
                        return new MethodResponse(false, $"Insufficient stock for part '{jp.Part.Name}'");
                    }
                    jp.Part.Quantity -= jp.Quantity;
                }
            }
            else if (oldStatus != Shared.Enums.JobStatus.AwaitingParts && newStatus == Shared.Enums.JobStatus.AwaitingParts)
            {
                foreach (var jp in job.JobParts)
                {
                    jp.Part.Quantity += jp.Quantity;
                }
            }

            job.JobTypeId = model.JobTypeId;
            job.WorkerId = model.WorkerId;
            job.Description = model.Description;
            job.Status = model.Status;
            job.LaborCost = model.LaborCost;
            job.StartTime = model.StartTime;
            job.EndTime = model.EndTime;

            // Parts sync - use oldStatus for part quantity calculations
            var partIdsInModel = model.Parts.Select(p => p.PartId).ToList();
            var partsToRemove = job.JobParts.Where(jp => !partIdsInModel.Contains(jp.PartId)).ToList();
            
            foreach (var jp in partsToRemove)
            {
                if (jp.Part != null)
                {
                    if (oldStatus == Shared.Enums.JobStatus.AwaitingParts)
                    {
                        jp.Part.AvailabilityBalance += jp.Quantity;
                    }
                    else
                    {
                        jp.Part.Quantity += jp.Quantity;
                        jp.Part.AvailabilityBalance += jp.Quantity;
                    }
                }
                changes.Add($"removed part '<b>{jp.Part?.Name}</b>'");
                _context.JobParts.Remove(jp);
            }

            foreach (var partModel in model.Parts)
            {
                var existingJobPart = job.JobParts.FirstOrDefault(jp => jp.PartId == partModel.PartId);
                var part = await _context.Parts.FindAsync(partModel.PartId);
                if (part == null) continue;

                if (existingJobPart != null)
                {
                    if (existingJobPart.Quantity != partModel.Quantity)
                    {
                        changes.Add($"changed quantity of '<b>{part.Name}</b>' from <b>{FormatQty(existingJobPart.Quantity)}</b> to <b>{FormatQty(partModel.Quantity)}</b>");
                        int diff = partModel.Quantity - existingJobPart.Quantity;
                        
                        if (oldStatus == Shared.Enums.JobStatus.AwaitingParts)
                        {
                            part.AvailabilityBalance -= diff;
                        }
                        else
                        {
                            if (part.Quantity >= diff)
                            {
                                part.Quantity -= diff;
                                part.AvailabilityBalance -= diff;
                            }
                            else
                            {
                                return new MethodResponse(false, $"Insufficient stock for part '{part.Name}'");
                            }
                        }
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
                    changes.Add($"added part '<b>{part.Name}</b>'");
                    
                    if (oldStatus == Shared.Enums.JobStatus.AwaitingParts)
                    {
                        part.AvailabilityBalance -= partModel.Quantity;
                    }
                    else
                    {
                        if (part.Quantity >= partModel.Quantity)
                        {
                            part.Quantity -= partModel.Quantity;
                            part.AvailabilityBalance -= partModel.Quantity;
                        }
                        else
                        {
                            return new MethodResponse(false, $"Insufficient stock for part '{part.Name}'");
                        }
                    }
                }

                if (part.AvailabilityBalance < part.MinimumQuantity)
                {
                    await _notificationService.SendStockNotificationAsync(workshopId, part.Id, part.Name, part.AvailabilityBalance, part.MinimumQuantity);
                }
            }

            await _context.SaveChangesAsync();

            if (changes.Count > 0)
            {
                string carName = $"{job.Order.Car.Model.CarMake.Name} {job.Order.Car.Model.Name} ({job.Order.Car.RegistrationNumber})";
                string orderLink = $"<a href='/orders' class='log-link target-link'>order for {carName}</a>";
                string actionHtml;

                if (changes.Count == 1 && changes[0].Contains("from"))
                {
                    actionHtml = $"changed {changes[0]} of job '{oldJobTypeName}' for {orderLink}";
                }
                else if (changes.Count == 1 && changes[0].Contains("Updated interval"))
                {
                    actionHtml = $"{changes[0]} for job '{oldJobTypeName}' for {orderLink}";
                }
                else if (changes.All(c => c == "updated description" || (!c.Contains("from") && !c.Contains("added") && !c.Contains("removed") && !c.Contains("interval"))))
                {
                    actionHtml = $"updated details of job '{oldJobTypeName}' for {orderLink}";
                }
                else
                {
                    actionHtml = $"updated job '{oldJobTypeName}' for {orderLink}: {string.Join(", ", changes)}";
                }

                await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
            }

            return new MethodResponse(true, "Job updated successfully");
        }
    }
}
