using HUNting.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HUNting.API.Services
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly HuntingContext _ctx;
        private readonly DbSet<T> _set;

        public GenericRepository(HuntingContext ctx)
        {
            _ctx = ctx;
            _set = ctx.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> q = _set.AsQueryable();
            foreach (var inc in includes) q = q.Include(inc);
            return await q.ToListAsync();
        }

        public Task<T?> GetByIdAsync(int id) => _set.FindAsync(id).AsTask();

        public async Task AddAsync(T entity) => await _set.AddAsync(entity);

        public void Update(T entity) => _set.Update(entity);

        public void Delete(T entity) => _set.Remove(entity);

        public Task SaveAsync() => _ctx.SaveChangesAsync();
    }
}
