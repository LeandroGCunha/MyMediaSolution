using Microsoft.Extensions.Logging;
using MyMedia.Client.RCL.Services;
using MyMedia.Client.RCL; // Para acederes aos teus componentes
using Microsoft.AspNetCore.Components.Authorization;

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

            // ==============================================================
            // 1. CONFIGURAR A URL DA API (O TRUQUE DO ANDROID) 📱
            // ==============================================================
            string apiUrl;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // No Emulador Android, 10.0.2.2 é o endereço do teu PC
                // (Nota: Se usares HTTPS, precisas de configuração extra de certificados, 
                //  por agora vamos tentar HTTP ou aceitar certificados inválidos é complexo.
                //  O DevTunnels é melhor, mas vamos pelo básico primeiro)
                apiUrl = "https://10.0.2.2:7198/";
            }
            else
            {
                // No Windows/Mac, é localhost normal
                apiUrl = "https://localhost:7198/";
            }

            // ==============================================================
            // 2. REGISTAR SERVIÇOS (IGUAL À WEB, MAS SEM COOKIES DO SERVER)
            // ==============================================================

            // Aqui usamos um HttpClient simples (sem o CookieHandler complexo da Web por enquanto)
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

            // Serviços da RCL
            builder.Services.AddScoped<ClientProdutoService>();
            builder.Services.AddScoped<CarrinhoService>();

            // AUTH: No Mobile, o AuthStateProvider tem de ser diferente do da Web
            // (Mais tarde tratamos disto, para já põe o básico para arrancar)
            builder.Services.AddAuthorizationCore();
            // builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>(); 

            return builder.Build();
        }
    }
}