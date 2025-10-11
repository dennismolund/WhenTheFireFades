using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhenTheFireFades.Migrations
{
    /// <inheritdoc />
    public partial class FixGameWinnerType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rounds_GameId",
                table: "Rounds");

            migrationBuilder.DropIndex(
                name: "IX_MissionVotes_RoundId",
                table: "MissionVotes");

            migrationBuilder.DropIndex(
                name: "IX_GamePlayers_GameId",
                table: "GamePlayers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "TeamProposalVotes",
                type: "Datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "GameWinner",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConnectionCode",
                table: "Games",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nickname",
                table: "GamePlayers",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_TeamProposals_TeamProposalId_AttemptNumber",
                table: "TeamProposals",
                columns: new[] { "TeamProposalId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamProposalMembers_TeamProposalMemberId_Seat",
                table: "TeamProposalMembers",
                columns: new[] { "TeamProposalMemberId", "Seat" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_GameId_RoundNumber",
                table: "Rounds",
                columns: new[] { "GameId", "RoundNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MissionVotes_RoundId_Seat",
                table: "MissionVotes",
                columns: new[] { "RoundId", "Seat" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_ConnectionCode",
                table: "Games",
                column: "ConnectionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_GameId_Seat",
                table: "GamePlayers",
                columns: new[] { "GameId", "Seat" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamProposals_TeamProposalId_AttemptNumber",
                table: "TeamProposals");

            migrationBuilder.DropIndex(
                name: "IX_TeamProposalMembers_TeamProposalMemberId_Seat",
                table: "TeamProposalMembers");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_GameId_RoundNumber",
                table: "Rounds");

            migrationBuilder.DropIndex(
                name: "IX_MissionVotes_RoundId_Seat",
                table: "MissionVotes");

            migrationBuilder.DropIndex(
                name: "IX_Games_ConnectionCode",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_GamePlayers_GameId_Seat",
                table: "GamePlayers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "TeamProposalVotes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "Datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "GameWinner",
                table: "Games",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ConnectionCode",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Nickname",
                table: "GamePlayers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_GameId",
                table: "Rounds",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionVotes_RoundId",
                table: "MissionVotes",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_GameId",
                table: "GamePlayers",
                column: "GameId");
        }
    }
}
