using Microsoft.EntityFrameworkCore;

namespace GarageControl.Infrastructure.Data.Common
{
    public interface IRepository
    {
        IQueryable<T> GetAllAsync<T>() where T : class;
        IQueryable<T> GetAllAsNoTrackingAsync<T>()where T : class;
        IQueryable<T> GetAllAttachedAsync<T>() where T : class;
        Task<T> GetByIdAsync<T>(object id)where T : class;
        Task AddAsync<T>(T entity)where T : class;
        Task AddRangeAsync<T>(IEnumerable<T> entities)where T : class;
        Task DeleteAsync<T>(object id)where T : class;
        void Delete<T>(T entity) where T : class;
        Task SaveChangesAsync();
    }
}