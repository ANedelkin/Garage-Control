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
        private readonly IActivityLogService _activityLogService;
 
        public MakeService(IRepository repo, IWorkshopService workshopService, IActivityLogService activityLogService)
        {
            _repo = repo;
            _workshopService = workshopService;
            _activityLogService = activityLogService;
        }

        public async Task<string> CreateMake(MakeVM model, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var workshopId = await _workshopService.GetWorkshopId(userId);
            // If bossId is null (Admin or unassigned), we create a Global make (CreatorId = null)
            // Assumes caller has verified permissions if needed, or Admin uses this flow.
            
            var make = new CarMake
            {
                Name = model.Name,
                CreatorId = bossId
            };

            await _repo.AddAsync(make);
            await _repo.SaveChangesAsync();

            if (workshopId != null)
            {
                await _activityLogService.LogActionAsync(userId, workshopId, $"created custom make <b>{make.Name}</b>");
            }

            return make.Id;
        }

        public async Task DeleteMake(string id, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var workshopId = await _workshopService.GetWorkshopId(userId);
            var make = await _repo.GetByIdAsync<CarMake>(id);
            
            if (make == null) return;

            // Security: If bossId is not null (Workshop), cannot delete Global (CreatorId == null)
            // or other workshop's items (though repo query usually filters, here we check ID explicitly)
            if (bossId != null && make.CreatorId != bossId)
            {
                throw new UnauthorizedAccessException("Cannot delete global or other workshop's make.");
            }

            string makeName = make.Name;
            await _repo.DeleteAsync<CarMake>(id);
            await _repo.SaveChangesAsync();

            if (workshopId != null)
            {
                await _activityLogService.LogActionAsync(userId, workshopId, $"deleted make <b>{makeName}</b>");
            }
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

            var makes = await _repo.GetAllAsNoTrackingAsync<CarMake>()
                .Where(m => m.CreatorId == null || (bossId != null && m.CreatorId == bossId))
                .ToListAsync();

            return makes.Select(m => new MakeVM
            {
                Id = m.Id,
                Name = m.Name,
                IsCustom = m.CreatorId != null,
                GlobalId = m.CreatorId != null 
                    ? makes.FirstOrDefault(gm => gm.CreatorId == null && gm.Name.ToUpper() == m.Name.ToUpper())?.Id 
                    : null
            });
        }

        public async Task<IEnumerable<MetricSuggestionVM>> GetSuggestions()
        {
            // Fetch all makes that have user content (Make is User OR has User Models)
            var data = await _repo.GetAllAsNoTrackingAsync<CarMake>()
                .Include(m => m.CarModels)
                .Where(m => m.CreatorId != null || m.CarModels.Any(model => model.CreatorId != null))
                .Select(m => new { m.Name, m.CreatorId })
                .ToListAsync();

            return data
                .GroupBy(n => n.Name.Trim().ToUpper())
                .Select(g => new MetricSuggestionVM 
                { 
                    Name = g.First().Name.Trim(), 
                    Count = g.Count(), // Or sum of new models? The original count was just grouping makes. Stick to that for now.
                    IsExisting = g.Any(x => x.CreatorId == null) // If any make in this group is System, it's Existing.
                })
                .OrderByDescending(x => x.Count);
        }

        public async Task<IEnumerable<MetricSuggestionVM>> GetSuggestedModels(string makeName)
        {
             var normalized = makeName.Trim().ToUpper();
             
             // Get models from makes with this name, where model is user-created.
             var models = await _repo.GetAllAsNoTrackingAsync<CarMake>()
                .Include(m => m.CarModels)
                .Where(m => m.Name.ToUpper() == normalized)
                .SelectMany(m => m.CarModels)
                .Where(model => model.CreatorId != null)
                .Select(Model => Model.Name)
                .ToListAsync();

             return models
                .GroupBy(n => n.Trim().ToUpper())
                .Select(g => new MetricSuggestionVM
                {
                    Name = g.First(),
                    Count = g.Count(),
                    IsExisting = false // Models are inherently new suggestions here
                })
                .OrderByDescending(x => x.Count);
        }

        public async Task PromoteSuggestion(string name, string? newName)
        {
            var finalName = newName?.Trim() ?? name.Trim();
            
            var globalMake = await _repo.GetAllAttachedAsync<CarMake>()
                .FirstOrDefaultAsync(m => m.CreatorId == null && m.Name.ToUpper() == finalName.ToUpper());

            if (globalMake == null)
            {
                globalMake = new CarMake
                {
                    Name = finalName,
                    CreatorId = null 
                };
                await _repo.AddAsync(globalMake);
                await _repo.SaveChangesAsync();
            }

            // Create notifications for creators of matching custom makes
            var originalNormalized = name.Trim().ToUpper();
            var finalNormalized = finalName.ToUpper();

            var customMakes = await _repo.GetAllAsNoTrackingAsync<CarMake>()
                .Where(m => m.CreatorId != null && (m.Name.ToUpper() == originalNormalized || m.Name.ToUpper() == finalNormalized))
                .Select(m => new { m.Id, m.CreatorId })
                .ToListAsync();

            var uniqueCreators = customMakes
                .Where(m => m.CreatorId != null)
                .GroupBy(m => m.CreatorId)
                .Select(g => new { CreatorId = g.Key!, CustomMakeId = g.First().Id })
                .ToList();

            foreach (var creator in uniqueCreators)
            {
                // Check if notification already exists for this specific merge to avoid spam
                var exists = await _repo.GetAllAsNoTrackingAsync<Notification>()
                    .AnyAsync(n => n.UserId == creator.CreatorId && n.Link!.Contains($"customId={creator.CustomMakeId}") && n.Link!.Contains($"globalId={globalMake.Id}"));

                if (!exists)
                {
                    var notification = new Notification
                    {
                        UserId = creator.CreatorId,
                        Message = $"A global version of '{finalName}' make has been added. Click here to review and optionally replace your custom version.",
                        Link = $"/makes-and-models?merge=make&customId={creator.CustomMakeId}&globalId={globalMake.Id}",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _repo.AddAsync(notification);
                }
            }

            await _repo.SaveChangesAsync();
        }

        public async Task UpdateMake(MakeVM model, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var workshopId = await _workshopService.GetWorkshopId(userId);
            var make = await _repo.GetByIdAsync<CarMake>(model.Id!);
            
            if (make != null)
            {
                if (bossId != null && make.CreatorId != bossId)
                {
                     throw new UnauthorizedAccessException("Cannot edit global or other workshop's make.");
                }
 
                string oldName = make.Name;
                make.Name = model.Name;
                await _repo.SaveChangesAsync();

                if (workshopId != null && oldName != model.Name)
                {
                    await _activityLogService.LogActionAsync(userId, workshopId, $"renamed make <b>{oldName}</b> to <b>{model.Name}</b>");
                }
            }
        }

        public async Task PromoteModelSuggestion(string makeName, string modelName, string? newModelName, string? newMakeName)
        {
            var finalMakeName = newMakeName?.Trim() ?? makeName.Trim();
            var finalModelName = newModelName?.Trim() ?? modelName.Trim();

            // Find or create global make
            var normalizedMakeName = finalMakeName.ToUpper();
            var globalMake = await _repo.GetAllAttachedAsync<CarMake>()
                .FirstOrDefaultAsync(m => m.CreatorId == null && m.Name.ToUpper() == normalizedMakeName);

            if (globalMake == null)
            {
                globalMake = new CarMake
                {
                    Name = finalMakeName,
                    CreatorId = null
                };
                await _repo.AddAsync(globalMake);
                await _repo.SaveChangesAsync();
            }

            // Check if model already exists
            var normalizedModelName = finalModelName.ToUpper();
            var globalModel = await _repo.GetAllAttachedAsync<CarModel>()
                .FirstOrDefaultAsync(m => m.CarMakeId == globalMake.Id && m.CreatorId == null && m.Name.ToUpper() == normalizedModelName);

            if (globalModel == null)
            {
                globalModel = new CarModel
                {
                    Name = finalModelName,
                    CarMakeId = globalMake.Id,
                    CreatorId = null
                };
                await _repo.AddAsync(globalModel);
                await _repo.SaveChangesAsync();
            }

            // Create notifications for creators of matching custom models
            var originalMakeNormalized = makeName.Trim().ToUpper();
            var originalModelNormalized = modelName.Trim().ToUpper();
            var finalModelNormalized = finalModelName.ToUpper();

            var customModels = await _repo.GetAllAsNoTrackingAsync<CarModel>()
                .Include(m => m.CarMake)
                .Where(m => m.CreatorId != null 
                    && (m.Name.ToUpper() == originalModelNormalized || m.Name.ToUpper() == finalModelNormalized)
                    && m.CarMake.Name.ToUpper() == originalMakeNormalized)
                .Select(m => new { m.Id, m.CreatorId })
                .ToListAsync();

            var uniqueCreators = customModels
                .Where(m => m.CreatorId != null)
                .GroupBy(m => m.CreatorId)
                .Select(g => new { CreatorId = g.Key!, CustomModelId = g.First().Id })
                .ToList();

            foreach (var creator in uniqueCreators)
            {
                // Avoid duplicate notifications
                var exists = await _repo.GetAllAsNoTrackingAsync<Notification>()
                    .AnyAsync(n => n.UserId == creator.CreatorId && n.Link!.Contains($"customId={creator.CustomModelId}") && n.Link!.Contains($"globalId={globalModel.Id}"));

                if (!exists)
                {
                    var notification = new Notification
                    {
                        UserId = creator.CreatorId,
                        Message = $"A global version of '{finalMakeName} {finalModelName}' model has been added. Click here to review and optionally replace your custom version.",
                        Link = $"/makes-and-models?merge=model&customId={creator.CustomModelId}&globalId={globalModel.Id}",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _repo.AddAsync(notification);
                }
            }

            await _repo.SaveChangesAsync();
        }

        public async Task MergeMakeWithGlobal(string customMakeId, string globalMakeId, string userId)
        {
            var bossId = await _workshopService.GetWorkshopBossId(userId);
            var workshopId = await _workshopService.GetWorkshopId(userId);
            if (bossId == null)
            {
                throw new UnauthorizedAccessException("Only workshop owners can merge makes.");
            }
 
            var customMake = await _repo.GetByIdAsync<CarMake>(customMakeId);
            if (customMake == null || customMake.CreatorId != bossId)
            {
                throw new UnauthorizedAccessException("You can only merge your own custom makes.");
            }
 
            var globalMake = await _repo.GetByIdAsync<CarMake>(globalMakeId);
            if (globalMake == null || globalMake.CreatorId != null)
            {
                throw new ArgumentException("Invalid global make.");
            }
 
            string customMakeName = customMake.Name;
            string globalMakeName = globalMake.Name;

            // Update all cars using custom make to use global make
            var cars = await _repo.GetAllAttachedAsync<Car>()
                .Include(c => c.Model)
                .Where(c => c.Model.CarMakeId == customMakeId)
                .ToListAsync();
 
            // For each car, find matching global model or keep current
            foreach (var car in cars)
            {
                var globalModel = await _repo.GetAllAsNoTrackingAsync<CarModel>()
                    .FirstOrDefaultAsync(m => m.CarMakeId == globalMakeId 
                        && m.CreatorId == null 
                        && m.Name.ToUpper() == car.Model.Name.ToUpper());
 
                if (globalModel != null)
                {
                    car.ModelId = globalModel.Id;
                }
            }
 
            // Move or Delete custom models under this make
            var customModels = await _repo.GetAllAttachedAsync<CarModel>()
                .Where(m => m.CarMakeId == customMakeId)
                .ToListAsync();
 
            foreach (var model in customModels)
            {
                // Check if this specific model was merged (already handled in cars update)
                // Actually, let's see if a car still uses it
                var isUsed = await _repo.GetAllAsNoTrackingAsync<Car>().AnyAsync(c => c.ModelId == model.Id);
                
                if (isUsed)
                {
                    // Move to global make so car references don't break
                    model.CarMakeId = globalMakeId;
                }
                else
                {
                    // Not used anymore, delete
                    await _repo.DeleteAsync<CarModel>(model.Id);
                }
            }
 
            // Delete custom make
            await _repo.DeleteAsync<CarMake>(customMakeId);
            await _repo.SaveChangesAsync();
 
            // Delete related notifications
            var notifications = await _repo.GetAllAttachedAsync<Notification>()
                .Where(n => n.UserId == bossId && n.Link!.Contains($"customId={customMakeId}"))
                .ToListAsync();
 
            foreach (var notification in notifications)
            {
                await _repo.DeleteAsync<Notification>(notification.Id);
            }
 
            await _repo.SaveChangesAsync();

            if (workshopId != null)
            {
                await _activityLogService.LogActionAsync(userId, workshopId, $"merged custom make <b>{customMakeName}</b> into <b>{globalMakeName}</b>");
            }
        }

        private async Task<string?> GetBossId(string userId)
        {
            return await _workshopService.GetWorkshopBossId(userId);
        }
    }
}
