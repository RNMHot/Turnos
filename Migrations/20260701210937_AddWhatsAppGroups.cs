using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageLogs_Persons_PersonId",
                table: "MessageLogs");

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "MessageLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "WhatsAppGroupId",
                table: "MessageLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WhatsAppGroups",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppGroups", x => x.GroupId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageLogs_WhatsAppGroupId",
                table: "MessageLogs",
                column: "WhatsAppGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLogs_Persons_PersonId",
                table: "MessageLogs",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLogs_WhatsAppGroups_WhatsAppGroupId",
                table: "MessageLogs",
                column: "WhatsAppGroupId",
                principalTable: "WhatsAppGroups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageLogs_Persons_PersonId",
                table: "MessageLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageLogs_WhatsAppGroups_WhatsAppGroupId",
                table: "MessageLogs");

            migrationBuilder.DropTable(
                name: "WhatsAppGroups");

            migrationBuilder.DropIndex(
                name: "IX_MessageLogs_WhatsAppGroupId",
                table: "MessageLogs");

            migrationBuilder.DropColumn(
                name: "WhatsAppGroupId",
                table: "MessageLogs");

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "MessageLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLogs_Persons_PersonId",
                table: "MessageLogs",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "PersonId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
