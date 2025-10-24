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
    public async Task<GamePlayer> ExecuteAsync(Game game, int creatorTempUserId, string? creatorUsername = null, string? userId = null)
    {
        var nextSeat = await gamePlayerRepository.GetNextAvailableSeatAsync(game.GameId);

        var player = new GamePlayer
        {
            GameId = game.GameId,
            TempUserId = creatorTempUserId,
            UserId = userId,
            Nickname = creatorUsername ?? $"Player#{creatorTempUserId}",
            Seat = nextSeat,
            Role = PlayerRole.Human,
            IsReady = false,
            IsConnected = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        await gamePlayerRepository.AddPlayerAsync(player);
        await gamePlayerRepository.SaveChangesAsync();

        return player;
    }
}
