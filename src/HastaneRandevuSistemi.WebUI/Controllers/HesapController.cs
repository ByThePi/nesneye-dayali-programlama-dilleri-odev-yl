using System.Security.Claims;
using HastaneRandevuSistemi.Business.DTOs;
using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Exceptions;
using HastaneRandevuSistemi.WebUI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.WebUI.Controllers
{
    public class HesapController : Controller
    {
        private readonly IKurumYonetimService _kurumService;

        public HesapController(IKurumYonetimService kurumService)
        {
            _kurumService = kurumService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Giris(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new GirisSayfaViewModel
            {
                ReturnUrl = returnUrl,
                DemoHesaplar = await _kurumService.TumKullanicilariGetirAsync(),
                Subeler = await _kurumService.SubeleriDetayliGetirAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giris(GirisDto model, string? returnUrl = null)
        {
            var kullanici = await _kurumService.GirisDogrulaAsync(model.Email, model.Sifre);
            if (kullanici == null)
            {
                TempData["HataMesaji"] = "E-posta adresi veya şifre hatalı. Lütfen bilgilerinizi kontrol ediniz.";
                return RedirectToAction(nameof(Giris), new { returnUrl });
            }

            await OturumAcAsync(kullanici);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult KayitOl()
        {
            return View(new HastaKayitDto());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KayitOl(HastaKayitDto model)
        {
            try
            {
                var yeniHasta = await _kurumService.HastaKaydetAsync(model);

                if (User.Identity?.IsAuthenticated == true)
                {
                    TempData["BasariMesaji"] = $"{yeniHasta.TamAd} (TC: {yeniHasta.TcKimlikNo}) hasta olarak başarıyla kaydedildi.";
                    return RedirectToAction("Index", "Randevu");
                }

                await OturumAcAsync(yeniHasta);
                TempData["BasariMesaji"] = $"Hoş geldiniz {yeniHasta.TamAd}! Hasta kaydınız oluşturuldu, randevu alabilirsiniz.";
                return RedirectToAction("Index", "Randevu");
            }
            catch (DomainException ex)
            {
                TempData["HataMesaji"] = ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cikis()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Giris));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ErisimEngellendi()
        {
            return View();
        }

        private async Task OturumAcAsync(Kullanici kullanici)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                new(ClaimTypes.Name, kullanici.TamAd),
                new(ClaimTypes.Email, kullanici.Email),
                new(ClaimTypes.Role, kullanici.Rol.ToString())
            };

            if (kullanici is KurumYoneticisi ky && ky.SorumluSubeId.HasValue)
            {
                claims.Add(new Claim("HastaneSubeId", ky.SorumluSubeId.Value.ToString()));
            }
            else if (kullanici is Calisan calisan)
            {
                claims.Add(new Claim("HastaneSubeId", calisan.HastaneSubeId.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}
