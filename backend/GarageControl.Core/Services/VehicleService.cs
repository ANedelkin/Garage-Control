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

            return await _repo.GetAllAsNoTrackingAsync<Car>()
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
                    Kilometers = c.Kilometers,
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

            var carWithNames = await _repo.GetAllAsNoTrackingAsync<Car>()
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarMake)
                .FirstOrDefaultAsync(c => c.Id == car.Id);
            
            string carDisplayName = carWithNames != null 
                ? $"{carWithNames.Model.CarMake.Name} {carWithNames.Model.Name} ({carWithNames.RegistrationNumber})"
                : "New Car";

            await _activityLogService.LogActionAsync(userId, workshopId, 
                $"added car <b>{carDisplayName}</b> to client <a href='/clients/{client.Id}' class='log-link target-link'>{client.Name}</a>");
        }

        public async Task Edit(VehicleVM model, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) return;

            var car = await _repo.GetAllAttachedAsync<Car>()
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarMake)
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (car != null)
            {
                var changes = new List<string>();
                void TrackChange(string fieldName, object? oldValue, object? newValue)
                {
                    string oldStr = oldValue?.ToString() ?? "";
                    string newStr = newValue?.ToString() ?? "";
                    if (oldStr != newStr)
                    {
                        changes.Add($"{fieldName} from <b>{(string.IsNullOrEmpty(oldStr) ? "[empty]" : oldStr)}</b> to <b>{(string.IsNullOrEmpty(newStr) ? "[empty]" : newStr)}</b>");
                    }
                }

                if (car.ModelId != model.ModelId)
                {
                    var oldModel = car.Model;
                    var newModel = await _repo.GetAllAsNoTrackingAsync<CarModel>().Include(m => m.CarMake).FirstOrDefaultAsync(m => m.Id == model.ModelId);
                    changes.Add($"model from <b>{oldModel.CarMake.Name} {oldModel.Name}</b> to <b>{newModel?.CarMake.Name} {newModel?.Name}</b>");
                }

                TrackChange("registration number", car.RegistrationNumber, model.RegistrationNumber);
                TrackChange("VIN", car.VIN, model.VIN);
                
                if (car.Kilometers != model.Kilometers)
                {
                    changes.Add($"kilometers from <b>{car.Kilometers}</b> to <b>{model.Kilometers}</b>");
                }

                car.ModelId = model.ModelId;
                car.RegistrationNumber = model.RegistrationNumber;
                car.VIN = model.VIN;
                car.Kilometers = model.Kilometers;
                
                await _repo.SaveChangesAsync();

                if (changes.Count > 0)
                {
                    string carLink = $"<b>{car.Model.CarMake.Name} {car.Model.Name} ({car.RegistrationNumber})</b>";
                    string actionHtml = changes.Count == 1 && changes[0].Contains("from")
                        ? $"changed {changes[0]} of car {carLink}"
                        : $"updated car {carLink}: {string.Join(", ", changes)}";

                    await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
                }
            }
        }
 
        public async Task Delete(string id, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) return;

            var car = await _repo.GetAllAsNoTrackingAsync<Car>()
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarMake)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car != null)
            {
                string carDisplayName = $"{car.Model.CarMake.Name} {car.Model.Name} ({car.RegistrationNumber})";
                await _repo.DeleteAsync<Car>(id);
                await _repo.SaveChangesAsync();

                await _activityLogService.LogActionAsync(userId, workshopId, $"deleted car <b>{carDisplayName}</b>");
            }
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
                    Kilometers = c.Kilometers,
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
