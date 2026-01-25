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

            return await _repo.GetAllAsNoTrackingAsync<CarModel>()
                .Where(m => m.CarMakeId == makeId && (m.CreatorId == null || (bossId != null && m.CreatorId == bossId)))
                .Select(m => new ModelVM
                {
                    Id = m.Id,
                    Name = m.Name,
                    MakeId = m.CarMakeId
                })
                .ToListAsync();
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

        private async Task<string?> GetBossId(string userId)
        {
            return await _workshopService.GetWorkshopBossId(userId);
        }
    }
}
