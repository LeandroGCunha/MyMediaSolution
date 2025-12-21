using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyMedia.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMedia.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Encomenda> Encomendas { get; set; }
        public DbSet<ItemEncomenda> ItensEncomenda { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- 1. REGRA DE SEGURANÇA PARA CATEGORIAS (Hierarquia) ---
            // OBRIGATÓRIO: 'Restrict' impede ciclos e erros do SQL Server.
            builder.Entity<Categoria>()
                .HasOne(c => c.CategoriaPai)
                .WithMany(c => c.SubCategorias)
                .HasForeignKey(c => c.CategoriaPaiId)
                .OnDelete(DeleteBehavior.Restrict); // <--- TEM DE SER RESTRICT!

            // --- 2. REGRA PARA PRODUTOS (Ficar sem categoria) ---
            // Aqui usamos SetNull: Se a categoria for apagada, o produto fica sem categoria.
            builder.Entity<Produto>()
                .HasOne(p => p.Categoria)
                .WithMany()
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}