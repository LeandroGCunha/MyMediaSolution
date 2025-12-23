using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMedia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMoradaEncomenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodigoPostal",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MetodoPagamento",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Morada",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "CodigoPostal",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "MetodoPagamento",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "Morada",
                table: "Encomendas");
        }
    }
}
