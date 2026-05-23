using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using lab09.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace lab09.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index(string? przedmiot, string? prowadzacy, bool? bezOdpowiedzi)
    {
        var query = _context.Posts
    		.Include(p => p.PostInfo).ThenInclude(pi => pi!.Przedmiot)
    		.Include(p => p.PostInfo).ThenInclude(pi => pi!.Prowadzacy)
            .Include(p => p.Replies).ThenInclude(r => r.Login)
            .Include(p => p.Login)
            .AsQueryable();

        if (bezOdpowiedzi == true)
            query = query.Where(p => p.PostInfo != null && p.PostInfo.CzyPytanie && !p.Replies.Any());

        if (!string.IsNullOrEmpty(przedmiot))
            query = query.Where(p => p.PostInfo != null && p.PostInfo.Przedmiot.Nazwa.Contains(przedmiot));

        if (!string.IsNullOrEmpty(prowadzacy))
            query = query.Where(p => p.PostInfo != null && p.PostInfo.Prowadzacy != null && p.PostInfo.Prowadzacy.Nazwa.Contains(prowadzacy));

        ViewData["Feed"] = query.OrderByDescending(p => p.Id).ToList();
        ViewData["Przedmiot"] = przedmiot;
        ViewData["Prowadzacy"] = prowadzacy;
        return View();
    }
    
	public IActionResult ManageData()
	{
    	var dataList = _context.Posts.Select(p => p.Informacja).ToList();
    	ViewData["Przedmioty"] = _context.Przedmioty.ToList();
    	ViewData["Prowadzacy"] = _context.Prowadzacy.ToList();
    	return View(dataList);
	}

	public IActionResult GetProwadzacy(int przedmiotId)
	{
    	var prowadzacy = _context.Przedmioty
        	.Include(p => p.Prowadzacy)
        	.FirstOrDefault(p => p.Id == przedmiotId)
        	?.Prowadzacy
        	?.Select(p => new { p.Id, p.Nazwa })
        	.ToList();

    	return Json(prowadzacy);
	}

    [HttpPost]
    public IActionResult AddData(string newData, int? przedmiotId, int? prowadzacyId, bool czyPytanie)
    {
    	if (przedmiotId.HasValue && prowadzacyId.HasValue)
    	{
        	var przedmiot = _context.Przedmioty
            	.Include(p => p.Prowadzacy)
            	.FirstOrDefault(p => p.Id == przedmiotId);

        	bool valid = przedmiot?.Prowadzacy?.Any(p => p.Id == prowadzacyId) ?? false;

        	if (!valid)
        	{
            	ViewData["Error"] = "Wybrany prowadzący nie prowadzi tego przedmiotu.";
            	ViewData["Przedmioty"] = _context.Przedmioty.ToList();
            	ViewData["Prowadzacy"] = _context.Prowadzacy.ToList();
            	return View(_context.Posts.Select(p => p.Informacja).ToList());
        	}
    	}

    	if (!string.IsNullOrEmpty(newData))
    	{
        	var username = HttpContext.Session.GetString("User") ?? "";
        	var loginUser = _context.Logins.FirstOrDefault(l => l.LoginName == username);
        	if (loginUser == null) return RedirectToAction("Index");

        	_context.Posts.Add(new Post
        	{
            	LoginId = loginUser.Id,
            	Informacja = newData,
            	PostInfo = new PostInfo
            	{
                	PrzedmiotId = przedmiotId,
                	ProwadzacyId = prowadzacyId,
                	Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                	CzyPytanie = czyPytanie
            	}
        	});
        	_context.SaveChanges();
    	}
    	return RedirectToAction("ManageData");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult AddReply(int postId, string tresc)
    {
        if (!string.IsNullOrEmpty(tresc))
        {
        	var username = HttpContext.Session.GetString("User") ?? "";
        	var loginUser = _context.Logins.FirstOrDefault(l => l.LoginName == username);
        	if (loginUser == null) return RedirectToAction("Index");
            _context.PostReplies.Add(new PostReply
            {
                PostId = postId,
				LoginId = loginUser.Id,
                Tresc = tresc,
                Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            });
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }
}