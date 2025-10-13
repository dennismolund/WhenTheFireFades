using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using WhenTheFireFades.Data;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Hubs;
using WhenTheFireFades.Models;

namespace WhenTheFireFades.Domain.Services;

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

    public async Task StartGameAsync(Game game)
    {
        if (game == null)
            throw new ArgumentException("Game not found.");
        if (game.Status != GameStatus.Lobby)
            throw new InvalidOperationException("Game is not in a state that can be started.");

        //var playerCount = game.Players.Count;
        //if (playerCount < 5)
        //    throw new InvalidOperationException("Not enough players to start the game. Minimum is 5.");


        AssignRoles(game);

        game.Status = GameStatus.InProgress;
        game.UpdatedAtUtc = DateTime.UtcNow;
        game.RoundCounter = 1;
        game.LeaderSeat = 1;

        await _gameRepository.SaveChangesAsync();
    }

    public async Task<Round> CreateRoundAsync(int gameId, int roundNumber, int leaderSeat)
    {
        var round = new Round
        {
            GameId = gameId,
            RoundNumber = roundNumber,
            LeaderSeat = leaderSeat,
            TeamSize = 2, // TODO: Fixa sedan, bara för testning nu
            Status = RoundStatus.TeamSelection,
            Result = RoundResult.Unknown,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _roundRepository.AddRoundAsync(round);
        await _roundRepository.SaveChangesAsync();

        return round;
    }


    private void AssignRoles(Game game)
    {
        var players = game.Players.ToList();

        //var shuffled = players.OrderBy(x => _random.Next()).ToList();

        players[0].Role = PlayerRole.Shapeshifter;
        players[1].Role = PlayerRole.Human;
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
}
