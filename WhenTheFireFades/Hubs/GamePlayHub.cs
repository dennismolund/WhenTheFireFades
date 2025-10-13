using Microsoft.AspNetCore.SignalR;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Domain.Helpers;

namespace WhenTheFireFades.Hubs;

public class GamePlayHub(
    IGameRepository gameRepository,
    IGamePlayerRepository gamePlayerRepository,
    SessionHelper sessionHelper) : Hub
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IGamePlayerRepository _gamePlayerRepository = gamePlayerRepository;
    private readonly SessionHelper _sessionHelper = sessionHelper;

    public async Task JoinGame(string gameCode, int tempUserId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        Console.WriteLine($"Player {tempUserId} joined game {gameCode}");
    }

    public async Task ProposeTeam(string gameCode, int leaderTempUserId, List<int> teamMemberSeats)
    {
        var game = await _gameRepository.GetByCodeWithPlayersAsync(gameCode);
        if (game == null) return;

        // Verify the person proposing is the leader
        var leader = game.Players.FirstOrDefault(p => p.Seat == game.LeaderSeat);
        if (leader == null || leader.TempUserId != leaderTempUserId) return;

        // Get the proposed team members
        var teamMembers = game.Players
            .Where(p => teamMemberSeats.Contains(p.Seat))
            .Select(p => new
            {
                p.TempUserId,
                p.Nickname,
                p.Seat
            })
            .ToList();

        // Broadcast to all players
        await Clients.Group(gameCode).SendAsync("TeamProposed", new
        {
            leaderSeat = game.LeaderSeat,
            leaderNickname = leader.Nickname,
            teamMembers,
            teamSize = teamMembers.Count
        });
    }

}
