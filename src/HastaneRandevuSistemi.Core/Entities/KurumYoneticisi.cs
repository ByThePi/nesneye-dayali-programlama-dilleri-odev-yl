using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Enums;

namespace HastaneRandevuSistemi.Core.Entities
{
    public class KurumYoneticisi : Kullanici
    {
        public KurumYoneticisi()
        {
            Rol = KullaniciRolu.KurumYoneticisi;
        }

        public int? SorumluSubeId { get; set; }
        public HastaneSube? SorumluSube { get; set; }
        public string YetkiUnvani { get; set; } = "Başhekim / Klinik Yöneticisi";

        public override string KullaniciOzetiGetir()
        {
            return $"[Kurum Yöneticisi] {TamAd} | Yetki: {YetkiUnvani}";
        }

        public override bool RandevuOnaylayabilirMi() => true;

        public override bool KurumTanimlayabilirMi() => true;
    }
}
