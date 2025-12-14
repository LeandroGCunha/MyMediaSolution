using MyMedia.Client.Web.Components;
using MyMedia.Client.RCL.Services;
using Microsoft.Extensions.FileProviders;
using System.IO; // <--- Necessário para usar Path e Directory

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<ClientProdutoService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7198/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// 1. Carrega os ficheiros estáticos normais da Loja (CSS, JS)
app.UseStaticFiles();

// -----------------------------------------------------------------------------
// 2. CONFIGURAÇÃO EXTRA: Permitir ler as imagens da pasta do Admin
// -----------------------------------------------------------------------------
try 
{
    // Procura a pasta do projeto vizinho (MyMedia.Admin/wwwroot)
    var caminhoAdmin = Path.Combine(Directory.GetCurrentDirectory(), "..", "MyMedia.Admin", "wwwroot");

    if (Directory.Exists(caminhoAdmin))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(caminhoAdmin),
            RequestPath = "" // Permite que o link /imagens/15.jpg funcione
        });
    }
}
catch 
{
    // Se der erro (ex: pasta não existe), o site continua a funcionar sem imagens
}
// -----------------------------------------------------------------------------

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(ClientProdutoService).Assembly);

app.Run();