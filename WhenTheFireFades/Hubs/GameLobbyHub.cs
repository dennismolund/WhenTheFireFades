namespace WhenTheFireFades.Hubs;
using Microsoft.AspNetCore.SignalR;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Domain.Helpers;

public class GameLobbyHub(
    IGameRepository gameRepository,
    IGamePlayerRepository gamePlayerRepository,
    SessionHelper sessionHelper) : Hub
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IGamePlayerRepository _gamePlayerRepository = gamePlayerRepository;
    private readonly SessionHelper _sessionHelper = sessionHelper;

    public async Task JoinGameLobby(string gameCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);

        var game = await _gameRepository.GetByCodeWithPlayersAsync(gameCode);

        if (game != null)
        {
            await Clients.Group(gameCode).SendAsync("PlayerJoined", new
            {
                players = game.Players.Select(p => new
                {
                    p.TempUserId,
                    p.Nickname,
                    p.Seat,
                    p.IsReady,
                    p.IsConnected
                }).ToList(),
                totalPlayers = game.Players.Count
            });
        }
    }

    public async Task UpdateReadyStatus(string gameCode, bool isReady)
    {
        var tempUserId = _sessionHelper.GetOrCreateTempUserId();
        var game = await _gameRepository.GetByCodeWithPlayersAsync(gameCode);

        if (game != null)
        {
            var player = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);
            if (player != null)
            {
                player.IsReady = isReady;
                player.UpdatedAtUtc = DateTime.UtcNow;
                await _gamePlayerRepository.SaveChangesAsync();

                await Clients.Group(gameCode).SendAsync("PlayerReadyChanged", new
                {
                    tempUserId = player.TempUserId,
                    nickname = player.Nickname,
                    isReady = player.IsReady,
                    allPlayersReady = game.Players.All(p => p.IsReady)
                });
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
