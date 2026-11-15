using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Enums;

namespace HastaneRandevuSistemi.Core.Entities
{
    /// <summary>
    /// Belirli bir hastane/klinik şubesinden sorumlu yönetici.
    /// Yalnızca kendi şubesine (`SorumluSubeId`) ait bölümleri, hizmetleri ve çalışanları yönetebilir.
    /// </summary>
    public class KurumYoneticisi : Kullanici
    {
        public KurumYoneticisi()
        {
            Rol = KullaniciRolu.KurumYoneticisi;
        }

        public int? SorumluSubeId { get; set; }
        public HastaneSube? SorumluSube { get; set; }
        public string YetkiUnvani { get; set; } = "Hastane / Klinik Yöneticisi";

        public override string KullaniciOzetiGetir()
        {
            return $"{TamAd} - {YetkiUnvani}";
        }

        public override bool RandevuOnaylayabilirMi() => true;

        public override bool KurumTanimlayabilirMi() => true;

        public override bool SubeTanimlayabilirMi() => false;

        public override bool SubeYonetimYetkisiVarMi(int hastaneSubeId)
        {
            return SorumluSubeId.HasValue && SorumluSubeId.Value == hastaneSubeId;
        }
    }
}
