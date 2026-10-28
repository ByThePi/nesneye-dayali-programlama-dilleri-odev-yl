using HastaneRandevuSistemi.Core.Entities;

namespace HastaneRandevuSistemi.Core.Entities.Abstract
{
    /// <summary>
    /// Hastane veya klinikte görev yapan tüm sağlık çalışanlarının (Doktor, Sağlık Personeli)
    /// ortak özelliklerini barındıran soyut sınıf.
    /// </summary>
    public abstract class Calisan : Kullanici
    {
        public int HastaneSubeId { get; set; }
        public HastaneSube? HastaneSube { get; set; }

        public int DepartmanId { get; set; }
        public Departman? Departman { get; set; }

        public string SicilNo { get; set; } = string.Empty;
        public string Unvan { get; set; } = string.Empty;
        public DateTime IseBaslamaTarihi { get; set; } = DateTime.UtcNow;

        public ICollection<CalisanUygunluk> UygunlukTakvimi { get; set; } = new List<CalisanUygunluk>();
    }
}
