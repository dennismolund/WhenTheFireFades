using Microsoft.AspNetCore.Mvc;

namespace WhenTheFireFades.Controllers;
public class GamesController : Controller
{

    public IActionResult Index()
    {
        return View();
    }
}
