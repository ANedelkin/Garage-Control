using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
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

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"created client <a href='/clients/{client.Id}' class='log-link target-link'>{client.Name}</a>");
        }

        public async Task Delete(string id, string userId)
        {
            var workshopId = await RequireWorkshopId(userId);

            var client = await _repo.GetByIdAsync<Client>(id);

            await _repo.DeleteAsync<Client>(id);
            await _repo.SaveChangesAsync();

            await _activityLogService.LogActionAsync(
                userId,
                workshopId,
                $"deleted client <b>{client.Name}</b>");
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
                var actionHtml = BuildChangeLog(client, changes);

                await _activityLogService.LogActionAsync(
                    userId,
                    workshopId,
                    actionHtml);
            }
        }

        private static List<string> GetChanges(Client client, ClientVM model)
        {
            var changes = new List<string>();

            void Track(string field, string? oldVal, string? newVal)
            {
                if (oldVal == newVal) return;

                string oldDisp = string.IsNullOrEmpty(oldVal) ? "[empty]" : oldVal;
                string newDisp = string.IsNullOrEmpty(newVal) ? "[empty]" : newVal;

                if (oldDisp.Length > 100 || newDisp.Length > 100)
                    changes.Add(field);
                else
                    changes.Add($"{field} from <b>{oldDisp}</b> to <b>{newDisp}</b>");
            }

            Track("name", client.Name, model.Name);
            Track("phone number", client.PhoneNumber, model.PhoneNumber);
            Track("email", client.Email, model.Email);
            Track("address", client.Address, model.Address);
            Track("registration number", client.RegistrationNumber, model.RegistrationNumber);

            return changes;
        }

        private static string BuildChangeLog(Client client, List<string> changes)
        {
            string link = $"<a href='/clients/{client.Id}' class='log-link target-link'>{client.Name}</a>";

            if (changes.Count == 1 && changes[0].Contains("from"))
                return $"changed {changes[0]} of client {link}";

            if (changes.All(c => !c.Contains("from")))
                return $"updated details of client {link}";

            return $"updated client {link}: {string.Join(", ", changes)}";
        }
    }

}
