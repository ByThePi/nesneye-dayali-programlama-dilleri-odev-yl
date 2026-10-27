using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Enums;

namespace HastaneRandevuSistemi.Core.Entities
{
    public class Doktor : Calisan
    {
        public Doktor()
        {
            Rol = KullaniciRolu.Doktor;
        }

        public string UzmanlikAlani { get; set; } = string.Empty;
        public string DiplomaNo { get; set; } = string.Empty;

        public ICollection<DoktorHizmet> DoktorHizmetleri { get; set; } = new List<DoktorHizmet>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        public override string KullaniciOzetiGetir()
        {
            return $"[Doktor] {Unvan} {TamAd} | Uzmanlık: {UzmanlikAlani} | Sicil: {SicilNo}";
        }

        public override bool RandevuOnaylayabilirMi() => true;
    }
}
