using System.Security.Claims;
using HastaneRandevuSistemi.Business.DTOs;
using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Enums;
using HastaneRandevuSistemi.Core.Exceptions;
using HastaneRandevuSistemi.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.WebUI.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> Index(int? subeId, int? doktorId, RandevuDurumu? durum, string siralama = "kronolojik")
        {
            int aktifKullaniciId = AktifKullaniciIdGetir();
            var aktifKullanici = await _kurumService.KullaniciGetirAsync(aktifKullaniciId);
            var tumRandevular = await _randevuService.TumRandevulariGetirAsync();

            var filtrelenmisRandevular = tumRandevular.AsEnumerable();

            // Rol bazlı randevu görünürlük kapsamı
            if (aktifKullanici is Hasta)
            {
                filtrelenmisRandevular = filtrelenmisRandevular.Where(r => r.HastaId == aktifKullanici.Id);
            }
            else if (aktifKullanici is Doktor dr)
            {
                filtrelenmisRandevular = filtrelenmisRandevular.Where(r => r.DoktorId == dr.Id);
            }
            else if (aktifKullanici is SaglikPersoneli personel)
            {
                filtrelenmisRandevular = filtrelenmisRandevular.Where(r => r.HastaneSubeId == personel.HastaneSubeId);
            }
            else if (aktifKullanici is KurumYoneticisi yonetici && yonetici.SorumluSubeId.HasValue)
            {
                filtrelenmisRandevular = filtrelenmisRandevular.Where(r => r.HastaneSubeId == yonetici.SorumluSubeId.Value);
            }

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

            // Kronolojik tarih ve saat sıralaması
            filtrelenmisRandevular = siralama == "azalan"
                ? filtrelenmisRandevular.OrderByDescending(r => r.RandevuTarihi).ThenByDescending(r => r.BaslangicSaati)
                : filtrelenmisRandevular.OrderBy(r => r.RandevuTarihi).ThenBy(r => r.BaslangicSaati);

            var hastalar = await _kurumService.HastalariGetirAsync();
            if (aktifKullanici is Hasta hastaKullanici)
            {
                hastalar = hastalar.Where(h => h.Id == hastaKullanici.Id).ToList();
            }

            var viewModel = new RandevuSayfaViewModel
            {
                Randevular = filtrelenmisRandevular.ToList(),
                Subeler = await _kurumService.SubeleriDetayliGetirAsync(),
                Hastalar = hastalar,
                Doktorlar = await _kurumService.DoktorlariDetayliGetirAsync(),
                Hizmetler = await _kurumService.HizmetleriGetirAsync(),
                AktifKullanici = aktifKullanici,
                SeciliSubeId = subeId,
                SeciliDoktorId = doktorId,
                SeciliDurum = durum,
                Siralama = siralama
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur(RandevuOlusturDto model)
        {
            try
            {
                int aktifId = AktifKullaniciIdGetir();
                var aktifKullanici = await _kurumService.KullaniciGetirAsync(aktifId);
                if (aktifKullanici is Hasta)
                {
                    model.HastaId = aktifKullanici.Id;
                }

                await _randevuService.RandevuOlusturAsync(model);
                TempData["BasariMesaji"] = "Randevu talebiniz başarıyla oluşturuldu ve onay sürecine alındı.";
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int randevuId, string? onayNotu)
        {
            try
            {
                await _randevuService.RandevuOnaylaAsync(randevuId, AktifKullaniciIdGetir(), onayNotu);
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
        public async Task<IActionResult> Reddet(int randevuId, string redSebebi)
        {
            try
            {
                await _randevuService.RandevuReddetAsync(randevuId, AktifKullaniciIdGetir(), redSebebi);
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
        public async Task<IActionResult> Tamamla(int randevuId, string? muayeneSonucNotu)
        {
            try
            {
                await _randevuService.RandevuTamamlaAsync(randevuId, AktifKullaniciIdGetir(), muayeneSonucNotu);
                TempData["BasariMesaji"] = $"Randevu (#{randevuId}) muayenesi tamamlandı olarak işaretlendi.";
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

        private int AktifKullaniciIdGetir()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id);
            return id;
        }
    }
}
