using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Turnos.Data;

#nullable disable

namespace Turnos.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260626123500_AddEventStaffingRequirements")]
    public partial class AddEventStaffingRequirements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequiredOther",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequiredOtherLabel",
                table: "Events",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "Other");

            migrationBuilder.AddColumn<int>(
                name: "RequiredSupervisors",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequiredUshers",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredOther",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RequiredOtherLabel",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RequiredSupervisors",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RequiredUshers",
                table: "Events");
        }
    }
}
