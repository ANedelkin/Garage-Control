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
        private readonly ICarServiceService _carServiceService;

        public ModelService(IRepository repo, ICarServiceService carServiceService)
        {
            _repo = repo;
            _carServiceService = carServiceService;
        }

        public async Task CreateModel(ModelVM model, string userId)
        {
            var bossId = await GetBossId(userId);
            if (bossId == null) throw new ArgumentException("User is not associated with a service or owner.");

            var carModel = new CarModel
            {
                Name = model.Name,
                CarMakeId = model.MakeId,
                CreatorId = bossId
            };

            await _repo.AddAsync(carModel);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteModel(string id)
        {
            await _repo.DeleteAsync<CarModel>(id);
            await _repo.SaveChangesAsync();
        }

        public async Task<IEnumerable<ModelVM>> GetModels(string makeId)
        {
            return await _repo.GetAllAsNoTrackingAsync<CarModel>()
                .Where(m => m.CarMakeId == makeId)
                .Select(m => new ModelVM
                {
                    Id = m.Id,
                    Name = m.Name,
                    MakeId = m.CarMakeId
                })
                .ToListAsync();
        }

        public async Task UpdateModel(ModelVM model)
        {
            var carModel = await _repo.GetByIdAsync<CarModel>(model.Id!);
            if (carModel != null)
            {
                carModel.Name = model.Name;
                await _repo.SaveChangesAsync();
            }
        }

        private async Task<string?> GetBossId(string userId)
        {
            var serviceId = await _carServiceService.GetServiceId(userId);
            if (serviceId == null) return null;

            var service = await _repo.GetByIdAsync<CarService>(serviceId);
            return service?.BossId;
        }
    }
}
