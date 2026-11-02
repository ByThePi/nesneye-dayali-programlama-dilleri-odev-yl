using HastaneRandevuSistemi.Business.DTOs;
using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Exceptions;
using HastaneRandevuSistemi.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Business.Services
{
    public class RandevuService : IRandevuService
    {
        private readonly AppDbContext _context;

        public RandevuService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Randevu>> TumRandevulariGetirAsync()
        {
            return await _context.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Doktor)
                .Include(r => r.SaglikHizmeti)
                .Include(r => r.HastaneSube)
                .OrderByDescending(r => r.RandevuTarihi)
                .ThenBy(r => r.BaslangicSaati)
                .ToListAsync();
        }

        public async Task<Randevu> RandevuOlusturAsync(RandevuOlusturDto model)
        {
            var hasta = await _context.Hastalar.FirstOrDefaultAsync(h => h.Id == model.HastaId && h.AktifMi)
                ?? throw new DomainException("Seçilen hasta sistemde bulunamadı.");

            var doktor = await _context.Doktorlar
                .Include(d => d.HastaneSube)
                .Include(d => d.UygunlukTakvimi)
                .Include(d => d.DoktorHizmetleri)
                .FirstOrDefaultAsync(d => d.Id == model.DoktorId && d.AktifMi)
                ?? throw new DomainException("Seçilen doktor sistemde bulunamadı.");

            var hizmet = await _context.SaglikHizmetleri.FirstOrDefaultAsync(s => s.Id == model.SaglikHizmetiId && s.AktifMi)
                ?? throw new DomainException("Seçilen sağlık hizmeti bulunamadı.");

            // 1. Doktor bu hizmeti sunuyor mu kontrolü (Bugfix: Uzmanlık dışı işlem seçimi engellendi)
            var doktorHizmet = doktor.DoktorHizmetleri.FirstOrDefault(dh => dh.SaglikHizmetiId == hizmet.Id && dh.AktifMi);
            if (doktorHizmet == null)
            {
                throw new DomainException(
                    $"{doktor.Unvan} {doktor.TamAd} ({doktor.UzmanlikAlani}), '{hizmet.HizmetAdi}' işlemini uygulamamaktadır. Lütfen doktorun sunduğu bir sağlık hizmeti seçiniz.");
            }

            decimal ozelFark = doktorHizmet.OzelUcretFarki;

            TimeSpan talepBaslangic = model.BaslangicSaati;
            TimeSpan talepBitis = talepBaslangic.Add(TimeSpan.FromMinutes(hizmet.SureDakika));

            // 2. Şube çalışma saatleri kontrolü
            if (doktor.HastaneSube != null && !doktor.HastaneSube.CalismaSaatleriIcindeMi(talepBaslangic, talepBitis))
            {
                throw new DomainException(
                    $"Seçilen saat aralığı ({talepBaslangic:hh\\:mm} - {talepBitis:hh\\:mm}), " +
                    $"{doktor.HastaneSube.SubeAdi} çalışma saatleri ({doktor.HastaneSube.AcilisSaati:hh\\:mm} - {doktor.HastaneSube.KapanisSaati:hh\\:mm}) dışındadır.");
            }

            // 3. Doktorun uygunluk takvimi kontrolü
            DayOfWeek gun = model.RandevuTarihi.DayOfWeek;
            var gunlukUygunluklar = doktor.UygunlukTakvimi.Where(u => u.Gun == gun && u.AktifMi).ToList();
            if (!gunlukUygunluklar.Any() || !gunlukUygunluklar.Any(u => u.ZamanDilimiUygunMu(talepBaslangic, talepBitis)))
            {
                throw new DomainException(
                    $"{doktor.Unvan} {doktor.TamAd} seçilen gün ve saat aralığında ({talepBaslangic:hh\\:mm} - {talepBitis:hh\\:mm}) uygun değildir (mesai dışı veya öğle molası).");
            }

            // 4. Randevu Çakışma Kontrolü (Doktor veya Hasta için aynı saatte başka randevu var mı?)
            var ayniGundekiRandevular = await _context.Randevular
                .Where(r => r.RandevuTarihi.Date == model.RandevuTarihi.Date && r.AktifMi)
                .ToListAsync();

            var doktorCakisma = ayniGundekiRandevular
                .FirstOrDefault(r => r.DoktorId == doktor.Id && r.ZamanCakisiyorMu(model.RandevuTarihi, talepBaslangic, talepBitis));

            if (doktorCakisma != null)
            {
                throw new RandevuCakismaException(
                    $"{doktor.Unvan} {doktor.TamAd} için {model.RandevuTarihi:dd.MM.yyyy} tarihinde " +
                    $"{doktorCakisma.BaslangicSaati:hh\\:mm}-{doktorCakisma.BitisSaati:hh\\:mm} saatleri arasında başka bir randevu bulunmaktadır.");
            }

            var hastaCakisma = ayniGundekiRandevular
                .FirstOrDefault(r => r.HastaId == hasta.Id && r.ZamanCakisiyorMu(model.RandevuTarihi, talepBaslangic, talepBitis));

            if (hastaCakisma != null)
            {
                throw new RandevuCakismaException(
                    $"Hastanın {model.RandevuTarihi:dd.MM.yyyy} tarihinde {hastaCakisma.BaslangicSaati:hh\\:mm}-{hastaCakisma.BitisSaati:hh\\:mm} saatleri arasında çakışan başka bir randevusu bulunmaktadır.");
            }

            // 5. Çok biçimli (Polymorphic) ücret hesaplama ve randevu oluşturma
            decimal netUcret = hizmet.NetUcretHesapla(hasta.OzelSigortaVarMi, ozelFark);

            var yeniRandevu = new Randevu
            {
                HastaId = hasta.Id,
                DoktorId = doktor.Id,
                SaglikHizmetiId = hizmet.Id,
                HastaneSubeId = doktor.HastaneSubeId,
                HastaNotu = model.HastaNotu
            };

            yeniRandevu.RandevuPlanla(model.RandevuTarihi, talepBaslangic, hizmet.SureDakika, netUcret);

            _context.Randevular.Add(yeniRandevu);
            await _context.SaveChangesAsync();

            return yeniRandevu;
        }

        public async Task RandevuOnaylaAsync(int randevuId, int onaylayanKullaniciId, string? onayNotu)
        {
            var randevu = await _context.Randevular.FirstOrDefaultAsync(r => r.Id == randevuId)
                ?? throw new DomainException("Randevu bulunamadı.");

            var onaylayan = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id == onaylayanKullaniciId && k.AktifMi)
                ?? throw new DomainException("Onaylayan yetkili kullanıcı bulunamadı.");

            randevu.Onayla(onaylayan, onayNotu);
            await _context.SaveChangesAsync();
        }

        public async Task RandevuReddetAsync(int randevuId, int onaylayanKullaniciId, string redSebebi)
        {
            var randevu = await _context.Randevular.FirstOrDefaultAsync(r => r.Id == randevuId)
                ?? throw new DomainException("Randevu bulunamadı.");

            var onaylayan = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id == onaylayanKullaniciId && k.AktifMi)
                ?? throw new DomainException("İşlem yapan yetkili kullanıcı bulunamadı.");

            randevu.Reddet(onaylayan, redSebebi);
            await _context.SaveChangesAsync();
        }

        public async Task RandevuIptalEtAsync(int randevuId, string iptalSebebi)
        {
            var randevu = await _context.Randevular.FirstOrDefaultAsync(r => r.Id == randevuId)
                ?? throw new DomainException("Randevu bulunamadı.");

            randevu.IptalEt(iptalSebebi);
            await _context.SaveChangesAsync();
        }
    }
}
