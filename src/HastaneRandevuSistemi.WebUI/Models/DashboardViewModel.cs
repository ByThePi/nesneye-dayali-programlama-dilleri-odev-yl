using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Enums;

namespace HastaneRandevuSistemi.WebUI.Models
{
    public class GirisSayfaViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
        public IReadOnlyList<Kullanici> DemoHesaplar { get; set; } = new List<Kullanici>();
        public IReadOnlyList<HastaneSube> Subeler { get; set; } = new List<HastaneSube>();
    }

    public class DashboardViewModel
    {
        public IReadOnlyList<HastaneSube> Subeler { get; set; } = new List<HastaneSube>();
        public IReadOnlyList<Doktor> Doktorlar { get; set; } = new List<Doktor>();
        public IReadOnlyList<SaglikHizmeti> Hizmetler { get; set; } = new List<SaglikHizmeti>();
        public IReadOnlyList<Randevu> Randevular { get; set; } = new List<Randevu>();
        public Kullanici? AktifKullanici { get; set; }
        public string? SorumluSubeAdi { get; set; }

        // Filtre Seçenekleri İçin Tüm Aktif Kayıtlar
        public IReadOnlyList<HastaneSube> TumSubeler { get; set; } = new List<HastaneSube>();
        public IReadOnlyList<Departman> TumDepartmanlar { get; set; } = new List<Departman>();
        public IReadOnlyList<Doktor> TumDoktorlar { get; set; } = new List<Doktor>();

        // Seçili Filtre Değerleri
        public int? SeciliSubeId { get; set; }
        public int? SeciliDepartmanId { get; set; }
        public int? SeciliDoktorId { get; set; }
        public string? AramaMetni { get; set; }
    }

    public class RandevuSayfaViewModel
    {
        public IReadOnlyList<Randevu> Randevular { get; set; } = new List<Randevu>();
        public IReadOnlyList<HastaneSube> Subeler { get; set; } = new List<HastaneSube>();
        public IReadOnlyList<Hasta> Hastalar { get; set; } = new List<Hasta>();
        public IReadOnlyList<Doktor> Doktorlar { get; set; } = new List<Doktor>();
        public IReadOnlyList<SaglikHizmeti> Hizmetler { get; set; } = new List<SaglikHizmeti>();
        public Kullanici? AktifKullanici { get; set; }

        public int? SeciliSubeId { get; set; }
        public int? SeciliDoktorId { get; set; }
        public RandevuDurumu? SeciliDurum { get; set; }
        public string Siralama { get; set; } = "kronolojik";
    }

    public class KurumYonetimSayfaViewModel
    {
        public Kullanici? AktifYonetici { get; set; }
        public bool SistemYoneticisiMi { get; set; }
        public int? SorumluSubeId { get; set; }
        public string KapsamBasligi { get; set; } = string.Empty;

        public IReadOnlyList<SaglikKurumu> Kurumlar { get; set; } = new List<SaglikKurumu>();
        public IReadOnlyList<HastaneSube> Subeler { get; set; } = new List<HastaneSube>();
        public IReadOnlyList<Departman> Departmanlar { get; set; } = new List<Departman>();
        public IReadOnlyList<SaglikHizmeti> Hizmetler { get; set; } = new List<SaglikHizmeti>();
        public IReadOnlyList<Doktor> Doktorlar { get; set; } = new List<Doktor>();
        public IReadOnlyList<SaglikPersoneli> SaglikPersonelleri { get; set; } = new List<SaglikPersoneli>();
        public IReadOnlyList<KurumYoneticisi> SubeYoneticileri { get; set; } = new List<KurumYoneticisi>();
    }
}
