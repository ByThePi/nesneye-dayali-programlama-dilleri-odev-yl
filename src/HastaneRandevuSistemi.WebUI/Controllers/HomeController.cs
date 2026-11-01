using System.Diagnostics;
using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.WebUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IKurumYonetimService _kurumService;
        private readonly IRandevuService _randevuService;

        public HomeController(IKurumYonetimService kurumService, IRandevuService randevuService)
        {
            _kurumService = kurumService;
            _randevuService = randevuService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                Subeler = await _kurumService.SubeleriDetayliGetirAsync(),
                Doktorlar = await _kurumService.DoktorlariDetayliGetirAsync(),
                Hizmetler = await _kurumService.HizmetleriGetirAsync(),
                Kullanicilar = await _kurumService.TumKullanicilariGetirAsync(),
                Randevular = await _randevuService.TumRandevulariGetirAsync()
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
