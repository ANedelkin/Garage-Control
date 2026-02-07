using GarageControl.Core.ViewModels.Parts;

namespace GarageControl.Core.Contracts
{
    public interface IPartService
    {
        Task<List<PartViewModel>> GetAllPartsAsync(string garageId);
        Task<PartViewModel> CreatePartAsync(string userId, string garageId, CreatePartViewModel model);
        Task<PartWithPathViewModel?> GetPartAsync(string garageId, string partId);
        Task EditPartAsync(string userId, string garageId, UpdatePartViewModel model);
        Task DeletePartAsync(string userId, string garageId, string partId);
        Task MovePartAsync(string userId, string garageId, string partId, string? newParentId);
    }
}
