using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Models;
using WhenTheFireFades.Tests.TestHelpers;
using Xunit;

namespace WhenTheFireFades.Tests.Data;

public class GamePlayerRepositoryTests
{
    [Fact]
    public async Task GetNextAvailableSeatAsync_ReturnsFirstAvailableGap()
    {
        using var context = DbContextFactory.CreateContext();

        var game = new Game { ConnectionCode = "CODE1" };
        context.Games.Add(game);
        await context.SaveChangesAsync();

        context.GamePlayers.AddRange(
            CreatePlayer(game.GameId, 1),
            CreatePlayer(game.GameId, 2),
            CreatePlayer(game.GameId, 4));
        await context.SaveChangesAsync();

        var repository = new GamePlayerRepository(context);

        var seat = await repository.GetNextAvailableSeatAsync(game.GameId);

        seat.Should().Be(3);
    }

    [Fact]
    public async Task GetNextAvailableSeatAsync_ReusesSeatsThenAdvances()
    {
        using var context = DbContextFactory.CreateContext();

        var game = new Game { ConnectionCode = "CODE2" };
        context.Games.Add(game);
        await context.SaveChangesAsync();

        var players = Enumerable.Range(1, 3)
            .Select(seat => CreatePlayer(game.GameId, seat))
            .ToList();
        context.GamePlayers.AddRange(players);
        await context.SaveChangesAsync();

        var repository = new GamePlayerRepository(context);

        context.GamePlayers.Remove(players[1]); // seat 2 leaves
        await context.SaveChangesAsync();

        var seatForRejoin = await repository.GetNextAvailableSeatAsync(game.GameId);
        seatForRejoin.Should().Be(2);

        var returningPlayer = CreatePlayer(game.GameId, seatForRejoin);
        context.GamePlayers.Add(returningPlayer);
        await context.SaveChangesAsync();

        context.GamePlayers.Remove(players[2]); // seat 3 leaves
        await context.SaveChangesAsync();

        var nextSeat = await repository.GetNextAvailableSeatAsync(game.GameId);
        nextSeat.Should().Be(3);
    }

    private static GamePlayer CreatePlayer(int gameId, int seat)
    {
        return new GamePlayer
        {
            GameId = gameId,
            Seat = seat,
            Nickname = $"Player{seat}",
            Role = PlayerRole.Human,
            IsReady = false,
            IsConnected = true
        };
    }
}