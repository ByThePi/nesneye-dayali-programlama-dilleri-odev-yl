using HastaneRandevuSistemi.Core.Enums;

namespace HastaneRandevuSistemi.Business.DTOs
{
    public class SubeOlusturDto
    {
        public int IslemYapanKullaniciId { get; set; }
        public int SaglikKurumuId { get; set; }
        public string SubeAdi { get; set; } = string.Empty;
        public string Sehir { get; set; } = string.Empty;
        public string Ilce { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public TimeSpan AcilisSaati { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan KapanisSaati { get; set; } = new TimeSpan(18, 0, 0);
    }

    public class DepartmanOlusturDto
    {
        public int IslemYapanKullaniciId { get; set; }
        public int HastaneSubeId { get; set; }
        public string DepartmanAdi { get; set; } = string.Empty;
        public string KatBilgisi { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
    }

    public class SaglikHizmetiOlusturDto
    {
        public int IslemYapanKullaniciId { get; set; }
        public int DepartmanId { get; set; }
        public string HizmetAdi { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public HizmetTipi HizmetTipi { get; set; } = HizmetTipi.GenelMuayene;
        public int SureDakika { get; set; } = 30;
        public decimal TemelUcret { get; set; }
    }

    public class DoktorKayitDto
    {
        public int IslemYapanKullaniciId { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string Unvan { get; set; } = "Uzm. Dr.";
        public string UzmanlikAlani { get; set; } = string.Empty;
        public string SicilNo { get; set; } = string.Empty;
        public string DiplomaNo { get; set; } = string.Empty;
        public int HastaneSubeId { get; set; }
        public int DepartmanId { get; set; }
        public int SaglikHizmetiId { get; set; }
        public decimal OzelUcretFarki { get; set; }
    }

    public class CalisanUygunlukTanimlaDto
    {
        public int IslemYapanKullaniciId { get; set; }
        public int DoktorId { get; set; }
        public DayOfWeek Gun { get; set; } = DayOfWeek.Monday;
        public TimeSpan BaslangicSaati { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan BitisSaati { get; set; } = new TimeSpan(17, 0, 0);
        public TimeSpan? MolaBaslangic { get; set; } = new TimeSpan(12, 30, 0);
        public TimeSpan? MolaBitis { get; set; } = new TimeSpan(13, 30, 0);
    }
}
