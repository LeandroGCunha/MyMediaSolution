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
    }
}