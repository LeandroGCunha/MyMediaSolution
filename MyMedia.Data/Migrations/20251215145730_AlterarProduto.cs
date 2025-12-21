using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMedia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterarProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Forçar a eliminação da coluna 'Tipo' que ainda está na BD
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Produtos");

            // 2. Forçar a 'Descricao' a aceitar valores nulos (nullable: true)
            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Produtos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
