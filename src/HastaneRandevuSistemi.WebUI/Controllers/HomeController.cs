using System.Diagnostics;
using System.Security.Claims;
using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.WebUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IKurumYonetimService _kurumService;
        private readonly IRandevuService _randevuService;

        public HomeController(IKurumYonetimService kurumService, IRandevuService randevuService)
        {
            _kurumService = kurumService;
            _randevuService = randevuService;
        }

        public async Task<IActionResult> Index(int? subeId, int? departmanId, int? doktorId, string? arama)
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int kullaniciId);
            var aktifKullanici = await _kurumService.KullaniciGetirAsync(kullaniciId);
            var tumSubeler = await _kurumService.SubeleriDetayliGetirAsync();
            var tumDepartmanlar = await _kurumService.DepartmanlariGetirAsync();
            var tumDoktorlar = await _kurumService.DoktorlariDetayliGetirAsync();
            var tumHizmetler = await _kurumService.HizmetleriGetirAsync();

            string? sorumluSubeAdi = null;
            if (aktifKullanici is KurumYoneticisi ky && ky.SorumluSubeId.HasValue)
            {
                sorumluSubeAdi = tumSubeler.FirstOrDefault(s => s.Id == ky.SorumluSubeId.Value)?.SubeAdi;
            }

            var filtrelenmisSubeler = tumSubeler.AsEnumerable();
            var filtrelenmisDoktorlar = tumDoktorlar.AsEnumerable();

            if (subeId.HasValue && subeId.Value > 0)
            {
                filtrelenmisSubeler = filtrelenmisSubeler.Where(s => s.Id == subeId.Value);
                filtrelenmisDoktorlar = filtrelenmisDoktorlar.Where(d => d.HastaneSubeId == subeId.Value);
            }

            if (departmanId.HasValue && departmanId.Value > 0)
            {
                filtrelenmisSubeler = filtrelenmisSubeler.Where(s => s.Departmanlar.Any(dep => dep.Id == departmanId.Value));
                filtrelenmisDoktorlar = filtrelenmisDoktorlar.Where(d => d.DepartmanId == departmanId.Value);
            }

            if (doktorId.HasValue && doktorId.Value > 0)
            {
                var seciliDr = tumDoktorlar.FirstOrDefault(d => d.Id == doktorId.Value);
                filtrelenmisDoktorlar = filtrelenmisDoktorlar.Where(d => d.Id == doktorId.Value);
                if (seciliDr != null && !subeId.HasValue)
                {
                    filtrelenmisSubeler = filtrelenmisSubeler.Where(s => s.Id == seciliDr.HastaneSubeId);
                }
            }

            if (!string.IsNullOrWhiteSpace(arama))
            {
                string q = arama.Trim().ToLowerInvariant();
                filtrelenmisDoktorlar = filtrelenmisDoktorlar.Where(d =>
                    d.TamAd.ToLowerInvariant().Contains(q) ||
                    d.UzmanlikAlani.ToLowerInvariant().Contains(q) ||
                    (d.Departman?.DepartmanAdi.ToLowerInvariant().Contains(q) ?? false) ||
                    (d.HastaneSube?.SubeAdi.ToLowerInvariant().Contains(q) ?? false) ||
                    d.DoktorHizmetleri.Any(dh => dh.SaglikHizmeti?.HizmetAdi.ToLowerInvariant().Contains(q) == true));

                filtrelenmisSubeler = filtrelenmisSubeler.Where(s =>
                    s.SubeAdi.ToLowerInvariant().Contains(q) ||
                    s.Sehir.ToLowerInvariant().Contains(q) ||
                    s.Departmanlar.Any(dep =>
                        dep.DepartmanAdi.ToLowerInvariant().Contains(q) ||
                        dep.Hizmetler.Any(h => h.HizmetAdi.ToLowerInvariant().Contains(q))) ||
                    filtrelenmisDoktorlar.Any(d => d.HastaneSubeId == s.Id));
            }

            var viewModel = new DashboardViewModel
            {
                Subeler = filtrelenmisSubeler.ToList(),
                Doktorlar = filtrelenmisDoktorlar.ToList(),
                Hizmetler = tumHizmetler,
                Randevular = await _randevuService.TumRandevulariGetirAsync(),
                AktifKullanici = aktifKullanici,
                SorumluSubeAdi = sorumluSubeAdi,
                TumSubeler = tumSubeler,
                TumDepartmanlar = tumDepartmanlar,
                TumDoktorlar = tumDoktorlar,
                SeciliSubeId = subeId,
                SeciliDepartmanId = departmanId,
                SeciliDoktorId = doktorId,
                AramaMetni = arama
            };

            return View(viewModel);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
