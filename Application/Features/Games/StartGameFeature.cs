using Application.Features.Rounds;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Games;
public sealed class StartGameFeature(IGameRepository gameRepository, CreateRoundFeature createRoundFeature)
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly CreateRoundFeature _createRoundFeature = createRoundFeature;
    private readonly Random _random = new();
    private readonly int _minimumPlayerCount = 2;

    public async Task<Round> ExecuteAsync(Game game)
    {
        if (game == null)
            throw new ArgumentException("Game not found.");
        if (game.Status != GameStatus.Lobby)
            throw new InvalidOperationException("Game is not in a state that can be started.");

        var players = game.Players.ToList();
        var playerCount = players.Count;

        

        if (playerCount < _minimumPlayerCount)
            throw new InvalidOperationException($"Not enough players to start the game. Minimum is {_minimumPlayerCount}.");

        AssignRoles(players);


        game.Status = GameStatus.InProgress;
        game.UpdatedAtUtc = DateTime.UtcNow;
        game.RoundCounter = 1;
        game.LeaderSeat = 1;

        await _gameRepository.SaveChangesAsync();

        return await _createRoundFeature.ExecuteAsync(game, game.RoundCounter, game.LeaderSeat);

    }

    private void AssignRoles(List<GamePlayer> players)
    {
        var shuffled = players
            .OrderBy(_ => _random.Next())
            .ToList();

        foreach (var player in shuffled)
        {
            player.Role = PlayerRole.Human;
        }

        var shapeshifterCount = DetermineShapeshifterCount(players.Count);
        for (var i = 0; i < shapeshifterCount; i++)
        {
            shuffled[i].Role = PlayerRole.Shapeshifter;
        }
    }

    private static int DetermineShapeshifterCount(int playerCount)
    {
        return playerCount switch
        {
            >= 10 => 4,
            9 => 3,
            8 => 3,
            7 => 3,
            6 => 2,
            5 => 2,
            _ => 1
        };
    }

}
