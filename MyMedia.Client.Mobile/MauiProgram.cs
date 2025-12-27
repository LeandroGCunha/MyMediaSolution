using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
// 👇 ADICIONA ESTE USING PARA ENCONTRAR OS SERVIÇOS 👇
using MyMedia.Client.RCL.Services;

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

            // --- 1. API ---
            string apiUrl = "https://localhost:7198/";
            if (DeviceInfo.Platform == DevicePlatform.Android) apiUrl = "http://10.0.2.2:5000/";

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

            // --- 2. LOGIN (Já tinhas isto) ---
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProviderMobile>();

            // --- 3. 👇 SERVIÇOS DO CARRINHO E PRODUTOS (ADICIONA ISTO!) 👇 ---
            // Tens de registar todos os serviços que usas na RCL
            builder.Services.AddScoped<CarrinhoService>();
            builder.Services.AddScoped<ClientProdutoService>();

            // ⚠️ NOTA: Se o teu CarrinhoService usar "LocalStorage", 
            // vais precisar de instalar o pacote 'Blazored.LocalStorage' no projeto Mobile também
            // e adicionar: builder.Services.AddBlazoredLocalStorage();

            return builder.Build();
        }
    }

    // (A classe AuthStateProviderMobile continua aqui em baixo igual...)
    public class AuthStateProviderMobile : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}