using HastaneRandevuSistemi.Business.Interfaces;
using HastaneRandevuSistemi.Business.Services;
using HastaneRandevuSistemi.Core.Interfaces;
using HastaneRandevuSistemi.DataAccess.Context;
using HastaneRandevuSistemi.DataAccess.Repositories;
using HastaneRandevuSistemi.DataAccess.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=HastaneRandevuSistemi.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IRandevuRepository, RandevuRepository>();
builder.Services.AddScoped<IDoktorRepository, DoktorRepository>();
builder.Services.AddScoped<IHastaneSubeRepository, HastaneSubeRepository>();
builder.Services.AddScoped<ISaglikHizmetiRepository, SaglikHizmetiRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRandevuService, RandevuService>();
builder.Services.AddScoped<IKurumYonetimService, KurumYonetimService>();

var app = builder.Build();

// Veritabanını otomatik oluştur ve başlangıç verilerini (Seed) yükle
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
