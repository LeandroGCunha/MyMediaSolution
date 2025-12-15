using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

public class CookieHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Verifica se estamos num pedido web com contexto
        var context = _httpContextAccessor.HttpContext;

        if (context != null)
        {
            // 2. Procura o cookie de autenticação (aquele .AspNetCore.Identity.Application)
            foreach (var cookie in context.Request.Cookies)
            {
                // 3. Cola o cookie no pedido que vai para a API
                request.Headers.Add("Cookie", $"{cookie.Key}={cookie.Value}");
            }
        }

        // 4. Envia o pedido já com o selo (cookie)
        return await base.SendAsync(request, cancellationToken);
    }
}