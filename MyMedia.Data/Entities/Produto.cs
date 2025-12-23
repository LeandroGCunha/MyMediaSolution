using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMedia.Data.Entities
{
    public class Produto
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }

        public string? Descricao { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoBase { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MargemLucro { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoFinal { get; set; }

        [Display(Name = "Imagem")]
        public string? ImagemUrl { get; set; }

        public int Stock { get; set; }
        public bool IsAtivo { get; set; } = false;

        // Relacionamentos
        public int? CategoriaId { get; set; }
        public virtual Categoria? Categoria { get; set; }

        public string? FornecedorId { get; set; }
        public virtual ApplicationUser Fornecedor { get; set; }
    }
}