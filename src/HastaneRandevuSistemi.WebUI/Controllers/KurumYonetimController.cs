using System.Security.Claims;
using HastaneRandevuSistemi.Business.DTOs;
using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Exceptions;
using HastaneRandevuSistemi.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.WebUI.Controllers
{
    [Authorize(Roles = "SistemYoneticisi,KurumYoneticisi")]
    public class KurumYonetimController : Controller
    {
        private readonly IKurumYonetimService _kurumService;

        public KurumYonetimController(IKurumYonetimService kurumService)
        {
            _kurumService = kurumService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int aktifKullaniciId = AktifKullaniciIdGetir();
            var aktifYonetici = await _kurumService.KullaniciGetirAsync(aktifKullaniciId);
            if (aktifYonetici == null)
            {
                return RedirectToAction("Giris", "Hesap");
            }

            var tumSubeler = await _kurumService.SubeleriDetayliGetirAsync();
            var tumDepartmanlar = await _kurumService.DepartmanlariGetirAsync();
            var tumHizmetler = await _kurumService.HizmetleriGetirAsync();
            var tumDoktorlar = await _kurumService.DoktorlariDetayliGetirAsync();
            var tumKullanicilar = await _kurumService.TumKullanicilariGetirAsync();

            bool sistemYoneticisiMi = aktifYonetici.SubeTanimlayabilirMi();
            int? sorumluSubeId = (aktifYonetici as KurumYoneticisi)?.SorumluSubeId;

            // Şube Yöneticisi yalnızca kendi sorumlu olduğu hastane şubesini görebilir ve yönetebilir
            var yetkiliSubeler = sistemYoneticisiMi
                ? tumSubeler
                : tumSubeler.Where(s => s.Id == sorumluSubeId).ToList();

            var yetkiliSubeIdSet = yetkiliSubeler.Select(s => s.Id).ToHashSet();

            var yetkiliDepartmanlar = tumDepartmanlar
                .Where(d => yetkiliSubeIdSet.Contains(d.HastaneSubeId))
                .ToList();

            var yetkiliDepartmanIdSet = yetkiliDepartmanlar.Select(d => d.Id).ToHashSet();

            var yetkiliHizmetler = tumHizmetler
                .Where(h => yetkiliDepartmanIdSet.Contains(h.DepartmanId))
                .ToList();

            var yetkiliDoktorlar = tumDoktorlar
                .Where(d => yetkiliSubeIdSet.Contains(d.HastaneSubeId))
                .ToList();

            var tumPersoneller = await _kurumService.SaglikPersonelleriniGetirAsync();
            var yetkiliPersoneller = tumPersoneller
                .Where(p => yetkiliSubeIdSet.Contains(p.HastaneSubeId))
                .ToList();

            string kapsamBasligi = sistemYoneticisiMi
                ? "Genel Merkez Yönetimi (Tüm Hastaneler ve Şubeler)"
                : $"{yetkiliSubeler.FirstOrDefault()?.SubeAdi ?? "Şube"} Yönetim Paneli";

            var viewModel = new KurumYonetimSayfaViewModel
            {
                AktifYonetici = aktifYonetici,
                SistemYoneticisiMi = sistemYoneticisiMi,
                SorumluSubeId = sorumluSubeId,
                KapsamBasligi = kapsamBasligi,
                Kurumlar = await _kurumService.KurumlariGetirAsync(),
                Subeler = yetkiliSubeler,
                Departmanlar = yetkiliDepartmanlar,
                Hizmetler = yetkiliHizmetler,
                Doktorlar = yetkiliDoktorlar,
                SaglikPersonelleri = yetkiliPersoneller,
                SubeYoneticileri = tumKullanicilar.OfType<KurumYoneticisi>().ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubeEkle(SubeOlusturDto model)
        {
            try
            {
                model.IslemYapanKullaniciId = AktifKullaniciIdGetir();
                var sube = await _kurumService.SubeOlusturAsync(model);
                TempData["BasariMesaji"] = $"'{sube.SubeAdi}' hastane şubesi ve çalışma saatleri başarıyla tanımlandı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubeYoneticisiEkle(SubeYoneticisiKayitDto model)
        {
            try
            {
                model.IslemYapanKullaniciId = AktifKullaniciIdGetir();
                var yonetici = await _kurumService.SubeYoneticisiKaydetAsync(model);
                TempData["BasariMesaji"] = $"{yonetici.TamAd} ({yonetici.YetkiUnvani}) olarak başarıyla atandı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DepartmanEkle(DepartmanOlusturDto model)
        {
            try
            {
                model.IslemYapanKullaniciId = AktifKullaniciIdGetir();
                var departman = await _kurumService.DepartmanOlusturAsync(model);
                TempData["BasariMesaji"] = $"'{departman.DepartmanAdi}' bölümü başarıyla tanımlandı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizmetEkle(SaglikHizmetiOlusturDto model)
        {
            try
            {
                model.IslemYapanKullaniciId = AktifKullaniciIdGetir();
                var hizmet = await _kurumService.SaglikHizmetiOlusturAsync(model);
                TempData["BasariMesaji"] = $"'{hizmet.HizmetAdi}' hizmeti ({hizmet.SureDakika} dk / {hizmet.TemelUcret:N2} ₺) başarıyla tanımlandı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoktorEkle(DoktorKayitDto model)
        {
            try
            {
                model.IslemYapanKullaniciId = AktifKullaniciIdGetir();
                var doktor = await _kurumService.DoktorKaydetAsync(model);
                TempData["BasariMesaji"] = $"{doktor.Unvan} {doktor.TamAd} ({doktor.UzmanlikAlani}) sisteme başarıyla kaydedildi.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoktoraHizmetEkle(DoktorHizmetEslestirDto model)
        {
            try
            {
                model.IslemYapanKullaniciId = AktifKullaniciIdGetir();
                await _kurumService.DoktoraHizmetTanimlaAsync(model);
                TempData["BasariMesaji"] = "Doktora yeni sağlık hizmeti ve ek muayene/işlem fark ücreti başarıyla tanımlandı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaglikPersoneliEkle(SaglikPersoneliKayitDto model)
        {
            try
            {
                model.IslemYapanKullaniciId = AktifKullaniciIdGetir();
                var personel = await _kurumService.SaglikPersoneliKaydetAsync(model);
                TempData["BasariMesaji"] = $"{personel.Unvan} {personel.TamAd} ({personel.GorevAlani}) sisteme başarıyla kaydedildi.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UygunlukTanimla(CalisanUygunlukTanimlaDto model)
        {
            try
            {
                model.IslemYapanKullaniciId = AktifKullaniciIdGetir();
                var uygunluk = await _kurumService.DoktorUygunlukTanimlaAsync(model);
                TempData["BasariMesaji"] = $"Doktor uygunluk takvimi ({uygunluk.Gun}: {uygunluk.BaslangicSaati:hh\\:mm}-{uygunluk.BitisSaati:hh\\:mm}) güncellendi.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubeYoneticisiYetkiKaldir(int yoneticiId)
        {
            try
            {
                await _kurumService.SubeYoneticisiYetkiKaldirAsync(yoneticiId, AktifKullaniciIdGetir());
                TempData["BasariMesaji"] = "Şube yöneticisinin yetkisi başarıyla kaldırıldı ve hesabı pasife alındı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubeKaldir(int subeId)
        {
            try
            {
                await _kurumService.SubePasiflestirAsync(subeId, AktifKullaniciIdGetir());
                TempData["BasariMesaji"] = "Hastane/klinik şubesi pasife alındı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DepartmanKaldir(int departmanId)
        {
            try
            {
                await _kurumService.DepartmanPasiflestirAsync(departmanId, AktifKullaniciIdGetir());
                TempData["BasariMesaji"] = "Poliklinik/bölüm kaydı kaldırıldı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizmetKaldir(int hizmetId)
        {
            try
            {
                await _kurumService.SaglikHizmetiPasiflestirAsync(hizmetId, AktifKullaniciIdGetir());
                TempData["BasariMesaji"] = "Sağlık hizmeti kaydı kaldırıldı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoktorKaldir(int doktorId)
        {
            try
            {
                await _kurumService.DoktorPasiflestirAsync(doktorId, AktifKullaniciIdGetir());
                TempData["BasariMesaji"] = "Doktor kaydı ve poliklinik yetkisi kaldırıldı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoktorHizmetKaldir(int doktorHizmetId)
        {
            try
            {
                await _kurumService.DoktorHizmetKaldirAsync(doktorHizmetId, AktifKullaniciIdGetir());
                TempData["BasariMesaji"] = "Doktorun sağlık hizmeti eşleştirmesi kaldırıldı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaglikPersoneliKaldir(int personelId)
        {
            try
            {
                await _kurumService.SaglikPersoneliPasiflestirAsync(personelId, AktifKullaniciIdGetir());
                TempData["BasariMesaji"] = "Sağlık personeli kaydı kaldırıldı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private int AktifKullaniciIdGetir()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id);
            return id;
        }
    }
}
