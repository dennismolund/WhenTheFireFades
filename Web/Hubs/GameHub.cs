namespace Web.Hubs;

using Application.Interfaces;
using Application.Services;
using Domain.Entities;  
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Helpers;

public class GameHub(
    IGameRepository gameRepository,
    IGamePlayerRepository gamePlayerRepository,
    IRoundRepository roundRepository,
    ITeamProposalRepository teamProposalRepository,
    ITeamProposalVoteRepository teamProposalVoteRepository,
    IMissionVoteRepository missionVoteRepository,
    GameOrchestrator gameOrchestrator,
    SessionHelper sessionHelper) : Hub
{
    public async Task JoinGameLobby(string gameCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);

        var game = await gameRepository.GetByCodeWithPlayersAsync(gameCode);

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
        var tempUserId = sessionHelper.GetTempUserId();
        if (tempUserId == null)
        {
            return;
        }

        var game = await gameRepository.GetByCodeWithPlayersAsync(gameCode);
        if (game != null)
        {
            var player = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId.Value);
            if (player != null)
            {
                player.IsReady = isReady;
                player.UpdatedAtUtc = DateTime.UtcNow;
                await gamePlayerRepository.SaveChangesAsync();

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
        var tempUserId = sessionHelper.GetTempUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        Console.WriteLine($"Player {tempUserId} joined game {gameCode}");
    }

    public async Task VoteOnTeam(string gameCode, bool isApproved)
    {
        var game = await gameRepository.GetByCodeWithPlayersAndRoundsAsync(gameCode);
        if (game == null) return;

        var tempUserId = sessionHelper.GetTempUserId();
        if (tempUserId == null) return;

        var voter = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId.Value);
        if (voter == null) return;

        var round = game.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault();
        if (round == null || round.Status != RoundStatus.VoteOnTeam) return;

        var teamProposal = await teamProposalRepository.GetActiveByRoundIdAsync(round.RoundId);
        if (teamProposal == null) return;

        var existingVotes = await teamProposalVoteRepository.GetByTeamProposalAsync(teamProposal.TeamProposalId);
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

        await teamProposalVoteRepository.AddTeamProposalVoteAsync(teamProposalVote);
        await teamProposalVoteRepository.SaveChangesAsync();

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
            // Har marioteten röstat ja så går teamet igenom
            var approvalCount = existingVotes.Count(v => v.IsApproved) + 1; // +1 för att räkna med ledarens röst
            var rejectionCount = existingVotes.Count(v => !v.IsApproved);
            var voteIsApproved = approvalCount > rejectionCount;

            await teamProposalRepository.SaveChangesAsync();

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

            await gameRepository.SaveChangesAsync();

            await Clients.Group(gameCode).SendAsync("GameEnded", new
            {
                winner = "Shapeshifters",
                reason = "5 consecutive team proposals were rejected",
                gameResult = GameResult.Shapeshifter
            });

            return;
        }

        game.LeaderSeat = GetNewLeaderSet(game);
        game.UpdatedAtUtc = DateTime.UtcNow;

        round.Status = RoundStatus.TeamSelection;
        round.UpdatedAtUtc = DateTime.UtcNow;

        teamProposal.IsActive = false;

        await gameRepository.SaveChangesAsync();
        await roundRepository.SaveChangesAsync();
        await teamProposalRepository.SaveChangesAsync();

        var newLeader = game.Players.First(p => p.Seat == game.LeaderSeat);

        await Clients.Group(gameCode).SendAsync("NewLeaderSelected", new
        {
            newLeaderSeat = game.LeaderSeat,
            newLeaderNickname = newLeader.Nickname,
            attemptNumber = game.ConsecutiveRejectedProposals + 1,
            remainingAttempts = 5 - game.ConsecutiveRejectedProposals
        });
    }

    private int GetNewLeaderSet(Game game)
    {
        return (game.LeaderSeat == game.Players.Count) ? 1 : game.LeaderSeat + 1;
    }

    private async Task HandleTeamApproved(Game game, Round round, TeamProposal teamProposal, string gameCode)
    {
        game.ConsecutiveRejectedProposals = 0;

        round.Status = RoundStatus.SecretChoices;
        round.UpdatedAtUtc = DateTime.UtcNow;

        await gameRepository.SaveChangesAsync();
        await roundRepository.SaveChangesAsync();

        await Clients.Group(gameCode).SendAsync("MissionStarted", new
        {
            teamProposalId = teamProposal.TeamProposalId,
            roundNumber = round.RoundNumber,
        });

    }

    public async Task VoteOnMission(string gameCode, bool isSuccess)
    {
        var game = await gameRepository.GetByCodeWithPlayersAndRoundsAsync(gameCode);
        if (game == null) return;

        var tempUserId = sessionHelper.GetTempUserId();

        var voter = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);
        if (voter == null) return;

        var round = game.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault();
        if (round == null || round.Status != RoundStatus.SecretChoices) return;

        var teamProposal = await teamProposalRepository.GetByRoundIdAsync(round.RoundId);
        if (teamProposal == null) return;

        var existingVotes = await missionVoteRepository.GetByRoundIdAsync(round.RoundId);
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

        await missionVoteRepository.AddMissionVoteAsync(missionVote);
        await missionVoteRepository.SaveChangesAsync();

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
            await teamProposalRepository.SaveChangesAsync();

            if (voteIsSuccessful)
            {
                await HandleVoteSuccessful(game, round, gameCode);
            }
            else
            {
                await HandleVoteSabotaged(game, round, gameCode);
            }

            await Clients.Group(gameCode).SendAsync("MissionVoteResult", new
            {
                roundNumber = round.RoundNumber,
                successVotes,
                failVotes
            });

            await StartNextRound(gameCode);
        }
    }

    public async Task StartNextRound(string gameCode)
        {
        var game = await gameRepository.GetByCodeWithPlayersAndRoundsAsync(gameCode);
        if (game == null) return;
        game.RoundCounter++;
        game.LeaderSeat = GetNewLeaderSet(game);
        await gameOrchestrator.CreateRoundAsync(game, game.RoundCounter, game.LeaderSeat);
        await gamePlayerRepository.SaveChangesAsync();

        await Clients.Group(gameCode).SendAsync("StartNextRound", new
        {
            roundNumber = game.RoundCounter,
            leaderSeat = game.LeaderSeat
        });
    }

    private async Task HandleVoteSabotaged(Game game, Round round, string gameCode)
    {
        game.SabotageCount++;
        game.UpdatedAtUtc = DateTime.UtcNow;

        round.SabotageCounter++;
        round.Result = RoundResult.Sabotage;
        round.Status = RoundStatus.Completed;
        round.UpdatedAtUtc = DateTime.UtcNow;

        await gameRepository.SaveChangesAsync();
        await roundRepository.SaveChangesAsync();

        if(game.SabotageCount >= 3)
        {
            game.Status = GameStatus.Finished;
            game.GameWinner = GameResult.Shapeshifter;
            game.UpdatedAtUtc = DateTime.UtcNow;
            await gameRepository.SaveChangesAsync();
            await Clients.Group(gameCode).SendAsync("GameEnded", new
            {
                winner = "Shapeshifters",
                reason = "3 sabotaged missions",
                gameResult = GameResult.Shapeshifter
            });
        }
    }

    private async Task HandleVoteSuccessful(Game game, Round round, string gameCode)
    {
        
        game.SuccessCount++;
        game.UpdatedAtUtc = DateTime.UtcNow;

        round.Result = RoundResult.Success;
        round.Status = RoundStatus.Completed;
        round.UpdatedAtUtc = DateTime.UtcNow;

        await gameRepository.SaveChangesAsync();
        await roundRepository.SaveChangesAsync();

        if (game.SuccessCount >= 3)
        {
            game.Status = GameStatus.Finished;
            game.GameWinner = GameResult.Human;
            game.UpdatedAtUtc = DateTime.UtcNow;
            await gameRepository.SaveChangesAsync();
            await Clients.Group(gameCode).SendAsync("GameEnded", new
            {
                winner = "Resistance",
                reason = "3 successful missions",
                gameResult = GameResult.Human
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
