# Hastane / Klinik Randevu Sistemi (`nesneye-dayali-programlama-dilleri-odev-yl`)

Sakarya Üniversitesi Yüksek Lisans **Nesneye Dayalı Programlama Dilleri** dersi kapsamında **.NET 10.0 LTS**, **ASP.NET Core MVC** ve **Entity Framework Core (SQLite)** kullanılarak geliştirilmiş çok katmanlı (N-Tier / Clean Architecture) Hastane ve Klinik Randevu Sistemi.

## 1. Mimari Yapı ve Katmanlar

Çözüm (`HastaneRandevuSistemi.sln`) 4 ana katmandan oluşmaktadır:

1. **`HastaneRandevuSistemi.Core`** *(Domain Katmanı)*
   - Soyut sınıflar (`BaseEntity`, `Kullanici`, `Calisan`), somut varlıklar (`SaglikKurumu`, `HastaneSube`, `Departman`, `SaglikHizmeti`, `Doktor`, `SaglikPersoneli`, `KurumYoneticisi`, `Hasta`, `CalisanUygunluk`, `Randevu`), `IRepository<T>` / `IUnitOfWork` arayüzleri ve özel iş kuralı istisnaları (`DomainException`, `RandevuCakismaException`, `YetkisizIslemException`).
2. **`HastaneRandevuSistemi.DataAccess`** *(Veri Erişim Katmanı)*
   - Entity Framework Core `AppDbContext` (TPT - Table Per Type kalıtım eşlemesi), Generic Repository & Unit of Work implementasyonları ve örnek başlangıç verilerini otomatik yükleyen `DataSeeder`.
3. **`HastaneRandevuSistemi.Business`** *(İş Mantığı Katmanı)*
   - Şube çalışma saatleri doğrulaması, doktor uygunluk zaman dilimi kontrolü, randevu çakışma kontrolü, polimorfik ücret hesaplama ve randevu onay/red mekanizmalarını yürüten servisler (`RandevuService`, `KurumYonetimService`).
4. **`HastaneRandevuSistemi.WebUI`** *(Sunum Katmanı - ASP.NET Core MVC)*
   - Kullanıcı rolleri, şube/bölüm/hizmet tanımları, doktor uygunluk takvimi ve randevu oluşturma/onay süreçlerini sunan MVC arayüzü.

## 2. Uygulanan OOP İlkeleri

- **Soyutlama (Abstraction):** `BaseEntity`, `Kullanici` ve `Calisan` soyut (`abstract`) sınıfları ile ortak özellik ve davranış sözleşmeleri tanımlanmıştır.
- **Kalıtım (Inheritance):** `Hasta`, `KurumYoneticisi` ve `Calisan` (`Doktor`, `SaglikPersoneli`) sınıfları `Kullanici` sınıfından türetilmiştir.
- **Çok Biçimlilik (Polymorphism):** `KullaniciOzetiGetir()`, `RandevuOnaylayabilirMi()`, `KurumTanimlayabilirMi()` ve `SaglikHizmeti.NetUcretHesapla()` metotları çalışma zamanında nesnenin gerçek tipine göre farklı davranış sergiler.
- **Kapsülleme (Encapsulation):** `Randevu`, `HastaneSube`, `SaglikHizmeti` ve `CalisanUygunluk` sınıflarındaki kritik özellikler `private set` / `protected set` ile korunmuş; durum değişiklikleri iş kurallarını denetleyen davranış metotları (`RandevuPlanla`, `Onayla`, `ZamanDilimiAyarla` vb.) üzerinden gerçekleştirilmiştir.

## 3. Kurulum ve Çalıştırma (Visual Studio / .NET CLI)

1. `HastaneRandevuSistemi.sln` dosyasını **Visual Studio** ile açın.
2. Başlangıç projesi (Startup Project) olarak **`HastaneRandevuSistemi.WebUI`** projesini seçin ve **F5** (veya `dotnet run --project src/HastaneRandevuSistemi.WebUI`) ile çalıştırın.
3. İlk çalıştırmada SQLite veritabanı (`HastaneRandevuSistemi.db`) ve örnek hastane/klinik, doktor, uygunluk takvimi ve randevu verileri otomatik olarak oluşturulur.
