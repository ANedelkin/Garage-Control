using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Vehicles;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Services
{
    public class ModelService : IModelService
    {
        private readonly IRepository _repo;
        private readonly IWorkshopService _workshopService;
        private readonly IActivityLogService _activityLogService;
 
        public ModelService(IRepository repo, IWorkshopService workshopService, IActivityLogService activityLogService)
        {
            _repo = repo;
            _workshopService = workshopService;
            _activityLogService = activityLogService;
        }

        public async Task CreateModel(ModelVM model, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var workshopId = await _workshopService.GetWorkshopId(userId);
            // Allow null bossId for Global creation
            
            var carModel = new CarModel
            {
                Name = model.Name,
                CarMakeId = model.MakeId,
                CreatorId = bossId 
            };

            await _repo.AddAsync(carModel);
            await _repo.SaveChangesAsync();

            if (workshopId != null)
            {
                var make = await _repo.GetByIdAsync<CarMake>(model.MakeId);
                await _activityLogService.LogActionAsync(userId, workshopId, "Model",
                    new ActivityLogData("added", carModel.Id, carModel.Name,
                        SecondaryEntityId: model.MakeId, SecondaryEntityName: make?.Name ?? "Unknown"));
            }
        }

        public async Task DeleteModel(string id, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var workshopId = await _workshopService.GetWorkshopId(userId);
            var carModel = await _repo.GetAllAsNoTracking<CarModel>().Include(m => m.CarMake).FirstOrDefaultAsync(m => m.Id == id);

            if (carModel == null) return;

             if (bossId != null && carModel.CreatorId != bossId)
            {
                throw new UnauthorizedAccessException("Cannot delete global or other workshop's model.");
            }
 
            string modelName = carModel.Name;
            string makeName = carModel.CarMake.Name;
            await _repo.DeleteAsync<CarModel>(id);
            await _repo.SaveChangesAsync();

            if (workshopId != null)
            {
                await _activityLogService.LogActionAsync(userId, workshopId, "Model",
                    new ActivityLogData("deleted", null, modelName,
                        SecondaryEntityName: makeName));
            }
        }

        public async Task<ModelVM?> GetModel(string id)
        {
            return await _repo.GetAllAsNoTracking<CarModel>()
                .Where(m => m.Id == id)
                .Select(m => new ModelVM
                {
                    Id = m.Id,
                    Name = m.Name,
                    MakeId = m.CarMakeId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ModelVM?> GetModel(string id, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var carModel = await _repo.GetAllAsNoTracking<CarModel>()
                .Include(m => m.CarMake)
                .FirstOrDefaultAsync(m => m.Id == id && (m.CreatorId == null || (bossId != null && m.CreatorId == bossId)));

            if (carModel == null) return null;

            var vm = new ModelVM
            {
                Id = carModel.Id,
                Name = carModel.Name,
                MakeId = carModel.CarMakeId,
                IsCustom = carModel.CreatorId != null
            };

            if (vm.IsCustom)
            {
                // If this is a custom model, look for matching global model
                // First check under the same make, then under the global version of this make
                var globalMatch = await _repo.GetAllAsNoTracking<CarModel>()
                    .FirstOrDefaultAsync(gm => gm.CreatorId == null 
                        && gm.CarMakeId == carModel.CarMakeId 
                        && gm.Name.ToUpper() == carModel.Name.ToUpper());
                
                if (globalMatch == null && carModel.CarMake?.CreatorId != null)
                {
                    // Model is custom and in a custom make, look under the global version of this make
                    var globalMake = await _repo.GetAllAsNoTracking<CarMake>()
                        .FirstOrDefaultAsync(m => m.CreatorId == null && m.Name.ToUpper() == carModel.CarMake.Name.ToUpper());
                    
                    if (globalMake != null)
                    {
                        globalMatch = await _repo.GetAllAsNoTracking<CarModel>()
                            .FirstOrDefaultAsync(gm => gm.CreatorId == null 
                                && gm.CarMakeId == globalMake.Id 
                                && gm.Name.ToUpper() == carModel.Name.ToUpper());
                    }
                }
                
                vm.GlobalId = globalMatch?.Id;
            }

            return vm;
        }

        public async Task<IEnumerable<ModelVM>> GetModels(string makeId, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var make = await _repo.GetByIdAsync<CarMake>(makeId);

            var models = await _repo.GetAllAsNoTracking<CarModel>()
                .Where(m => m.CarMakeId == makeId && (m.CreatorId == null || (bossId != null && m.CreatorId == bossId)))
                .ToListAsync();

            var result = new List<ModelVM>();

            // If this is a custom make, we need to look for matching global models under the GLOBAL version of this make
            string? globalMakeId = null;
            if (make?.CreatorId != null)
            {
                var globalMake = await _repo.GetAllAsNoTracking<CarMake>()
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
                    var globalMatch = await _repo.GetAllAsNoTracking<CarModel>()
                        .FirstOrDefaultAsync(gm => gm.CreatorId == null 
                            && gm.CarMakeId == targetMakeId 
                            && gm.Name.ToUpper() == m.Name.ToUpper());
                    
                    vm.GlobalId = globalMatch?.Id;
                }

                result.Add(vm);
            }

            return result;
        }

        public async Task UpdateModel(string id, ModelVM model, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var workshopId = await _workshopService.GetWorkshopId(userId);
            var carModel = await _repo.GetAllAttached<CarModel>().Include(m => m.CarMake).FirstOrDefaultAsync(m => m.Id == id);
            
            if (carModel != null)
            {
                if (bossId != null && carModel.CreatorId != bossId)
                {
                    throw new UnauthorizedAccessException("Cannot edit global or other workshop's model.");
                }
 
                string oldName = carModel.Name;
                carModel.Name = model.Name;
                await _repo.SaveChangesAsync();

                if (workshopId != null && oldName != model.Name)
                {
                    await _activityLogService.LogActionAsync(userId, workshopId, "Model",
                        new ActivityLogData("renamed", carModel.Id, oldName,
                            SecondaryEntityId: carModel.CarMakeId, SecondaryEntityName: model.Name));
                }
            }
        }

        public async Task MergeModelWithGlobal(string customModelId, string globalModelId, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var workshopId = await _workshopService.GetWorkshopId(userId);
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

            string customModelName = customModel.Name;
            string globalModelName = globalModel.Name;
 
            // Update all cars using custom model to use global model
            var cars = await _repo.GetAllAttached<Car>()
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
            var notifications = await _repo.GetAllAttached<Notification>()
                .Where(n => n.UserId == bossId && n.Link!.Contains($"customId={customModelId}"))
                .ToListAsync();
 
            foreach (var notification in notifications)
            {
                await _repo.DeleteAsync<Notification>(notification.Id);
            }
 
            await _repo.SaveChangesAsync();

            if (workshopId != null)
            {
                await _activityLogService.LogActionAsync(userId, workshopId, "Model",
                    new ActivityLogData("merged", globalModelId, globalModelName,
                        SecondaryEntityId: globalModel.CarMakeId,
                        SecondaryEntityName: customModelName));
            }
        }

        private async Task<string?> GetBossId(string userId)
        {
            return await _workshopService.GetWorkshopBossId(userId);
        }
    }
}
