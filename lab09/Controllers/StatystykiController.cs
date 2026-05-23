using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lab09;

public class StatystykiController : Controller
{
    private readonly AppDbContext _context;

    public StatystykiController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        ViewData["PoWydzialach"] = _context.Posts
            .Include(p => p.PostInfo).ThenInclude(pi => pi!.Przedmiot).ThenInclude(p => p!.Wydzial)
            .Where(p => p.PostInfo != null && p.PostInfo.Przedmiot != null && p.PostInfo.Przedmiot.Wydzial != null)
            .GroupBy(p => p.PostInfo!.Przedmiot!.Wydzial!.Nazwa)
            .Select(g => new { Nazwa = g.Key, Liczba = g.Count() })
            .ToList();

        ViewData["PoPrzedmiotach"] = _context.Posts
            .Include(p => p.PostInfo).ThenInclude(pi => pi!.Przedmiot)
            .Where(p => p.PostInfo != null && p.PostInfo.Przedmiot != null)
            .GroupBy(p => p.PostInfo!.Przedmiot!.Nazwa)
            .Select(g => new { Nazwa = g.Key, Liczba = g.Count() })
            .ToList();

        ViewData["PoProwadzacych"] = _context.Posts
            .Include(p => p.PostInfo).ThenInclude(pi => pi!.Prowadzacy)
            .Where(p => p.PostInfo != null && p.PostInfo.Prowadzacy != null)
            .GroupBy(p => p.PostInfo!.Prowadzacy!.Nazwa)
            .Select(g => new { Nazwa = g.Key, Liczba = g.Count() })
            .ToList();

        return View();
    }
}