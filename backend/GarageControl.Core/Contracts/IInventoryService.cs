using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Shared.Enums;

namespace GarageControl.Core.Contracts
{
    public interface IInventoryService
    {
        void SendParts(Part part, int quantity);
        void ReturnParts(Part part, int quantity);
        Task RecalculateAvailabilityBalanceAsync(string workshopId, string? partId = null, int? previousMinimumQuantity = null);
        // Task CheckLowStockAsync(string workshopId, Part part);
        Task<int> GetPartsToSendAsync(string partId);
        Task<Dictionary<string, int>> GetPartsToSendAsync(string workshopId, IEnumerable<string> partIds);
        Task<Part?> GetPartByIdAsync(string partId);
    }
}