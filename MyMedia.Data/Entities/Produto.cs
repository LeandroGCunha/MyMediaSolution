using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMedia.Data.Entities
{
    public class Produto
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }

        public string Descricao { get; set; }

        // O enunciado distingue "Suporte" (CD/DVD) de "Acessório". 
        // Pode usar um Enum ou outra classe para Tipo.
        public string Tipo { get; set; } // Ex: "Filme", "Música", "Acessório"

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoBase { get; set; }

        // Percentagem de lucro da loja
        [Column(TypeName = "decimal(18,2)")]
        public decimal MargemLucro { get; set; }

        // Preço Final = PreçoBase + (PreçoBase * Margem) - Pode ser calculado ou guardado
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoFinal { get; set; }

        public int Stock { get; set; }
        public bool IsAtivo { get; set; } = false; // Começa inativo/pendente

        // Relacionamentos
        public int CategoriaId { get; set; }
        public virtual Categoria Categoria { get; set; }

        // Quem está a vender? (O Fornecedor)
        public string FornecedorId { get; set; }
        public virtual ApplicationUser Fornecedor { get; set; }
    }

}
