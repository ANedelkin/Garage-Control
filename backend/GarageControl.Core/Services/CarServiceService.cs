using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.Core.Services
{
    public class CarServiceService : ICarServiceService
    {
        private readonly IRepository _repository;
        public CarServiceService(IRepository repository)
        {
            _repository = repository;
        }
        public async Task CreateService(string userId, ServiceVM model)
        {
            await _repository.AddAsync<CarService>(new CarService
            {
                Name = model.Name,
                Address = model.Address,
                RegistrationNumber = model.RegistrationNumber,
                BossId = userId
            });

            await _repository.SaveChangesAsync();
        }

        public async Task<ServiceVM> GetServiceDetails(int serviceId)
        {
            CarService service =  await _repository.GetByIdAsync<CarService>(serviceId);
            return new ServiceVM
            {
                Name = service.Name,
                Address = service.Address,
                RegistrationNumber = service.RegistrationNumber ?? string.Empty
            };
        }

        public async Task<ServiceVM?> GetServiceDetailsByUser(string userId)
        {
            var serviceId = await GetServiceId(userId);
            if (serviceId == null) return null;

            CarService service =  await _repository.GetByIdAsync<CarService>(serviceId);
            
            if (service == null) return null;

            return new ServiceVM
            {
                Name = service.Name,
                Address = service.Address,
                RegistrationNumber = service.RegistrationNumber ?? string.Empty
            };
        }

        public async Task UpdateServiceDetails(string ownerId, ServiceVM model)
        {
            var serviceId = await GetServiceId(ownerId);
            if (serviceId == null) throw new Exception("Service not found");

            var service = await _repository.GetByIdAsync<CarService>(serviceId);
            if (service == null) throw new Exception("Service not found");

            service.Name = model.Name;
            service.Address = model.Address;
            service.RegistrationNumber = model.RegistrationNumber;
            await _repository.SaveChangesAsync();
        }
        public async Task<string?> GetServiceId(string userId)
        {
            // 1. Check if user is an Owner (Boss)
            var serviceId = (await _repository.GetAllAsNoTrackingAsync<CarService>()
                .Where(s => s.BossId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync());

            if (serviceId != null) return serviceId;

            // 2. Check if user is a Worker
            serviceId = (await _repository.GetAllAsNoTrackingAsync<Worker>()
                .Where(w => w.UserId == userId)
                .Select(w => w.CarServiceId)
                .FirstOrDefaultAsync());

            return serviceId;
        }
    }
}