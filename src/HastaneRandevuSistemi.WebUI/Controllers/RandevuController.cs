using HastaneRandevuSistemi.Business.DTOs;
using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Core.Enums;
using HastaneRandevuSistemi.Core.Exceptions;
using HastaneRandevuSistemi.WebUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.WebUI.Controllers
{
    public class RandevuController : Controller
    {
        private readonly IRandevuService _randevuService;
        private readonly IKurumYonetimService _kurumService;

        public RandevuController(IRandevuService randevuService, IKurumYonetimService kurumService)
        {
            _randevuService = randevuService;
            _kurumService = kurumService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? subeId, int? doktorId, RandevuDurumu? durum)
        {
            var tumKullanicilar = await _kurumService.TumKullanicilariGetirAsync();
            var tumRandevular = await _randevuService.TumRandevulariGetirAsync();

            var filtrelenmisRandevular = tumRandevular.AsEnumerable();

            if (subeId.HasValue && subeId.Value > 0)
            {
                filtrelenmisRandevular = filtrelenmisRandevular.Where(r => r.HastaneSubeId == subeId.Value);
            }

            if (doktorId.HasValue && doktorId.Value > 0)
            {
                filtrelenmisRandevular = filtrelenmisRandevular.Where(r => r.DoktorId == doktorId.Value);
            }

            if (durum.HasValue)
            {
                filtrelenmisRandevular = filtrelenmisRandevular.Where(r => r.Durum == durum.Value);
            }

            var viewModel = new RandevuSayfaViewModel
            {
                Randevular = filtrelenmisRandevular.ToList(),
                Subeler = await _kurumService.SubeleriDetayliGetirAsync(),
                Hastalar = await _kurumService.HastalariGetirAsync(),
                Doktorlar = await _kurumService.DoktorlariDetayliGetirAsync(),
                Hizmetler = await _kurumService.HizmetleriGetirAsync(),
                OnayYetkilileri = tumKullanicilar.Where(k => k.RandevuOnaylayabilirMi()).ToList(),
                SeciliSubeId = subeId,
                SeciliDoktorId = doktorId,
                SeciliDurum = durum
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur(RandevuOlusturDto model)
        {
            try
            {
                await _randevuService.RandevuOlusturAsync(model);
                TempData["BasariMesaji"] = "Randevu başarıyla oluşturuldu ve onay sürecine alındı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int randevuId, int onaylayanKullaniciId, string? onayNotu)
        {
            try
            {
                await _randevuService.RandevuOnaylaAsync(randevuId, onaylayanKullaniciId, onayNotu);
                TempData["BasariMesaji"] = $"Randevu (#{randevuId}) başarıyla onaylandı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reddet(int randevuId, int onaylayanKullaniciId, string redSebebi)
        {
            try
            {
                await _randevuService.RandevuReddetAsync(randevuId, onaylayanKullaniciId, redSebebi);
                TempData["BasariMesaji"] = $"Randevu (#{randevuId}) reddedildi.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tamamla(int randevuId, int onaylayanKullaniciId, string? muayeneSonucNotu)
        {
            try
            {
                await _randevuService.RandevuTamamlaAsync(randevuId, onaylayanKullaniciId, muayeneSonucNotu);
                TempData["BasariMesaji"] = $"Randevu (#{randevuId}) muayenesi başarıyla tamamlandı olarak işaretlendi.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IptalEt(int randevuId, string iptalSebebi)
        {
            try
            {
                await _randevuService.RandevuIptalEtAsync(randevuId, iptalSebebi);
                TempData["BasariMesaji"] = $"Randevu (#{randevuId}) iptal edildi.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
