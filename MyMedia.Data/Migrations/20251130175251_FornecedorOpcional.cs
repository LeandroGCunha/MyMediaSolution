using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMedia.Data.Migrations
{
    /// <inheritdoc />
    public partial class FornecedorOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_AspNetUsers_FornecedorId",
                table: "Produtos");

            migrationBuilder.AlterColumn<string>(
                name: "FornecedorId",
                table: "Produtos",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_AspNetUsers_FornecedorId",
                table: "Produtos",
                column: "FornecedorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_AspNetUsers_FornecedorId",
                table: "Produtos");

            migrationBuilder.AlterColumn<string>(
                name: "FornecedorId",
                table: "Produtos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_AspNetUsers_FornecedorId",
                table: "Produtos",
                column: "FornecedorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
