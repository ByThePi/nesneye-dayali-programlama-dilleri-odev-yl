using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Entities.Abstract;

namespace HastaneRandevuSistemi.Business.Interfaces
{
    public interface IRandevuService
    {
        Task<IReadOnlyList<Randevu>> TumRandevulariGetirAsync();
        Task<Randevu> RandevuOlusturAsync(DTOs.RandevuOlusturDto model);
        Task RandevuOnaylaAsync(int randevuId, int onaylayanKullaniciId, string? onayNotu);
        Task RandevuReddetAsync(int randevuId, int onaylayanKullaniciId, string redSebebi);
        Task RandevuIptalEtAsync(int randevuId, string iptalSebebi);
    }

    public interface IKurumYonetimService
    {
        Task<IReadOnlyList<HastaneSube>> SubeleriDetayliGetirAsync();
        Task<IReadOnlyList<Doktor>> DoktorlariDetayliGetirAsync();
        Task<IReadOnlyList<SaglikHizmeti>> HizmetleriGetirAsync();
        Task<IReadOnlyList<Hasta>> HastalariGetirAsync();
        Task<IReadOnlyList<Kullanici>> TumKullanicilariGetirAsync();
    }
}
