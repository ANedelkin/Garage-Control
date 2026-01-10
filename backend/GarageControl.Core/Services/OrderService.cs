using GarageControl.Core.ViewModels.Orders;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GarageControl.Core.Services
{
    public interface IOrderService
    {
        Task<List<OrderListViewModel>> GetOrdersAsync(string garageId);
        Task<object> CreateOrderAsync(string garageId, CreateOrderViewModel model);
        Task<OrderDetailsViewModel?> GetOrderByIdAsync(string id, string garageId);
        Task<object> UpdateOrderAsync(string id, string garageId, UpdateOrderViewModel model);
    }

    public class OrderService : IOrderService
    {
        private readonly GarageControlDbContext _context;

        public OrderService(GarageControlDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderListViewModel>> GetOrdersAsync(string garageId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Car.Owner.CarServiceId == garageId)
                .OrderByDescending(o => o.Jobs.Any() ? o.Jobs.Max(j => j.StartTime) : DateTime.MinValue)
                .Select(o => new OrderListViewModel
                {
                    Id = o.Id,
                    CarId = o.CarId,
                    CarName = o.Car.Model.CarMake.Name + " " + o.Car.Model.Name,
                    CarRegistrationNumber = o.Car.RegistrationNumber,
                    ClientName = o.Car.Owner.Name,

                    Date = o.Jobs.Any() ? o.Jobs.Min(j => j.StartTime) : DateTime.Now,
                    Jobs = o.Jobs.Select(j => new JobListViewModel
                    {
                        Id = j.Id,
                        Type = j.JobType.Name,
                        Description = j.Description ?? "",
                        Status = j.Status == Shared.Enums.JobStatus.Pending ? "pending" :
                                 j.Status == Shared.Enums.JobStatus.InProgress ? "inprogress" : "finished",
                        MechanicName = j.Worker.Name,
                        StartTime = j.StartTime.ToString("HH:mm"),
                        EndTime = j.EndTime.ToString("HH:mm"),
                        LaborCost = j.LaborCost
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<object> CreateOrderAsync(string garageId, CreateOrderViewModel model)
        {
            // Verify Car belongs to service
            var car = await _context.Cars
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.Id == model.CarId && c.Owner.CarServiceId == garageId);

            if (car == null)
            {
                throw new Exception("Car not found or access denied.");
            }

            var order = new Order
            {
                CarId = model.CarId,
                // Additional fields? Order date? usually derived or added to model.
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
                            Price = part.Price // Use the price from the part in the database
                        };
                        _context.JobParts.Add(jobPart);

                        // Deduct stock
                        if (part.Quantity >= partModel.Quantity)
                        {
                            part.Quantity -= partModel.Quantity;
                        } 
                    }
                }
            }

            await _context.SaveChangesAsync();
            
            // Return a simple object instead of the full Order entity to avoid circular references
            return new { orderId = order.Id, message = "Order created successfully" };
        }

        public async Task<OrderDetailsViewModel?> GetOrderByIdAsync(string id, string garageId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id && o.Car.Owner.CarServiceId == garageId)
                .Select(o => new OrderDetailsViewModel
                {
                    Id = o.Id,
                    CarId = o.CarId,
                    CarName = o.Car.Model.CarMake.Name + " " + o.Car.Model.Name,
                    CarRegistrationNumber = o.Car.RegistrationNumber,
                    ClientName = o.Car.Owner.Name,
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

        public async Task<object> UpdateOrderAsync(string id, string garageId, UpdateOrderViewModel model)
        {
            var order = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Owner)
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.JobParts)
                        .ThenInclude(jp => jp.Part)
                .FirstOrDefaultAsync(o => o.Id == id && o.Car.Owner.CarServiceId == garageId);

            if (order == null)
            {
                throw new Exception("Order not found or access denied.");
            }

            // Update order-level details if any (e.g. CarId)
            order.CarId = model.CarId;

            // Handle Jobs
            // 1. Remove jobs that are no longer in the model (and return stock for parts)
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

            // 2. Update existing jobs or add new ones
            foreach (var jobModel in model.Jobs)
            {
                Job? job;
                if (jobModel.Id != null)
                {
                    job = order.Jobs.FirstOrDefault(j => j.Id == jobModel.Id);
                    if (job == null) continue; // Should not happen based on logic above
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

                // Handle Parts for this job
                // a. Remove parts
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

                // b. Update/Add parts
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
            return new { orderId = order.Id, message = "Order updated successfully" };
        }
    }
}
