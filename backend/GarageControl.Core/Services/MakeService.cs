using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Data.Common;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Core.Services
{
    public class MakeService : IMakeService
    {
        private readonly IRepository _repo;
        private readonly IWorkshopService _workshopService;

        public MakeService(IRepository repo, IWorkshopService workshopService)
        {
            _repo = repo;
            _workshopService = workshopService;
        }

        public async Task CreateMake(MakeVM model, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            if (bossId == null) throw new ArgumentException("User is not associated with a workshop or owner.");

            var make = new CarMake
            {
                Name = model.Name,
                CreatorId = bossId
            };

            await _repo.AddAsync(make);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteMake(string id)
        {
            await _repo.DeleteAsync<CarMake>(id);
            await _repo.SaveChangesAsync();
        }

        public async Task<MakeVM?> GetMake(string id)
        {
            return await _repo.GetAllAsNoTrackingAsync<CarMake>()
                .Where(m => m.Id == id)
                .Select(m => new MakeVM
                {
                    Id = m.Id,
                    Name = m.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MakeVM>> GetMakes(string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            if (bossId == null) return new List<MakeVM>();

            return await _repo.GetAllAsNoTrackingAsync<CarMake>()
                .Where(m => m.CreatorId == bossId)
                .Select(m => new MakeVM
                {
                    Id = m.Id,
                    Name = m.Name
                })
                .ToListAsync();
        }

        public async Task UpdateMake(MakeVM model)
        {
            var make = await _repo.GetByIdAsync<CarMake>(model.Id!);
            if (make != null)
            {
                make.Name = model.Name;
                await _repo.SaveChangesAsync();
            }
        }

        private async Task<string?> GetBossId(string userId)
        {
            return await _workshopService.GetWorkshopBossId(userId);
        }
    }
}
