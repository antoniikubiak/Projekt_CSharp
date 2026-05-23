using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace lab09.Controllers;

public class UserController : Controller
{   
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Profile(string username)
    {
		var loginUser = _context.Logins.FirstOrDefault(l => l.LoginName == username);

        var posty = _context.Posts
    		.Include(p => p.PostInfo).ThenInclude(pi => pi!.Przedmiot)
    		.Include(p => p.PostInfo).ThenInclude(pi => pi!.Prowadzacy)
        	.Include(p => p.Login)
        	.Include(p => p.Replies).ThenInclude(r => r.Login)
        	.Where(p => p.LoginId == loginUser.Id)
        	.OrderByDescending(p => p.Id)
        	.ToList();

		var userInfo = _context.UserInfos
    		.Include(u => u.Wydzial)
    		.FirstOrDefault(u => u.Login == username);

        ViewData["Username"] = username;
        ViewData["Posty"] = posty;
        ViewData["Wydzial"] = userInfo?.Wydzial?.Nazwa;
        ViewData["Kierunek"] = userInfo?.Kierunek;
        return View();
    }

    public ActionResult AllProfiles()
    {
        var profiles = _context.Logins.Select(l => l.LoginName).Distinct().ToList();
        ViewData["Profiles"] = profiles;
        return View();
    }

    public IActionResult EditProfile()
    {
        var username = HttpContext.Session.GetString("User");
    	var userInfo = _context.UserInfos
        	.Include(u => u.Wydzial)
        	.FirstOrDefault(u => u.Login == username);

    	ViewData["Wydzialy"] = new SelectList(_context.Wydzialy, "Id", "Nazwa", userInfo?.WydzialId);
    	ViewData["WydzialId"] = userInfo?.WydzialId;
        ViewData["Kierunek"] = userInfo?.Kierunek;
        return View();
    }

    [HttpPost]
    public IActionResult EditProfile(int? wydzialId, string kierunek)
    {
        var username = HttpContext.Session.GetString("User");
        var userInfo = _context.UserInfos.FirstOrDefault(u => u.Login == username);
        if (userInfo == null)
        {
        	_context.UserInfos.Add(new UserInfo { Login = username!, WydzialId = wydzialId, Kierunek = kierunek });
        }
        else
        {
        	userInfo.WydzialId = wydzialId;
        	userInfo.Kierunek = kierunek;
        }
        _context.SaveChanges();
        return RedirectToAction("Profile", new { username });
    }
}