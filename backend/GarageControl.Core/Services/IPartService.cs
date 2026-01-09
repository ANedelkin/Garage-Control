using GarageControl.Core.ViewModels.Parts;

namespace GarageControl.Core.Services
{
    public interface IPartService
    {
        Task<FolderContentViewModel> GetFolderContentAsync(string garageId, string? folderId);
        Task<List<PartViewModel>> GetAllPartsAsync(string garageId);
        Task<PartViewModel> CreatePartAsync(string garageId, CreatePartViewModel model);
        Task EditPartAsync(string garageId, UpdatePartViewModel model);
        Task DeletePartAsync(string garageId, string partId);
        Task<PartsFolderViewModel> CreateFolderAsync(string garageId, CreateFolderViewModel model);
        Task RenameFolderAsync(string garageId, string folderId, string newName);
        Task DeleteFolderAsync(string garageId, string folderId);
    }
}
