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
                $"created client <a href='/clients/{client.Id}' class='log-link target-link'>{client.Name}</a>");
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
                $"deleted client <b>{clientName}</b>");
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
                var changes = new List<string>();
                void TrackChange(string fieldName, string? oldValue, string? newValue)
                {
                    if (oldValue != newValue)
                    {
                        string oldDisp = string.IsNullOrEmpty(oldValue) ? "[empty]" : oldValue;
                        string newDisp = string.IsNullOrEmpty(newValue) ? "[empty]" : newValue;
                        
                        if (oldDisp.Length > 100 || newDisp.Length > 100)
                        {
                            changes.Add(fieldName);
                        }
                        else
                        {
                            changes.Add($"{fieldName} from <b>{oldDisp}</b> to <b>{newDisp}</b>");
                        }
                    }
                }

                string oldName = client.Name;
                TrackChange("name", client.Name, model.Name);
                TrackChange("phone number", client.PhoneNumber, model.PhoneNumber);
                TrackChange("email", client.Email, model.Email);
                TrackChange("address", client.Address, model.Address);
                TrackChange("registration number", client.RegistrationNumber, model.RegistrationNumber);

                client.Name = model.Name;
                client.PhoneNumber = model.PhoneNumber;
                client.Email = model.Email;
                client.Address = model.Address;
                client.RegistrationNumber = model.RegistrationNumber;
                
                await _repo.SaveChangesAsync();

                if (changes.Count > 0)
                {
                    string clientLink = $"<a href='/clients/{client.Id}' class='log-link target-link'>{client.Name}</a>";
                    string actionHtml;

                    if (changes.Count == 1 && changes[0].Contains("from"))
                    {
                        actionHtml = $"changed {changes[0]} of client {clientLink}";
                    }
                    else if (changes.All(c => !c.Contains("from")))
                    {
                        actionHtml = $"updated details of client {clientLink}";
                    }
                    else
                    {
                        actionHtml = $"updated client {clientLink}: {string.Join(", ", changes)}";
                    }

                    await _activityLogService.LogActionAsync(userId, workshopId, actionHtml);
                }
            }
        }
    }
}
