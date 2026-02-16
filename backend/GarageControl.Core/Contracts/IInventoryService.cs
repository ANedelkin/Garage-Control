using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Shared.Enums;

namespace GarageControl.Core.Contracts
{
    public interface IInventoryService
    {
        void ApplyPartChange(Part part, int quantity, JobStatus status);
        void RevertPartChange(Part part, int quantity, JobStatus status);
        Task RecalculateAvailabilityBalanceAsync(string workshopId, string? partId = null);
        // Task CheckLowStockAsync(string workshopId, Part part);
        Task<double> GetPartsToSendAsync(string partId);
        Task<Part?> GetPartByIdAsync(string partId);
    }
}