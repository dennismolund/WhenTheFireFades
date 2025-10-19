using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhenTheFireFades.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSuccessToMissionVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSabotage",
                table: "MissionVotes",
                newName: "IsSuccess");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSuccess",
                table: "MissionVotes",
                newName: "IsSabotage");
        }
    }
}
