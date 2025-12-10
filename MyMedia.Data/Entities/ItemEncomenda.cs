using System.ComponentModel.DataAnnotations.Schema;

namespace MyMedia.Data.Entities
{
    public class ItemEncomenda
    {
        public int Id { get; set; }

        // A que encomenda pertence
        public int EncomendaId { get; set; }
        public Encomenda? Encomenda { get; set; }

        // Que produto é
        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }

        public int Quantidade { get; set; }

        // Guardamos o preço unitário aqui para histórico
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoUnitario { get; set; }
    }
}