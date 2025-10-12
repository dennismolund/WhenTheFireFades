using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WhenTheFireFades.Domain.Helpers;
using WhenTheFireFades.Models;
using WhenTheFireFades.ViewModels;

namespace WhenTheFireFades.Controllers
{
    public class HomeController(SessionHelper sessionHelper) : Controller
    {
        private readonly SessionHelper _sessionHelper = sessionHelper;
        public IActionResult Index()
        {
            ViewBag.UserId = _sessionHelper.GetOrCreateTempUserId();
            ViewBag.Nickname = _sessionHelper.GetPlayerNickname();

            var gameCode = _sessionHelper.GetCurrentGameCode();
            if (gameCode != null)
            {
                return RedirectToAction(nameof(GameController.Lobby), "Game", new { code = gameCode });
            }

            return View();
        }

    }
}
