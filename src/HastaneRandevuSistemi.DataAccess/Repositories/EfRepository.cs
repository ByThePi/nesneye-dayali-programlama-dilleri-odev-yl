using System.Linq.Expressions;
using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Interfaces;
using HastaneRandevuSistemi.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.DataAccess.Repositories
{
    public class EfRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public EfRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            entity.GuncellemeTarihiniAyarla();
            _dbSet.Update(entity);
        }

        public void Remove(T entity)
        {
            entity.Pasiflestir();
            _dbSet.Update(entity);
        }
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IRepository<T> Repository<T>() where T : BaseEntity
        {
            var type = typeof(T);
            if (!_repositories.TryGetValue(type, out var repo))
            {
                repo = new EfRepository<T>(_context);
                _repositories[type] = repo;
            }

            return (IRepository<T>)repo;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
