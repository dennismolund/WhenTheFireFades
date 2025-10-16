namespace WhenTheFireFades.Hubs;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Domain.Helpers;
using WhenTheFireFades.Models;

public class GameHub(
    IGameRepository gameRepository,
    IGamePlayerRepository gamePlayerRepository,
    IRoundRepository roundRepository,
    ITeamProposalRepository teamProposalRepository,
    ITeamProposalVoteRepository teamProposalVoteRepository,
    SessionHelper sessionHelper) : Hub
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IGamePlayerRepository _gamePlayerRepository = gamePlayerRepository;
    private readonly IRoundRepository _roundRepository = roundRepository;
    private readonly ITeamProposalRepository _teamProposalRepository = teamProposalRepository;
    private readonly ITeamProposalVoteRepository _teamProposalVoteRepository = teamProposalVoteRepository;
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

    public async Task LeaveGameLobby(string gameCode)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameCode);
        var tempUserId = _sessionHelper.GetOrCreateTempUserId();
        var game = await _gameRepository.GetByCodeWithPlayersAsync(gameCode);
        if (game != null)
        {
            var player = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);
            if (player != null)
            {
                player.IsConnected = false;
                player.UpdatedAtUtc = DateTime.UtcNow;
                await _gamePlayerRepository.SaveChangesAsync();
                await Clients.Group(gameCode).SendAsync("PlayerLeft", new
                {
                    tempUserId = player.TempUserId,
                    nickname = player.Nickname,
                    totalPlayers = game.Players.Count
                });
            }
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

    public async Task JoinGame(string gameCode, int tempUserId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        Console.WriteLine($"Player {tempUserId} joined game {gameCode}");
    }

    public async Task VoteOnTeam(string gameCode, int voterTempUserId, bool isApproved)
    {
        var game = await _gameRepository.GetByCodeWithPlayersAndRoundsAsync(gameCode);
        if (game == null) return;
        var voter = game.Players.FirstOrDefault(p => p.TempUserId == voterTempUserId);
        if (voter == null) return;
        var round = game.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault();
        if (round == null || round.Status != RoundStatus.VoteOnTeam) return;

        var teamProposal = await _teamProposalRepository.GetByRoundIdAsync(round.RoundId);
        if (teamProposal == null) return;

        var teamProposalVote = new TeamProposalVote()
        {
            TeamProposalId = teamProposal.TeamProposalId,
            Seat = voter.Seat,
            IsApproved = isApproved,
            CreatedAtUtc = DateTime.UtcNow,
            TeamProposal = teamProposal
        };

        await _teamProposalVoteRepository.AddTeamProposalVoteAsync(teamProposalVote);
        await _teamProposalVoteRepository.SaveChangesAsync();

        var votes = await _teamProposalVoteRepository.GetByTeamProposalAsync(teamProposal.TeamProposalId);
        var voteCount = votes.Count;

        //Ledaren behöver inte rösta.
        if (voteCount >= game.Players.Count - 1)
        {
            // Har någon röstat nej så går teamet inte igenom.
            var voteIsApproved = !votes.Any(v => !v.IsApproved);

            await Clients.Group(gameCode).SendAsync("TeamVoteResult", new
            {
                teamProposalId = teamProposal.TeamProposalId,
                rejectionCount = votes.Count(v => !v.IsApproved),
                approvalCount = votes.Count(v => v.IsApproved),
                voteIsApproved
            });
        }
    }

    public async Task GameStarted(string gameCode)
    {
        await Clients.Group(gameCode).SendAsync("GameStarted");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
