using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMedia.Client.RCL.Models
{
    // Esta classe serve SÓ para ler o que a API responde
    public class LoginResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
    }
}
