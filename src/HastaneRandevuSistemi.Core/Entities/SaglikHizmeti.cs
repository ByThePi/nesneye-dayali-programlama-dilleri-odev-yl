using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Enums;
using HastaneRandevuSistemi.Core.Exceptions;

namespace HastaneRandevuSistemi.Core.Entities
{
    /// <summary>
    /// Kurumun sunduğu sağlık hizmetlerini (Genel Muayene, Radyoloji, Laboratuvar vb.),
    /// süresini ve ücretini kapsülleyerek temsil eder.
    /// </summary>
    public class SaglikHizmeti : BaseEntity
    {
        public int DepartmanId { get; set; }
        public Departman? Departman { get; set; }

        public string HizmetAdi { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public HizmetTipi HizmetTipi { get; set; } = HizmetTipi.GenelMuayene;

        public int SureDakika { get; private set; } = 30;
        public decimal TemelUcret { get; private set; }

        public ICollection<DoktorHizmet> DoktorHizmetleri { get; set; } = new List<DoktorHizmet>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        public void HizmetBilgileriniAyarla(int sureDakika, decimal temelUcret)
        {
            if (sureDakika <= 0 || sureDakika > 480)
            {
                throw new DomainException("Hizmet süresi 1 ile 480 dakika arasında olmalıdır.");
            }

            if (temelUcret < 0)
            {
                throw new DomainException("Hizmet ücreti negatif olamaz.");
            }

            SureDakika = sureDakika;
            TemelUcret = temelUcret;
            GuncellemeTarihiniAyarla();
        }

        /// <summary>
        /// Çok biçimli (Polymorphic) ücret hesaplama metodu.
        /// Özel sigortalı hastalara hizmet tipine göre indirim uygular.
        /// </summary>
        public virtual decimal NetUcretHesapla(bool ozelSigortaVarMi, decimal doktorOzelFarki = 0m)
        {
            decimal bazToplam = TemelUcret + doktorOzelFarki;
            if (!ozelSigortaVarMi)
            {
                return bazToplam;
            }

            decimal indirimOrani = HizmetTipi switch
            {
                HizmetTipi.KontrolMuayenesi => 1.00m,
                HizmetTipi.Laboratuvar => 0.40m,
                HizmetTipi.Radyoloji => 0.35m,
                _ => 0.25m
            };

            return Math.Round(bazToplam * (1 - indirimOrani), 2);
        }
    }
}
