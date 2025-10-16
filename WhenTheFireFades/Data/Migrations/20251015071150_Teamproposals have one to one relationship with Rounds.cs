using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhenTheFireFades.Migrations
{
    /// <inheritdoc />
    public partial class TeamproposalshaveonetoonerelationshipwithRounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamProposals_RoundId",
                table: "TeamProposals");

            migrationBuilder.CreateIndex(
                name: "IX_TeamProposals_RoundId",
                table: "TeamProposals",
                column: "RoundId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamProposals_RoundId",
                table: "TeamProposals");

            migrationBuilder.CreateIndex(
                name: "IX_TeamProposals_RoundId",
                table: "TeamProposals",
                column: "RoundId");
        }
    }
}
