namespace WhenTheFireFades.Hubs;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Domain.Helpers;
using WhenTheFireFades.Models;

public class GameHub(
    IGameRepository gameRepository,
    IGamePlayerRepository gamePlayerRepository,
    IRoundRepository roundRepository,
    ITeamProposalRepository teamProposalRepository,
    ITeamProposalVoteRepository teamProposalVoteRepository,
    IMissionVoteRepository missionVoteRepository,
    SessionHelper sessionHelper) : Hub
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IGamePlayerRepository _gamePlayerRepository = gamePlayerRepository;
    private readonly IRoundRepository _roundRepository = roundRepository;
    private readonly ITeamProposalRepository _teamProposalRepository = teamProposalRepository;
    private readonly ITeamProposalVoteRepository _teamProposalVoteRepository = teamProposalVoteRepository;
    private readonly IMissionVoteRepository _missionVoteRepository = missionVoteRepository;
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
        var tempUserId = _sessionHelper.GetTempUserId();
        if (tempUserId == null)
        {
            return;
        }
        var game = await _gameRepository.GetByCodeWithPlayersAsync(gameCode);
        if (game != null)
        {
            var player = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId.Value);
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
        var tempUserId = _sessionHelper.GetTempUserId();
        if (tempUserId == null)
        {
            return;
        }

        var game = await _gameRepository.GetByCodeWithPlayersAsync(gameCode);
        if (game != null)
        {
            var player = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId.Value);
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

    public async Task JoinGame(string gameCode)
    {
        var tempUserId = _sessionHelper.GetTempUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        Console.WriteLine($"Player {tempUserId} joined game {gameCode}");
    }

    public async Task VoteOnTeam(string gameCode, bool isApproved)
    {
        var game = await _gameRepository.GetByCodeWithPlayersAndRoundsAsync(gameCode);
        if (game == null) return;

        var tempUserId = _sessionHelper.GetTempUserId();
        if (tempUserId == null) return;

        var voter = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId.Value);
        if (voter == null) return;

        var round = game.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault();
        if (round == null || round.Status != RoundStatus.VoteOnTeam) return;

        var teamProposal = await _teamProposalRepository.GetActiveByRoundIdAsync(round.RoundId);
        if (teamProposal == null) return;

        var existingVotes = await _teamProposalVoteRepository.GetByTeamProposalAsync(teamProposal.TeamProposalId);
        if (existingVotes.Any(v => v.Seat == voter.Seat))
        {
            return;
        }

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

        await Clients.Group(gameCode).SendAsync("PlayerVoted", new
        {
            playerSeat = voter.Seat,
            playerNickname = voter.Nickname,
            isApproved
        });

        existingVotes.Add(teamProposalVote);
        var requiredVotes = game.Players.Count - 1; // Alla utom ledaren ska rösta.

        if (existingVotes.Count >= requiredVotes)
        {
            // Har någon röstat nej så går teamet inte igenom.
            var approvalCount = existingVotes.Count(v => v.IsApproved);
            var rejectionCount = existingVotes.Count(v => !v.IsApproved);
            var voteIsApproved = rejectionCount == 0;

            await _teamProposalRepository.SaveChangesAsync();

            await Clients.Group(gameCode).SendAsync("TeamVoteResult", new
            {
                teamProposalId = teamProposal.TeamProposalId,
                rejectionCount,
                approvalCount,
                voteIsApproved,
                attemptNumber = teamProposal.AttemptNumber
            });

            if (voteIsApproved)
            {
                await HandleTeamApproved(game, round, teamProposal, gameCode);
            }
            else
            {
                await HandleTeamRejected(game, round, teamProposal, gameCode);
            }
        }
    }

    private async Task HandleTeamRejected(Game game, Round round, TeamProposal teamProposal, string gameCode)
    {
        game.ConsecutiveRejectedProposals++;

        if (game.ConsecutiveRejectedProposals >= 5)
        {
            game.Status = GameStatus.Finished;
            game.GameWinner = GameResult.Shapeshifter;
            game.UpdatedAtUtc = DateTime.UtcNow;

            await _gameRepository.SaveChangesAsync();

            await Clients.Group(gameCode).SendAsync("GameEnded", new
            {
                winner = "Shapeshifters",
                reason = "5 consecutive team proposals were rejected",
                gameResult = GameResult.Shapeshifter
            });

            return;
        }

        game.LeaderSeat = (game.LeaderSeat == game.Players.Count) ? 1 : game.LeaderSeat + 1;
        game.UpdatedAtUtc = DateTime.UtcNow;

        round.Status = RoundStatus.TeamSelection;
        round.UpdatedAtUtc = DateTime.UtcNow;

        teamProposal.IsActive = false;

        await _gameRepository.SaveChangesAsync();
        await _roundRepository.SaveChangesAsync();
        await _teamProposalRepository.SaveChangesAsync();

        var newLeader = game.Players.First(p => p.Seat == game.LeaderSeat);

        await Clients.Group(gameCode).SendAsync("NewLeaderSelected", new
        {
            newLeaderSeat = game.LeaderSeat,
            newLeaderNickname = newLeader.Nickname,
            attemptNumber = game.ConsecutiveRejectedProposals + 1,
            remainingAttempts = 5 - game.ConsecutiveRejectedProposals
        });
    }


    private async Task HandleTeamApproved(Game game, Round round, TeamProposal teamProposal, string gameCode)
    {
        game.ConsecutiveRejectedProposals = 0;

        round.Status = RoundStatus.SecretChoices;
        round.UpdatedAtUtc = DateTime.UtcNow;

        await _gameRepository.SaveChangesAsync();
        await _roundRepository.SaveChangesAsync();

        await Clients.Group(gameCode).SendAsync("MissionStarted", new
        {
            teamProposalId = teamProposal.TeamProposalId,
            roundNumber = round.RoundNumber,
        });

    }

    public async Task VoteOnMission(string gameCode, bool isSuccess)
    {
        var game = await _gameRepository.GetByCodeWithPlayersAndRoundsAsync(gameCode);
        if (game == null) return;

        var tempUserId = _sessionHelper.GetTempUserId();

        var voter = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);
        if (voter == null) return;

        var round = game.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault();
        if (round == null || round.Status != RoundStatus.SecretChoices) return;

        var teamProposal = await _teamProposalRepository.GetByRoundIdAsync(round.RoundId);
        if (teamProposal == null) return;

        var existingVotes = await _missionVoteRepository.GetByRoundIdAsync(round.RoundId);
        if (existingVotes.Any(v => v.Seat == voter.Seat))
        {
            return;
        }

        var missionVote = new MissionVote()
        {
            RoundId = round.RoundId,
            Seat = voter.Seat,
            IsSuccess = isSuccess,
            CreatedAtUtc = DateTime.UtcNow,
            Round = round
        };

        await _missionVoteRepository.AddMissionVoteAsync(missionVote);
        await _missionVoteRepository.SaveChangesAsync();

        await Clients.Group(gameCode).SendAsync("MissionVoteSubmitted", new
        {
            playerSeat = voter.Seat,
            playerNickname = voter.Nickname
        });

        existingVotes.Add(missionVote);
        var requiredVotes = teamProposal.Members.Count;

        
        if (existingVotes.Count >= requiredVotes)
        {
            int successVotes = existingVotes.Count(v => v.IsSuccess);
            int failVotes = existingVotes.Count(v => !v.IsSuccess);
            var voteIsSuccessful = failVotes == 0;


            //Teamet har röstat klart så vi sätter teamet som inaktivt
            teamProposal.IsActive = false;
            await _teamProposalRepository.SaveChangesAsync();

            await Clients.Group(gameCode).SendAsync("MissionVoteResult", new
            {
                roundNumber = round.RoundNumber,
                successVotes,
                failVotes
            });

            if (voteIsSuccessful)
            {
                await HandleVoteSuccessful(game, round, gameCode);
            }
            else
            {
                await HandleVoteSabotaged(game, round, gameCode);
            }
        }

    }

    private async Task HandleVoteSabotaged(Game game, Round round, string gameCode)
    {
        game.SabotageCount++;
        game.UpdatedAtUtc = DateTime.UtcNow;

        round.SabotageCounter++;
        round.Result = RoundResult.Sabotage;
        round.Status = RoundStatus.Completed;
        round.UpdatedAtUtc = DateTime.UtcNow;

        await _gameRepository.SaveChangesAsync();
        await _roundRepository.SaveChangesAsync();

        if(game.SabotageCount >= 3)
        {
            game.Status = GameStatus.Finished;
            game.GameWinner = GameResult.Shapeshifter;
            game.UpdatedAtUtc = DateTime.UtcNow;
            await _gameRepository.SaveChangesAsync();
            await Clients.Group(gameCode).SendAsync("GameEnded", new
            {
                winner = "Shapeshifters",
                reason = "3 sabotaged missions",
                gameResult = GameResult.Shapeshifter
            });
            return;
        }

    }

    private async Task HandleVoteSuccessful(Game game, Round round, string gameCode)
    {
        
        game.SuccessCount++;
        game.UpdatedAtUtc = DateTime.UtcNow;

        round.Result = RoundResult.Success;
        round.Status = RoundStatus.Completed;
        round.UpdatedAtUtc = DateTime.UtcNow;

        await _gameRepository.SaveChangesAsync();
        await _roundRepository.SaveChangesAsync();

        if (game.SuccessCount >= 3)
        {
            game.Status = GameStatus.Finished;
            game.GameWinner = GameResult.Human;
            game.UpdatedAtUtc = DateTime.UtcNow;
            await _gameRepository.SaveChangesAsync();
            await Clients.Group(gameCode).SendAsync("GameEnded", new
            {
                winner = "Resistance",
                reason = "3 successful missions",
                gameResult = GameResult.Human
            });
            return;
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
