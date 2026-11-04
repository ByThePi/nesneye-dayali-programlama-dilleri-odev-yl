using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Enums;
using HastaneRandevuSistemi.Core.Exceptions;
using HastaneRandevuSistemi.Core.Strategies;

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
        /// Strategy Tasarım Deseni üzerinden çok biçimli (Polymorphic) ücret hesaplama metodu.
        /// </summary>
        public virtual decimal NetUcretHesapla(IUcretHesaplamaStratejisi strateji, decimal doktorOzelFarki = 0m)
        {
            return strateji.Hesapla(this, doktorOzelFarki);
        }

        /// <summary>
        /// Sigorta durumuna göre uygun stratejiyi seçerek net ücreti hesaplar.
        /// </summary>
        public virtual decimal NetUcretHesapla(bool ozelSigortaVarMi, decimal doktorOzelFarki = 0m)
        {
            var strateji = UcretStratejiSecici.StratejiBelirle(ozelSigortaVarMi);
            return NetUcretHesapla(strateji, doktorOzelFarki);
        }
    }
}
