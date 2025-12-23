using MyMedia.Data.Entities;
using System.Net.Http.Json;

namespace MyMedia.Client.RCL.Services
{
    public class ClientProdutoService
    {
        private readonly HttpClient _httpClient;

        // A injeção de dependência vai entregar um HttpClient já configurado com o endereço da API
        public ClientProdutoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Método para obter todos os produtos
        public async Task<List<Produto>> GetProdutosAsync()
        {
            // Tenta ir buscar os dados ao endpoint que criámos na API (/api/produtos)
            var resultado = await _httpClient.GetFromJsonAsync<List<Produto>>("api/produtos");
            return resultado ?? new List<Produto>();
        }

        // Método para obter um produto específico (para a página de detalhes)
        public async Task<Produto?> GetProdutoByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Produto>($"api/produtos/{id}");
        }

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            var resultado = await _httpClient.GetFromJsonAsync<List<Categoria>>("api/produtos/categorias");
            return resultado ?? new List<Categoria>();
        }

    }
}