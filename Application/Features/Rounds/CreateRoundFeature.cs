using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Rounds;
public sealed class CreateRoundFeature(IRoundRepository roundRepository)
{
    private readonly IRoundRepository _roundRepository = roundRepository;

    public async Task<Round> ExecuteAsync(Game game, int roundNumber, int leaderSeat)
    {
        var playerCount = game.Players.Count;
        var teamSize = DetermineTeamSize(playerCount, roundNumber);

        var round = new Round
        {
            GameId = game.GameId,
            RoundNumber = roundNumber,
            LeaderSeat = leaderSeat,
            TeamSize = teamSize,
            Status = RoundStatus.TeamSelection,
            Result = RoundResult.Unknown,
            CreatedAtUtc = DateTime.UtcNow,
            SabotageCounter = 0,
        };

        await _roundRepository.AddRoundAsync(round);
        await _roundRepository.SaveChangesAsync();

        return round;
    }

    private static int DetermineTeamSize(int playerCount, int roundNumber)
    {
        var lookup = new Dictionary<int, int[]>
        {
            { 2, new[] { 2, 2, 2, 2, 2 } }, // För testning
            { 3, new[] { 2, 3, 2, 3, 3 } }, // För testning
            { 4, new[] { 2, 3, 2, 3, 3 } }, // För testning
            { 5, new[] { 2, 3, 2, 3, 3 } },
            { 6, new[] { 2, 3, 4, 3, 4 } },
            { 7, new[] { 2, 3, 3, 4, 4 } },
            { 8, new[] { 3, 4, 4, 5, 5 } },
            { 9, new[] { 3, 4, 4, 5, 5 } },
            { 10, new[] { 3, 4, 4, 5, 5 } }
        };

        if (!lookup.TryGetValue(playerCount, out var sizes))
        {
            throw new InvalidOperationException($"Unsupported player count: {playerCount}");
        }

        if (roundNumber < 1 || roundNumber > sizes.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(roundNumber), roundNumber, "Round number is out of range.");
        }

        return sizes[roundNumber - 1];
    }
}
