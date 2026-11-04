using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Entities.Abstract;
using HastaneRandevuSistemi.Core.Interfaces;

namespace HastaneRandevuSistemi.Business.Services
{
    /// <summary>
    /// Dependency Inversion Prensibi (DIP) kapsamında IUnitOfWork soyutlaması ile çalışan kurum yönetim servisi.
    /// </summary>
    public class KurumYonetimService : IKurumYonetimService
    {
        private readonly IUnitOfWork _unitOfWork;

        public KurumYonetimService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<HastaneSube>> SubeleriDetayliGetirAsync()
        {
            return await _unitOfWork.HastaneSubeleri.SubeleriDetayliGetirAsync();
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
    }
}
