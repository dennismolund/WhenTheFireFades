using Microsoft.AspNetCore.Mvc;

namespace WhenTheFireFades.Controllers;

public class AccountController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
