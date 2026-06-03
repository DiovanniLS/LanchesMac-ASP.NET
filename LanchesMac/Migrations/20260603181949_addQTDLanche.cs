    using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanchesMac.Migrations
{
    public partial class addQTDLanche : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuantidadeEstoque",
                table: "Lanches",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantidadeEstoque",
                table: "Lanches");
        }
    }
}
