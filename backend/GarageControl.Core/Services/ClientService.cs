using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Clients;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Services
{
    public class ClientService : IClientService
    {
        private readonly IRepository _repo;
        private readonly IWorkshopService _workshopService;
        private readonly IActivityLogService _activityLogService;

        public ClientService(
            IRepository repo,
            IWorkshopService workshopService,
            IActivityLogService activityLogService)
        {
            _repo = repo;
            _workshopService = workshopService;
            _activityLogService = activityLogService;
        }

        private async Task<string> RequireWorkshopId(string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null)
                throw new ArgumentException("User does not have a workshop");

            return workshopId;
        }

        private static ClientVM MapClient(Client c) => new ClientVM
        {
            Id = c.Id,
            Name = c.Name,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            Address = c.Address,
            RegistrationNumber = c.RegistrationNumber
        };

        public async Task<IEnumerable<ClientVM>> All(string userId)
        {
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (workshopId == null)
                return Enumerable.Empty<ClientVM>();

            return await _repo.GetAllAsNoTracking<Client>()
                .Where(c => c.WorkshopId == workshopId)
                .Select(c => MapClient(c))
                .ToListAsync();
        }

        public async Task Create(ClientVM model, string userId)
        {
            var workshopId = await RequireWorkshopId(userId);

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

            await _activityLogService.LogActionAsync(userId, workshopId, "Client",
                new ActivityLogData("created", client.Id, client.Name));
        }

        public async Task Delete(string id, string userId)
        {
            var workshopId = await RequireWorkshopId(userId);

            var client = await _repo.GetByIdAsync<Client>(id);

            await _repo.DeleteAsync<Client>(id);
            await _repo.SaveChangesAsync();

            await _activityLogService.LogActionAsync(userId, workshopId, "Client",
                new ActivityLogData("deleted", null, client?.Name));
        }

        public async Task<ClientVM?> Details(string id)
        {
            return await _repo.GetAllAsNoTracking<Client>()
                .Where(c => c.Id == id)
                .Select(c => MapClient(c))
                .FirstOrDefaultAsync();
        }

        public async Task Edit(string id, ClientVM model, string userId)
        {
            var workshopId = await RequireWorkshopId(userId);

            var client = await _repo.GetByIdAsync<Client>(id);

            var changes = GetChanges(client, model);

            client.Name = model.Name;
            client.PhoneNumber = model.PhoneNumber;
            client.Email = model.Email;
            client.Address = model.Address;
            client.RegistrationNumber = model.RegistrationNumber;

            await _repo.SaveChangesAsync();

            if (changes.Count > 0)
            {
                await _activityLogService.LogActionAsync(userId, workshopId, "Client",
                    new ActivityLogData("updated", client.Id, client.Name, Changes: changes));
            }
        }

        private static List<ActivityPropertyChange> GetChanges(Client client, ClientVM model)
        {
            var changes = new List<ActivityPropertyChange>();

            void Track(string field, string? oldVal, string? newVal)
            {
                if (oldVal != newVal)
                    changes.Add(new ActivityPropertyChange(field, oldVal ?? "", newVal ?? ""));
            }

            Track("name", client.Name, model.Name);
            Track("phone number", client.PhoneNumber, model.PhoneNumber);
            Track("email", client.Email, model.Email);
            Track("address", client.Address, model.Address);
            Track("registration number", client.RegistrationNumber, model.RegistrationNumber);

            return changes;
        }
    }

}
