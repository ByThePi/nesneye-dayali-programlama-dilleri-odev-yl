using HastaneRandevuSistemi.Core.Enums;

namespace HastaneRandevuSistemi.Core.Entities.Abstract
{
    /// <summary>
    /// Sistemdeki tüm kullanıcı rolleri için ortak soyut temel sınıf.
    /// </summary>
    public abstract class Kullanici : BaseEntity
    {
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string SifreHash { get; set; } = string.Empty;
        public KullaniciRolu Rol { get; protected set; }

        public string TamAd => $"{Ad} {Soyad}".Trim();

        public abstract string KullaniciOzetiGetir();

        /// <summary>
        /// Kullanıcının randevu onaylama/reddetme yetkisi olup olmadığını döner.
        /// </summary>
        public virtual bool RandevuOnaylayabilirMi() => false;

        /// <summary>
        /// Kullanıcının hastane/şube içi bölüm, hizmet ve çalışan tanımlama yetkisi olup olmadığını döner.
        /// </summary>
        public virtual bool KurumTanimlayabilirMi() => false;

        /// <summary>
        /// Kullanıcının sisteme yeni bir hastane/klinik şubesi ve şube yöneticisi tanımlama yetkisi olup olmadığını döner.
        /// Yalnızca Sistem Yöneticisi (Ana Admin) bu yetkiye sahiptir.
        /// </summary>
        public virtual bool SubeTanimlayabilirMi() => false;

        /// <summary>
        /// Kullanıcının belirtilen hastane şubesi üzerinde yönetim yetkisi olup olmadığını kontrol eder.
        /// </summary>
        public virtual bool SubeYonetimYetkisiVarMi(int hastaneSubeId) => false;
    }
}
