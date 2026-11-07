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
            if (tarih.Date < DateTime.Today)
            {
                throw new DomainException("Geçmiş bir tarihe randevu oluşturulamaz.");
            }

            if (baslangicSaati < TimeSpan.Zero || baslangicSaati >= TimeSpan.FromHours(24))
            {
                throw new DomainException("Geçerli bir başlangıç saati seçilmelidir.");
            }

            if (tarih.Date == DateTime.Today && baslangicSaati < DateTime.Now.TimeOfDay)
            {
                throw new DomainException("Bugün için geçmiş bir saate randevu oluşturulamaz.");
            }

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

            if (islemYapan is Doktor && islemYapan.Id != DoktorId)
            {
                throw new YetkisizIslemException("Bir doktor yalnızca kendisine ait randevuları onaylayabilir.");
            }

            if (Durum != RandevuDurumu.OnayBekliyor)
            {
                throw new DomainException("Yalnızca onay bekleyen randevular onaylanabilir.");
            }

            Durum = RandevuDurumu.Onaylandi;
            OnaylayanKullaniciId = islemYapan.Id;
            OnayNotu = string.IsNullOrWhiteSpace(onayNotu)
                ? $"{islemYapan.TamAd} ({islemYapan.Rol}) tarafından onaylandı."
                : onayNotu.Trim();
            OnayTarihi = DateTime.UtcNow;
            GuncellemeTarihiniAyarla();
        }

        public void Reddet(Kullanici islemYapan, string redSebebi)
        {
            if (!islemYapan.RandevuOnaylayabilirMi())
            {
                throw new YetkisizIslemException($"{islemYapan.TamAd} ({islemYapan.Rol}) randevu reddetme yetkisine sahip değildir.");
            }

            if (islemYapan is Doktor && islemYapan.Id != DoktorId)
            {
                throw new YetkisizIslemException("Bir doktor yalnızca kendisine ait randevuları reddedebilir.");
            }

            if (Durum != RandevuDurumu.OnayBekliyor)
            {
                throw new DomainException("Yalnızca onay bekleyen randevular reddedilebilir.");
            }

            if (string.IsNullOrWhiteSpace(redSebebi))
            {
                throw new DomainException("Randevu reddedilirken açıklama girilmesi zorunludur.");
            }

            Durum = RandevuDurumu.Reddedildi;
            OnaylayanKullaniciId = islemYapan.Id;
            OnayNotu = $"{islemYapan.TamAd}: {redSebebi.Trim()}";
            OnayTarihi = DateTime.UtcNow;
            GuncellemeTarihiniAyarla();
        }

        /// <summary>
        /// Onaylanmış bir randevuyu muayene/işlem sonrasında tamamlandı durumuna geçirir.
        /// </summary>
        public void Tamamla(Kullanici islemYapan, string? muayeneSonucNotu = null)
        {
            if (!islemYapan.RandevuOnaylayabilirMi())
            {
                throw new YetkisizIslemException($"{islemYapan.TamAd} ({islemYapan.Rol}) randevu tamamlama yetkisine sahip değildir.");
            }

            if (islemYapan is Doktor && islemYapan.Id != DoktorId)
            {
                throw new YetkisizIslemException("Bir doktor yalnızca kendisine ait randevunun muayenesini tamamlayabilir.");
            }

            if (Durum != RandevuDurumu.Onaylandi)
            {
                throw new DomainException("Yalnızca onaylanmış durumdaki randevular tamamlandı olarak işaretlenebilir.");
            }

            Durum = RandevuDurumu.Tamamlandi;
            OnaylayanKullaniciId = islemYapan.Id;
            OnayNotu = string.IsNullOrWhiteSpace(muayeneSonucNotu)
                ? $"Muayene/işlem {islemYapan.TamAd} tarafından tamamlandı."
                : $"Muayene Sonucu ({islemYapan.TamAd}): {muayeneSonucNotu.Trim()}";
            GuncellemeTarihiniAyarla();
        }

        public void IptalEt(string iptalSebebi)
        {
            if (Durum == RandevuDurumu.Tamamlandi)
            {
                throw new DomainException("Tamamlanmış bir randevu iptal edilemez.");
            }

            if (Durum == RandevuDurumu.IptalEdildi || Durum == RandevuDurumu.Reddedildi)
            {
                throw new DomainException("Randevu zaten iptal edilmiş veya reddedilmiş durumda.");
            }

            Durum = RandevuDurumu.IptalEdildi;
            OnayNotu = string.IsNullOrWhiteSpace(iptalSebebi)
                ? "Hasta talebiyle iptal edildi."
                : iptalSebebi.Trim();
            GuncellemeTarihiniAyarla();
        }
    }
}
