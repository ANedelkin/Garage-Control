using GarageControl.Core.ViewModels;

public interface IJobService
{
    Task<MethodResponseVM> CreateJobAsync(
        string userId,
        string orderId,
        string workshopId,
        CreateJobVM model);

    Task<MethodResponseVM> UpdateJobAsync(
        string userId,
        string jobId,
        string workshopId,
        UpdateJobVM model);

    Task<JobDetailsVM?> GetJobByIdAsync(
        string jobId,
        string workshopId);

    Task<List<JobToDoVM>> GetMyJobsAsync(
        string userId,
        string workshopId);
}
