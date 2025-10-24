using Domain.Entities;
using Application.Features.Games;
using Application.Features.GamePlayers;
using Application.Features.Rounds;


namespace Application.Services;

public sealed class GameOrchestrator(
    CreateGameFeature createGameFeature,
    CreateGamePlayerFeature createGamePlayerFeature,
    StartGameFeature startGameFeature,
    CreateRoundFeature createRoundFeature
    )
{
    public async Task<Game> CreateGameAsync()
    {
        return await createGameFeature.ExecuteAsync();
    }

    public async Task CreateGamePlayerAsync(Game game, int creatorTempUserId, string? creatorUsername = null, string? userId = null)
    {
        await createGamePlayerFeature.ExecuteAsync(game, creatorTempUserId, creatorUsername, userId);
    }

    public async Task StartGameAsync(Game game)
    {
        await startGameFeature.ExecuteAsync(game);
    }

    public async Task CreateRoundAsync(Game game, int roundNumber, int leaderSeat)
    {
        await createRoundFeature.ExecuteAsync(game, roundNumber, leaderSeat);
    }
}
