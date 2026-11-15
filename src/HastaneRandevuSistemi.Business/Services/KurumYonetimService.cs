using HastaneRandevuSistemi.Business.DTOs;
using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Exceptions;
using HastaneRandevuSistemi.Core.Interfaces;

namespace HastaneRandevuSistemi.Business.Services
{
    public class KurumYonetimService : IKurumYonetimService
    {
        private readonly IUnitOfWork _unitOfWork;

        public KurumYonetimService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Kullanici?> GirisDogrulaAsync(string email, string sifre)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(sifre))
            {
                return null;
            }

            string arananEmail = email.Trim().ToLowerInvariant();
            var kullanicilar = await _unitOfWork.Repository<Kullanici>().GetAllAsync(k => k.AktifMi);

            return kullanicilar.FirstOrDefault(k =>
                string.Equals(k.Email.Trim(), arananEmail, StringComparison.OrdinalIgnoreCase) &&
                k.SifreHash == sifre.Trim());
        }

        public async Task<Kullanici?> KullaniciGetirAsync(int kullaniciId)
        {
            return await _unitOfWork.Repository<Kullanici>().GetByIdAsync(kullaniciId);
        }

        public async Task<IReadOnlyList<SaglikKurumu>> KurumlariGetirAsync()
        {
            return await _unitOfWork.Repository<SaglikKurumu>().GetAllAsync(k => k.AktifMi);
        }

        public async Task<IReadOnlyList<HastaneSube>> SubeleriDetayliGetirAsync()
        {
            return await _unitOfWork.HastaneSubeleri.SubeleriDetayliGetirAsync();
        }

        public async Task<IReadOnlyList<Departman>> DepartmanlariGetirAsync()
        {
            return await _unitOfWork.Repository<Departman>().GetAllAsync(
                d => d.AktifMi && d.HastaneSube!.AktifMi,
                d => d.HastaneSube!);
        }

        public async Task<IReadOnlyList<Doktor>> DoktorlariDetayliGetirAsync()
        {
            return await _unitOfWork.Doktorlar.DoktorlariDetayliGetirAsync();
        }

        public async Task<IReadOnlyList<SaglikHizmeti>> HizmetleriGetirAsync()
        {
            return await _unitOfWork.SaglikHizmetleri.HizmetleriDetayliGetirAsync();
        }

        public async Task<IReadOnlyList<SaglikPersoneli>> SaglikPersonelleriniGetirAsync()
        {
            return await _unitOfWork.Repository<SaglikPersoneli>().GetAllAsync(
                p => p.AktifMi && p.HastaneSube!.AktifMi && p.Departman!.AktifMi,
                p => p.HastaneSube!,
                p => p.Departman!);
        }

        public async Task<IReadOnlyList<Hasta>> HastalariGetirAsync()
        {
            return await _unitOfWork.Repository<Hasta>().GetAllAsync(h => h.AktifMi);
        }

        public async Task<IReadOnlyList<Kullanici>> TumKullanicilariGetirAsync()
        {
            return await _unitOfWork.Repository<Kullanici>().GetAllAsync(k => k.AktifMi);
        }

        public async Task<HastaneSube> SubeOlusturAsync(SubeOlusturDto model)
        {
            var islemYapan = await _unitOfWork.Repository<Kullanici>().GetByIdAsync(model.IslemYapanKullaniciId)
                ?? throw new DomainException("Oturum açmış kullanıcı bulunamadı.");

            if (!islemYapan.SubeTanimlayabilirMi())
            {
                throw new YetkisizIslemException(
                    "Sisteme yeni hastane veya klinik şubesi tanımlama yetkisi yalnızca Genel Merkez Sistem Yöneticisine (Ana Admin) aittir.");
            }

            if (string.IsNullOrWhiteSpace(model.SubeAdi))
            {
                throw new DomainException("Şube/Klinik adı boş bırakılamaz.");
            }

            var yeniSube = new HastaneSube
            {
                SaglikKurumuId = model.SaglikKurumuId,
                SubeAdi = model.SubeAdi.Trim(),
                Sehir = model.Sehir.Trim(),
                Ilce = model.Ilce.Trim(),
                Adres = model.Adres.Trim(),
                Telefon = model.Telefon.Trim()
            };

            yeniSube.CalismaSaatleriniAyarla(model.AcilisSaati, model.KapanisSaati);

            await _unitOfWork.HastaneSubeleri.AddAsync(yeniSube);
            await _unitOfWork.SaveChangesAsync();

            return yeniSube;
        }

        public async Task<KurumYoneticisi> SubeYoneticisiKaydetAsync(SubeYoneticisiKayitDto model)
        {
            var islemYapan = await _unitOfWork.Repository<Kullanici>().GetByIdAsync(model.IslemYapanKullaniciId)
                ?? throw new DomainException("Oturum açmış kullanıcı bulunamadı.");

            if (!islemYapan.SubeTanimlayabilirMi())
            {
                throw new YetkisizIslemException(
                    "Hastane şubelerine yönetici atama yetkisi yalnızca Genel Merkez Sistem Yöneticisine (Ana Admin) aittir.");
            }

            var sube = await _unitOfWork.HastaneSubeleri.GetByIdAsync(model.HastaneSubeId)
                ?? throw new DomainException("Seçilen hastane şubesi bulunamadı.");

            if (string.IsNullOrWhiteSpace(model.Ad) || string.IsNullOrWhiteSpace(model.Soyad) || string.IsNullOrWhiteSpace(model.Email))
            {
                throw new DomainException("Yönetici ad, soyad ve e-posta bilgileri zorunludur.");
            }

            var yeniYonetici = new KurumYoneticisi
            {
                Ad = model.Ad.Trim(),
                Soyad = model.Soyad.Trim(),
                Email = model.Email.Trim(),
                Telefon = model.Telefon.Trim(),
                SifreHash = string.IsNullOrWhiteSpace(model.Sifre) ? "123456" : model.Sifre.Trim(),
                SorumluSubeId = sube.Id,
                YetkiUnvani = $"{sube.SubeAdi} Yöneticisi"
            };

            await _unitOfWork.Repository<KurumYoneticisi>().AddAsync(yeniYonetici);
            await _unitOfWork.SaveChangesAsync();

            return yeniYonetici;
        }

        public async Task<Departman> DepartmanOlusturAsync(DepartmanOlusturDto model)
        {
            await SubeYonetimYetkisiDogrulaAsync(model.IslemYapanKullaniciId, model.HastaneSubeId);

            if (string.IsNullOrWhiteSpace(model.DepartmanAdi))
            {
                throw new DomainException("Bölüm/Poliklinik adı boş bırakılamaz.");
            }

            var yeniDepartman = new Departman
            {
                HastaneSubeId = model.HastaneSubeId,
                DepartmanAdi = model.DepartmanAdi.Trim(),
                KatBilgisi = model.KatBilgisi.Trim(),
                Aciklama = model.Aciklama.Trim()
            };

            await _unitOfWork.Repository<Departman>().AddAsync(yeniDepartman);
            await _unitOfWork.SaveChangesAsync();

            return yeniDepartman;
        }

        public async Task<SaglikHizmeti> SaglikHizmetiOlusturAsync(SaglikHizmetiOlusturDto model)
        {
            var departman = await _unitOfWork.Repository<Departman>().GetByIdAsync(model.DepartmanId)
                ?? throw new DomainException("Seçilen bölüm/departman bulunamadı.");

            await SubeYonetimYetkisiDogrulaAsync(model.IslemYapanKullaniciId, departman.HastaneSubeId);

            if (string.IsNullOrWhiteSpace(model.HizmetAdi))
            {
                throw new DomainException("Sağlık hizmeti adı boş bırakılamaz.");
            }

            var yeniHizmet = new SaglikHizmeti
            {
                DepartmanId = model.DepartmanId,
                HizmetAdi = model.HizmetAdi.Trim(),
                Aciklama = model.Aciklama.Trim(),
                HizmetTipi = model.HizmetTipi
            };

            yeniHizmet.HizmetBilgileriniAyarla(model.SureDakika, model.TemelUcret);

            await _unitOfWork.SaglikHizmetleri.AddAsync(yeniHizmet);
            await _unitOfWork.SaveChangesAsync();

            return yeniHizmet;
        }

        public async Task<Doktor> DoktorKaydetAsync(DoktorKayitDto model)
        {
            var departman = await _unitOfWork.Repository<Departman>().GetByIdAsync(model.DepartmanId)
                ?? throw new DomainException("Seçilen bölüm/departman bulunamadı.");

            await SubeYonetimYetkisiDogrulaAsync(model.IslemYapanKullaniciId, departman.HastaneSubeId);

            if (string.IsNullOrWhiteSpace(model.Ad) || string.IsNullOrWhiteSpace(model.Soyad))
            {
                throw new DomainException("Doktor ad ve soyad bilgileri zorunludur.");
            }

            if (string.IsNullOrWhiteSpace(model.UzmanlikAlani))
            {
                throw new DomainException("Doktor uzmanlık alanı belirtilmelidir.");
            }

            if (model.SaglikHizmetiId > 0)
            {
                var secilenHizmet = await _unitOfWork.SaglikHizmetleri.GetByIdAsync(model.SaglikHizmetiId)
                    ?? throw new DomainException("Seçilen sağlık hizmeti bulunamadı.");

                if (secilenHizmet.DepartmanId != departman.Id)
                {
                    throw new DomainException("Seçilen sağlık hizmeti, doktorun görev yapacağı bölüme ait olmalıdır.");
                }
            }

            var yeniDoktor = new Doktor
            {
                Ad = model.Ad.Trim(),
                Soyad = model.Soyad.Trim(),
                Email = model.Email.Trim(),
                Telefon = model.Telefon.Trim(),
                SifreHash = "123456",
                Unvan = model.Unvan.Trim(),
                UzmanlikAlani = model.UzmanlikAlani.Trim(),
                SicilNo = model.SicilNo.Trim(),
                DiplomaNo = model.DiplomaNo.Trim(),
                HastaneSubeId = departman.HastaneSubeId,
                DepartmanId = departman.Id
            };

            await _unitOfWork.Doktorlar.AddAsync(yeniDoktor);
            await _unitOfWork.SaveChangesAsync();

            if (model.SaglikHizmetiId > 0)
            {
                var doktorHizmet = new DoktorHizmet
                {
                    DoktorId = yeniDoktor.Id,
                    SaglikHizmetiId = model.SaglikHizmetiId,
                    OzelUcretFarki = Math.Max(0, model.OzelUcretFarki)
                };

                await _unitOfWork.Repository<DoktorHizmet>().AddAsync(doktorHizmet);
            }

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
                var uygunluk = new CalisanUygunluk { CalisanId = yeniDoktor.Id };
                uygunluk.ZamanDilimiAyarla(gun, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), new TimeSpan(12, 30, 0), new TimeSpan(13, 30, 0));
                await _unitOfWork.Repository<CalisanUygunluk>().AddAsync(uygunluk);
            }

            await _unitOfWork.SaveChangesAsync();
            return yeniDoktor;
        }

        public async Task<DoktorHizmet> DoktoraHizmetTanimlaAsync(DoktorHizmetEslestirDto model)
        {
            var doktor = await _unitOfWork.Doktorlar.DoktorDetayliGetirAsync(model.DoktorId)
                ?? throw new DomainException("Seçilen doktor bulunamadı.");

            await SubeYonetimYetkisiDogrulaAsync(model.IslemYapanKullaniciId, doktor.HastaneSubeId);

            var hizmet = await _unitOfWork.SaglikHizmetleri.GetByIdAsync(model.SaglikHizmetiId)
                ?? throw new DomainException("Seçilen sağlık hizmeti bulunamadı.");

            if (hizmet.DepartmanId != doktor.DepartmanId)
            {
                throw new DomainException(
                    $"'{hizmet.HizmetAdi}' hizmeti, {doktor.Unvan} {doktor.TamAd}'ın görev yaptığı bölüme ait değildir.");
            }

            var mevcutEslesme = doktor.DoktorHizmetleri.FirstOrDefault(dh => dh.SaglikHizmetiId == hizmet.Id && dh.AktifMi);
            if (mevcutEslesme != null)
            {
                mevcutEslesme.OzelUcretFarki = Math.Max(0, model.OzelUcretFarki);
                _unitOfWork.Repository<DoktorHizmet>().Update(mevcutEslesme);
                await _unitOfWork.SaveChangesAsync();
                return mevcutEslesme;
            }

            var yeniEslesme = new DoktorHizmet
            {
                DoktorId = doktor.Id,
                SaglikHizmetiId = hizmet.Id,
                OzelUcretFarki = Math.Max(0, model.OzelUcretFarki)
            };

            await _unitOfWork.Repository<DoktorHizmet>().AddAsync(yeniEslesme);
            await _unitOfWork.SaveChangesAsync();

            return yeniEslesme;
        }

        public async Task<SaglikPersoneli> SaglikPersoneliKaydetAsync(SaglikPersoneliKayitDto model)
        {
            var departman = await _unitOfWork.Repository<Departman>().GetByIdAsync(model.DepartmanId)
                ?? throw new DomainException("Seçilen bölüm/departman bulunamadı.");

            await SubeYonetimYetkisiDogrulaAsync(model.IslemYapanKullaniciId, departman.HastaneSubeId);

            if (string.IsNullOrWhiteSpace(model.Ad) || string.IsNullOrWhiteSpace(model.Soyad))
            {
                throw new DomainException("Sağlık personeli ad ve soyad bilgileri zorunludur.");
            }

            if (string.IsNullOrWhiteSpace(model.GorevAlani))
            {
                throw new DomainException("Sağlık personeli görev alanı belirtilmelidir.");
            }

            var yeniPersonel = new SaglikPersoneli
            {
                Ad = model.Ad.Trim(),
                Soyad = model.Soyad.Trim(),
                Email = model.Email.Trim(),
                Telefon = model.Telefon.Trim(),
                SifreHash = "123456",
                Unvan = model.Unvan.Trim(),
                GorevAlani = model.GorevAlani.Trim(),
                VardiyaBilgisi = model.VardiyaBilgisi.Trim(),
                SicilNo = model.SicilNo.Trim(),
                HastaneSubeId = departman.HastaneSubeId,
                DepartmanId = departman.Id
            };

            await _unitOfWork.Repository<SaglikPersoneli>().AddAsync(yeniPersonel);
            await _unitOfWork.SaveChangesAsync();

            return yeniPersonel;
        }

        public async Task<Hasta> HastaKaydetAsync(HastaKayitDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Ad) || string.IsNullOrWhiteSpace(model.Soyad))
            {
                throw new DomainException("Hasta ad ve soyad bilgileri zorunludur.");
            }

            if (string.IsNullOrWhiteSpace(model.TcKimlikNo) || model.TcKimlikNo.Trim().Length != 11)
            {
                throw new DomainException("TC Kimlik Numarası 11 haneli olmalıdır.");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                throw new DomainException("E-posta adresi zorunludur.");
            }

            var mevcutHastalar = await _unitOfWork.Repository<Hasta>().GetAllAsync(
                h => (h.TcKimlikNo == model.TcKimlikNo.Trim() || h.Email.ToLower() == model.Email.Trim().ToLower()) && h.AktifMi);

            if (mevcutHastalar.Any())
            {
                throw new DomainException("Bu TC Kimlik Numarası veya E-Posta ile kayıtlı bir hasta zaten bulunmaktadır.");
            }

            var yeniHasta = new Hasta
            {
                Ad = model.Ad.Trim(),
                Soyad = model.Soyad.Trim(),
                TcKimlikNo = model.TcKimlikNo.Trim(),
                DogumTarihi = model.DogumTarihi.Date,
                KanGrubu = model.KanGrubu.Trim(),
                Email = model.Email.Trim(),
                Telefon = model.Telefon.Trim(),
                SifreHash = string.IsNullOrWhiteSpace(model.Sifre) ? "123456" : model.Sifre.Trim(),
                OzelSigortaVarMi = model.OzelSigortaVarMi
            };

            await _unitOfWork.Repository<Hasta>().AddAsync(yeniHasta);
            await _unitOfWork.SaveChangesAsync();

            return yeniHasta;
        }

        public async Task<CalisanUygunluk> DoktorUygunlukTanimlaAsync(CalisanUygunlukTanimlaDto model)
        {
            var islemYapan = await _unitOfWork.Repository<Kullanici>().GetByIdAsync(model.IslemYapanKullaniciId)
                ?? throw new DomainException("İşlem yapan kullanıcı bulunamadı.");

            var doktor = await _unitOfWork.Doktorlar.DoktorDetayliGetirAsync(model.DoktorId)
                ?? throw new DomainException("Doktor bulunamadı.");

            bool kendiTakvimiMi = islemYapan.Id == doktor.Id;
            bool subeYoneticisiMi = islemYapan.SubeYonetimYetkisiVarMi(doktor.HastaneSubeId);

            if (!kendiTakvimiMi && !subeYoneticisiMi)
            {
                throw new YetkisizIslemException(
                    "Bu doktorun çalışma takvimini yalnızca kendisi veya bağlı olduğu hastanenin yöneticisi güncelleyebilir.");
            }

            var mevcutGunKaydi = doktor.UygunlukTakvimi.FirstOrDefault(u => u.Gun == model.Gun && u.AktifMi);
            if (mevcutGunKaydi != null)
            {
                mevcutGunKaydi.ZamanDilimiAyarla(
                    model.Gun,
                    model.BaslangicSaati,
                    model.BitisSaati,
                    model.MolaBaslangic,
                    model.MolaBitis);
                _unitOfWork.Repository<CalisanUygunluk>().Update(mevcutGunKaydi);
                await _unitOfWork.SaveChangesAsync();
                return mevcutGunKaydi;
            }

            var yeniUygunluk = new CalisanUygunluk
            {
                CalisanId = doktor.Id
            };

            yeniUygunluk.ZamanDilimiAyarla(
                model.Gun,
                model.BaslangicSaati,
                model.BitisSaati,
                model.MolaBaslangic,
                model.MolaBitis);

            await _unitOfWork.Repository<CalisanUygunluk>().AddAsync(yeniUygunluk);
            await _unitOfWork.SaveChangesAsync();

            return yeniUygunluk;
        }

        public async Task SubeYoneticisiYetkiKaldirAsync(int yoneticiId, int islemYapanKullaniciId)
        {
            var islemYapan = await _unitOfWork.Repository<Kullanici>().GetByIdAsync(islemYapanKullaniciId)
                ?? throw new DomainException("Oturum açmış kullanıcı bulunamadı.");

            if (!islemYapan.SubeTanimlayabilirMi())
            {
                throw new YetkisizIslemException(
                    "Şube yöneticisi yetkisini kaldırma işlemi yalnızca Genel Merkez Sistem Yöneticisine (Ana Admin) aittir.");
            }

            var yonetici = await _unitOfWork.Repository<KurumYoneticisi>().GetByIdAsync(yoneticiId)
                ?? throw new DomainException("Yetkisi kaldırılacak şube yöneticisi bulunamadı.");

            yonetici.SorumluSubeId = null;
            _unitOfWork.Repository<KurumYoneticisi>().Remove(yonetici);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SubePasiflestirAsync(int subeId, int islemYapanKullaniciId)
        {
            var islemYapan = await _unitOfWork.Repository<Kullanici>().GetByIdAsync(islemYapanKullaniciId)
                ?? throw new DomainException("Oturum açmış kullanıcı bulunamadı.");

            if (!islemYapan.SubeTanimlayabilirMi())
            {
                throw new YetkisizIslemException(
                    "Hastane veya klinik şubesini pasife alma yetkisi yalnızca Genel Merkez Sistem Yöneticisine (Ana Admin) aittir.");
            }

            var sube = await _unitOfWork.HastaneSubeleri.GetByIdAsync(subeId)
                ?? throw new DomainException("Kaldırılacak hastane şubesi bulunamadı.");

            _unitOfWork.HastaneSubeleri.Remove(sube);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DepartmanPasiflestirAsync(int departmanId, int islemYapanKullaniciId)
        {
            var departman = await _unitOfWork.Repository<Departman>().GetByIdAsync(departmanId)
                ?? throw new DomainException("Kaldırılacak poliklinik/bölüm bulunamadı.");

            await SubeYonetimYetkisiDogrulaAsync(islemYapanKullaniciId, departman.HastaneSubeId);

            _unitOfWork.Repository<Departman>().Remove(departman);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SaglikHizmetiPasiflestirAsync(int hizmetId, int islemYapanKullaniciId)
        {
            var hizmet = await _unitOfWork.SaglikHizmetleri.GetByIdAsync(hizmetId)
                ?? throw new DomainException("Kaldırılacak sağlık hizmeti bulunamadı.");

            var departman = await _unitOfWork.Repository<Departman>().GetByIdAsync(hizmet.DepartmanId)
                ?? throw new DomainException("Hizmetin bağlı olduğu poliklinik bulunamadı.");

            await SubeYonetimYetkisiDogrulaAsync(islemYapanKullaniciId, departman.HastaneSubeId);

            _unitOfWork.SaglikHizmetleri.Remove(hizmet);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DoktorPasiflestirAsync(int doktorId, int islemYapanKullaniciId)
        {
            var doktor = await _unitOfWork.Doktorlar.GetByIdAsync(doktorId)
                ?? throw new DomainException("Kaldırılacak doktor kaydı bulunamadı.");

            await SubeYonetimYetkisiDogrulaAsync(islemYapanKullaniciId, doktor.HastaneSubeId);

            _unitOfWork.Doktorlar.Remove(doktor);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DoktorHizmetKaldirAsync(int doktorHizmetId, int islemYapanKullaniciId)
        {
            var eslesme = await _unitOfWork.Repository<DoktorHizmet>().GetByIdAsync(doktorHizmetId)
                ?? throw new DomainException("Kaldırılacak doktor-hizmet eşleştirmesi bulunamadı.");

            var doktor = await _unitOfWork.Doktorlar.GetByIdAsync(eslesme.DoktorId)
                ?? throw new DomainException("İlgili doktor bulunamadı.");

            await SubeYonetimYetkisiDogrulaAsync(islemYapanKullaniciId, doktor.HastaneSubeId);

            _unitOfWork.Repository<DoktorHizmet>().Remove(eslesme);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SaglikPersoneliPasiflestirAsync(int personelId, int islemYapanKullaniciId)
        {
            var personel = await _unitOfWork.Repository<SaglikPersoneli>().GetByIdAsync(personelId)
                ?? throw new DomainException("Kaldırılacak sağlık personeli bulunamadı.");

            await SubeYonetimYetkisiDogrulaAsync(islemYapanKullaniciId, personel.HastaneSubeId);

            _unitOfWork.Repository<SaglikPersoneli>().Remove(personel);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task SubeYonetimYetkisiDogrulaAsync(int kullaniciId, int hastaneSubeId)
        {
            var islemYapan = await _unitOfWork.Repository<Kullanici>().GetByIdAsync(kullaniciId)
                ?? throw new DomainException("Oturum açmış kullanıcı sistemde bulunamadı.");

            if (!islemYapan.KurumTanimlayabilirMi())
            {
                throw new YetkisizIslemException("Bu işlemi gerçekleştirmek için yönetici yetkisine sahip olmanız gerekmektedir.");
            }

            if (!islemYapan.SubeYonetimYetkisiVarMi(hastaneSubeId))
            {
                throw new YetkisizIslemException(
                    "Yalnızca kendi sorumlu olduğunuz hastane şubesi üzerinde bölüm, hizmet veya çalışan tanımlaması yapabilirsiniz.");
            }
        }
    }
}
