using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMedia.Data.Entities
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        public string Descricao { get; set; }

        // Relação 1-para-Muitos com Produtos
        public virtual ICollection<Produto> Produtos { get; set; }
    }
}
