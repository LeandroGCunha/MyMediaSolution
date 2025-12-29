using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyMedia.Data.Entities;
using System.ComponentModel.DataAnnotations; // Necessário para validar emails e passwords
using MyMedia.Client.RCL.Models;

namespace MyMedia.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            // 1. Criar o utilizador
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                NomeCompleto = request.NomeCompleto, 
                NIF = request.NIF,
                IsActive = false
            };

            // 2. Gravar na BD (com password segura)
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                if (request.Role == "Cliente" || request.Role == "Fornecedor")
                {
                    await _userManager.AddToRoleAsync(user, request.Role);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Cliente");
                }

                return Ok(new { message = "Registo efetuado com sucesso!" });
            }

            // 3. Devolver erro se falhar
            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginRequest)
        {
            // 1. Procurar o utilizador
            var user = await _userManager.FindByNameAsync(loginRequest.UserName);

            if (user == null) return BadRequest("User não encontrado.");

            // 👇 2. (NOVO) VERIFICAR SE ESTÁ ATIVO 🛑
            // Se o utilizador existir, mas o IsActive for falso, barramos a entrada aqui.
            if (user.IsActive == false)
            {
                // Retornamos 403 (Forbidden). 
                // O teu Program.cs do Client vai apanhar este 403 e mostrar o aviso amarelo.
                return StatusCode(403, "Conta pendente de aprovação.");
            }

            // 3. Verificar a Password
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, false);

            if (!result.Succeeded) return BadRequest("Password errada.");

            // 4. Buscar Roles e Responder
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                Message = "Login com sucesso!",
                Roles = roles,
                UserId = user.Id
            });
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet("user")]
        public IActionResult GetCurrentUser()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Ok(new
                {
                    UserName = User.Identity.Name,
                    IsAuthenticated = true
                });
            }
            return Unauthorized();
        }
    }
}