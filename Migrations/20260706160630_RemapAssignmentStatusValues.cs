using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.Migrations
{
    /// <inheritdoc />
    public partial class RemapAssignmentStatusValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Old AssignmentStatus ints: Disponible=0, Ofrecido=1, Asignado=4, Confirmado=5, Candidato=8
            // New AssignmentStatus ints: Candidato=0, Ofrecido=1, Aceptado=2, Denegado=3, Asignado=4, Confirmado=5
            // Ofrecido/Asignado/Confirmado keep the same int, so only Disponible and Candidato need remapping.
            // Order matters: move old Disponible(0) out of the way before old Candidato(8) takes over 0.
            migrationBuilder.Sql("UPDATE Assignments SET Status = 2 WHERE Status = 0;"); // Disponible -> Aceptado
            migrationBuilder.Sql("UPDATE Assignments SET Status = 0 WHERE Status = 8;"); // Candidato -> Candidato (new int)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Best-effort reverse; rows that became Denegado (3) have no pre-migration equivalent and are left as-is.
            migrationBuilder.Sql("UPDATE Assignments SET Status = 8 WHERE Status = 0;"); // Candidato -> old int
            migrationBuilder.Sql("UPDATE Assignments SET Status = 0 WHERE Status = 2;"); // Aceptado -> Disponible
        }
    }
}
