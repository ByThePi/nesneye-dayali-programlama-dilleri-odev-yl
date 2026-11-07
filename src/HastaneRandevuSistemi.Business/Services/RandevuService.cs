using HastaneRandevuSistemi.Business.DTOs;
using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Exceptions;
using HastaneRandevuSistemi.Core.Interfaces;
using HastaneRandevuSistemi.Core.Strategies;

namespace HastaneRandevuSistemi.Business.Services
{
    /// <summary>
    /// Dependency Inversion Prensibi (DIP) gereği doğrudan AppDbContext yerine
    /// yalnızca IUnitOfWork ve Repository soyutlamaları üzerinden çalışan randevu iş servisi.
    /// </summary>
    public class RandevuService : IRandevuService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RandevuService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<Randevu>> TumRandevulariGetirAsync()
        {
            return await _unitOfWork.Randevular.TumRandevulariDetayliGetirAsync();
        }

        public async Task<Randevu> RandevuOlusturAsync(RandevuOlusturDto model)
        {
            var hasta = await _unitOfWork.Repository<Hasta>().GetByIdAsync(model.HastaId);
            if (hasta == null || !hasta.AktifMi)
            {
                throw new DomainException("Seçilen hasta sistemde bulunamadı.");
            }

            var doktor = await _unitOfWork.Doktorlar.DoktorDetayliGetirAsync(model.DoktorId)
                ?? throw new DomainException("Seçilen doktor sistemde bulunamadı.");

            var hizmet = await _unitOfWork.SaglikHizmetleri.GetByIdAsync(model.SaglikHizmetiId);
            if (hizmet == null || !hizmet.AktifMi)
            {
                throw new DomainException("Seçilen sağlık hizmeti bulunamadı.");
            }

            // 1. Doktor bu hizmeti sunuyor mu kontrolü
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
            var ayniGundekiRandevular = await _unitOfWork.Randevular.GunlukAktifRandevulariGetirAsync(model.RandevuTarihi);

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

            // 5. Strategy Tasarım Deseni ile çok biçimli (Polymorphic) ücret hesaplama ve randevu oluşturma
            IUcretHesaplamaStratejisi ucretStratejisi = UcretStratejiSecici.StratejiBelirle(hasta.OzelSigortaVarMi);
            decimal netUcret = hizmet.NetUcretHesapla(ucretStratejisi, ozelFark);

            var yeniRandevu = new Randevu
            {
                HastaId = hasta.Id,
                DoktorId = doktor.Id,
                SaglikHizmetiId = hizmet.Id,
                HastaneSubeId = doktor.HastaneSubeId,
                HastaNotu = model.HastaNotu
            };

            yeniRandevu.RandevuPlanla(model.RandevuTarihi, talepBaslangic, hizmet.SureDakika, netUcret);

            await _unitOfWork.Randevular.AddAsync(yeniRandevu);
            await _unitOfWork.SaveChangesAsync();

            return yeniRandevu;
        }

        public async Task RandevuOnaylaAsync(int randevuId, int onaylayanKullaniciId, string? onayNotu)
        {
            var randevu = await _unitOfWork.Randevular.GetByIdAsync(randevuId)
                ?? throw new DomainException("Randevu bulunamadı.");

            var onaylayan = await _unitOfWork.Repository<Kullanici>().GetByIdAsync(onaylayanKullaniciId);
            if (onaylayan == null || !onaylayan.AktifMi)
            {
                throw new DomainException("Onaylayan yetkili kullanıcı bulunamadı.");
            }

            randevu.Onayla(onaylayan, onayNotu);
            _unitOfWork.Randevular.Update(randevu);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RandevuReddetAsync(int randevuId, int onaylayanKullaniciId, string redSebebi)
        {
            var randevu = await _unitOfWork.Randevular.GetByIdAsync(randevuId)
                ?? throw new DomainException("Randevu bulunamadı.");

            var onaylayan = await _unitOfWork.Repository<Kullanici>().GetByIdAsync(onaylayanKullaniciId);
            if (onaylayan == null || !onaylayan.AktifMi)
            {
                throw new DomainException("İşlem yapan yetkili kullanıcı bulunamadı.");
            }

            randevu.Reddet(onaylayan, redSebebi);
            _unitOfWork.Randevular.Update(randevu);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RandevuTamamlaAsync(int randevuId, int onaylayanKullaniciId, string? muayeneSonucNotu)
        {
            var randevu = await _unitOfWork.Randevular.GetByIdAsync(randevuId)
                ?? throw new DomainException("Randevu bulunamadı.");

            var islemYapan = await _unitOfWork.Repository<Kullanici>().GetByIdAsync(onaylayanKullaniciId);
            if (islemYapan == null || !islemYapan.AktifMi)
            {
                throw new DomainException("İşlem yapan yetkili kullanıcı bulunamadı.");
            }

            randevu.Tamamla(islemYapan, muayeneSonucNotu);
            _unitOfWork.Randevular.Update(randevu);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RandevuIptalEtAsync(int randevuId, string iptalSebebi)
        {
            var randevu = await _unitOfWork.Randevular.GetByIdAsync(randevuId)
                ?? throw new DomainException("Randevu bulunamadı.");

            randevu.IptalEt(iptalSebebi);
            _unitOfWork.Randevular.Update(randevu);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
