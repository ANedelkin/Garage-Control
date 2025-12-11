
using Microsoft.EntityFrameworkCore;
using GarageControl.Infrastructure.Data;

namespace GarageControl.Infrastructure.Data.Common
{
    public class Repository : IRepository
    {
        private readonly GarageControlDbContext _context;

        public Repository(GarageControlDbContext context)
        {
            _context = context;
        }

        private DbSet<T> DbSet<T>() where T : class
        {
            return _context.Set<T>();
        }
        public IQueryable<T> GetAllAsync<T>() where T : class
        {
            return DbSet<T>();
        }
        public IQueryable<T> GetAllAttachedAsync<T>() where T : class
        {
            return DbSet<T>();
        }
        public IQueryable<T> GetAllAsNoTrackingAsync<T>() where T : class
        {
            return DbSet<T>().AsNoTracking();
        }
        public async Task AddAsync<T>(T entity) where T : class
        {
            await DbSet<T>().AddAsync(entity);
        }
        public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class
        {
            await DbSet<T>().AddRangeAsync(entities);
        }
        public async Task DeleteAsync<T>(object id) where T : class
        {
            T? entity = await GetByIdAsync<T>(id);

            if (entity == null)
            {
                return;
            }

            DbSet<T>().Remove(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            DbSet<T>().Remove(entity);
        }

        public async Task<T> GetByIdAsync<T>(object id) where T : class
        {
            T? entity = await DbSet<T>().FindAsync(id);

            if(entity == null)
            {
                throw new Exception($"Entity of type {typeof(T).Name} with id {id} not found");
            }

            return entity;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


    }
}