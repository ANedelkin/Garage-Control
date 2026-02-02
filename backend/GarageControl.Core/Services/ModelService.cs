using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Services
{
    public class ModelService : IModelService
    {
        private readonly IRepository _repo;
        private readonly IWorkshopService _workshopService;

        public ModelService(IRepository repo, IWorkshopService workshopService)
        {
            _repo = repo;
            _workshopService = workshopService;
        }

        public async Task CreateModel(ModelVM model, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            // Allow null bossId for Global creation
            
            var carModel = new CarModel
            {
                Name = model.Name,
                CarMakeId = model.MakeId,
                CreatorId = bossId 
            };

            await _repo.AddAsync(carModel);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteModel(string id, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var carModel = await _repo.GetByIdAsync<CarModel>(id);

            if (carModel == null) return;

             if (bossId != null && carModel.CreatorId != bossId)
            {
                throw new UnauthorizedAccessException("Cannot delete global or other workshop's model.");
            }

            await _repo.DeleteAsync<CarModel>(id);
            await _repo.SaveChangesAsync();
        }

        public async Task<ModelVM?> GetModel(string id)
        {
            return await _repo.GetAllAsNoTrackingAsync<CarModel>()
                .Where(m => m.Id == id)
                .Select(m => new ModelVM
                {
                    Id = m.Id,
                    Name = m.Name,
                    MakeId = m.CarMakeId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ModelVM>> GetModels(string makeId, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var make = await _repo.GetByIdAsync<CarMake>(makeId);

            var models = await _repo.GetAllAsNoTrackingAsync<CarModel>()
                .Where(m => m.CarMakeId == makeId && (m.CreatorId == null || (bossId != null && m.CreatorId == bossId)))
                .ToListAsync();

            var result = new List<ModelVM>();

            // If this is a custom make, we need to look for matching global models under the GLOBAL version of this make
            string? globalMakeId = null;
            if (make?.CreatorId != null)
            {
                var globalMake = await _repo.GetAllAsNoTrackingAsync<CarMake>()
                    .FirstOrDefaultAsync(m => m.CreatorId == null && m.Name.ToUpper() == make.Name.ToUpper());
                globalMakeId = globalMake?.Id;
            }

            foreach (var m in models)
            {
                var vm = new ModelVM
                {
                    Id = m.Id,
                    Name = m.Name,
                    MakeId = m.CarMakeId,
                    IsCustom = m.CreatorId != null
                };

                if (vm.IsCustom)
                {
                    // Match by name within the same make, OR under the global version of this make
                    var targetMakeId = globalMakeId ?? makeId;
                    var globalMatch = await _repo.GetAllAsNoTrackingAsync<CarModel>()
                        .FirstOrDefaultAsync(gm => gm.CreatorId == null 
                            && gm.CarMakeId == targetMakeId 
                            && gm.Name.ToUpper() == m.Name.ToUpper());
                    
                    vm.GlobalId = globalMatch?.Id;
                }

                result.Add(vm);
            }

            return result;
        }

        public async Task UpdateModel(ModelVM model, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var carModel = await _repo.GetByIdAsync<CarModel>(model.Id!);
            
            if (carModel != null)
            {
                if (bossId != null && carModel.CreatorId != bossId)
                {
                    throw new UnauthorizedAccessException("Cannot edit global or other workshop's model.");
                }

                carModel.Name = model.Name;
                await _repo.SaveChangesAsync();
            }
        }

        public async Task MergeModelWithGlobal(string customModelId, string globalModelId, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            if (bossId == null)
            {
                throw new UnauthorizedAccessException("Only workshop owners can merge models.");
            }

            var customModel = await _repo.GetByIdAsync<CarModel>(customModelId);
            if (customModel == null || customModel.CreatorId != bossId)
            {
                throw new UnauthorizedAccessException("You can only merge your own custom models.");
            }

            var globalModel = await _repo.GetByIdAsync<CarModel>(globalModelId);
            if (globalModel == null || globalModel.CreatorId != null)
            {
                throw new ArgumentException("Invalid global model.");
            }

            // Update all cars using custom model to use global model
            var cars = await _repo.GetAllAttachedAsync<Car>()
                .Where (c => c.ModelId == customModelId)
                .ToListAsync();

            foreach (var car in cars)
            {
                car.ModelId = globalModelId;
            }

            // Delete custom model
            await _repo.DeleteAsync<CarModel>(customModelId);
            await _repo.SaveChangesAsync();

            // Delete related notifications
            var notifications = await _repo.GetAllAttachedAsync<Notification>()
                .Where(n => n.UserId == bossId && n.Link!.Contains($"customId={customModelId}"))
                .ToListAsync();

            foreach (var notification in notifications)
            {
                await _repo.DeleteAsync<Notification>(notification.Id);
            }

            await _repo.SaveChangesAsync();
        }

        private async Task<string?> GetBossId(string userId)
        {
            return await _workshopService.GetWorkshopBossId(userId);
        }
    }
}
