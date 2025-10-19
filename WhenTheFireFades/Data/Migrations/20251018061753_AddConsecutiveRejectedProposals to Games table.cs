using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhenTheFireFades.Migrations
{
    /// <inheritdoc />
    public partial class AddConsecutiveRejectedProposalstoGamestable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveRejectedProposals",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsecutiveRejectedProposals",
                table: "Games");
        }
    }
}
