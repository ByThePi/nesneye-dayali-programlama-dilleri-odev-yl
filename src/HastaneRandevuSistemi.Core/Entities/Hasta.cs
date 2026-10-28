using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Enums;

namespace HastaneRandevuSistemi.Core.Entities
{
    public class Hasta : Kullanici
    {
        public Hasta()
        {
            Rol = KullaniciRolu.Hasta;
        }

        public string TcKimlikNo { get; set; } = string.Empty;
        public DateTime DogumTarihi { get; set; }
        public string KanGrubu { get; set; } = string.Empty;
        public bool OzelSigortaVarMi { get; set; }

        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        public override string KullaniciOzetiGetir()
        {
            string sigortaBilgisi = OzelSigortaVarMi ? "Özel Sigortalı" : "Standart (SGK)";
            return $"[Hasta] {TamAd} | TC: {TcKimlikNo} | {sigortaBilgisi}";
        }
    }
}
