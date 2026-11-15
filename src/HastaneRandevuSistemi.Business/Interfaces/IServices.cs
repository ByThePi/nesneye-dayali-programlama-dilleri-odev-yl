using HastaneRandevuSistemi.Business.DTOs;
using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Entities.Abstract;

namespace HastaneRandevuSistemi.Business.Interfaces
{
    public interface IRandevuService
    {
        Task<IReadOnlyList<Randevu>> TumRandevulariGetirAsync();
        Task<Randevu> RandevuOlusturAsync(RandevuOlusturDto model);
        Task RandevuOnaylaAsync(int randevuId, int onaylayanKullaniciId, string? onayNotu);
        Task RandevuReddetAsync(int randevuId, int onaylayanKullaniciId, string redSebebi);
        Task RandevuTamamlaAsync(int randevuId, int onaylayanKullaniciId, string? muayeneSonucNotu);
        Task RandevuIptalEtAsync(int randevuId, string iptalSebebi);
    }

    public interface IKurumYonetimService
    {
        Task<Kullanici?> GirisDogrulaAsync(string email, string sifre);
        Task<Kullanici?> KullaniciGetirAsync(int kullaniciId);
        Task<IReadOnlyList<SaglikKurumu>> KurumlariGetirAsync();
        Task<IReadOnlyList<HastaneSube>> SubeleriDetayliGetirAsync();
        Task<IReadOnlyList<Departman>> DepartmanlariGetirAsync();
        Task<IReadOnlyList<Doktor>> DoktorlariDetayliGetirAsync();
        Task<IReadOnlyList<SaglikHizmeti>> HizmetleriGetirAsync();
        Task<IReadOnlyList<SaglikPersoneli>> SaglikPersonelleriniGetirAsync();
        Task<IReadOnlyList<Hasta>> HastalariGetirAsync();
        Task<IReadOnlyList<Kullanici>> TumKullanicilariGetirAsync();

        Task<HastaneSube> SubeOlusturAsync(SubeOlusturDto model);
        Task<KurumYoneticisi> SubeYoneticisiKaydetAsync(SubeYoneticisiKayitDto model);
        Task<Departman> DepartmanOlusturAsync(DepartmanOlusturDto model);
        Task<SaglikHizmeti> SaglikHizmetiOlusturAsync(SaglikHizmetiOlusturDto model);
        Task<Doktor> DoktorKaydetAsync(DoktorKayitDto model);
        Task<DoktorHizmet> DoktoraHizmetTanimlaAsync(DoktorHizmetEslestirDto model);
        Task<SaglikPersoneli> SaglikPersoneliKaydetAsync(SaglikPersoneliKayitDto model);
        Task<Hasta> HastaKaydetAsync(HastaKayitDto model);
        Task<CalisanUygunluk> DoktorUygunlukTanimlaAsync(CalisanUygunlukTanimlaDto model);

        Task SubeYoneticisiYetkiKaldirAsync(int yoneticiId, int islemYapanKullaniciId);
        Task SubePasiflestirAsync(int subeId, int islemYapanKullaniciId);
        Task DepartmanPasiflestirAsync(int departmanId, int islemYapanKullaniciId);
        Task SaglikHizmetiPasiflestirAsync(int hizmetId, int islemYapanKullaniciId);
        Task DoktorPasiflestirAsync(int doktorId, int islemYapanKullaniciId);
        Task DoktorHizmetKaldirAsync(int doktorHizmetId, int islemYapanKullaniciId);
        Task SaglikPersoneliPasiflestirAsync(int personelId, int islemYapanKullaniciId);
    }
}
