using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Services
{
    public class ClientService : IClientService
    {
        private readonly IRepository _repo;
        private readonly ICarServiceService _carServiceService;

        public ClientService(IRepository repo, ICarServiceService carServiceService)
        {
            _repo = repo;
            _carServiceService = carServiceService;
        }

        public async Task<IEnumerable<ClientVM>> All(string userId)
        {
            var serviceId = await _carServiceService.GetServiceId(userId);
            if (serviceId == null) return new List<ClientVM>();

            return await _repo.GetAllAsNoTrackingAsync<Client>()
                .Where(c => c.CarServiceId == serviceId)
                .Select(c => new ClientVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    Address = c.Address,
                    RegistrationNumber = c.RegistrationNumber
                })
                .ToListAsync();
        }

        public async Task Create(ClientVM model, string userId)
        {
            var serviceId = await _carServiceService.GetServiceId(userId);
            if (serviceId == null) throw new ArgumentException("User does not have a service");

            var client = new Client
            {
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Address = model.Address,
                RegistrationNumber = model.RegistrationNumber,
                CarServiceId = serviceId
            };

            await _repo.AddAsync(client);
            await _repo.SaveChangesAsync();
        }

        public async Task Delete(string id)
        {
            await _repo.DeleteAsync<Client>(id);
            await _repo.SaveChangesAsync();
        }

        public async Task<ClientVM?> Details(string id)
        {
            return await _repo.GetAllAsNoTrackingAsync<Client>()
                .Where(c => c.Id == id)
                .Select(c => new ClientVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    Address = c.Address,
                    RegistrationNumber = c.RegistrationNumber
                })
                .FirstOrDefaultAsync();
        }

        public async Task Edit(ClientVM model)
        {
            var client = await _repo.GetByIdAsync<Client>(model.Id!);
            if (client != null)
            {
                client.Name = model.Name;
                client.PhoneNumber = model.PhoneNumber;
                client.Email = model.Email;
                client.Address = model.Address;
                client.RegistrationNumber = model.RegistrationNumber;
                
                await _repo.SaveChangesAsync();
            }
        }
    }
}
