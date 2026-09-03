using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TendaOnline.Data;
using TendaOnline.Models.ViewModels;

namespace TendaOnline.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var inicioHoje = DateTime.UtcNow.Date;
        var fimHoje = inicioHoje.AddDays(1);

        // Vendas de hoje (não canceladas)
        var vendasHoje = await _context.Vendas
            .AsNoTracking()
            .Where(v => v.DataVenda >= inicioHoje && v.DataVenda < fimHoje)
            .Where(v => v.Observacoes == null || !v.Observacoes.Contains("[CANCELADA]"))
            .ToListAsync();

        // Produtos com estoque igual ou abaixo do mínimo definido
        var produtosEstoqueCritico = await _context.Produtos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .Where(p => p.Ativo && p.QuantidadeEstoque <= p.EstoqueMinimo)
            .OrderBy(p => p.QuantidadeEstoque)
            .ToListAsync();

        // 5 vendas mais recentes
        var ultimasVendas = await _context.Vendas
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .AsNoTracking()
            .OrderByDescending(v => v.DataVenda)
            .Take(5)
            .ToListAsync();

        var viewModel = new DashboardViewModel
        {
            FaturamentoHoje = vendasHoje.Sum(v => v.ValorTotal),
            TotalVendasHoje = vendasHoje.Count,
            TotalProdutosCadastrados = await _context.Produtos.CountAsync(p => p.Ativo),
            TotalItensAbaixoMinimo = produtosEstoqueCritico.Count,
            ProdutosEstoqueCritico = produtosEstoqueCritico,
            UltimasVendas = ultimasVendas,
            FaturamentoPorPagamento = vendasHoje
                .GroupBy(v => v.FormaPagamento.ToString())
                .ToDictionary(g => g.Key, g => g.Sum(v => v.ValorTotal))
        };

        return View(viewModel);
    }
}