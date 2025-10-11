using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WhenTheFireFades.Domain.Helpers;
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
            return View();
        }

    }
}
