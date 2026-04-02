using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Vehicles;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IRepository _repo;
        private readonly IWorkshopService _workshopService;
        private readonly IActivityLogService _activityLogService;
 
        public VehicleService(IRepository repo, IWorkshopService workshopService, IActivityLogService activityLogService)
        {
            _repo = repo;
            _workshopService = workshopService;
            _activityLogService = activityLogService;
        }

        public async Task<IEnumerable<VehicleVM>> All(string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) return new List<VehicleVM>();

            return await _repo.GetAllAsNoTracking<Car>()
                .Include(c => c.Owner)
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarMake)
                .Where(c => c.Owner.WorkshopId == workshopId)
                .Select(c => new VehicleVM
                {
                    Id = c.Id,
                    MakeId = c.Model.CarMakeId,
                    ModelId = c.ModelId,
                    RegistrationNumber = c.RegistrationNumber,
                    VIN = c.VIN,
                    OwnerId = c.OwnerId,
                    OwnerName = c.Owner.Name,
                    Kilometers = c.Kilometers,
                    Model = new ModelVM
                    {
                        Name = c.Model.Name,
                        MakeId = c.Model.CarMakeId,
                        Make = new MakeVM
                        {
                            Name = c.Model.CarMake.Name
                        }
                    }
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<VehicleVM>> GetByClient(string clientId)
        {
            return await _repo.GetAllAsNoTracking<Car>()
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
                    Kilometers = c.Kilometers,
                    Model = new ModelVM
                    {
                        Name = c.Model.Name,
                        MakeId = c.Model.CarMakeId,
                        Make = new MakeVM
                        {
                            Name = c.Model.CarMake.Name
                        }
                    }
                })
                .ToListAsync();
        }

        public async Task Create(VehicleVM model, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) throw new ArgumentException("User does not have a workshop");
 
            var client = await _repo.GetByIdAsync<Client>(model.OwnerId);
            if (client == null || client.WorkshopId != workshopId)
            {
                throw new ArgumentException("Invalid client or access denied");
            }
 
            var car = new Car
            {
                ModelId = model.ModelId,
                RegistrationNumber = model.RegistrationNumber,
                VIN = model.VIN,
                OwnerId = model.OwnerId,
                Kilometers = model.Kilometers
            };
 
            await _repo.AddAsync(car);
            await _repo.SaveChangesAsync();

            var carWithNames = await _repo.GetAllAsNoTracking<Car>()
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarMake)
                .FirstOrDefaultAsync(c => c.Id == car.Id);
            
            string carDisplayName = carWithNames != null 
                ? $"{carWithNames.Model.CarMake.Name} {carWithNames.Model.Name} ({carWithNames.RegistrationNumber})"
                : "New Car";

            await _activityLogService.LogActionAsync(userId, workshopId, "Vehicle",
                new ActivityLogData("added", car.Id, carDisplayName,
                    SecondaryEntityId: client.Id, SecondaryEntityName: client.Name));
        }

        public async Task Edit(string id, VehicleVM model, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) return;

            var car = await _repo.GetAllAttached<Car>()
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarMake)
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car != null)
            {
                var changes = new List<ActivityPropertyChange>();

                if (car.ModelId != model.ModelId)
                {
                    var oldModel = car.Model;
                    var newModel = await _repo.GetAllAsNoTracking<CarModel>().Include(m => m.CarMake).FirstOrDefaultAsync(m => m.Id == model.ModelId);
                    changes.Add(new ActivityPropertyChange("model",
                        $"{oldModel.CarMake.Name} {oldModel.Name}",
                        $"{newModel?.CarMake.Name} {newModel?.Name}"));
                }

                void Track(string field, string? oldVal, string? newVal)
                {
                    if (oldVal != newVal)
                        changes.Add(new ActivityPropertyChange(field, oldVal ?? "", newVal ?? ""));
                }

                Track("registration number", car.RegistrationNumber, model.RegistrationNumber);
                Track("VIN", car.VIN, model.VIN);
                if (car.Kilometers != model.Kilometers)
                    changes.Add(new ActivityPropertyChange("kilometers", car.Kilometers.ToString(), model.Kilometers.ToString()));

                car.ModelId = model.ModelId;
                car.RegistrationNumber = model.RegistrationNumber;
                car.VIN = model.VIN;
                car.Kilometers = model.Kilometers;

                await _repo.SaveChangesAsync();

                if (changes.Count > 0)
                {
                    string carDisplayName = $"{car.Model.CarMake.Name} {car.Model.Name} ({car.RegistrationNumber})";
                    await _activityLogService.LogActionAsync(userId, workshopId, "Vehicle",
                        new ActivityLogData("updated", car.Id, carDisplayName, Changes: changes));
                }
            }
        }
 
        public async Task Delete(string id, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) return;

            var car = await _repo.GetAllAsNoTracking<Car>()
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarMake)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car != null)
            {
                string carDisplayName = $"{car.Model.CarMake.Name} {car.Model.Name} ({car.RegistrationNumber})";
                await _repo.DeleteAsync<Car>(id);
                await _repo.SaveChangesAsync();

                await _activityLogService.LogActionAsync(userId, workshopId, "Vehicle",
                    new ActivityLogData("deleted", null, carDisplayName));
            }
        }

        public async Task<VehicleVM?> Details(string id)
        {
            return await _repo.GetAllAsNoTracking<Car>()
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
                    Kilometers = c.Kilometers,
                    Model = new ModelVM
                    {
                        Name = c.Model.Name,
                        MakeId = c.Model.CarMakeId,
                        Make = new MakeVM
                        {
                            Name = c.Model.CarMake.Name
                        }
                    }
                })
                .FirstOrDefaultAsync();
        }
    }
}
