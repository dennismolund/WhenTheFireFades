using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Games;
public class CreateGameFeature(IGameRepository gameRepository)
{
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<Game> ExecuteAsync()
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

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
