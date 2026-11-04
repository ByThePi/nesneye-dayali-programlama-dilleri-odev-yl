using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Enums;

namespace HastaneRandevuSistemi.Core.Strategies
{
    /// <summary>
    /// Strategy Tasarım Deseni (Design Pattern): Sağlık hizmeti ücret hesaplama algoritmalarını soyutlar.
    /// Açık/Kapalı Prensibi (OCP) gereği yeni sigorta veya indirim tipleri mevcut kodu değiştirmeden eklenebilir.
    /// </summary>
    public interface IUcretHesaplamaStratejisi
    {
        decimal Hesapla(SaglikHizmeti hizmet, decimal doktorOzelFarki = 0m);
    }

    /// <summary>
    /// Standart (SGK / Özel sigortası olmayan) hastalar için ücret hesaplama stratejisi.
    /// </summary>
    public class StandartSgkUcretStratejisi : IUcretHesaplamaStratejisi
    {
        public decimal Hesapla(SaglikHizmeti hizmet, decimal doktorOzelFarki = 0m)
        {
            return Math.Round(hizmet.TemelUcret + doktorOzelFarki, 2);
        }
    }

    /// <summary>
    /// Özel sağlık sigortası bulunan hastalar için hizmet tipine göre kademeli indirim uygulayan strateji.
    /// </summary>
    public class OzelSigortaUcretStratejisi : IUcretHesaplamaStratejisi
    {
        public decimal Hesapla(SaglikHizmeti hizmet, decimal doktorOzelFarki = 0m)
        {
            decimal bazToplam = hizmet.TemelUcret + doktorOzelFarki;

            decimal indirimOrani = hizmet.HizmetTipi switch
            {
                HizmetTipi.KontrolMuayenesi => 1.00m,
                HizmetTipi.Laboratuvar => 0.40m,
                HizmetTipi.Radyoloji => 0.35m,
                _ => 0.25m
            };

            return Math.Round(bazToplam * (1 - indirimOrani), 2);
        }
    }

    /// <summary>
    /// Hastanın sigorta durumuna göre uygun ücret hesaplama stratejisini üreten seçici (Factory Method).
    /// </summary>
    public static class UcretStratejiSecici
    {
        public static IUcretHesaplamaStratejisi StratejiBelirle(bool ozelSigortaVarMi)
        {
            return ozelSigortaVarMi
                ? new OzelSigortaUcretStratejisi()
                : new StandartSgkUcretStratejisi();
        }
    }
}
