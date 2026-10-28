using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Exceptions;

namespace HastaneRandevuSistemi.Core.Entities
{
    /// <summary>
    /// Sağlık kurumuna bağlı her bir hastane veya klinik şubesini, çalışma saatlerini ve bölümlerini temsil eder.
    /// </summary>
    public class HastaneSube : BaseEntity
    {
        public int SaglikKurumuId { get; set; }
        public SaglikKurumu? SaglikKurumu { get; set; }

        public string SubeAdi { get; set; } = string.Empty;
        public string Sehir { get; set; } = string.Empty;
        public string Ilce { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;

        public TimeSpan AcilisSaati { get; private set; } = new TimeSpan(8, 0, 0);
        public TimeSpan KapanisSaati { get; private set; } = new TimeSpan(18, 0, 0);

        public ICollection<Departman> Departmanlar { get; set; } = new List<Departman>();
        public ICollection<Calisan> Calisanlar { get; set; } = new List<Calisan>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        /// <summary>
        /// Kapsülleme (Encapsulation): Çalışma saatlerinin mantıksal bütünlüğünü koruyarak günceller.
        /// </summary>
        public void CalismaSaatleriniAyarla(TimeSpan acilis, TimeSpan kapanis)
        {
            if (kapanis <= acilis)
            {
                throw new DomainException("Şube kapanış saati, açılış saatinden sonra olmalıdır.");
            }

            AcilisSaati = acilis;
            KapanisSaati = kapanis;
            GuncellemeTarihiniAyarla();
        }

        public bool CalismaSaatleriIcindeMi(TimeSpan baslangic, TimeSpan bitis)
        {
            return baslangic >= AcilisSaati && bitis <= KapanisSaati && bitis > baslangic;
        }
    }
}
