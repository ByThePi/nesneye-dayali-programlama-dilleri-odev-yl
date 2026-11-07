using HastaneRandevuSistemi.Business.DTOs;
using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Core.Exceptions;
using HastaneRandevuSistemi.WebUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.WebUI.Controllers
{
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
            var viewModel = new KurumYonetimSayfaViewModel
            {
                Kurumlar = await _kurumService.KurumlariGetirAsync(),
                Subeler = await _kurumService.SubeleriDetayliGetirAsync(),
                Departmanlar = await _kurumService.DepartmanlariGetirAsync(),
                Hizmetler = await _kurumService.HizmetleriGetirAsync(),
                Doktorlar = await _kurumService.DoktorlariDetayliGetirAsync(),
                Kullanicilar = await _kurumService.TumKullanicilariGetirAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubeEkle(SubeOlusturDto model)
        {
            try
            {
                var sube = await _kurumService.SubeOlusturAsync(model);
                TempData["BasariMesaji"] = $"'{sube.SubeAdi}' şubesi ve çalışma saatleri başarıyla tanımlandı.";
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
                await _kurumService.DoktoraHizmetTanimlaAsync(model);
                TempData["BasariMesaji"] = "Doktora yeni sağlık hizmeti ve özel ücret farkı başarıyla tanımlandı.";
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
        public async Task<IActionResult> HastaEkle(HastaKayitDto model)
        {
            try
            {
                var hasta = await _kurumService.HastaKaydetAsync(model);
                TempData["BasariMesaji"] = $"{hasta.TamAd} (TC: {hasta.TcKimlikNo}) hasta olarak başarıyla kaydedildi.";
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
                var uygunluk = await _kurumService.DoktorUygunlukTanimlaAsync(model);
                TempData["BasariMesaji"] = $"Doktor uygunluk takvimi ({uygunluk.Gun}: {uygunluk.BaslangicSaati:hh\\:mm}-{uygunluk.BitisSaati:hh\\:mm}) güncellendi.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
