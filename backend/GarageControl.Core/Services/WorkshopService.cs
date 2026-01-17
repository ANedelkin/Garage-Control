using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Core.Services
{
    public class WorkshopService : IWorkshopService
    {
        private readonly IRepository _repository;
        public WorkshopService(IRepository repository)
        {
            _repository = repository;
        }
        public async Task CreateWorkshop(string userId, WorkshopVM model)
        {
            await _repository.AddAsync<Workshop>(new Workshop
            {
                Name = model.Name,
                Address = model.Address,
                RegistrationNumber = model.RegistrationNumber,
                BossId = userId
            });

            await _repository.SaveChangesAsync();
        }

        public async Task<WorkshopVM> GetWorkshopDetails(string workshopId)
        {
            Workshop workshop =  await _repository.GetByIdAsync<Workshop>(workshopId);
            return new WorkshopVM
            {
                Name = workshop.Name,
                Address = workshop.Address,
                RegistrationNumber = workshop.RegistrationNumber ?? string.Empty
            };
        }

        public async Task<WorkshopVM?> GetWorkshopDetailsByUser(string userId)
        {
            var workshopId = await GetWorkshopId(userId);
            if (workshopId == null) return null;

            Workshop workshop =  await _repository.GetByIdAsync<Workshop>(workshopId);
            
            if (workshop == null) return null;

            return new WorkshopVM
            {
                Name = workshop.Name,
                Address = workshop.Address,
                RegistrationNumber = workshop.RegistrationNumber ?? string.Empty
            };
        }

        public async Task UpdateWorkshopDetails(string ownerId, WorkshopVM model)
        {
            var workshopId = await GetWorkshopId(ownerId);
            if (workshopId == null) throw new Exception("Workshop not found");

            var workshop = await _repository.GetByIdAsync<Workshop>(workshopId);
            if (workshop == null) throw new Exception("Workshop not found");

            workshop.Name = model.Name;
            workshop.Address = model.Address;
            workshop.RegistrationNumber = model.RegistrationNumber;
            await _repository.SaveChangesAsync();
        }
        public async Task<string?> GetWorkshopId(string userId)
        {
            // 1. Check if user is an Owner (Boss)
            var workshopId = (await _repository.GetAllAsNoTrackingAsync<Workshop>()
                .Where(s => s.BossId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync());

            if (workshopId != null) return workshopId;

            // 2. Check if user is a Worker
            workshopId = (await _repository.GetAllAsNoTrackingAsync<Worker>()
                .Where(w => w.UserId == userId)
                .Select(w => w.WorkshopId)
                .FirstOrDefaultAsync());

            return workshopId;
        }

        public async Task<string?> GetWorkshopBossId(string userId)
        {
            var workshop = await _repository.GetAllAsNoTrackingAsync<Workshop>()
                .FirstOrDefaultAsync(s => s.BossId == userId);

            if (workshop != null) return workshop.BossId;

            var worker = await _repository.GetAllAsNoTrackingAsync<Worker>()
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (worker != null)
            {
                var workerWorkshop = await _repository.GetAllAsNoTrackingAsync<Workshop>()
                    .FirstOrDefaultAsync(s => s.Id == worker.WorkshopId);
                return workerWorkshop?.BossId;
            }

            return null;
        }
    }
}
