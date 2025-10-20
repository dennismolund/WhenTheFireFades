using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
//using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Domain.Helpers;
//using WhenTheFireFades.Models;
using WhenTheFireFades.ViewModels;

namespace WhenTheFireFades.Controllers;

public class HomeController(SessionHelper sessionHelper, IGameRepository gameRepository) : Controller
{
    private readonly SessionHelper _sessionHelper = sessionHelper;
    private readonly IGameRepository _gameRepository = gameRepository;
    public async Task<IActionResult> Index()
    {
        ViewBag.UserId = _sessionHelper.GetOrCreateTempUserId();
        ViewBag.Nickname = _sessionHelper.GetPlayerNickname();

        var gameCode = _sessionHelper.GetCurrentGameCode();
        if (gameCode != null)
        {
            var game = await _gameRepository.GetByCodeAsync(gameCode);
            if (game == null)
            {
                _sessionHelper.ClearCurrentGameCode();
                return View();
            }

            return game.Status switch
            {
                GameStatus.Lobby => RedirectToAction(nameof(GameController.Lobby), "Game", new { code = gameCode }),
                GameStatus.InProgress => RedirectToAction(nameof(GameController.Play), "Game", new { code = gameCode }),
                GameStatus.Finished => View(),
                _ => View(),
            };

        }
        return View();

    }
}
