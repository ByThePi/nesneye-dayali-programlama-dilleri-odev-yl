using HastaneRandevuSistemi.Core.Entities.Abstract;

namespace HastaneRandevuSistemi.Core.Entities
{
    /// <summary>
    /// Hastane veya klinik bünyesindeki tıbbi bölümleri / poliklinikleri (Kardiyoloji, Radyoloji, Laboratuvar vb.) temsil eder.
    /// </summary>
    public class Departman : BaseEntity
    {
        public int HastaneSubeId { get; set; }
        public HastaneSube? HastaneSube { get; set; }

        public string DepartmanAdi { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public string KatBilgisi { get; set; } = string.Empty;

        public ICollection<SaglikHizmeti> Hizmetler { get; set; } = new List<SaglikHizmeti>();
        public ICollection<Calisan> Calisanlar { get; set; } = new List<Calisan>();
    }
}
