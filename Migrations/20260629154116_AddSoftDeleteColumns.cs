using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "StaffRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Records",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Persons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "PersonRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "MessageLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Availabilities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "AuditLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "AttendanceBreaks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Assignments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "StaffRoles");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "PersonRoles");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "MessageLogs");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Availabilities");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "AttendanceBreaks");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Assignments");
        }
    }
}
