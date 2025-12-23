using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMedia.Data.Entities
{
    public class Encomenda
    {
        public int Id { get; set; }

        public DateTime Data { get; set; } = DateTime.Now;

        // Valor total da compra
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }

        // Estado: "Pendente", "Paga", "Enviada"
        public string Estado { get; set; } = "Pendente";

        // Quem comprou
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        // Lista de produtos comprados
        public List<ItemEncomenda> Itens { get; set; } = new();

        public string Morada { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string MetodoPagamento { get; set; } = string.Empty;
    }
}