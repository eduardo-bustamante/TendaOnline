using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TendaOnline.Data;
using TendaOnline.DTOs.Vendas;
using TendaOnline.Models.Enums;
using TendaOnline.Services.Interfaces;

namespace TendaOnline.Controllers;

[Authorize]
public class VendasController : Controller
{
    private readonly AppDbContext _context;
    private readonly IVendaService _vendaService;

    public VendasController(AppDbContext context, IVendaService vendaService)
    {
        _context = context;
        _vendaService = vendaService;
    }

    // GET: /Vendas
    public async Task<IActionResult> Index()
    {
        var vendas = await _context.Vendas
            .AsNoTracking()
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .OrderByDescending(v => v.DataVenda)
            .Take(50) // Traz as últimas 50 vendas
            .ToListAsync();

        return View(vendas);
    }

    // GET: /Vendas/PDV (Tela de Frente de Caixa)
    public async Task<IActionResult> PDV()
    {
        await CarregarDadosPDVAsync();
        return View(new CriarVendaDto());
    }

    // POST: /Vendas/PDV
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PDV([FromBody] CriarVendaDto dto)
    {
        if (!ModelState.IsValid)
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new { sucesso = false, mensagem = string.Join("; ", erros) });
        }

        try
        {
            var vendaRealizada = await _vendaService.RealizarVendaAsync(dto);
            return Ok(new { sucesso = true, vendaId = vendaRealizada.Id, total = vendaRealizada.ValorTotal });
        }
        catch (Exception ex)
        {
            return BadRequest(new { sucesso = false, mensagem = ex.Message });
        }
    }

    // GET: /Vendas/Comprovante/5
    public async Task<IActionResult> Comprovante(int id)
    {
        var venda = await _vendaService.ObterPorIdAsync(id);
        if (venda == null) return NotFound();

        return View(venda);
    }

    private async Task CarregarDadosPDVAsync()
    {
        // Carrega produtos com estoque disponível para venda
        var produtos = await _context.Produtos
            .AsNoTracking()
            .Where(p => p.Ativo && p.QuantidadeEstoque > 0)
            .OrderBy(p => p.Nome)
            .Select(p => new
            {
                p.Id,
                p.Nome,
                p.PrecoVenda,
                p.QuantidadeEstoque,
                p.CodigoBarras
            })
            .ToListAsync();

        ViewBag.ProdutosDisponiveis = produtos;
        ViewBag.FormasPagamento = Enum.GetValues(typeof(FormaPagamento))
            .Cast<FormaPagamento>()
            .Select(f => new SelectListItem
            {
                Value = ((int)f).ToString(),
                Text = f.ToString()
            }).ToList();
    }

    // POST: /Vendas/Cancelar
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancelar(int vendaId, string motivoCancelamento)
    {
        if (string.IsNullOrWhiteSpace(motivoCancelamento))
        {
            TempData["MensagemErro"] = "É obrigatório informar o motivo do cancelamento.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _vendaService.CancelarVendaAsync(vendaId, motivoCancelamento);
            TempData["MensagemSucesso"] = $"Venda #{vendaId} cancelada com sucesso! Os itens retornaram ao estoque.";
        }
        catch (Exception ex)
        {
            TempData["MensagemErro"] = $"Erro ao cancelar venda: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}