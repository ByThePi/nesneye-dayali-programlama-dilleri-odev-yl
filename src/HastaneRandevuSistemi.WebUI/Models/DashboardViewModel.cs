using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Enums;

namespace HastaneRandevuSistemi.WebUI.Models
{
    public class DashboardViewModel
    {
        public IReadOnlyList<HastaneSube> Subeler { get; set; } = new List<HastaneSube>();
        public IReadOnlyList<Doktor> Doktorlar { get; set; } = new List<Doktor>();
        public IReadOnlyList<SaglikHizmeti> Hizmetler { get; set; } = new List<SaglikHizmeti>();
        public IReadOnlyList<Kullanici> Kullanicilar { get; set; } = new List<Kullanici>();
        public IReadOnlyList<Randevu> Randevular { get; set; } = new List<Randevu>();
    }

    public class RandevuSayfaViewModel
    {
        public IReadOnlyList<Randevu> Randevular { get; set; } = new List<Randevu>();
        public IReadOnlyList<HastaneSube> Subeler { get; set; } = new List<HastaneSube>();
        public IReadOnlyList<Hasta> Hastalar { get; set; } = new List<Hasta>();
        public IReadOnlyList<Doktor> Doktorlar { get; set; } = new List<Doktor>();
        public IReadOnlyList<SaglikHizmeti> Hizmetler { get; set; } = new List<SaglikHizmeti>();
        public IReadOnlyList<Kullanici> OnayYetkilileri { get; set; } = new List<Kullanici>();

        public int? SeciliSubeId { get; set; }
        public int? SeciliDoktorId { get; set; }
        public RandevuDurumu? SeciliDurum { get; set; }
    }

    public class KurumYonetimSayfaViewModel
    {
        public IReadOnlyList<SaglikKurumu> Kurumlar { get; set; } = new List<SaglikKurumu>();
        public IReadOnlyList<HastaneSube> Subeler { get; set; } = new List<HastaneSube>();
        public IReadOnlyList<Departman> Departmanlar { get; set; } = new List<Departman>();
        public IReadOnlyList<SaglikHizmeti> Hizmetler { get; set; } = new List<SaglikHizmeti>();
        public IReadOnlyList<Doktor> Doktorlar { get; set; } = new List<Doktor>();
        public IReadOnlyList<Kullanici> Kullanicilar { get; set; } = new List<Kullanici>();
    }
}
