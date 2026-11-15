using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Enums;

namespace HastaneRandevuSistemi.Core.Entities
{
    /// <summary>
    /// Tüm sağlık grubu / sistem genelinde yeni hastane şubeleri ve şube yöneticileri tanımlayabilen Ana Sistem Yöneticisi (Super Admin).
    /// </summary>
    public class SistemYoneticisi : Kullanici
    {
        public SistemYoneticisi()
        {
            Rol = KullaniciRolu.SistemYoneticisi;
        }

        public string Unvan { get; set; } = "Genel Merkez Sistem Yöneticisi";

        public override string KullaniciOzetiGetir()
        {
            return $"{TamAd} - {Unvan}";
        }

        public override bool RandevuOnaylayabilirMi() => true;

        public override bool KurumTanimlayabilirMi() => true;

        public override bool SubeTanimlayabilirMi() => true;

        public override bool SubeYonetimYetkisiVarMi(int hastaneSubeId) => true;
    }
}
