using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Domain.Services;
using WhenTheFireFades.Models;
using WhenTheFireFades.Tests.TestHelpers;
using Xunit;

namespace WhenTheFireFades.Tests.Domain;

public class GameServiceTests
{
    [Fact]
    public async Task StartGameAsync_ThrowsWhenNotEnoughPlayers()
    {
        using var context = DbContextFactory.CreateContext();
        var game = new Game
        {
            ConnectionCode = "ABCD",
            Status = GameStatus.Lobby,
            LeaderSeat = 1,
            Players = new List<GamePlayer>
            {
                CreatePlayer(1),
                CreatePlayer(2),
                CreatePlayer(3),
                CreatePlayer(4)
            }
        };

        context.Games.Add(game);
        await context.SaveChangesAsync();

        var gameRepository = new GameRepository(context);
        var playerRepository = new GamePlayerRepository(context);
        var roundRepository = new RoundRepository(context);
        var service = new GameService(gameRepository, playerRepository, roundRepository);

        var fetchedGame = await gameRepository.GetByIdWithPlayersAsync(game.GameId);

        Func<Task> act = async () => await service.StartGameAsync(fetchedGame!);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Not enough players to start the game. Minimum is *.");
    }

    [Fact]
    public async Task StartGameAsync_AssignsRolesAndCreatesRound()
    {
        using var context = DbContextFactory.CreateContext();
        var game = new Game
        {
            ConnectionCode = "ABCDE",
            Status = GameStatus.Lobby,
            LeaderSeat = 1,
            Players = Enumerable.Range(1, 5)
                .Select(CreatePlayer)
                .ToList()
        };

        context.Games.Add(game);
        await context.SaveChangesAsync();

        var gameRepository = new GameRepository(context);
        var playerRepository = new GamePlayerRepository(context);
        var roundRepository = new RoundRepository(context);
        var service = new GameService(gameRepository, playerRepository, roundRepository);

        var fetchedGame = await gameRepository.GetByIdWithPlayersAsync(game.GameId);

        var round = await service.StartGameAsync(fetchedGame!);

        fetchedGame!.Status.Should().Be(GameStatus.InProgress);
        fetchedGame.RoundCounter.Should().Be(1);
        fetchedGame.LeaderSeat.Should().Be(1);

        fetchedGame.Players.Should().AllSatisfy(p =>
            p.Role.Should().BeOneOf(PlayerRole.Human, PlayerRole.Shapeshifter));

        fetchedGame.Players.Count(p => p.Role == PlayerRole.Shapeshifter)
            .Should().Be(2);

        var savedRound = context.Rounds.Single();
        savedRound.RoundNumber.Should().Be(1);
        savedRound.TeamSize.Should().Be(2);
        savedRound.Status.Should().Be(RoundStatus.TeamSelection);
        savedRound.TeamProposals.Should().BeEmpty();
        round.RoundId.Should().Be(savedRound.RoundId);
    }

    private static GamePlayer CreatePlayer(int seat)
    {
        return new GamePlayer
        {
            Seat = seat,
            Nickname = $"Player{seat}",
            Role = PlayerRole.Human,
            IsReady = false,
            IsConnected = true
        };
    }
}