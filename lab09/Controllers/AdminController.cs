using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lab09;

public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("User") != "admin")
        {
            return RedirectToAction("Index", "Home");
        }        
        
        ViewData["Prowadzacy"] = _context.Prowadzacy.Include(p => p.Wydzial).Include(p => p.Przedmioty).ToList();
        ViewData["Przedmioty"] = _context.Przedmioty.Include(p => p.Wydzial).ToList();
        ViewData["Wydzialy"] = _context.Wydzialy.ToList();
        return View();
    }

    [HttpPost]
    public IActionResult AddProwadzacy(string nazwa, int? wydzialId)
    {
        _context.Prowadzacy.Add(new Prowadzacy { Nazwa = nazwa, WydzialId = wydzialId });
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult AddPrzedmiot(string nazwa, int? wydzialId, string? kierunek, int[] prowadzacyIds)
    {
        var przedmiot = new Przedmiot { Nazwa = nazwa, WydzialId = wydzialId, Kierunek = kierunek };
        if (prowadzacyIds.Length > 0)
            przedmiot.Prowadzacy = _context.Prowadzacy.Where(p => prowadzacyIds.Contains(p.Id)).ToList();
        _context.Przedmioty.Add(przedmiot);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}