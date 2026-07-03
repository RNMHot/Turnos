using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationPositionId",
                table: "Assignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LocationPositions",
                columns: table => new
                {
                    LocationPositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationPositions", x => x.LocationPositionId);
                    table.ForeignKey(
                        name: "FK_LocationPositions_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_LocationPositionId",
                table: "Assignments",
                column: "LocationPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationPositions_LocationId",
                table: "LocationPositions",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_LocationPositions_LocationPositionId",
                table: "Assignments",
                column: "LocationPositionId",
                principalTable: "LocationPositions",
                principalColumn: "LocationPositionId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_LocationPositions_LocationPositionId",
                table: "Assignments");

            migrationBuilder.DropTable(
                name: "LocationPositions");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_LocationPositionId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "LocationPositionId",
                table: "Assignments");
        }
    }
}
