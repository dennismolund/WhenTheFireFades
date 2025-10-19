using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Moq;
using WhenTheFireFades.Data;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Domain.Helpers;
using WhenTheFireFades.Hubs;
using WhenTheFireFades.Models;
using WhenTheFireFades.Tests.TestHelpers;
using Xunit;

namespace WhenTheFireFades.Tests.Hubs;

public class GameHubTests
{
    [Fact]
    public async Task UpdateReadyStatus_UsesExistingSessionAndPersists()
    {
        using var context = DbContextFactory.CreateContext();
        var (game, player) = SeedGameWithPlayers(context, 2);

        var (hub, sessionHelper, _, groupProxy) = CreateHub(context);
        sessionHelper.SetTempUserId(player.TempUserId!.Value);

        object?[]? payload = null;
        groupProxy
            .Setup(proxy => proxy.SendCoreAsync(
                "PlayerReadyChanged",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, object?[], CancellationToken>((_, args, _) => payload = args)
            .Returns(Task.CompletedTask);

        await hub.UpdateReadyStatus(game.ConnectionCode, true);

        var reloadedPlayer = context.GamePlayers.Single(p => p.GamePlayerId == player.GamePlayerId);
        reloadedPlayer.IsReady.Should().BeTrue();

        payload.Should().NotBeNull();
        dynamic message = payload![0]!;
        ((int)message.tempUserId).Should().Be(player.TempUserId);
        ((bool)message.isReady).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateReadyStatus_IgnoresAnonymousConnections()
    {
        using var context = DbContextFactory.CreateContext();
        var (game, player) = SeedGameWithPlayers(context, 2);

        var (hub, _, _, groupProxy) = CreateHub(context);

        await hub.UpdateReadyStatus(game.ConnectionCode, true);

        var storedPlayer = context.GamePlayers.Single(p => p.GamePlayerId == player.GamePlayerId);
        storedPlayer.IsReady.Should().BeFalse();

        groupProxy.Verify(proxy => proxy.SendCoreAsync(
                "PlayerReadyChanged",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LeaveGameLobby_IgnoresAnonymousConnections()
    {
        using var context = DbContextFactory.CreateContext();
        var (game, player) = SeedGameWithPlayers(context, 2);

        var (hub, _, _, groupProxy) = CreateHub(context);

        await hub.LeaveGameLobby(game.ConnectionCode);

        var storedPlayer = context.GamePlayers.Single(p => p.GamePlayerId == player.GamePlayerId);
        storedPlayer.IsConnected.Should().BeTrue();

        groupProxy.Verify(proxy => proxy.SendCoreAsync(
                "PlayerLeft",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task VoteOnTeam_PreventsDuplicateVotes()
    {
        using var context = DbContextFactory.CreateContext();
        var (game, player) = SeedGameWithPlayers(context, 5, includeRound: true);
        var round = context.Rounds.Single();

        var teamProposal = new TeamProposal
        {
            RoundId = round.RoundId,
            AttemptNumber = 1,
            IsActive = true,
            Round = round
        };
        context.TeamProposals.Add(teamProposal);
        await context.SaveChangesAsync();

        var (hub, sessionHelper, _, groupProxy) = CreateHub(context);
        sessionHelper.SetTempUserId(player.TempUserId!.Value);

        await hub.VoteOnTeam(game.ConnectionCode, player.TempUserId!.Value, true);
        await hub.VoteOnTeam(game.ConnectionCode, player.TempUserId!.Value, false);

        context.TeamProposalVotes.Count().Should().Be(1);

        groupProxy.Verify(proxy => proxy.SendCoreAsync(
                "TeamVoteResult",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task VoteOnTeam_BroadcastsWhenThresholdReached()
    {
        using var context = DbContextFactory.CreateContext();
        var (game, _) = SeedGameWithPlayers(context, 5, includeRound: true);
        var round = context.Rounds.Single();

        var teamProposal = new TeamProposal
        {
            RoundId = round.RoundId,
            AttemptNumber = 1,
            IsActive = true,
            Round = round
        };
        context.TeamProposals.Add(teamProposal);
        await context.SaveChangesAsync();

        var (hub, _, _, groupProxy) = CreateHub(context);

        object?[]? payload = null;
        groupProxy
            .Setup(proxy => proxy.SendCoreAsync(
                "TeamVoteResult",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, object?[], CancellationToken>((_, args, _) => payload = args)
            .Returns(Task.CompletedTask);

        var voters = game.Players.Where(p => p.Seat != game.LeaderSeat).ToList();
        foreach (var voter in voters)
        {
            await hub.VoteOnTeam(game.ConnectionCode, voter.TempUserId!.Value, true);
        }

        context.TeamProposalVotes.Count().Should().Be(voters.Count);
        payload.Should().NotBeNull();
        dynamic message = payload![0]!;
        ((int)message.approvalCount).Should().Be(voters.Count);
        ((bool)message.voteIsApproved).Should().BeTrue();
    }

    private static (Game game, GamePlayer leader) SeedGameWithPlayers(ApplicationDbContext context, int playerCount, bool includeRound = false)
    {
        var game = new Game
        {
            ConnectionCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant(),
            LeaderSeat = 1,
            Status = GameStatus.InProgress
        };

        var players = Enumerable.Range(1, playerCount)
            .Select(seat => new GamePlayer
            {
                Seat = seat,
                Nickname = $"Player{seat}",
                Role = PlayerRole.Human,
                IsConnected = true,
                IsReady = false,
                TempUserId = 1000 + seat,
                Game = game
            })
            .ToList();

        game.Players = players;

        if (includeRound)
        {
            var round = new Round
            {
                Game = game,
                RoundNumber = 1,
                LeaderSeat = game.LeaderSeat,
                Status = RoundStatus.VoteOnTeam,
                TeamSize = 2
            };
            game.Rounds.Add(round);
        }

        context.Games.Add(game);
        context.GamePlayers.AddRange(players);
        context.SaveChanges();

        return (game, players.First());
    }

    private static (GameHub hub, SessionHelper sessionHelper, TestSession session, Mock<IClientProxy> groupProxy) CreateHub(ApplicationDbContext context)
    {
        var gameRepository = new GameRepository(context);
        var playerRepository = new GamePlayerRepository(context);
        var roundRepository = new RoundRepository(context);
        var teamProposalRepository = new TeamProposalRepository(context);
        var voteRepository = new TeamProposalVoteRepository(context);

        var httpContext = new DefaultHttpContext();
        var session = new TestSession();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var sessionHelper = new SessionHelper(accessor);

        var hub = new GameHub(
            gameRepository,
            playerRepository,
            roundRepository,
            teamProposalRepository,
            voteRepository,
            sessionHelper);

        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.ConnectionId).Returns(Guid.NewGuid().ToString());
        hub.Context = mockContext.Object;

        var groupProxy = new Mock<IClientProxy>();
        var clientsMock = new Mock<IHubCallerClients>();
        clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(groupProxy.Object);
        hub.Clients = clientsMock.Object;

        var groupManager = new Mock<IGroupManager>();
        hub.Groups = groupManager.Object;

        return (hub, sessionHelper, session, groupProxy);
    }
}