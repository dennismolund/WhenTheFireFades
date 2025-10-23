using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;

namespace Web.Controllers;

public class HomeController(
    SessionHelper sessionHelper, 
    IGameRepository gameRepository) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.UserId = sessionHelper.GetOrCreateTempUserId();
        ViewBag.Nickname = sessionHelper.GetPlayerNickname();

        var gameCode = sessionHelper.GetCurrentGameCode();
        if (gameCode == null) return View();
        var game = await gameRepository.GetByCodeAsync(gameCode);
        if (game != null)
            return game.Status switch
            {
                GameStatus.Lobby => RedirectToAction(nameof(GameController.Lobby), "Game", new { code = gameCode }),
                GameStatus.InProgress => RedirectToAction(nameof(GameController.Play), "Game",
                    new { code = gameCode }),
                _ => View(),
            };
        sessionHelper.ClearCurrentGameCode();
        return View();
    }
}
