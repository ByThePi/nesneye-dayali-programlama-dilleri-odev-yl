namespace HastaneRandevuSistemi.Core.Entities.Abstract
{
    /// <summary>
    /// Tüm domain varlıkları için ortak soyut temel sınıf (Abstraction & Encapsulation).
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; protected set; }
        public DateTime OlusturulmaTarihi { get; protected set; } = DateTime.UtcNow;
        public DateTime? GuncellenmeTarihi { get; protected set; }
        public bool AktifMi { get; protected set; } = true;

        public virtual void Aktiflestir()
        {
            AktifMi = true;
            GuncellemeTarihiniAyarla();
        }

        public virtual void Pasiflestir()
        {
            AktifMi = false;
            GuncellemeTarihiniAyarla();
        }

        public void GuncellemeTarihiniAyarla()
        {
            GuncellenmeTarihi = DateTime.UtcNow;
        }
    }
}
