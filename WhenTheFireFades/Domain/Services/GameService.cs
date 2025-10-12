using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using WhenTheFireFades.Data;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Hubs;
using WhenTheFireFades.Models;

namespace WhenTheFireFades.Domain.Services;

public sealed class GameService(IGameRepository gameRepository, IGamePlayerRepository gamePlayerRepository)
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IGamePlayerRepository _gamePlayerRepository = gamePlayerRepository;

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
