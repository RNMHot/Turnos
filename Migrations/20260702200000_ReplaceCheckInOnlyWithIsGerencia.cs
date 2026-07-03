using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCheckInOnlyWithIsGerencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGerencia",
                table: "Persons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Migrate data: persons who were NOT CheckInOnly had full app access → they become Gerencia
            migrationBuilder.Sql("UPDATE Persons SET IsGerencia = CASE WHEN CheckInOnly = 0 THEN 1 ELSE 0 END");

            migrationBuilder.DropColumn(
                name: "CheckInOnly",
                table: "Persons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CheckInOnly",
                table: "Persons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE Persons SET CheckInOnly = CASE WHEN IsGerencia = 0 THEN 1 ELSE 0 END");

            migrationBuilder.DropColumn(
                name: "IsGerencia",
                table: "Persons");
        }
    }
}
