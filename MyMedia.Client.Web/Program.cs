using MyMedia.Client.Web.Components;
using MyMedia.Client.RCL.Services;
using MyMedia.Client.RCL.Models; // <--- GARANTE QUE TENS ISTO PARA O DTO
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IO;
using System.Text.Json; // <--- NECESSÁRIO PARA LER A RESPOSTA DA API

var builder = WebApplication.CreateBuilder(args);

// ===========================================================================
// 1. SERVIÇOS DO BLAZOR E HTTP
// ===========================================================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CookieHandler>();

// Configura o HttpClient da API
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7198/");
})
.AddHttpMessageHandler<CookieHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));
builder.Services.AddHttpClient<ClientProdutoService>(client => { client.BaseAddress = new Uri("https://localhost:7198/"); });
builder.Services.AddScoped<CarrinhoService>();

// ===========================================================================
// 2. CONFIGURAÇÃO DE AUTH
// ===========================================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MyMediaAuth";
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    });

builder.Services.AddAuthorization();

// ? COMENTEI ISTO! O CustomAuth estava a bloquear o cookie normal
// builder.Services.AddScoped<CustomAuthStateProvider>();
// builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
// builder.Services.AddScoped<ClientAuthService>();

// ? ISTO É O SUFICIENTE PARA LER O COOKIE
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// ===========================================================================
// 3. PIPELINE
// ===========================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configuração Imagens Admin
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
catch { }

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ===========================================================================
// 4. O LOGIN CORRIGIDO ?
// ===========================================================================

app.MapPost("/account/login", async (HttpContext context, [FromForm] string email, [FromForm] string password, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("API");

    // 1. Enviar pedido à API
    var response = await client.PostAsJsonAsync("api/auth/login", new { UserName = email, Password = password });

    if (!response.IsSuccessStatusCode)
    {
        return Results.Redirect("/login?error=credenciais_invalidas");
    }

    // 2. ?? LER A RESPOSTA (JSON) PARA SABER AS ROLES
    var jsonString = await response.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    // Usa o DTO que criámos (LoginResponseDto)
    var loginResult = JsonSerializer.Deserialize<LoginResponseDto>(jsonString, options);

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, email),
        new Claim(ClaimTypes.NameIdentifier, loginResult.UserId ?? "")
    };

    // 3. ?? ADICIONAR AS ROLES REAIS (FORNECEDOR OU CLIENTE)
    if (loginResult?.Roles != null)
    {
        foreach (var role in loginResult.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
    }

    // 4. Criar o Cookie
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = DateTime.UtcNow.AddDays(30)
    });

    return Results.Redirect("/");
})
.DisableAntiforgery();

app.MapGet("/account/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(ClientProdutoService).Assembly);

app.Run();
