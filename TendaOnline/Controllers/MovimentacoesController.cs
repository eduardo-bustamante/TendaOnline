using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TendaOnline.Data;
using TendaOnline.Models.Enums;
using TendaOnline.Models.ViewModels;

namespace TendaOnline.Controllers;

[Authorize(Roles = "Admin")]
public class MovimentacoesController : Controller
{
    private readonly AppDbContext _context;

    public MovimentacoesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Movimentacoes
    public async Task<IActionResult> Index(FiltroMovimentacaoViewModel filtro)
    {
        var query = _context.MovimentacoesEstoque
            .Include(m => m.Produto)
            .Include(m => m.Venda)
            .AsNoTracking()
            .AsQueryable();

        // Filtro por Produto
        if (filtro.ProdutoId.HasValue && filtro.ProdutoId.Value > 0)
        {
            query = query.Where(m => m.ProdutoId == filtro.ProdutoId.Value);
        }

        // Filtro por Tipo de Movimentação
        if (filtro.Tipo.HasValue)
        {
            query = query.Where(m => m.Tipo == filtro.Tipo.Value);
        }

        // Filtro por Data Inicial
        if (filtro.DataInicio.HasValue)
        {
            var dataInicioUtc = filtro.DataInicio.Value.Date.ToUniversalTime();
            query = query.Where(m => m.DataMovimentacao >= dataInicioUtc);
        }

        // Filtro por Data Final
        if (filtro.DataFim.HasValue)
        {
            var dataFimUtc = filtro.DataFim.Value.Date.AddDays(1).ToUniversalTime();
            query = query.Where(m => m.DataMovimentacao < dataFimUtc);
        }

        filtro.Movimentacoes = await query
            .OrderByDescending(m => m.DataMovimentacao)
            .Take(150) // Limite das últimas 150 movimentações para performance
            .ToListAsync();

        // Carregar listas para os selects
        var produtos = await _context.Produtos
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .Select(p => new { p.Id, p.Nome })
            .ToListAsync();

        filtro.ListaProdutos = new SelectList(produtos, "Id", "Nome", filtro.ProdutoId);

        var tipos = Enum.GetValues(typeof(TipoMovimentacao))
            .Cast<TipoMovimentacao>()
            .Select(t => new { Id = (int)t, Nome = t.ToString() });

        filtro.ListaTipos = new SelectList(tipos, "Id", "Nome", filtro.Tipo.HasValue ? (int)filtro.Tipo.Value : null);

        return View(filtro);
    }
}