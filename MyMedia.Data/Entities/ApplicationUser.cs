using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MyMedia.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Pode adicionar campos extra aqui, ex:
        public string NomeCompleto { get; set; }
        public string NIF { get; set; }

        // Data de criação da conta (útil para gestão)
        public DateTime DataRegisto { get; set; } = DateTime.Now;
    }
}
