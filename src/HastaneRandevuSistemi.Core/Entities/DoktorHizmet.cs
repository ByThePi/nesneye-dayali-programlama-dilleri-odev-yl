using HastaneRandevuSistemi.Core.Entities.Abstract;

namespace HastaneRandevuSistemi.Core.Entities
{
    /// <summary>
    /// Doktorların sunduğu sağlık hizmetlerini ve varsa doktor bazlı ek muayene/işlem farkını tanımlar.
    /// </summary>
    public class DoktorHizmet : BaseEntity
    {
        public int DoktorId { get; set; }
        public Doktor? Doktor { get; set; }

        public int SaglikHizmetiId { get; set; }
        public SaglikHizmeti? SaglikHizmeti { get; set; }

        public decimal OzelUcretFarki { get; set; }
    }
}
