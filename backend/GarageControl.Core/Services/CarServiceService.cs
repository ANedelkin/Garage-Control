using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Common;
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
                Name = model.ServiceName,
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
                ServiceName = service.Name,
                Address = service.Address,
                RegistrationNumber = service.RegistrationNumber ?? string.Empty
            };
        }

        public async Task<ServiceVM?> GetServiceDetailsByUser(string userId)
        {
            CarService? service =  await _repository.GetAllAsNoTrackingAsync<CarService>()
                                                   .Where(s => s.BossId == userId)
                                                   .FirstOrDefaultAsync();
            if (service == null)
                return null;
            return new ServiceVM
            {
                ServiceName = service.Name,
                Address = service.Address,
                RegistrationNumber = service.RegistrationNumber ?? string.Empty
            };
        }

        public async Task UpdateServiceDetails(string serviceId, ServiceVM model)
        {
            CarService service = await _repository.GetByIdAsync<CarService>(serviceId);
            service.Name = model.ServiceName;
            service.Address = model.Address;
            service.RegistrationNumber = model.RegistrationNumber;
            await _repository.SaveChangesAsync();
        }
    }
}