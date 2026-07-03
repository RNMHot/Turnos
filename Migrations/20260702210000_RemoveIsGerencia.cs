using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsGerencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGerencia",
                table: "Persons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGerencia",
                table: "Persons",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
