using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Enums;

namespace HastaneRandevuSistemi.Core.Entities
{
    public class SaglikPersoneli : Calisan
    {
        public SaglikPersoneli()
        {
            Rol = KullaniciRolu.SaglikPersoneli;
        }

        public string GorevAlani { get; set; } = string.Empty;
        public string VardiyaBilgisi { get; set; } = string.Empty;

        public override string KullaniciOzetiGetir()
        {
            return $"[Sağlık Personeli] {Unvan} {TamAd} | Görev: {GorevAlani} | Vardiya: {VardiyaBilgisi}";
        }

        public override bool RandevuOnaylayabilirMi() => true;
    }
}
