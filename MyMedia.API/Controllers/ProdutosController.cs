using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMedia.Data;
using MyMedia.Data.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization; 

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
        [Authorize(Roles = "Fornecedor")] 
        public async Task<ActionResult<List<Produto>>> GetProdutosDoFornecedor(string userId)
        {

            var loggedUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != loggedUser) return Forbid();

            var produtos = await _context.Produtos
                .Include(p => p.Fornecedor) 
                .Include(p => p.Categoria)  
                .Where(p => p.FornecedorId == userId) 
                .ToListAsync();

            return Ok(produtos);
        }

        [HttpPost]
        [Authorize(Roles = "Fornecedor")] // 👈 Proteção
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

       
        [HttpPut("{id}")]
        [Authorize(Roles = "Fornecedor")]
        public async Task<IActionResult> PutProduto(int id, Produto produto)
        {
            if (id != produto.Id) return BadRequest("ID mismatch");

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // 1. REGRA DE NEGÓCIO: Fornecedor edita -> Fica Pendente
            if (userRole == "Fornecedor")
            {
                produto.IsAtivo = false;
            }

            // 2. LÓGICA DE IMAGEM NO EDITAR (FALTAVA ISTO!) 📸
            // Se o utilizador carregou uma nova imagem, ela vem em Base64
            if (!string.IsNullOrEmpty(produto.ImagemUrl) && produto.ImagemUrl.Contains("base64"))
            {
                try
                {
                    var pastaImagens = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagens");
                    if (!Directory.Exists(pastaImagens)) Directory.CreateDirectory(pastaImagens);

                    var partes = produto.ImagemUrl.Split(',');
                    var base64Limpa = partes.Length > 1 ? partes[1] : partes[0];
                    var bytesImagem = Convert.FromBase64String(base64Limpa);

                    // Sobrescreve a imagem antiga usando o mesmo ID
                    var nomeFicheiro = $"{produto.Id}.png";
                    var caminhoCompleto = Path.Combine(pastaImagens, nomeFicheiro);

                    await System.IO.File.WriteAllBytesAsync(caminhoCompleto, bytesImagem);

                    // Atualiza o caminho para ser o ficheiro limpo
                    produto.ImagemUrl = $"imagens/{nomeFicheiro}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao atualizar imagem no disco: " + ex.Message);
                }
            }

            _context.Entry(produto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Produtos.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/produtos/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Fornecedor")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            // 1. Procurar o produto
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            // 2. SEGURANÇA: Verificar se o Fornecedor é o dono do produto
            // Se for um "Fornecedor", só pode apagar se o produto for dele.
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Fornecedor" && produto.FornecedorId != userId)
            {
                return Forbid(); // 403 - Proibido (Não é teu!)
            }

            // 3. Tentar Apagar
            try
            {
                // Apagar a imagem do disco (Opcional, mas boa prática para não deixar lixo)
                if (!string.IsNullOrEmpty(produto.ImagemUrl))
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", produto.ImagemUrl);
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }

                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // 4. ERRO DE CHAVE ESTRANGEIRA
                // Se o produto já estiver numa Encomenda, a BD lança erro. Nós apanhamos e avisamos.
                return BadRequest("Não é possível apagar este produto porque ele já tem vendas ou registos associados.");
            }

            return NoContent(); // 204 - Sucesso (sem conteúdo)
        }

    }
}