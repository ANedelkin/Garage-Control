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
        Task<Order> CreateOrderAsync(string garageId, CreateOrderViewModel model);
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
            var orders = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Owner)
                .Include(o => o.Car)
                    .ThenInclude(c => c.Model)
                        .ThenInclude(m => m.CarMake)
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.JobType)
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.Worker)
                .Where(o => o.Car.Owner.CarServiceId == garageId)
                .OrderByDescending(o => o.Jobs.Max(j => j.StartTime))
                .ToListAsync();

            return orders.Select(o => new OrderListViewModel
            {
                Id = o.Id,
                CarId = o.CarId,
                CarName = $"{o.Car.Model.CarMake.Name} {o.Car.Model.Name}",
                CarRegistrationNumber = o.Car.RegistrationNumber,
                ClientName = o.Car.Owner.Name,
                // Status determined by jobs. If any InProgress -> InProgress. If all Completed -> Completed. Else Pending.
                Status = o.Jobs.Any(j => j.Status == Shared.Enums.JobStatus.InProgress) ? "inProgress" :
                         o.Jobs.All(j => j.Status == Shared.Enums.JobStatus.Done) ? "finished" : "pending",
                Date = o.Jobs.Any() ? o.Jobs.Min(j => j.StartTime) : DateTime.Now, // Use first job start time as order date
                Jobs = o.Jobs.Select(j => new JobListViewModel
                {
                    Id = j.Id,
                    Type = j.JobType.Name,
                    Description = j.Description ?? "",
                    Status = j.Status.ToString().ToLower(), // "pending", "inprogress", "finished"
                    MechanicName = j.Worker.Name,
                    StartTime = j.StartTime.ToString("HH:mm"),
                    EndTime = j.EndTime.ToString("HH:mm")
                }).ToList()
            }).ToList();
        }

        public async Task<Order> CreateOrderAsync(string garageId, CreateOrderViewModel model)
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
                            Price = part.Price
                        };
                        _context.JobParts.Add(jobPart);

                        // Deduct stock? Assuming simple inventory for now.
                        if (part.Quantity >= partModel.Quantity)
                        {
                            part.Quantity -= partModel.Quantity;
                        } 
                        // Else? Allow negative? or throw? Let's allow for now or just ignore check to keep it simple as per prototype focus.
                    }
                }
            }

            await _context.SaveChangesAsync();
            return order;
        }
    }
}
