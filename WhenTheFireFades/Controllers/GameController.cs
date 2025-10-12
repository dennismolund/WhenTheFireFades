using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WhenTheFireFades.Data;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Domain.Helpers;
using WhenTheFireFades.Domain.Services;
using WhenTheFireFades.Hubs;

namespace WhenTheFireFades.Controllers;

public class GameController(GameService gameService, IGameRepository gameRepository, IGamePlayerRepository gamePlayerRepository, SessionHelper sessionHelper, IHubContext<GameLobbyHub> hubContext) : Controller
{
    private readonly GameService _gameService = gameService;
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IGamePlayerRepository _gamePlayerRepository = gamePlayerRepository;
    private readonly SessionHelper _sessionHelper = sessionHelper;
    private readonly IHubContext<GameLobbyHub> _hubContext = hubContext;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create()
    {
        var tempUserId = _sessionHelper.GetOrCreateTempUserId();

        var game = await _gameService.CreateGameAsync();
        var gamePlayer = await _gameService.CreateGamePlayerAsync(game, tempUserId);

        _sessionHelper.SetCurrentGameCode(game.ConnectionCode);
        return RedirectToAction(nameof(Lobby), new { code = game.ConnectionCode });
    }
    
    [HttpGet]
    public async Task<IActionResult> Lobby(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Code is required.");

        code = code.Trim().ToUpperInvariant();

        var game = await _gameRepository.GetByCodeWithPlayersAsync(code);
        if (game == null)
        {
            return NotFound();
        }

        var tempUserId = _sessionHelper.GetOrCreateTempUserId();
        var existingPlayer = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);

        if (existingPlayer == null)
        {
            var gamePlayer = await _gameService.CreateGamePlayerAsync(game, tempUserId);

            game = await _gameRepository.GetByCodeWithPlayersAsync(code);
        }

        ViewBag.TempUserId = tempUserId;
        ViewBag.PlayerNickname = _sessionHelper.GetPlayerNickname();
        ViewBag.GameCode = code;

        return View(game);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LeaveGameLobby(string code)
    {
        var game = await _gameRepository.GetByCodeWithPlayersAsync(code);
        if (game == null)
        {
            return NotFound();
        }
        var tempUserId = _sessionHelper.GetTempUserId();
        if (tempUserId != null)
        {
            var player = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);
            if (player != null)
            {
                _gamePlayerRepository.RemovePlayer(player);
                await _gamePlayerRepository.SaveChangesAsync();

                game = await _gameRepository.GetByCodeWithPlayersAsync(code);

                await _hubContext.Clients.Group(code).SendAsync("PlayerLeft", new
                {
                    players = game.Players.Select(p => new
                    {
                        p.TempUserId,
                        p.Nickname,
                        p.Seat,
                        p.IsReady,
                        p.IsConnected
                    }).ToList(),
                    totalPlayers = game.Players.Count,
                    leftUserId = tempUserId
                });
            }
        }
        _sessionHelper.ClearCurrentGameCode();
        return RedirectToAction("Index", "Home");
    }
}
