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
        private readonly IWorkshopService _workshopService;
        private readonly IActivityLogService _activityLogService;

        public ClientService(IRepository repo, IWorkshopService workshopService, IActivityLogService activityLogService)
        {
            _repo = repo;
            _workshopService = workshopService;
            _activityLogService = activityLogService;
        }

        public async Task<IEnumerable<ClientVM>> All(string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) return new List<ClientVM>();

            return await _repo.GetAllAsNoTrackingAsync<Client>()
                .Where(c => c.WorkshopId == workshopId)
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
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) throw new ArgumentException("User does not have a workshop");

            var client = new Client
            {
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Address = model.Address,
                RegistrationNumber = model.RegistrationNumber,
                WorkshopId = workshopId
            };

            await _repo.AddAsync(client);
            await _repo.SaveChangesAsync();

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                "created",
                client.Id,
                client.Name,
                "Client");
        }

        public async Task Delete(string id, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) throw new ArgumentException("User does not have a workshop");

            var client = await _repo.GetByIdAsync<Client>(id);
            if (client == null) return;

            string clientName = client.Name;

            await _repo.DeleteAsync<Client>(id);
            await _repo.SaveChangesAsync();

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                "deleted",
                null,
                clientName,
                "Client");
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

        public async Task Edit(ClientVM model, string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null) throw new ArgumentException("User does not have a workshop");

            var client = await _repo.GetByIdAsync<Client>(model.Id!);
            if (client != null)
            {
                client.Name = model.Name;
                client.PhoneNumber = model.PhoneNumber;
                client.Email = model.Email;
                client.Address = model.Address;
                client.RegistrationNumber = model.RegistrationNumber;
                
                await _repo.SaveChangesAsync();

                await _activityLogService.LogActionAsync(
                    userId,
                    workshopId,
                    "updated",
                    client.Id,
                    client.Name,
                    "Client");
            }
        }
    }
}
