using MyMedia.Client.Web.Components;
using MyMedia.Client.RCL.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication; // Necessário para SignInAsync
using Microsoft.AspNetCore.Mvc; // Necessário para [FromForm]
using System.Security.Claims; // Necessário para Claims
using System.IO;

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

// Serviços de suporte
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));
builder.Services.AddHttpClient<ClientProdutoService>(client => { client.BaseAddress = new Uri("https://localhost:7198/"); });
builder.Services.AddScoped<CarrinhoService>();

// ===========================================================================
// 2. CONFIGURAÇÃO DE AUTH (O SEGURANÇA) ?????
// ===========================================================================
// Usamos o esquema padrão "Cookies" para simplificar
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MyMediaAuth"; // Damos um nome fixo ao cookie
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<ClientAuthService>();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// ===========================================================================
// 3. PIPELINE (O Corredor de Entrada)
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

app.UseAuthentication(); // 1. Verifica o Cookie
app.UseAuthorization();  // 2. Verifica Permissões
app.UseAntiforgery();

// ===========================================================================
// 4. A ALTERNATIVA AO CONTROLLER (MINIMAL API) ??
// Aqui definimos a rota que recebe o formulário de Login e cria o Cookie
// ===========================================================================

app.MapPost("/account/login", async (HttpContext context, [FromForm] string email, [FromForm] string password, IHttpClientFactory clientFactory) =>
{
    // A. Pergunta à API se o user existe (Validação de password)
    var client = clientFactory.CreateClient("API");
    // Nota: A API deve ter um endpoint que aceita JSON para login
var response = await client.PostAsJsonAsync("api/auth/login", new { UserName = email, Password = password });
    if (!response.IsSuccessStatusCode)
    {
        // Se a API rejeitar, volta para o login com erro
        return Results.Redirect("/login?error=credenciais_invalidas");
    }

    // B. Se a API disse SIM, criamos o Crachá (Identidade)
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, email),
        new Claim(ClaimTypes.Role, "Cliente") 
        // Podes adicionar o Token JWT aqui se a API o devolver
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    // C. O PASSO MÁGICO: Colar o Cookie no Browser ??
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
    });

    // D. Redirecionar para a Home
    return Results.Redirect("/");
})
.DisableAntiforgery(); // Desliga a verificação de token para facilitar o login inicial

// Rota de Logout
app.MapGet("/account/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

// ===========================================================================

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(ClientProdutoService).Assembly);

app.Run();