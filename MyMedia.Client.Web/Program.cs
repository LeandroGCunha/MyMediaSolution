using MyMedia.Client.Web.Components;
using MyMedia.Client.RCL.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Components.Authorization;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ===========================================================================
// 1. O SEGREDO DO F5 (Configuração dos Cookies) ??
// ===========================================================================

// A) Permite ler os cookies do Browser
builder.Services.AddHttpContextAccessor();

// B) Regista o nosso "Carteiro" (Certifica-te que criaste o ficheiro CookieHandler.cs!)
builder.Services.AddTransient<CookieHandler>();

// C) Configura o HttpClient "API" para usar o Carteiro
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7198/");
})
.AddHttpMessageHandler<CookieHandler>(); // <--- Aqui é que a magia acontece!

// D) Diz ao Blazor: "Quando pedirem um HttpClient normal, usa este que tem o CookieHandler"
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

// ===========================================================================


// 2. SERVIÇO DE PRODUTOS
// (Podes deixar assim. Se quiseres produtos privados no futuro, adicionas .AddHttpMessageHandler<CookieHandler>() aqui também)
builder.Services.AddHttpClient<ClientProdutoService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7198/");
});

builder.Services.AddScoped<CarrinhoService>();

// 3. SEGURANÇA (AUTH)
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddCascadingAuthenticationState();


var app = builder.Build();

// Configure the HTTP request pipeline.
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
catch
{
    // Ignora erros
}

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(ClientProdutoService).Assembly);

app.Run();