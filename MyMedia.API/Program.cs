using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyMedia.Data;
using MyMedia.Data.Entities; // <--- Tens de ter este using para o ApplicationUser
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection; // <--- NOVO
using System.IO; // <--- NOVO

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var pathKeys = @"C:\MyMediaKeys";
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(pathKeys))
    .SetApplicationName("MyMediaStore");
// 1. CONFIGURAÇÃO DA BASE DE DADOS
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ---------------------------------------------------------
// 2. CONFIGURAÇÃO DO IDENTITY (O que faltava!) <--- NOVO
// ---------------------------------------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 3. CONFIGURAÇÃO DO COOKIE <--- NOVO
// Isto impede que a API tente redirecionar para uma página HTML de login quando dá erro
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "MyMediaAuthCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // TEMPO DE VIDA DO COOKIE (30 DIAS)
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true; // Renova o tempo se o utilizador voltar a entrar

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});
// ---------------------------------------------------------

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ---------------------------------------------------------
// 4. ATIVAR A AUTENTICAÇÃO <--- NOVO (Muito Importante!)
// Tem de vir ANTES do UseAuthorization
// ---------------------------------------------------------
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();