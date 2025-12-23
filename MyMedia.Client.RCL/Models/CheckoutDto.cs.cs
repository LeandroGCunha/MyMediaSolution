using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMedia.Client.RCL.Models
{
    public class CheckoutDto
    {
        public string Morada { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string MetodoPagamento { get; set; } = string.Empty;

        // O que vai comprar
        public List<CarrinhoItemDto> Itens { get; set; } = new();
    }
}
