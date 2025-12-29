using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyMedia.Data;
using MyMedia.Data.Entities;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);


var pathChaves = Path.Combine(Directory.GetCurrentDirectory(), "..", "ChavesPartilhadas");

// Garante que a pasta existe
if (!Directory.Exists(pathChaves))
{
    Directory.CreateDirectory(pathChaves);
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(pathChaves))
    .SetApplicationName("MyMediaApp"); // O Nome tem de ser igual na API

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. CONFIGURAÇÃO DO IDENTITY
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 3. AJUSTE DO COMPORTAMENTO DA API (Para não redirecionar para Login HTML)
// Mantemos isto apenas para garantir que, se houver erro de acesso, a API devolve erro 401 e não tenta abrir uma página web.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401; // Diz "Não Autorizado" em vez de redirecionar
        return Task.CompletedTask;
    };
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();

app.Run();