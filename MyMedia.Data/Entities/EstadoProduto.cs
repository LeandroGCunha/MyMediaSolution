using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMedia.Data.Entities // Ou o namespace que usas
{
    public enum EstadoProduto
    {
        Pendente = 0,   // Acabou de criar ou editou (espera Admin)
        Ativo = 1,      // Aprovado e visível na loja
        Pausado = 2,    // O Fornecedor escondeu (pode reativar)
        Rejeitado = 3   // O Admin bloqueou (Fornecedor não pode ativar)
    }
}