using HastaneRandevuSistemi.Business.DTOs;
using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Exceptions;
using HastaneRandevuSistemi.Core.Interfaces;

namespace HastaneRandevuSistemi.Business.Services
{
    /// <summary>
    /// Dependency Inversion Prensibi (DIP) kapsamında IUnitOfWork soyutlaması ile çalışan kurum ve çalışan yönetim servisi.
    /// </summary>
    public class KurumYonetimService : IKurumYonetimService
    {
        private readonly IUnitOfWork _unitOfWork;

        public KurumYonetimService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                d => d.AktifMi,
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
            await KurumTanimYetkisiDogrulaAsync(model.IslemYapanKullaniciId);

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

        public async Task<Departman> DepartmanOlusturAsync(DepartmanOlusturDto model)
        {
            await KurumTanimYetkisiDogrulaAsync(model.IslemYapanKullaniciId);

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
            await KurumTanimYetkisiDogrulaAsync(model.IslemYapanKullaniciId);

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
            await KurumTanimYetkisiDogrulaAsync(model.IslemYapanKullaniciId);

            if (string.IsNullOrWhiteSpace(model.Ad) || string.IsNullOrWhiteSpace(model.Soyad))
            {
                throw new DomainException("Doktor ad ve soyad bilgileri zorunludur.");
            }

            if (string.IsNullOrWhiteSpace(model.UzmanlikAlani))
            {
                throw new DomainException("Doktor uzmanlık alanı belirtilmelidir.");
            }

            var departman = await _unitOfWork.Repository<Departman>().GetByIdAsync(model.DepartmanId)
                ?? throw new DomainException("Seçilen bölüm/departman bulunamadı.");

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
                SifreHash = "DemoHash_YeniDoktor",
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

            // Yeni doktor için varsayılan hafta içi uygunluk takvimini de oluştur
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
            await KurumTanimYetkisiDogrulaAsync(model.IslemYapanKullaniciId);

            var doktor = await _unitOfWork.Doktorlar.DoktorDetayliGetirAsync(model.DoktorId)
                ?? throw new DomainException("Seçilen doktor bulunamadı.");

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
            await KurumTanimYetkisiDogrulaAsync(model.IslemYapanKullaniciId);

            if (string.IsNullOrWhiteSpace(model.Ad) || string.IsNullOrWhiteSpace(model.Soyad))
            {
                throw new DomainException("Sağlık personeli ad ve soyad bilgileri zorunludur.");
            }

            if (string.IsNullOrWhiteSpace(model.GorevAlani))
            {
                throw new DomainException("Sağlık personeli görev alanı belirtilmelidir.");
            }

            var departman = await _unitOfWork.Repository<Departman>().GetByIdAsync(model.DepartmanId)
                ?? throw new DomainException("Seçilen bölüm/departman bulunamadı.");

            var yeniPersonel = new SaglikPersoneli
            {
                Ad = model.Ad.Trim(),
                Soyad = model.Soyad.Trim(),
                Email = model.Email.Trim(),
                Telefon = model.Telefon.Trim(),
                SifreHash = "DemoHash_YeniPersonel",
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

            var mevcutHastalar = await _unitOfWork.Repository<Hasta>().GetAllAsync(
                h => h.TcKimlikNo == model.TcKimlikNo.Trim() && h.AktifMi);

            if (mevcutHastalar.Any())
            {
                throw new DomainException("Bu TC Kimlik Numarası ile kayıtlı bir hasta zaten bulunmaktadır.");
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
                SifreHash = "DemoHash_YeniHasta",
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

            // Polimorfik yetki kontrolü: Kurum yöneticisi veya ilgili doktorun kendisi tanımlayabilir
            if (!islemYapan.KurumTanimlayabilirMi() && islemYapan.Id != model.DoktorId)
            {
                throw new YetkisizIslemException(
                    $"{islemYapan.TamAd} ({islemYapan.Rol}), başka bir doktorun çalışma takvimini tanımlama yetkisine sahip değildir.");
            }

            var doktor = await _unitOfWork.Doktorlar.DoktorDetayliGetirAsync(model.DoktorId)
                ?? throw new DomainException("Doktor bulunamadı.");

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

        private async Task KurumTanimYetkisiDogrulaAsync(int kullaniciId)
        {
            var islemYapan = await _unitOfWork.Repository<Kullanici>().GetByIdAsync(kullaniciId)
                ?? throw new DomainException("İşlem yapan kullanıcı sistemde bulunamadı.");

            if (!islemYapan.KurumTanimlayabilirMi())
            {
                throw new YetkisizIslemException(
                    $"{islemYapan.TamAd} ({islemYapan.Rol}) hastane/klinik, hizmet veya çalışan tanımlama yetkisine sahip değildir. Yalnızca Kurum Yöneticisi bu işlemi yapabilir.");
            }
        }
    }
}
