using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Configuration; // ⚠️ Necessário para a config
using MyMedia.Client.RCL.Services; // ⚠️ Onde está o CustomAuthStateProvider

namespace MyMedia.Client.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // ============================================================
            // 1. CONFIGURAÇÃO "IS MOBILE" (Para funcionar no Windows)
            // ============================================================
            // Isto diz ao Login.razor: "Sim, sou uma app mobile" (mesmo no Windows)
            var settings = new Dictionary<string, string> { { "IsMobile", "true" } };
            builder.Configuration.AddInMemoryCollection(settings);


            // ============================================================
            // 2. HTTP CLIENT (COM BYPASS DE SSL PARA ANDROID)
            // ============================================================
            builder.Services.AddScoped(sp =>
            {
                // URL Base (Ajusta a porta 7198 se for diferente no teu launchSettings.json)
                string apiUrl = "https://localhost:7198/";

                // No Android, localhost é 10.0.2.2
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    apiUrl = "https://10.0.2.2:7198/";
                }

                // Handler para ignorar erros de certificado (SÓ PARA DEV)
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (message, cert, chain, errors) => true;

                return new HttpClient(handler) { BaseAddress = new Uri(apiUrl) };
            });


            // ============================================================
            // 3. AUTENTICAÇÃO (O CORRETO)
            // ============================================================
            builder.Services.AddAuthorizationCore();

            // ⚠️ MUDANÇA IMPORTANTE: 
            // Usamos o CustomAuthStateProvider da RCL (que tem o NotifyLogin)
            // e NÃO o AuthStateProviderMobile vazio.
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

            // Registar o Serviço de Auth (para o Login funcionar)
            builder.Services.AddScoped<ClientAuthService>();


            // ============================================================
            // 4. OUTROS SERVIÇOS
            // ============================================================
            builder.Services.AddScoped<CarrinhoService>();
            builder.Services.AddScoped<ClientProdutoService>();

            return builder.Build();
        }
    }
}