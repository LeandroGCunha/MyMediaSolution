using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            // 3. Criar a Encomenda
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

            // 4. Processar cada produto
            foreach (var itemDto in pedido.Itens)
            {
                var produtoDb = await _context.Produtos.FindAsync(itemDto.ProdutoId);

                // Se o produto não existir, salta à frente
                if (produtoDb == null) continue;

                // --- 🔴 NOVO: VALIDAÇÃO DE STOCK ---
                if (produtoDb.Stock < itemDto.Quantidade)
                {
                    // Se não houver stock suficiente, cancelamos tudo e avisamos o cliente
                    return BadRequest($"Lamentamos, mas já não temos stock suficiente do produto: {produtoDb.Titulo}");
                }

                // --- 🔴 NOVO: REDUZIR O STOCK ---
                produtoDb.Stock -= itemDto.Quantidade;
                // (O Entity Framework vai detetar esta mudança e gravar automaticamente no final)

                // Criar o detalhe da encomenda
                var itemEncomenda = new ItemEncomenda
                {
                    ProdutoId = produtoDb.Id,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = produtoDb.PrecoBase // Confirmado que usas PrecoBase
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

                // O SaveChangesAsync grava a Encomenda E atualiza o Stock dos produtos ao mesmo tempo!
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Encomenda registada com sucesso!", Id = novaEncomenda.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao gravar encomenda: {ex.Message}");
            }
        }
    }
}