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

        public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> q = _set.AsQueryable();
            foreach (var inc in includes) q = q.Include(inc);

            var entityType = _ctx.Model.FindEntityType(typeof(T))
                ?? throw new InvalidOperationException($"Entity metadata for {typeof(T).Name} not found.");
            var key = entityType.FindPrimaryKey()
                ?? throw new InvalidOperationException($"Primary key metadata for {typeof(T).Name} not found.");

            if (key.Properties.Count != 1)
            {
                throw new InvalidOperationException("GetByIdAsync only supports entities with a single primary key.");
            }

            var keyProperty = key.Properties[0];
            if (keyProperty.PropertyInfo is null)
            {
                throw new InvalidOperationException($"Primary key property for {typeof(T).Name} is not accessible.");
            }

            if (keyProperty.ClrType != typeof(int))
            {
                throw new InvalidOperationException("GetByIdAsync only supports integer keys.");
            }

            var parameter = Expression.Parameter(typeof(T), "entity");
            var propertyAccess = Expression.MakeMemberAccess(parameter, keyProperty.PropertyInfo);
            var equals = Expression.Equal(propertyAccess, Expression.Constant(id));
            var predicate = Expression.Lambda<Func<T, bool>>(equals, parameter);

            return await q.FirstOrDefaultAsync(predicate);
        }

        public async Task AddAsync(T entity) => await _set.AddAsync(entity);

        public void Update(T entity) => _set.Update(entity);

        public void Delete(T entity) => _set.Remove(entity);

        public Task SaveAsync() => _ctx.SaveChangesAsync();
    }
}
