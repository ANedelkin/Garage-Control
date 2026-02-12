using Microsoft.EntityFrameworkCore;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Infrastructure.Data.Common
{
    public class Repository : IRepository
    {
        private readonly GarageControlDbContext _context;

        public Repository(GarageControlDbContext context)
        {
            _context = context;
        }

        private DbSet<T> DbSet<T>() where T : class => _context.Set<T>();

        // Queries
        public IQueryable<T> GetAll<T>() where T : class => DbSet<T>();

        public IQueryable<T> GetAllAsNoTracking<T>() where T : class => DbSet<T>().AsNoTracking();

        public IQueryable<T> GetAllAttached<T>() where T : class => DbSet<T>();

        public async Task AddAsync<T>(T entity) where T : class => await DbSet<T>().AddAsync(entity);

        public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class
            => await DbSet<T>().AddRangeAsync(entities);

        public void Delete<T>(T entity) where T : class => DbSet<T>().Remove(entity);

        public async Task DeleteAsync<T>(object id) where T : class
        {
            var entity = await TryGetByIdAsync<T>(id);
            if (entity != null) DbSet<T>().Remove(entity);
        }

        public async Task<T> GetByIdAsync<T>(object id) where T : class
        {
            var entity = await DbSet<T>().FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with id {id} not found.");
            return entity;
        }

        public async Task<T?> TryGetByIdAsync<T>(object id) where T : class
            => await DbSet<T>().FindAsync(id);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
