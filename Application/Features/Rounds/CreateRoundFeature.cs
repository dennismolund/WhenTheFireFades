using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Services;

namespace Application.Features.Rounds;
public sealed class CreateRoundFeature(IRoundRepository roundRepository)
{
    public async Task<Round> ExecuteAsync(Game game, int roundNumber, int leaderSeat)
    {
        var playerCount = game.Players.Count;
        var teamSize = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        var round = new Round
        {
            GameId = game.GameId,
            RoundNumber = roundNumber,
            LeaderSeat = leaderSeat,
            TeamSize = teamSize,
            Status = RoundStatus.TeamSelection,
            Result = RoundResult.Unknown,
            SabotageCounter = 0,
        };

        await roundRepository.AddRoundAsync(round);
        await roundRepository.SaveChangesAsync();

        return round;
    }

    
}
