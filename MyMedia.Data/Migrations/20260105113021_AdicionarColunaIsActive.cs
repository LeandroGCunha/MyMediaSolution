using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMedia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarColunaIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "AspNetUsers",
            type: "bit",
            nullable: false,
            defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
