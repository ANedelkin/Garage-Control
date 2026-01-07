using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IRepository _repo;
        private readonly ICarServiceService _carServiceService;

        public VehicleService(IRepository repo, ICarServiceService carServiceService)
        {
            _repo = repo;
            _carServiceService = carServiceService;
        }

        public async Task<IEnumerable<VehicleVM>> All(string userId)
        {
            var serviceId = await _carServiceService.GetServiceId(userId);
            if (serviceId == null) return new List<VehicleVM>();

            return await _repo.GetAllAsNoTrackingAsync<Car>()
                .Include(c => c.Owner)
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarMake)
                .Where(c => c.Owner.CarServiceId == serviceId)
                .Select(c => new VehicleVM
                {
                    Id = c.Id,
                    MakeId = c.Model.CarMakeId,
                    ModelId = c.ModelId,
                    RegistrationNumber = c.RegistrationNumber,
                    VIN = c.VIN,
                    OwnerId = c.OwnerId,
                    OwnerName = c.Owner.Name,
                    Model = new ModelVM
                    {
                        Id = c.Model.Id,
                        Name = c.Model.Name,
                        MakeId = c.Model.CarMakeId,
                        Make = new MakeVM
                        {
                            Id = c.Model.CarMake.Id,
                            Name = c.Model.CarMake.Name
                        }
                    }
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<VehicleVM>> GetByClient(string clientId)
        {
            return await _repo.GetAllAsNoTrackingAsync<Car>()
                .Include(c => c.Owner)
                .Include(c => c.Model)
                .ThenInclude(m => m.CarMake)
                .Where(c => c.OwnerId == clientId)
                .Select(c => new VehicleVM
                {
                    Id = c.Id,
                    MakeId = c.Model.CarMakeId,
                    ModelId = c.ModelId,
                    RegistrationNumber = c.RegistrationNumber,
                    VIN = c.VIN,
                    OwnerId = c.OwnerId,
                    OwnerName = c.Owner.Name,
                    Model = new ModelVM
                    {
                        Id = c.Model.Id,
                        Name = c.Model.Name,
                        MakeId = c.Model.CarMakeId,
                        Make = new MakeVM
                        {
                            Id = c.Model.CarMake.Id,
                            Name = c.Model.CarMake.Name
                        }
                    }
                })
                .ToListAsync();
        }

        public async Task Create(VehicleVM model, string userId)
        {
            // Verify user belongs to the same service as the client (Owner)
            var serviceId = await _carServiceService.GetServiceId(userId);
            if (serviceId == null) throw new ArgumentException("User does not have a service");

            var client = await _repo.GetByIdAsync<Client>(model.OwnerId);
            if (client == null || client.CarServiceId != serviceId)
            {
                throw new ArgumentException("Invalid client or access denied");
            }

            var car = new Car
            {
                ModelId = model.ModelId,
                RegistrationNumber = model.RegistrationNumber,
                VIN = model.VIN,
                OwnerId = model.OwnerId
            };

            await _repo.AddAsync(car);
            await _repo.SaveChangesAsync();
        }

        public async Task Edit(VehicleVM model)
        {
            var car = await _repo.GetByIdAsync<Car>(model.Id!);
            if (car != null)
            {
                car.ModelId = model.ModelId;
                car.RegistrationNumber = model.RegistrationNumber;
                car.VIN = model.VIN;
                // OwnerId typically isn't changed during a simple edit, but if needed:
                // car.OwnerId = model.OwnerId; 
                
                await _repo.SaveChangesAsync();
            }
        }

        public async Task Delete(string id)
        {
            await _repo.DeleteAsync<Car>(id);
            await _repo.SaveChangesAsync();
        }

        public async Task<VehicleVM?> Details(string id)
        {
            return await _repo.GetAllAsNoTrackingAsync<Car>()
                .Include(c => c.Owner)
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarMake)
                .Where(c => c.Id == id)
                .Select(c => new VehicleVM
                {
                    Id = c.Id,
                    MakeId = c.Model.CarMakeId,
                    ModelId = c.ModelId,
                    RegistrationNumber = c.RegistrationNumber,
                    VIN = c.VIN,
                    OwnerId = c.OwnerId,
                    OwnerName = c.Owner.Name,
                    Model = new ModelVM
                    {
                        Id = c.Model.Id,
                        Name = c.Model.Name,
                        MakeId = c.Model.CarMakeId,
                        Make = new MakeVM
                        {
                            Id = c.Model.CarMake.Id,
                            Name = c.Model.CarMake.Name
                        }
                    }
                })
                .FirstOrDefaultAsync();
        }
    }
}
