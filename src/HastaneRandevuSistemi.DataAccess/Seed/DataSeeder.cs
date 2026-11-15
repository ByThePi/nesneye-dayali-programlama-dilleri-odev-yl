using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Enums;
using HastaneRandevuSistemi.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.DataAccess.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            try
            {
                // Şema güncellendiyse (ör: SistemYoneticileri tablosu yoksa) veritabanını temiz şemayla yeniden oluştur
                _ = await context.SistemYoneticileri.AnyAsync();
            }
            catch
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
            }

            if (await context.SaglikKurumlari.AnyAsync())
            {
                return;
            }

            // 1. Çok Şubeli Sağlık Kurumu Tanımı
            var kurum = new SaglikKurumu
            {
                KurumAdi = "Marmara Medikal Sağlık Grubu",
                VergiNo = "6120948120",
                MerkezAdres = "Serdivan / Sakarya",
                Telefon = "0264 555 00 00",
                CokSubeliMi = true
            };
            context.SaglikKurumlari.Add(kurum);
            await context.SaveChangesAsync();

            // 2. Şubeler (A Hastanesi: Sakarya Merkez Hastanesi, B Hastanesi: Kadıköy İhtisas Kliniği)
            var merkezSube = new HastaneSube
            {
                SaglikKurumuId = kurum.Id,
                SubeAdi = "Sakarya Merkez Hastanesi",
                Sehir = "Sakarya",
                Ilce = "Serdivan",
                Adres = "Kemalpaşa Mah. Üniversite Cad. No:42",
                Telefon = "0264 555 01 01"
            };
            merkezSube.CalismaSaatleriniAyarla(new TimeSpan(8, 0, 0), new TimeSpan(18, 0, 0));

            var kadikoySube = new HastaneSube
            {
                SaglikKurumuId = kurum.Id,
                SubeAdi = "Kadıköy İhtisas Kliniği",
                Sehir = "İstanbul",
                Ilce = "Kadıköy",
                Adres = "Bağdat Cad. No:118",
                Telefon = "0216 555 02 02"
            };
            kadikoySube.CalismaSaatleriniAyarla(new TimeSpan(9, 0, 0), new TimeSpan(19, 0, 0));

            context.HastaneSubeleri.AddRange(merkezSube, kadikoySube);
            await context.SaveChangesAsync();

            // 3. Departmanlar (Bölümler)
            var kardiyoloji = new Departman
            {
                HastaneSubeId = merkezSube.Id,
                DepartmanAdi = "Kardiyoloji Polikliniği",
                Aciklama = "Kalp ve damar hastalıkları tanı ve tedavi birimi",
                KatBilgisi = "A Blok - 2. Kat"
            };

            var radyoloji = new Departman
            {
                HastaneSubeId = merkezSube.Id,
                DepartmanAdi = "Radyoloji ve Görüntüleme Merkezi",
                Aciklama = "MR, BT, Ultrason ve Röntgen görüntüleme birimi",
                KatBilgisi = "Zemin Kat"
            };

            var dahiliye = new Departman
            {
                HastaneSubeId = kadikoySube.Id,
                DepartmanAdi = "İç Hastalıkları (Dahiliye) ve Laboratuvar",
                Aciklama = "Genel muayene ve biyokimya kan tahlil birimi",
                KatBilgisi = "1. Kat"
            };

            context.Departmanlar.AddRange(kardiyoloji, radyoloji, dahiliye);
            await context.SaveChangesAsync();

            // 4. Sağlık Hizmetleri (Süre ve Ücret Tanımlamaları)
            var ekgMuayene = new SaglikHizmeti
            {
                DepartmanId = kardiyoloji.Id,
                HizmetAdi = "Kardiyoloji Uzman Muayenesi + EKG",
                Aciklama = "Detaylı kalp muayenesi ve elektrokardiyografi çekimi",
                HizmetTipi = HizmetTipi.UzmanMuayene
            };
            ekgMuayene.HizmetBilgileriniAyarla(30, 1500m);

            var ekoHizmeti = new SaglikHizmeti
            {
                DepartmanId = kardiyoloji.Id,
                HizmetAdi = "Ekokardiyografi (EKO) İncelemesi",
                Aciklama = "Kalp ultrasonografisi ve kapakçık değerlendirmesi",
                HizmetTipi = HizmetTipi.Radyoloji
            };
            ekoHizmeti.HizmetBilgileriniAyarla(45, 2400m);

            var mrGoruntuleme = new SaglikHizmeti
            {
                DepartmanId = radyoloji.Id,
                HizmetAdi = "Kraniyal (Beyin) MR Görüntüleme",
                Aciklama = "Yüksek çözünürlüklü manyetik rezonans görüntüleme",
                HizmetTipi = HizmetTipi.Radyoloji
            };
            mrGoruntuleme.HizmetBilgileriniAyarla(40, 3200m);

            var genelCheckup = new SaglikHizmeti
            {
                DepartmanId = dahiliye.Id,
                HizmetAdi = "Genel Dahiliye Muayenesi ve Kan Paneli",
                Aciklama = "Uzman dahiliye muayenesi ve hemogram/biyokimya laboratuvar paketi",
                HizmetTipi = HizmetTipi.Laboratuvar
            };
            genelCheckup.HizmetBilgileriniAyarla(30, 1200m);

            context.SaglikHizmetleri.AddRange(ekgMuayene, ekoHizmeti, mrGoruntuleme, genelCheckup);
            await context.SaveChangesAsync();

            // 5. Kullanıcı Rolleri (Sistem Yöneticisi, Şube Yöneticileri, Doktorlar, Sağlık Personeli, Hastalar)
            var sistemAdmin = new SistemYoneticisi
            {
                Ad = "Hakan",
                Soyad = "Soylu",
                Email = "admin@marmaramedikal.com",
                Telefon = "0532 000 00 01",
                SifreHash = "123456",
                Unvan = "Genel Merkez Sistem Yöneticisi (Ana Admin)"
            };

            var merkezYonetici = new KurumYoneticisi
            {
                Ad = "Selim",
                Soyad = "Karahan",
                Email = "selim.karahan@marmaramedikal.com",
                Telefon = "0532 100 10 10",
                SifreHash = "123456",
                SorumluSubeId = merkezSube.Id,
                YetkiUnvani = "Sakarya Merkez Hastanesi Yöneticisi"
            };

            var kadikoyYonetici = new KurumYoneticisi
            {
                Ad = "Canan",
                Soyad = "Erdem",
                Email = "canan.erdem@marmaramedikal.com",
                Telefon = "0532 100 20 20",
                SifreHash = "123456",
                SorumluSubeId = kadikoySube.Id,
                YetkiUnvani = "Kadıköy İhtisas Kliniği Yöneticisi"
            };

            var drAhmet = new Doktor
            {
                Ad = "Ahmet",
                Soyad = "Yılmaz",
                Email = "ahmet.yilmaz@marmaramedikal.com",
                Telefon = "0532 200 20 20",
                SifreHash = "123456",
                HastaneSubeId = merkezSube.Id,
                DepartmanId = kardiyoloji.Id,
                SicilNo = "DR-5401",
                Unvan = "Prof. Dr.",
                UzmanlikAlani = "Kardiyoloji ve Girişimsel Kardiyoloji",
                DiplomaNo = "DIP-1998-442"
            };

            var drZeynep = new Doktor
            {
                Ad = "Zeynep",
                Soyad = "Kaya",
                Email = "zeynep.kaya@marmaramedikal.com",
                Telefon = "0532 300 30 30",
                SifreHash = "123456",
                HastaneSubeId = merkezSube.Id,
                DepartmanId = radyoloji.Id,
                SicilNo = "DR-5402",
                Unvan = "Uzm. Dr.",
                UzmanlikAlani = "Radyoloji ve Nöroradyoloji",
                DiplomaNo = "DIP-2012-881"
            };

            var drMert = new Doktor
            {
                Ad = "Mert",
                Soyad = "Demir",
                Email = "mert.demir@marmaramedikal.com",
                Telefon = "0532 400 40 40",
                SifreHash = "123456",
                HastaneSubeId = kadikoySube.Id,
                DepartmanId = dahiliye.Id,
                SicilNo = "DR-3401",
                Unvan = "Doç. Dr.",
                UzmanlikAlani = "İç Hastalıkları (Dahiliye)",
                DiplomaNo = "DIP-2008-319"
            };

            var personelElif = new SaglikPersoneli
            {
                Ad = "Elif",
                Soyad = "Aydın",
                Email = "elif.aydin@marmaramedikal.com",
                Telefon = "0532 500 50 50",
                SifreHash = "123456",
                HastaneSubeId = merkezSube.Id,
                DepartmanId = kardiyoloji.Id,
                SicilNo = "SP-104",
                Unvan = "Başhemşire",
                GorevAlani = "Kardiyoloji EKG & Hasta Kabul Koordinasyonu",
                VardiyaBilgisi = "08:00 - 17:00 (Gündüz)"
            };

            var hastaAli = new Hasta
            {
                Ad = "Ali",
                Soyad = "Öztürk",
                Email = "ali.ozturk@ornekmail.com",
                Telefon = "0544 111 22 33",
                SifreHash = "123456",
                TcKimlikNo = "12345678902",
                DogumTarihi = new DateTime(1988, 5, 14),
                KanGrubu = "A Rh+",
                OzelSigortaVarMi = true
            };

            var hastaAyse = new Hasta
            {
                Ad = "Ayşe",
                Soyad = "Çelik",
                Email = "ayse.celik@ornekmail.com",
                Telefon = "0544 444 55 66",
                SifreHash = "123456",
                TcKimlikNo = "98765432104",
                DogumTarihi = new DateTime(1995, 11, 23),
                KanGrubu = "0 Rh+",
                OzelSigortaVarMi = false
            };

            context.SistemYoneticileri.Add(sistemAdmin);
            context.KurumYoneticileri.AddRange(merkezYonetici, kadikoyYonetici);
            context.Doktorlar.AddRange(drAhmet, drZeynep, drMert);
            context.SaglikPersonelleri.Add(personelElif);
            context.Hastalar.AddRange(hastaAli, hastaAyse);
            await context.SaveChangesAsync();

            // 6. Doktor - Hizmet Eşleştirmeleri
            context.DoktorHizmetleri.AddRange(
                new DoktorHizmet { DoktorId = drAhmet.Id, SaglikHizmetiId = ekgMuayene.Id, OzelUcretFarki = 300m },
                new DoktorHizmet { DoktorId = drAhmet.Id, SaglikHizmetiId = ekoHizmeti.Id, OzelUcretFarki = 400m },
                new DoktorHizmet { DoktorId = drZeynep.Id, SaglikHizmetiId = mrGoruntuleme.Id, OzelUcretFarki = 0m },
                new DoktorHizmet { DoktorId = drMert.Id, SaglikHizmetiId = genelCheckup.Id, OzelUcretFarki = 200m }
            );

            // 7. Doktor Uygunluk Zaman Dilimleri (Pazartesi - Cuma)
            var isGunleri = new[]
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday
            };

            foreach (var gun in isGunleri)
            {
                var uAhmet = new CalisanUygunluk { CalisanId = drAhmet.Id };
                uAhmet.ZamanDilimiAyarla(gun, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), new TimeSpan(12, 30, 0), new TimeSpan(13, 30, 0));

                var uZeynep = new CalisanUygunluk { CalisanId = drZeynep.Id };
                uZeynep.ZamanDilimiAyarla(gun, new TimeSpan(8, 30, 0), new TimeSpan(16, 30, 0), new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0));

                var uMert = new CalisanUygunluk { CalisanId = drMert.Id };
                uMert.ZamanDilimiAyarla(gun, new TimeSpan(9, 30, 0), new TimeSpan(18, 0, 0), new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0));

                context.CalisanUygunluklari.AddRange(uAhmet, uZeynep, uMert);
            }
            await context.SaveChangesAsync();

            // 8. Örnek Randevular
            DateTime gelecekIsGunu = SonrakiIsGununuGetir(DateTime.Today.AddDays(1));

            var randevu1 = new Randevu
            {
                HastaId = hastaAli.Id,
                DoktorId = drAhmet.Id,
                SaglikHizmetiId = ekgMuayene.Id,
                HastaneSubeId = merkezSube.Id,
                HastaNotu = "Son 2 haftadır eforla gelen çarpıntı şikayeti."
            };
            decimal ucret1 = ekgMuayene.NetUcretHesapla(hastaAli.OzelSigortaVarMi, 300m);
            randevu1.RandevuPlanla(gelecekIsGunu, new TimeSpan(10, 0, 0), ekgMuayene.SureDakika, ucret1);
            randevu1.Onayla(drAhmet, "Poliklinik randevusu uygun görüldü, EKG çekimi için 15 dk erken geliniz.");

            var randevu2 = new Randevu
            {
                HastaId = hastaAyse.Id,
                DoktorId = drMert.Id,
                SaglikHizmetiId = genelCheckup.Id,
                HastaneSubeId = kadikoySube.Id,
                HastaNotu = "Yıllık rutin kan tahlili ve genel kontrol."
            };
            decimal ucret2 = genelCheckup.NetUcretHesapla(hastaAyse.OzelSigortaVarMi, 200m);
            randevu2.RandevuPlanla(gelecekIsGunu, new TimeSpan(11, 0, 0), genelCheckup.SureDakika, ucret2);

            context.Randevular.AddRange(randevu1, randevu2);
            await context.SaveChangesAsync();
        }

        private static DateTime SonrakiIsGununuGetir(DateTime baslangic)
        {
            while (baslangic.DayOfWeek == DayOfWeek.Saturday || baslangic.DayOfWeek == DayOfWeek.Sunday)
            {
                baslangic = baslangic.AddDays(1);
            }

            return baslangic.Date;
        }
    }
}
