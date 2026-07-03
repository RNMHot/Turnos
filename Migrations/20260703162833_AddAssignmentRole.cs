using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Assignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_RoleId",
                table: "Assignments",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_StaffRoles_RoleId",
                table: "Assignments",
                column: "RoleId",
                principalTable: "StaffRoles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_StaffRoles_RoleId",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_RoleId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Assignments");
        }
    }
}
