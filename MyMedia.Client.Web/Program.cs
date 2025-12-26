using MyMedia.Client.Web.Components;
using MyMedia.Client.RCL.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies; // <--- OBRIGATÓRIO PARA O F5
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ===========================================================================
// 1. O SEGREDO DO F5 (Configuração dos Cookies e HTTP)
// ===========================================================================

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CookieHandler>();

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7198/");
})
.AddHttpMessageHandler<CookieHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

builder.Services.AddHttpClient<ClientProdutoService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7198/");
});

builder.Services.AddScoped<CarrinhoService>();

// ===========================================================================
// 2. SEGURANÇA (AUTH) - AQUI ESTAVA A FALTAR O ESSENCIAL ???
// ===========================================================================

// --- ?? BLOCO NOVO OBRIGATÓRIO ?? ---
// Isto ensina o servidor a ler o Cookie quando dás F5
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.LoginPath = "/login"; // Se não tiver logado, manda para aqui
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });
// ------------------------------------

builder.Services.AddAuthorization(); // (Mudei de Core para Authorization completo, é mais seguro aqui)

builder.Services.AddScoped<CustomAuthStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddScoped<ClientAuthService>();
builder.Services.AddCascadingAuthenticationState();


var app = builder.Build();

// ===========================================================================
// 3. PIPELINE (Ordem dos pedidos)
// ===========================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configuração para ler imagens do Admin
try
{
    var caminhoAdmin = Path.Combine(Directory.GetCurrentDirectory(), "..", "MyMedia.Admin", "wwwroot");
    if (Directory.Exists(caminhoAdmin))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(caminhoAdmin),
            RequestPath = ""
        });
    }
}
catch { /* Ignora */ }

// --- ?? BLOCO NOVO OBRIGATÓRIO ?? ---
// Sem isto, o servidor ignora o login no F5
app.UseAuthentication();
app.UseAuthorization();
// ------------------------------------

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(ClientProdutoService).Assembly);

app.Run();