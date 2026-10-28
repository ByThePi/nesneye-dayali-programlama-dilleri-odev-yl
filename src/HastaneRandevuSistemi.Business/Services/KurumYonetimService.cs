using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Business.Services
{
    public class KurumYonetimService : IKurumYonetimService
    {
        private readonly AppDbContext _context;

        public KurumYonetimService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<HastaneSube>> SubeleriDetayliGetirAsync()
        {
            return await _context.HastaneSubeleri
                .Include(s => s.SaglikKurumu)
                .Include(s => s.Departmanlar)
                    .ThenInclude(d => d.Hizmetler)
                .Where(s => s.AktifMi)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Doktor>> DoktorlariDetayliGetirAsync()
        {
            return await _context.Doktorlar
                .Include(d => d.HastaneSube)
                .Include(d => d.Departman)
                .Include(d => d.UygunlukTakvimi)
                .Include(d => d.DoktorHizmetleri)
                    .ThenInclude(dh => dh.SaglikHizmeti)
                .Where(d => d.AktifMi)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<SaglikHizmeti>> HizmetleriGetirAsync()
        {
            return await _context.SaglikHizmetleri
                .Include(h => h.Departman)
                    .ThenInclude(d => d!.HastaneSube)
                .Where(h => h.AktifMi)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Hasta>> HastalariGetirAsync()
        {
            return await _context.Hastalar
                .Where(h => h.AktifMi)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Kullanici>> TumKullanicilariGetirAsync()
        {
            return await _context.Kullanicilar
                .Where(k => k.AktifMi)
                .ToListAsync();
        }
    }
}
