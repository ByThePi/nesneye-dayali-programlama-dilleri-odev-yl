using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Exceptions;

namespace HastaneRandevuSistemi.Core.Entities
{
    /// <summary>
    /// Doktorların uygunluk zaman dilimlerini tanımlar. Hastalar yalnızca bu zaman aralıklarına randevu alabilir.
    /// </summary>
    public class CalisanUygunluk : BaseEntity
    {
        public int CalisanId { get; set; }
        public Calisan? Calisan { get; set; }

        public DayOfWeek Gun { get; set; }
        public TimeSpan BaslangicSaati { get; private set; }
        public TimeSpan BitisSaati { get; private set; }
        public TimeSpan? MolaBaslangic { get; private set; }
        public TimeSpan? MolaBitis { get; private set; }

        public void ZamanDilimiAyarla(
            DayOfWeek gun,
            TimeSpan baslangic,
            TimeSpan bitis,
            TimeSpan? molaBaslangic = null,
            TimeSpan? molaBitis = null)
        {
            if (bitis <= baslangic)
            {
                throw new DomainException("Çalışan uygunluk bitiş saati, başlangıç saatinden sonra olmalıdır.");
            }

            if (molaBaslangic.HasValue && molaBitis.HasValue)
            {
                if (molaBitis.Value <= molaBaslangic.Value ||
                    molaBaslangic.Value < baslangic ||
                    molaBitis.Value > bitis)
                {
                    throw new DomainException("Mola saatleri çalışma aralığı içinde ve geçerli olmalıdır.");
                }
            }

            Gun = gun;
            BaslangicSaati = baslangic;
            BitisSaati = bitis;
            MolaBaslangic = molaBaslangic;
            MolaBitis = molaBitis;
            GuncellemeTarihiniAyarla();
        }

        public bool ZamanDilimiUygunMu(TimeSpan talepBaslangic, TimeSpan talepBitis)
        {
            if (!AktifMi || talepBitis <= talepBaslangic)
            {
                return false;
            }

            bool mesaiIcinde = talepBaslangic >= BaslangicSaati && talepBitis <= BitisSaati;
            if (!mesaiIcinde)
            {
                return false;
            }

            if (MolaBaslangic.HasValue && MolaBitis.HasValue)
            {
                bool molaIleCakisiyor = talepBaslangic < MolaBitis.Value && talepBitis > MolaBaslangic.Value;
                if (molaIleCakisiyor)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
