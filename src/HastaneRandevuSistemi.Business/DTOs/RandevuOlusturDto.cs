namespace HastaneRandevuSistemi.Business.DTOs
{
    public class RandevuOlusturDto
    {
        public int HastaId { get; set; }
        public int DoktorId { get; set; }
        public int SaglikHizmetiId { get; set; }
        public DateTime RandevuTarihi { get; set; }
        public TimeSpan BaslangicSaati { get; set; }
        public string? HastaNotu { get; set; }
    }
}
