using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Globalization;

namespace Application.Services;

public sealed class GameService(
    IGameRepository gameRepository, 
    IGamePlayerRepository gamePlayerRepository,
    IRoundRepository roundRepository)
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IGamePlayerRepository _gamePlayerRepository = gamePlayerRepository;
    private readonly IRoundRepository _roundRepository = roundRepository;
    private readonly Random _random = new();

    public async Task<Game> CreateGameAsync()
    {
        var game = new Game
        {
            ConnectionCode = GenerateCode(),
            LeaderSeat = 1,
            Status = GameStatus.Lobby,
            GameWinner = GameResult.Unknown,
            RoundCounter = 0,
            SuccessCount = 0,
            SabotageCount = 0,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _gameRepository.AddGameAsync(game);
        await _gameRepository.SaveChangesAsync();

        return game;
    }

    public async Task<GamePlayer> CreateGamePlayerAsync(Game game, int creatorTempUserId, string? creatorUsername = null)
    {
        var nextSeat = await _gamePlayerRepository.GetNextAvailableSeatAsync(game.GameId);

        var player = new GamePlayer
        {
            GameId = game.GameId,
            TempUserId = creatorTempUserId,
            Nickname = creatorUsername ?? $"Player#{creatorTempUserId}",
            Seat = nextSeat,
            Role = PlayerRole.Human,
            IsReady = false,
            IsConnected = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        await _gamePlayerRepository.AddPlayerAsync(player);
        await _gamePlayerRepository.SaveChangesAsync();

        return player;
    }


    private const int MinimumPlayerCount = 2; //Change to 5 later.

    public async Task<Round> StartGameAsync(Game game)
    {
        if (game == null)
            throw new ArgumentException("Game not found.");
        if (game.Status != GameStatus.Lobby)
            throw new InvalidOperationException("Game is not in a state that can be started.");

        var players = game.Players.ToList();
        var playerCount = players.Count;

        if(playerCount < MinimumPlayerCount)
            throw new InvalidOperationException($"Not enough players to start the game. Minimum is {MinimumPlayerCount}.");

        AssignRoles(players);


        game.Status = GameStatus.InProgress;
        game.UpdatedAtUtc = DateTime.UtcNow;
        game.RoundCounter = 1;
        game.LeaderSeat = 1;

        await _gameRepository.SaveChangesAsync();

        return await CreateRoundAsync(game, game.RoundCounter, game.LeaderSeat);

    }

    public async Task<Round> CreateRoundAsync(Game game, int roundNumber, int leaderSeat)
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


    private void AssignRoles(List<GamePlayer> players)
    {
        var shuffled = players
            .OrderBy(_ => _random.Next())
            .ToList();

        foreach(var player in shuffled)
        {
            player.Role = PlayerRole.Human;
        }

        var shapeshifterCount = DetermineShapeshifterCount(players.Count);
        for (var i = 0; i < shapeshifterCount; i++)
        {
            shuffled[i].Role = PlayerRole.Shapeshifter;
        }
    }

    //TODO: Need to handle max players
    public async Task<int> NextAvailableSeat(int gameId)
    {
        return await _gamePlayerRepository.GetNextAvailableSeatAsync(gameId);
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
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
