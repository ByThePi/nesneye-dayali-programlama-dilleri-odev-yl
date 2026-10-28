using HastaneRandevuSistemi.Core.Entities;
using HastaneRandevuSistemi.Core.Entities.Abstract;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.DataAccess.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<SaglikKurumu> SaglikKurumlari => Set<SaglikKurumu>();
        public DbSet<HastaneSube> HastaneSubeleri => Set<HastaneSube>();
        public DbSet<Departman> Departmanlar => Set<Departman>();
        public DbSet<SaglikHizmeti> SaglikHizmetleri => Set<SaglikHizmeti>();
        public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
        public DbSet<Calisan> Calisanlar => Set<Calisan>();
        public DbSet<Hasta> Hastalar => Set<Hasta>();
        public DbSet<Doktor> Doktorlar => Set<Doktor>();
        public DbSet<SaglikPersoneli> SaglikPersonelleri => Set<SaglikPersoneli>();
        public DbSet<KurumYoneticisi> KurumYoneticileri => Set<KurumYoneticisi>();
        public DbSet<DoktorHizmet> DoktorHizmetleri => Set<DoktorHizmet>();
        public DbSet<CalisanUygunluk> CalisanUygunluklari => Set<CalisanUygunluk>();
        public DbSet<Randevu> Randevular => Set<Randevu>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // OOP Kalıtım Hiyerarşisi (TPT - Table Per Type ile her alt sınıfın kendi tablosu olur)
            modelBuilder.Entity<Kullanici>().ToTable("Kullanicilar");
            modelBuilder.Entity<Calisan>().ToTable("Calisanlar");
            modelBuilder.Entity<Hasta>().ToTable("Hastalar");
            modelBuilder.Entity<Doktor>().ToTable("Doktorlar");
            modelBuilder.Entity<SaglikPersoneli>().ToTable("SaglikPersonelleri");
            modelBuilder.Entity<KurumYoneticisi>().ToTable("KurumYoneticileri");

            modelBuilder.Entity<SaglikKurumu>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.KurumAdi).IsRequired().HasMaxLength(200);
                entity.HasMany(x => x.Subeler)
                      .WithOne(s => s.SaglikKurumu)
                      .HasForeignKey(s => s.SaglikKurumuId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<HastaneSube>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.SubeAdi).IsRequired().HasMaxLength(200);
                entity.HasMany(x => x.Departmanlar)
                      .WithOne(d => d.HastaneSube)
                      .HasForeignKey(d => d.HastaneSubeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Departman>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.DepartmanAdi).IsRequired().HasMaxLength(150);
                entity.HasMany(x => x.Hizmetler)
                      .WithOne(h => h.Departman)
                      .HasForeignKey(h => h.DepartmanId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SaglikHizmeti>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.HizmetAdi).IsRequired().HasMaxLength(200);
                entity.Property(x => x.TemelUcret).HasPrecision(18, 2);
            });

            modelBuilder.Entity<DoktorHizmet>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.OzelUcretFarki).HasPrecision(18, 2);
                entity.HasOne(x => x.Doktor)
                      .WithMany(d => d.DoktorHizmetleri)
                      .HasForeignKey(x => x.DoktorId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.SaglikHizmeti)
                      .WithMany(h => h.DoktorHizmetleri)
                      .HasForeignKey(x => x.SaglikHizmetiId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CalisanUygunluk>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.Calisan)
                      .WithMany(c => c.UygunlukTakvimi)
                      .HasForeignKey(x => x.CalisanId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Randevu>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ToplamUcret).HasPrecision(18, 2);
                entity.HasOne(x => x.Hasta)
                      .WithMany(h => h.Randevular)
                      .HasForeignKey(x => x.HastaId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Doktor)
                      .WithMany(d => d.Randevular)
                      .HasForeignKey(x => x.DoktorId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.SaglikHizmeti)
                      .WithMany(s => s.Randevular)
                      .HasForeignKey(x => x.SaglikHizmetiId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.HastaneSube)
                      .WithMany(s => s.Randevular)
                      .HasForeignKey(x => x.HastaneSubeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
