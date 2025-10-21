using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.GamePlayers;
public sealed class CreateGamePlayerFeature(IGamePlayerRepository gamePlayerRepository)
{
    private readonly IGamePlayerRepository _gamePlayerRepository = gamePlayerRepository;

    public async Task<GamePlayer> ExecuteAsync(Game game, int creatorTempUserId, string? creatorUsername = null)
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
}
