using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Auth;
using GarageControl.Core.ViewModels.Workshop;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Models;

namespace GarageControl.Core.Services
{
    public class WorkshopService : IWorkshopService
    {
        private readonly IRepository _repository;
        private readonly IAuthService _authService;
        private readonly IActivityLogService _activityLogService;

        public WorkshopService(IRepository repository, IAuthService authService, IActivityLogService activityLogService)
        {
            _repository = repository;
            _authService = authService;
            _activityLogService = activityLogService;
        }

        public async Task<LoginResponseVM> CreateWorkshop(string userId, WorkshopVM model)
        {
            var workshop = new Workshop
            {
                Name = model.Name,
                Address = model.Address,
                RegistrationNumber = model.RegistrationNumber,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                BossId = userId
            };
            await _repository.AddAsync<Workshop>(workshop);

            var allAccesses = await _repository.GetAll<Access>().ToListAsync();
            var worker = new Worker
            {
                UserId = userId,
                Name = model.Name,
                Workshop = workshop,
                HiredOn = DateTime.Now,
                Accesses = new HashSet<Access>(allAccesses)
            };
            await _repository.AddAsync<Worker>(worker);

            await _repository.SaveChangesAsync();

            // Generate a new token with the updated accesses
            var tokenResponse = await _authService.GenerateTokenForUser(userId);
            return tokenResponse;
        }

        public async Task<WorkshopVM> GetWorkshopDetails(string workshopId)
        {
            Workshop workshop = await _repository.GetByIdAsync<Workshop>(workshopId);
            return new WorkshopVM
            {
                Name = workshop.Name,
                Address = workshop.Address,
                RegistrationNumber = workshop.RegistrationNumber ?? string.Empty,
                PhoneNumber = workshop.PhoneNumber,
                Email = workshop.Email
            };
        }

        public async Task<WorkshopVM?> GetWorkshopDetailsByUser(string userId)
        {
            var workshopId = await GetWorkshopId(userId);
            if (workshopId == null) return null;

            Workshop workshop = await _repository.GetByIdAsync<Workshop>(workshopId);

            if (workshop == null) return null;

            return new WorkshopVM
            {
                Name = workshop.Name,
                Address = workshop.Address,
                RegistrationNumber = workshop.RegistrationNumber ?? string.Empty,
                PhoneNumber = workshop.PhoneNumber,
                Email = workshop.Email
            };
        }

        public async Task UpdateWorkshopDetails(string ownerId, WorkshopVM model)
        {
            var workshopId = await GetWorkshopId(ownerId);
            if (workshopId == null) throw new Exception("Workshop not found");

            var workshop = await _repository.GetByIdAsync<Workshop>(workshopId);
            if (workshop == null) throw new Exception("Workshop not found");

            var changes = new List<ActivityPropertyChange>();
            void TrackProperty(string field, string? oldV, string? newV)
            {
                if (oldV != newV) changes.Add(new ActivityPropertyChange(field, oldV, newV));
            }

            TrackProperty("name", workshop.Name, model.Name);
            TrackProperty("address", workshop.Address, model.Address);
            TrackProperty("registration number", workshop.RegistrationNumber, model.RegistrationNumber);
            TrackProperty("phone", workshop.PhoneNumber, model.PhoneNumber);
            TrackProperty("email", workshop.Email, model.Email);

            workshop.Name = model.Name;
            workshop.Address = model.Address;
            workshop.RegistrationNumber = model.RegistrationNumber;
            workshop.PhoneNumber = model.PhoneNumber;
            workshop.Email = model.Email;
            await _repository.SaveChangesAsync();

            if (changes.Any())
            {
                await _activityLogService.LogActionAsync(ownerId, workshopId, "Workshop",
                    new ActivityLogData("updated", workshopId, workshop.Name, Changes: changes));
            }
        }
        public async Task<string?> GetWorkshopId(string userId)
        {
            // 1. Check if user is an Owner (Boss)
            var ownerWorkshopId = await _repository.GetAllAsNoTracking<Workshop>()
                .Where(s => s.BossId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (ownerWorkshopId != null) return ownerWorkshopId;

            // 2. Check if user is a Worker
            var workerWorkshopId = await _repository.GetAllAsNoTracking<Worker>()
                .Where(w => w.UserId == userId)
                .Select(w => w.WorkshopId)
                .FirstOrDefaultAsync();

            return workerWorkshopId; // may be null if not a worker
        }

        public async Task<string?> GetWorkshopBossId(string userId)
        {
            var workshop = await _repository.GetAllAsNoTracking<Workshop>()
                .FirstOrDefaultAsync(s => s.BossId == userId);

            if (workshop != null) return workshop.BossId;

            var worker = await _repository.GetAllAsNoTracking<Worker>()
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (worker != null)
            {
                var workerWorkshop = await _repository.GetAllAsNoTracking<Workshop>()
                    .FirstOrDefaultAsync(s => s.Id == worker.WorkshopId);
                return workerWorkshop?.BossId;
            }

            return null;
        }
    }
}
