using System.Linq.Expressions;
using HastaneRandevuSistemi.Core.Entities;
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

    public interface IRandevuRepository : IRepository<Randevu>
    {
        Task<IReadOnlyList<Randevu>> TumRandevulariDetayliGetirAsync();
        Task<IReadOnlyList<Randevu>> GunlukAktifRandevulariGetirAsync(DateTime tarih);
    }

    public interface IDoktorRepository : IRepository<Doktor>
    {
        Task<IReadOnlyList<Doktor>> DoktorlariDetayliGetirAsync();
        Task<Doktor?> DoktorDetayliGetirAsync(int doktorId);
    }

    public interface IHastaneSubeRepository : IRepository<HastaneSube>
    {
        Task<IReadOnlyList<HastaneSube>> SubeleriDetayliGetirAsync();
    }

    public interface ISaglikHizmetiRepository : IRepository<SaglikHizmeti>
    {
        Task<IReadOnlyList<SaglikHizmeti>> HizmetleriDetayliGetirAsync();
    }

    /// <summary>
    /// Unit of Work Tasarım Deseni arayüzü.
    /// Business katmanının doğrudan DbContext yerine soyutlamalara bağımlı olmasını (DIP) sağlar.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRandevuRepository Randevular { get; }
        IDoktorRepository Doktorlar { get; }
        IHastaneSubeRepository HastaneSubeleri { get; }
        ISaglikHizmetiRepository SaglikHizmetleri { get; }
        IRepository<T> Repository<T>() where T : BaseEntity;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
