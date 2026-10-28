using HastaneRandevuSistemi.Core.Enums;

namespace HastaneRandevuSistemi.Core.Entities.Abstract
{
    /// <summary>
    /// Sistemdeki tüm kullanıcı rolleri (Hasta, Doktor, Sağlık Personeli, Kurum Yöneticisi) için
    /// kalıtım ve çok biçimlilik (Inheritance & Polymorphism) sağlayan soyut temel sınıf.
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

        /// <summary>
        /// Çok biçimli (Polymorphic) kullanıcı özeti metodu. Alt sınıflar kendi rolüne göre ezer (override).
        /// </summary>
        public abstract string KullaniciOzetiGetir();

        /// <summary>
        /// Kullanıcının randevu onaylama/reddetme yetkisi olup olmadığını döner (Polymorphism).
        /// </summary>
        public virtual bool RandevuOnaylayabilirMi() => false;

        /// <summary>
        /// Kullanıcının hastane/klinik ve hizmet tanımlama yetkisi olup olmadığını döner (Polymorphism).
        /// </summary>
        public virtual bool KurumTanimlayabilirMi() => false;
    }
}
