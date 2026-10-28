using HastaneRandevuSistemi.Core.Entities.Abstract;

namespace HastaneRandevuSistemi.Core.Entities
{
    /// <summary>
    /// Hem tek bir klinik/hastaneyi hem de çok şubeli sağlık kuruluşlarını temsil eden çatı varlık.
    /// </summary>
    public class SaglikKurumu : BaseEntity
    {
        public string KurumAdi { get; set; } = string.Empty;
        public string VergiNo { get; set; } = string.Empty;
        public string MerkezAdres { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public bool CokSubeliMi { get; set; }

        public ICollection<HastaneSube> Subeler { get; set; } = new List<HastaneSube>();
    }
}
