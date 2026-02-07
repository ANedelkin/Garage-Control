using GarageControl.Core.ViewModels.Orders;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Shared.Enums;

namespace GarageControl.Core.Contracts
{
    public interface IInventoryService
    {
        Task ApplyPartChangeAsync(Part part, int quantity, JobStatus status);
        Task RevertPartChangeAsync(Part part, int quantity, JobStatus status);
        Task HandleStatusTransitionAsync(IEnumerable<JobPart> jobParts, JobStatus oldStatus, JobStatus newStatus);
        Task RecalculateAvailabilityBalanceAsync(string workshopId, string? partId = null);
        Task CheckLowStockAsync(string workshopId, Part part);
        Task<int> GetPartsReservedAsync(string partId);
    }
}