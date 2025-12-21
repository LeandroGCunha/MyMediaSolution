using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Categoria
{
    public int Id { get; set; }

    [Required]
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    // --- NOVO: Referência para a Categoria Pai ---
    public int? CategoriaPaiId { get; set; } // Pode ser null (se for raiz)

    // Navegação (para o Entity Framework saber ligar as tabelas)
    public virtual Categoria? CategoriaPai { get; set; }

    // Lista de filhas (útil para menus depois)
    public virtual ICollection<Categoria> SubCategorias { get; set; } = new List<Categoria>();
}