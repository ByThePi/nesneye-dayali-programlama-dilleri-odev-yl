using System.Linq.Expressions;
using HastaneRandevuSistemi.Core.Entities;
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

    public class RandevuRepository : EfRepository<Randevu>, IRandevuRepository
    {
        public RandevuRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Randevu>> TumRandevulariDetayliGetirAsync()
        {
            var randevular = await _dbSet
                .Include(r => r.Hasta)
                .Include(r => r.Doktor)
                .Include(r => r.SaglikHizmeti)
                .Include(r => r.HastaneSube)
                .OrderBy(r => r.RandevuTarihi)
                .ToListAsync();

            // SQLite TimeSpan tipinde SQL ORDER BY desteklemediği için saat sıralaması bellekte (LINQ to Objects) yapılır
            return randevular
                .OrderBy(r => r.RandevuTarihi)
                .ThenBy(r => r.BaslangicSaati)
                .ToList();
        }

        public async Task<IReadOnlyList<Randevu>> GunlukAktifRandevulariGetirAsync(DateTime tarih)
        {
            DateTime hedefTarih = tarih.Date;
            return await _dbSet
                .Where(r => r.RandevuTarihi == hedefTarih && r.AktifMi)
                .ToListAsync();
        }
    }

    public class DoktorRepository : EfRepository<Doktor>, IDoktorRepository
    {
        public DoktorRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Doktor>> DoktorlariDetayliGetirAsync()
        {
            return await _dbSet
                .Include(d => d.HastaneSube)
                .Include(d => d.Departman)
                .Include(d => d.UygunlukTakvimi.Where(u => u.AktifMi))
                .Include(d => d.DoktorHizmetleri.Where(dh => dh.AktifMi))
                    .ThenInclude(dh => dh.SaglikHizmeti)
                .Where(d => d.AktifMi && d.HastaneSube!.AktifMi && d.Departman!.AktifMi)
                .ToListAsync();
        }

        public async Task<Doktor?> DoktorDetayliGetirAsync(int doktorId)
        {
            return await _dbSet
                .Include(d => d.HastaneSube)
                .Include(d => d.UygunlukTakvimi.Where(u => u.AktifMi))
                .Include(d => d.DoktorHizmetleri.Where(dh => dh.AktifMi))
                .FirstOrDefaultAsync(d => d.Id == doktorId && d.AktifMi);
        }
    }

    public class HastaneSubeRepository : EfRepository<HastaneSube>, IHastaneSubeRepository
    {
        public HastaneSubeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<HastaneSube>> SubeleriDetayliGetirAsync()
        {
            return await _dbSet
                .Include(s => s.SaglikKurumu)
                .Include(s => s.Departmanlar.Where(d => d.AktifMi))
                    .ThenInclude(d => d.Hizmetler.Where(h => h.AktifMi))
                .Where(s => s.AktifMi)
                .ToListAsync();
        }
    }

    public class SaglikHizmetiRepository : EfRepository<SaglikHizmeti>, ISaglikHizmetiRepository
    {
        public SaglikHizmetiRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<SaglikHizmeti>> HizmetleriDetayliGetirAsync()
        {
            return await _dbSet
                .Include(h => h.Departman)
                    .ThenInclude(d => d!.HastaneSube)
                .Where(h => h.AktifMi && h.Departman!.AktifMi && h.Departman!.HastaneSube!.AktifMi)
                .ToListAsync();
        }
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        private IRandevuRepository? _randevular;
        private IDoktorRepository? _doktorlar;
        private IHastaneSubeRepository? _hastaneSubeleri;
        private ISaglikHizmetiRepository? _saglikHizmetleri;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IRandevuRepository Randevular => _randevular ??= new RandevuRepository(_context);
        public IDoktorRepository Doktorlar => _doktorlar ??= new DoktorRepository(_context);
        public IHastaneSubeRepository HastaneSubeleri => _hastaneSubeleri ??= new HastaneSubeRepository(_context);
        public ISaglikHizmetiRepository SaglikHizmetleri => _saglikHizmetleri ??= new SaglikHizmetiRepository(_context);

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
