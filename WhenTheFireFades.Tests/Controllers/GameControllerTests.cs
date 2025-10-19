using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhenTheFireFades.Controllers;
using WhenTheFireFades.Data;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Domain.Helpers;
using WhenTheFireFades.Domain.Services;
using WhenTheFireFades.Hubs;
using WhenTheFireFades.Models;
using WhenTheFireFades.Tests.TestHelpers;
using Xunit;

namespace WhenTheFireFades.Tests.Controllers;

public class GameControllerTests
{
    [Fact]
    public async Task ProposeTeam_PersistsMembersAndTransitionsRound()
    {
        using var context = DbContextFactory.CreateContext();

        var game = new Game
        {
            ConnectionCode = "TEAM01",
            LeaderSeat = 1,
            Status = GameStatus.InProgress
        };

        var leader = new GamePlayer
        {
            Game = game,
            Seat = 1,
            Nickname = "Leader",
            TempUserId = 2001,
            Role = PlayerRole.Human,
            IsConnected = true,
            IsReady = true
        };
        var teammate = new GamePlayer
        {
            Game = game,
            Seat = 2,
            Nickname = "Ally",
            TempUserId = 2002,
            Role = PlayerRole.Human,
            IsConnected = true,
            IsReady = true
        };
        var round = new Round
        {
            Game = game,
            RoundNumber = 1,
            LeaderSeat = game.LeaderSeat,
            Status = RoundStatus.TeamSelection,
            TeamSize = 2,
        };
        game.Players = new List<GamePlayer> { leader, teammate };
        game.Rounds = new List<Round> { round };

        context.Games.Add(game);
        context.GamePlayers.AddRange(leader, teammate);
        context.Rounds.Add(round);
        await context.SaveChangesAsync();

        var gameRepository = new GameRepository(context);
        var playerRepository = new GamePlayerRepository(context);
        var roundRepository = new RoundRepository(context);
        var teamProposalRepository = new TeamProposalRepository(context);
        var teamProposalVoteRepository = new TeamProposalVoteRepository(context);
        var gameService = new GameService(gameRepository, playerRepository, roundRepository);

        var httpContext = new DefaultHttpContext();
        var session = new TestSession();
        httpContext.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var sessionHelper = new SessionHelper(accessor);
        sessionHelper.SetTempUserId(leader.TempUserId!.Value);

        var hubClients = new Mock<IHubClients>();
        var clientProxy = new Mock<IClientProxy>();
        hubClients.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxy.Object);
        var hubContext = new Mock<IHubContext<GameHub>>();
        hubContext.SetupGet(c => c.Clients).Returns(hubClients.Object);

        var controller = new GameController(
            gameService,
            gameRepository,
            playerRepository,
            roundRepository,
            teamProposalRepository,
            teamProposalVoteRepository,
            sessionHelper,
            hubContext.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };

        var result = await controller.ProposeTeam(game.ConnectionCode, new List<int> { 1, 2 });

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(GameController.Play));

        var proposal = context.TeamProposals.Single();
        proposal.AttemptNumber.Should().Be(1);
        proposal.Members.Should().HaveCount(2);
        proposal.Members.Select(m => m.Seat).Should().BeEquivalentTo(new[] { 1, 2 });

        var updatedRound = context.Rounds.Single();
        updatedRound.Status.Should().Be(RoundStatus.VoteOnTeam);
        updatedRound.TeamProposals.Should().HaveCount(1);

        clientProxy.Verify(proxy => proxy.SendCoreAsync(
                "TeamProposed",
                It.IsAny<object?[]>(),
                It.IsAny<System.Threading.CancellationToken>()),
            Times.Once);
    }
}