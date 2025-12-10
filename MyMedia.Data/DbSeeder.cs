using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyMedia.Data.Entities;

namespace MyMedia.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Criar as Roles exigidas no Enunciado (Ponto 8)
            await CriarRole(roleManager, "Administrador"); // Acesso total Gestão Loja
            await CriarRole(roleManager, "Funcionario");   // Acesso limitado Gestão Loja
            await CriarRole(roleManager, "Fornecedor");    // Acesso Frontend (vendedor)
            await CriarRole(roleManager, "Cliente");       // Acesso Frontend (comprador)

            // 2. Criar o Utilizador Super-Admin (para conseguir entrar na Gestão de Loja)
            var adminEmail = "admin@mymedia.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    NomeCompleto = "Super Administrador",
                    DataRegisto = DateTime.Now,
                    NIF = "999999990"
                };

                // Cria o user com a password "Admin123!"
                await userManager.CreateAsync(adminUser, "Admin123!");

                // Atribui a função de "Administrador"
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
        }

        private static async Task CriarRole(RoleManager<IdentityRole> roleManager, string role)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}