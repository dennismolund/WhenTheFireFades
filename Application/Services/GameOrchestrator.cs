using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Globalization;
using Application.Features.Games;
using Application.Features.GamePlayers;
using Application.Features.Rounds;


namespace Application.Services;

public sealed class GameOrchestrator(
    IGameRepository gameRepository, 
    IGamePlayerRepository gamePlayerRepository,
    IRoundRepository roundRepository,
    CreateGameFeature createGameFeature,
    CreateGamePlayerFeature createGamePlayerFeature,
    StartGameFeature startGameFeature,
    CreateRoundFeature createRoundFeature
    )
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IGamePlayerRepository _gamePlayerRepository = gamePlayerRepository;
    private readonly IRoundRepository _roundRepository = roundRepository;

    public async Task<Game> CreateGameAsync()
    {
        return await createGameFeature.ExecuteAsync();
    }

    public async Task<GamePlayer> CreateGamePlayerAsync(Game game, int creatorTempUserId, string? creatorUsername = null, string? userId = null)
    {
        return await createGamePlayerFeature.ExecuteAsync(game, creatorTempUserId, creatorUsername, userId);
    }

    public async Task<Round> StartGameAsync(Game game)
    {
        return await startGameFeature.ExecuteAsync(game);
    }

    public async Task<Round> CreateRoundAsync(Game game, int roundNumber, int leaderSeat)
    {
        return await createRoundFeature.ExecuteAsync(game, roundNumber, leaderSeat);
    }
}
