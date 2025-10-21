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
    private readonly CreateGameFeature _createGameFeature = createGameFeature;
    private readonly CreateGamePlayerFeature _createGamePlayerFeature = createGamePlayerFeature;
    private readonly StartGameFeature _startGameFeature = startGameFeature;
    private readonly CreateRoundFeature _createRoundFeature = createRoundFeature;

    public async Task<Game> CreateGameAsync()
    {
        return await _createGameFeature.ExecuteAsync();
    }

    public async Task<GamePlayer> CreateGamePlayerAsync(Game game, int creatorTempUserId, string? creatorUsername = null)
    {
        return await _createGamePlayerFeature.ExecuteAsync(game, creatorTempUserId, creatorUsername);
    }


    public async Task<Round> StartGameAsync(Game game)
    {

        return await _startGameFeature.ExecuteAsync(game);

    }

    public async Task<Round> CreateRoundAsync(Game game, int roundNumber, int leaderSeat)
    {
        return await _createRoundFeature.ExecuteAsync(game, roundNumber, leaderSeat);
    }

    //TODO: Need to handle max players
    //public async Task<int> NextAvailableSeat(int gameId)
    //{
    //    return await _gamePlayerRepository.GetNextAvailableSeatAsync(gameId);
    //}



    
}
