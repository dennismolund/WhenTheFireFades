using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhenTheFireFades.Migrations
{
    /// <inheritdoc />
    public partial class RenamedRoundPhasetoRoundstatusinRounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Phase",
                table: "Rounds",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Rounds",
                newName: "Phase");
        }
    }
}
