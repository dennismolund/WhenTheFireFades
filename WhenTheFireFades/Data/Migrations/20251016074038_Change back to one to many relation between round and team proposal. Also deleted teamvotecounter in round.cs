using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhenTheFireFades.Migrations
{
    /// <inheritdoc />
    public partial class ChangebacktoonetomanyrelationbetweenroundandteamproposalAlsodeletedteamvotecounterinround : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamProposals_RoundId",
                table: "TeamProposals");

            migrationBuilder.DropColumn(
                name: "TeamVoteCounter",
                table: "Rounds");

            migrationBuilder.CreateIndex(
                name: "IX_TeamProposals_RoundId",
                table: "TeamProposals",
                column: "RoundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamProposals_RoundId",
                table: "TeamProposals");

            migrationBuilder.AddColumn<int>(
                name: "TeamVoteCounter",
                table: "Rounds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TeamProposals_RoundId",
                table: "TeamProposals",
                column: "RoundId",
                unique: true);
        }
    }
}
