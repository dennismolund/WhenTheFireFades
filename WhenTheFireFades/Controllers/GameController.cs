using Microsoft.AspNetCore.Mvc;
using WhenTheFireFades.Domain.Services;
using WhenTheFireFades.Data;
using Microsoft.EntityFrameworkCore;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Domain.Helpers;

namespace WhenTheFireFades.Controllers;

public class GameController(GameService gameService, IGameRepository gameRepository, IGamePlayerRepository gamePlayerRepository, SessionHelper sessionHelper) : Controller
{
    private readonly GameService _gameService = gameService;
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IGamePlayerRepository _gamePlayerRepository = gamePlayerRepository;
    private readonly SessionHelper _sessionHelper = sessionHelper;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create()
    {
        var tempUserId = _sessionHelper.GetOrCreateTempUserId();

        var game = await _gameService.CreateGameAsync();
        var gamePlayer = await _gameService.CreateGamePlayerAsync(game.GameId, tempUserId);

        _sessionHelper.SetCurrentGameCode(game.ConnectionCode);
        return RedirectToAction(nameof(Lobby), new { code = game.ConnectionCode });
    }

    [HttpGet("{code}/lobby")]
    public async Task<IActionResult> Lobby(string code)
    {
        var game = await _gameRepository.GetByCodeAsync(code);
        if (game == null)
        {
            return NotFound();
        }

        var tempUserId = _sessionHelper.GetOrCreateTempUserId();
        var existingPlayer = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);

        if (existingPlayer == null)
        {
            var gamePlayer = await _gameService.CreateGamePlayerAsync(game.GameId, tempUserId);

            //TODO: update player list fix this later, seems suboptimal with two database calls
            game = await _gameRepository.GetByCodeAsync(code);
        }

        ViewBag.UserId = tempUserId;
        ViewBag.Nickname = _sessionHelper.GetPlayerNickname();

        return View(game);
    }
}
