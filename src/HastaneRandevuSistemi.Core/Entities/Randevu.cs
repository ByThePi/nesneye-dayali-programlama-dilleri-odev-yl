using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Enums;
using HastaneRandevuSistemi.Core.Exceptions;

namespace HastaneRandevuSistemi.Core.Entities
{
    /// <summary>
    /// Randevu detaylarını (işlem, süre, ücret, çalışan, tarih/saat), çakışma kontrolünü ve onay mekanizmasını kapsüller.
    /// </summary>
    public class Randevu : BaseEntity
    {
        public int HastaId { get; set; }
        public Hasta? Hasta { get; set; }

        public int DoktorId { get; set; }
        public Doktor? Doktor { get; set; }

        public int SaglikHizmetiId { get; set; }
        public SaglikHizmeti? SaglikHizmeti { get; set; }

        public int HastaneSubeId { get; set; }
        public HastaneSube? HastaneSube { get; set; }

        public DateTime RandevuTarihi { get; private set; }
        public TimeSpan BaslangicSaati { get; private set; }
        public TimeSpan BitisSaati { get; private set; }
        public int SureDakika { get; private set; }
        public decimal ToplamUcret { get; private set; }

        public RandevuDurumu Durum { get; private set; } = RandevuDurumu.OnayBekliyor;
        public string? HastaNotu { get; set; }
        public string? OnayNotu { get; private set; }
        public int? OnaylayanKullaniciId { get; private set; }
        public DateTime? OnayTarihi { get; private set; }

        public void RandevuPlanla(
            DateTime tarih,
            TimeSpan baslangicSaati,
            int sureDakika,
            decimal toplamUcret)
        {
            if (sureDakika <= 0)
            {
                throw new DomainException("Randevu süresi 0'dan büyük olmalıdır.");
            }

            if (toplamUcret < 0)
            {
                throw new DomainException("Randevu ücreti negatif olamaz.");
            }

            RandevuTarihi = tarih.Date;
            BaslangicSaati = baslangicSaati;
            SureDakika = sureDakika;
            BitisSaati = baslangicSaati.Add(TimeSpan.FromMinutes(sureDakika));
            ToplamUcret = toplamUcret;
            Durum = RandevuDurumu.OnayBekliyor;
            GuncellemeTarihiniAyarla();
        }

        /// <summary>
        /// Verilen tarih ve saat aralığı ile bu randevunun çakışıp çakışmadığını kontrol eder.
        /// İptal edilen veya reddedilen randevular çakışma yaratmaz.
        /// </summary>
        public bool ZamanCakisiyorMu(DateTime tarih, TimeSpan talepBaslangic, TimeSpan talepBitis)
        {
            if (!AktifMi || Durum == RandevuDurumu.IptalEdildi || Durum == RandevuDurumu.Reddedildi)
            {
                return false;
            }

            if (RandevuTarihi.Date != tarih.Date)
            {
                return false;
            }

            return talepBaslangic < BitisSaati && talepBitis > BaslangicSaati;
        }

        /// <summary>
        /// Çok biçimli (Polymorphic) yetki kontrolü ile randevuyu onaylar.
        /// </summary>
        public void Onayla(Kullanici islemYapan, string? onayNotu = null)
        {
            if (!islemYapan.RandevuOnaylayabilirMi())
            {
                throw new YetkisizIslemException($"{islemYapan.TamAd} ({islemYapan.Rol}) randevu onaylama yetkisine sahip değildir.");
            }

            if (Durum != RandevuDurumu.OnayBekliyor)
            {
                throw new DomainException("Yalnızca onay bekleyen randevular onaylanabilir.");
            }

            Durum = RandevuDurumu.Onaylandi;
            OnaylayanKullaniciId = islemYapan.Id;
            OnayNotu = onayNotu;
            OnayTarihi = DateTime.UtcNow;
            GuncellemeTarihiniAyarla();
        }

        public void Reddet(Kullanici islemYapan, string redSebebi)
        {
            if (!islemYapan.RandevuOnaylayabilirMi())
            {
                throw new YetkisizIslemException($"{islemYapan.TamAd} ({islemYapan.Rol}) randevu reddetme yetkisine sahip değildir.");
            }

            if (string.IsNullOrWhiteSpace(redSebebi))
            {
                throw new DomainException("Randevu reddedilirken açıklama girilmesi zorunludur.");
            }

            Durum = RandevuDurumu.Reddedildi;
            OnaylayanKullaniciId = islemYapan.Id;
            OnayNotu = redSebebi;
            OnayTarihi = DateTime.UtcNow;
            GuncellemeTarihiniAyarla();
        }

        public void IptalEt(string iptalSebebi)
        {
            if (Durum == RandevuDurumu.Tamamlandi)
            {
                throw new DomainException("Tamamlanmış bir randevu iptal edilemez.");
            }

            Durum = RandevuDurumu.IptalEdildi;
            OnayNotu = iptalSebebi;
            GuncellemeTarihiniAyarla();
        }
    }
}
