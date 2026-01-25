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
            // If bossId is null (Admin or unassigned), we create a Global make (CreatorId = null)
            // Assumes caller has verified permissions if needed, or Admin uses this flow.
            
            var make = new CarMake
            {
                Name = model.Name,
                CreatorId = bossId
            };

            await _repo.AddAsync(make);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteMake(string id, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var make = await _repo.GetByIdAsync<CarMake>(id);
            
            if (make == null) return;

            // Security: If bossId is not null (Workshop), cannot delete Global (CreatorId == null)
            // or other workshop's items (though repo query usually filters, here we check ID explicitly)
            if (bossId != null && make.CreatorId != bossId)
            {
                throw new UnauthorizedAccessException("Cannot delete global or other workshop's make.");
            }

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

            return await _repo.GetAllAsNoTrackingAsync<CarMake>()
                .Where(m => m.CreatorId == null || (bossId != null && m.CreatorId == bossId))
                .Select(m => new MakeVM
                {
                    Id = m.Id,
                    Name = m.Name
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MetricSuggestionVM>> GetSuggestions()
        {
            // Fetch all local makes (CreatorId != null)
            // Group by Normalized Name
            var data = await _repo.GetAllAsNoTrackingAsync<CarMake>()
                .Where(m => m.CreatorId != null)
                .Select(m => m.Name)
                .ToListAsync();

            return data
                .GroupBy(n => n.Trim().ToUpper())
                .Select(g => new MetricSuggestionVM 
                { 
                    Name = g.First().Trim(), 
                    Count = g.Count() 
                })
                .OrderByDescending(x => x.Count);
        }

        public async Task<IEnumerable<MetricSuggestionVM>> GetSuggestedModels(string makeName)
        {
             var normalized = makeName.Trim().ToUpper();
             
             // 1. Find all local Makes with this name
             // 2. Select their Models
             // 3. Aggregate models by name
             
             // This needs to link CarMake -> CarModel
             // Assuming CarMake has collection CarModels
             
             var models = await _repo.GetAllAsNoTrackingAsync<CarMake>()
                .Include(m => m.CarModels)
                .Where(m => m.CreatorId != null && m.Name.ToUpper() == normalized)
                .SelectMany(m => m.CarModels)
                .Select(Model => Model.Name)
                .ToListAsync();

             return models
                .GroupBy(n => n.Trim().ToUpper())
                .Select(g => new MetricSuggestionVM
                {
                    Name = g.First(),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count);
        }

        public async Task PromoteSuggestion(string name, string? newName)
        {
            var finalName = newName?.Trim() ?? name.Trim();
            
            var existing = await _repo.GetAllAsNoTrackingAsync<CarMake>()
                .AnyAsync(m => m.CreatorId == null && m.Name.ToUpper() == finalName.ToUpper());

            if (!existing)
            {
                var make = new CarMake
                {
                    Name = finalName,
                    CreatorId = null 
                };
                await _repo.AddAsync(make);
                await _repo.SaveChangesAsync();
            }
        }

        public async Task UpdateMake(MakeVM model, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var make = await _repo.GetByIdAsync<CarMake>(model.Id!);
            
            if (make != null)
            {
                if (bossId != null && make.CreatorId != bossId)
                {
                     throw new UnauthorizedAccessException("Cannot edit global or other workshop's make.");
                }

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
