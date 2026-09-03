using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TendaOnline.Data;
using TendaOnline.Models;

namespace TendaOnline.Controllers;

[Authorize(Roles = "Admin")]
public class CategoriasController : Controller
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Categorias
    public async Task<IActionResult> Index()
    {
        var categorias = await _context.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync();

        return View(categorias);
    }

    // GET: /Categorias/Create
    public IActionResult Create()
    {
        return View(new Categoria());
    }

    // POST: /Categorias/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        if (ModelState.IsValid)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Categoria cadastrada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        return View(categoria);
    }
}