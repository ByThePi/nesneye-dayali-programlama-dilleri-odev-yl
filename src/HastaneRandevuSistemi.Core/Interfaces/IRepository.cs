using System.Linq.Expressions;
using HastaneRandevuSistemi.Core.Entities.Abstract;

namespace HastaneRandevuSistemi.Core.Interfaces
{
    /// <summary>
    /// Generic Repository Tasarım Deseni (Design Pattern) arayüzü.
    /// </summary>
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);
        Task<IReadOnlyList<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            params Expression<Func<T, object>>[] includes);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }

    /// <summary>
    /// Unit of Work Tasarım Deseni arayüzü.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : BaseEntity;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
