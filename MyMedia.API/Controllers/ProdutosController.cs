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
        public async Task<ActionResult<Produto>> PostProduto(Produto produto)
        {
            string? base64Imagem = null;

            if (!string.IsNullOrEmpty(produto.ImagemUrl) && produto.ImagemUrl.Contains("base64"))
            {
                base64Imagem = produto.ImagemUrl;
                produto.ImagemUrl = null;
            }

            // 1. CORREÇÃO: Ao criar, define o estado como PENDENTE (Enum)
            produto.Estado = EstadoProduto.Pendente;

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            if (base64Imagem != null)
            {
                try
                {
                    var pastaRaiz = Directory.GetCurrentDirectory();
                    var pastaImagens = Path.Combine(pastaRaiz, "wwwroot", "imagens");

                    if (!Directory.Exists(pastaImagens)) Directory.CreateDirectory(pastaImagens);

                    var partes = base64Imagem.Split(',');
                    var base64Limpa = partes.Length > 1 ? partes[1] : partes[0];
                    var bytesImagem = Convert.FromBase64String(base64Limpa);

                    var nomeFicheiro = $"{produto.Id}.png";
                    var caminhoCompleto = Path.Combine(pastaImagens, nomeFicheiro);

                    await System.IO.File.WriteAllBytesAsync(caminhoCompleto, bytesImagem);

                    produto.ImagemUrl = $"imagens/{nomeFicheiro}";

                    _context.Produtos.Update(produto);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERRO IMAGEM] {ex.Message}");
                }
            }

            return CreatedAtAction("GetProduto", new { id = produto.Id }, produto);
        }

        // PUT: api/produtos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, Produto produtoAtualizado)
        {
            if (id != produtoAtualizado.Id) return BadRequest("O ID não coincide.");

            // 1. Buscar o original (AsNoTracking para não bloquear)
            var produtoOriginal = await _context.Produtos
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (produtoOriginal == null) return NotFound();

            // 2. DETETAR ALTERAÇÕES CRÍTICAS
            bool mudouCoisaImportante =
                produtoOriginal.Titulo != produtoAtualizado.Titulo ||
                produtoOriginal.PrecoBase != produtoAtualizado.PrecoBase ||
                produtoOriginal.Descricao != produtoAtualizado.Descricao ||
                produtoOriginal.CategoriaId != produtoAtualizado.CategoriaId ||
                (!string.IsNullOrEmpty(produtoAtualizado.ImagemUrl) && produtoAtualizado.ImagemUrl.Contains("base64"));

            // 3. MÁQUINA DE ESTADOS (Lógica com Enums)
            if (mudouCoisaImportante)
            {
                // REGRA 1: Se mudou dados críticos, volta para PENDENTE (Enum)
                produtoAtualizado.Estado = EstadoProduto.Pendente;
            }
            else
            {
                // REGRA 2: Se não mudou dados (só mudou stock ou estado)...

                // Sub-regra: Se estava Pendente ou Rejeitado, o Fornecedor NÃO pode ativar sozinho.
                // Verifica se ele está a tentar passar para "Ativo".
                if ((produtoOriginal.Estado == EstadoProduto.Pendente || produtoOriginal.Estado == EstadoProduto.Rejeitado)
                    && produtoAtualizado.Estado == EstadoProduto.Ativo)
                {
                    // Bloqueia: Mantém o estado original (não deixa ativar)
                    produtoAtualizado.Estado = produtoOriginal.Estado;
                }

                // Se estava "Ativo" e passou para "Pausado" -> OK
                // Se estava "Pausado" e passou para "Ativo" -> OK (porque já tinha sido aprovado antes)
            }

            // 4. Lógica de Imagem
            if (!string.IsNullOrEmpty(produtoAtualizado.ImagemUrl) && produtoAtualizado.ImagemUrl.Contains("base64"))
            {
                try
                {
                    var pastaImagens = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagens");
                    if (!Directory.Exists(pastaImagens)) Directory.CreateDirectory(pastaImagens);

                    var partes = produtoAtualizado.ImagemUrl.Split(',');
                    var base64Limpa = partes.Length > 1 ? partes[1] : partes[0];
                    var bytesImagem = Convert.FromBase64String(base64Limpa);

                    var nomeFicheiro = $"{produtoAtualizado.Id}.png";
                    var caminhoCompleto = Path.Combine(pastaImagens, nomeFicheiro);

                    await System.IO.File.WriteAllBytesAsync(caminhoCompleto, bytesImagem);

                    produtoAtualizado.ImagemUrl = $"imagens/{nomeFicheiro}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERRO IMAGEM PUT] {ex.Message}");
                }
            }
            else
            {
                // Mantém a imagem antiga se não enviou nova
                if (string.IsNullOrEmpty(produtoAtualizado.ImagemUrl))
                {
                    produtoAtualizado.ImagemUrl = produtoOriginal.ImagemUrl;
                }
            }

            _context.Entry(produtoAtualizado).State = EntityState.Modified;

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
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Fornecedor" && produto.FornecedorId != userId)
            {
                return Forbid();
            }

            try
            {
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
                return BadRequest("Não é possível apagar este produto porque ele já tem vendas ou registos associados.");
            }

            return NoContent();
        }
    }
}