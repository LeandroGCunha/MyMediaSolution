using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMedia.Data;
using MyMedia.Data.Entities;

namespace MyMedia.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProdutosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            // Retorna a lista de produtos, incluindo a categoria
            return await _context.Produtos
                .Include(p => p.Categoria)
                .ToListAsync();
        }

        // GET: api/produtos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
            {
                return NotFound();
            }

            return produto;
        }
        [HttpGet("categorias")]
        public async Task<ActionResult<List<Categoria>>> GetCategorias()
        {
            // Carrega todas as categorias da BD
            return await _context.Categorias.ToListAsync();
        }

        [HttpGet("por-fornecedor/{userId}")]
        public async Task<ActionResult<List<Produto>>> GetProdutosDoFornecedor(string userId)
        {
            var produtos = await _context.Produtos
                .Include(p => p.Fornecedor) 
                .Include(p => p.Categoria)  
                .Where(p => p.FornecedorId == userId) 
                .ToListAsync();

            return Ok(produtos);
        }

        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<Produto>> PostProduto(Produto produto)
        {
            // 1. SEPARAR A IMAGEM (TRUQUE)
            // Guardamos a string gigante numa variável temporária e limpamos do produto
            // para não gravar lixo na base de dados no primeiro passo.
            string? base64Imagem = null;

            if (!string.IsNullOrEmpty(produto.ImagemUrl) && produto.ImagemUrl.Contains("base64"))
            {
                base64Imagem = produto.ImagemUrl;
                produto.ImagemUrl = null; // Deixa null por agora
            }

            // 2. PRIMEIRO SALVAMENTO (Para Gerar o ID)
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            // ✨ MAGIA: Agora o produto.Id já existe! (ex: 5)

            // 3. GRAVAR A IMAGEM COM O NOME DO ID
            if (base64Imagem != null)
            {
                try
                {
                    var pastaRaiz = Directory.GetCurrentDirectory();
                    var pastaImagens = Path.Combine(pastaRaiz, "wwwroot", "imagens");

                    if (!Directory.Exists(pastaImagens)) Directory.CreateDirectory(pastaImagens);

                    // A. Limpar o Base64
                    var partes = base64Imagem.Split(',');
                    var base64Limpa = partes.Length > 1 ? partes[1] : partes[0];
                    var bytesImagem = Convert.FromBase64String(base64Limpa);

                    // B. Criar o nome usando o ID recém-criado
                    // O ficheiro vai chamar-se "5.png" ou "102.png"
                    var nomeFicheiro = $"{produto.Id}.png";
                    var caminhoCompleto = Path.Combine(pastaImagens, nomeFicheiro);

                    // C. Gravar no disco
                    await System.IO.File.WriteAllBytesAsync(caminhoCompleto, bytesImagem);

                    // D. Atualizar o caminho no objeto produto
                    produto.ImagemUrl = $"imagens/{nomeFicheiro}";

                    // 4. SEGUNDO SALVAMENTO (Atualizar o link na BD)
                    _context.Produtos.Update(produto);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERRO IMAGEM] Não consegui gravar a imagem do ID {produto.Id}: {ex.Message}");
                    // Não fazemos throw para não cancelar a criação do produto, mas ele fica sem foto.
                }
            }

            return CreatedAtAction("GetProduto", new { id = produto.Id }, produto);
        }
    }
}