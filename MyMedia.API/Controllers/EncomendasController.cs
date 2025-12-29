using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMedia.Client.RCL.Models;
using MyMedia.Data;
using MyMedia.Data.Entities;
using System.Security.Claims;

namespace MyMedia.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EncomendasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EncomendasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. FAZER UMA NOVA ENCOMENDA (Checkout)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto pedido)
        {
            // 1. Validações Básicas
            if (pedido == null || pedido.Itens == null || !pedido.Itens.Any())
            {
                return BadRequest("O carrinho está vazio ou os dados são inválidos.");
            }


            
            // 2. Descobrir QUEM está a comprar
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilizador não identificado.");
            }
            

            // 3. Criar a "Cabeça" da Encomenda
            var novaEncomenda = new Encomenda
            {
                ApplicationUserId = userId,
                Data = DateTime.Now,
                Estado = "Paga",
                Morada = pedido.Morada,
                Cidade = pedido.Cidade,
                CodigoPostal = pedido.CodigoPostal,
                MetodoPagamento = pedido.MetodoPagamento,
                Itens = new List<ItemEncomenda>()
            };

            decimal totalCalculado = 0;

            // 4. Processar cada produto (Validar Stock e Deduzir)
            foreach (var itemDto in pedido.Itens)
            {
                var produtoDb = await _context.Produtos.FindAsync(itemDto.ProdutoId);

                // Se o produto não existir, ignora
                if (produtoDb == null) continue;

                // --- VALIDAÇÃO DE STOCK ---
                if (produtoDb.Stock < itemDto.Quantidade)
                {
                    return BadRequest($"Lamentamos, mas já não temos stock suficiente do produto: {produtoDb.Titulo}");
                }

                // --- REDUZIR O STOCK ---
                produtoDb.Stock -= itemDto.Quantidade;

                // Criar o detalhe da encomenda
                var itemEncomenda = new ItemEncomenda
                {
                    ProdutoId = produtoDb.Id,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = produtoDb.PrecoBase
                };

                novaEncomenda.Itens.Add(itemEncomenda);
                totalCalculado += (produtoDb.PrecoBase * itemDto.Quantidade);
            }

            // 5. Atualizar o valor total
            novaEncomenda.ValorTotal = totalCalculado;

            // 6. Gravar tudo na Base de Dados
            try
            {
                _context.Encomendas.Add(novaEncomenda);
                await _context.SaveChangesAsync(); // Grava Encomenda e Atualiza Stock

                return Ok(new { Message = "Encomenda registada com sucesso!", Id = novaEncomenda.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao gravar encomenda: {ex.Message}");
            }
        }

        // ============================================================
        // 2. LER AS MINHAS ENCOMENDAS (Histórico)
        // ============================================================
        [HttpGet("minhas")]
        public async Task<ActionResult<List<Encomenda>>> GetMinhasEncomendas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var encomendas = await _context.Encomendas
                .Include(e => e.Itens)
                .ThenInclude(i => i.Produto)
                .Where(e => e.ApplicationUserId == userId)
                .OrderByDescending(e => e.Data)
                .ToListAsync();

            return Ok(encomendas);
        }

      

        [HttpGet("fornecedor/{fornecedorId}")]
        public async Task<ActionResult<List<Encomenda>>> GetVendasDoFornecedor(string fornecedorId)
        {
            // 1. Procurar todas as encomendas que tenham pelo menos um item deste fornecedor
            var encomendas = await _context.Encomendas
                .Include(e => e.Itens)
                    .ThenInclude(i => i.Produto) 
                .Where(e => e.Itens.Any(i => i.Produto.FornecedorId == fornecedorId))
                .OrderByDescending(e => e.Data)
                .ToListAsync();

            return Ok(encomendas);
        }

    }
}