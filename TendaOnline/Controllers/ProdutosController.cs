using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TendaOnline.Data;
using TendaOnline.Models;
using TendaOnline.Models.ViewModels;
using TendaOnline.Services;
using TendaOnline.Services.Interfaces;
namespace TendaOnline.Controllers;

[Authorize(Roles = "Admin")]
public class ProdutosController : Controller
{
    private readonly AppDbContext _context;
    private readonly IEstoqueService _estoqueService;

    public ProdutosController(AppDbContext context, IEstoqueService estoqueService)
    {
        _context = context;
        _estoqueService = estoqueService;

    }

    // GET: Produtos
    public async Task<IActionResult> Index()
    {
        var produtos = await _context.Produtos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync();

        return View(produtos);
    }

    // GET: Produtos/Create
    public async Task<IActionResult> Create()
    {
        await CarregarCategoriasViewBagAsync();
        return View(new Produto());
    }

    // POST: Produtos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Produto produto)
    {
        if (ModelState.IsValid)
        {
            produto.DataCadastro = DateTime.UtcNow;
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Produto cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        await CarregarCategoriasViewBagAsync(produto.CategoriaId);
        return View(produto);
    }

    // GET: Produtos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null) return NotFound();

        await CarregarCategoriasViewBagAsync(produto.CategoriaId);
        return View(produto);
    }

    // POST: Produtos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Produto produto)
    {
        if (id != produto.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Busca a entidade original para preservar campos protegidos como DataCadastro e QuantidadeEstoque
                var produtoDb = await _context.Produtos.FindAsync(id);
                if (produtoDb == null) return NotFound();

                produtoDb.Nome = produto.Nome;
                produtoDb.CodigoBarras = produto.CodigoBarras;
                produtoDb.Descricao = produto.Descricao;
                produtoDb.PrecoCusto = produto.PrecoCusto;
                produtoDb.PrecoVenda = produto.PrecoVenda;
                produtoDb.EstoqueMinimo = produto.EstoqueMinimo;
                produtoDb.CategoriaId = produto.CategoriaId;
                produtoDb.Ativo = produto.Ativo;

                await _context.SaveChangesAsync();
                TempData["MensagemSucesso"] = "Produto atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Produtos.AnyAsync(e => e.Id == produto.Id))
                    return NotFound();
                else
                    throw;
            }
        }

        await CarregarCategoriasViewBagAsync(produto.CategoriaId);
        return View(produto);
    }

    // Carrega dropdown de categorias ativas
    private async Task CarregarCategoriasViewBagAsync(int? categoriaSelecionadaId = null)
    {
        var categorias = await _context.Categorias
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.Categorias = new SelectList(categorias, "Id", "Nome", categoriaSelecionadaId);
    }

    // GET: Produtos/EntradaEstoque/5
    public async Task<IActionResult> EntradaEstoque(int? id)
    {
        if (id == null) return NotFound();

        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null) return NotFound();

        var viewModel = new MovimentacaoEstoqueViewModel
        {
            ProdutoId = produto.Id,
            NomeProduto = produto.Nome,
            SaldoAtual = produto.QuantidadeEstoque,
            CustoUnitario = produto.PrecoCusto
        };

        return View(viewModel);
    }

    // POST: Produtos/EntradaEstoque
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EntradaEstoque(MovimentacaoEstoqueViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _estoqueService.RegistrarEntradaAsync(
                    produtoId: model.ProdutoId,
                    quantidade: model.Quantidade,
                    custoUnitario: model.CustoUnitario,
                    motivo: string.IsNullOrWhiteSpace(model.Motivo) ? "Entrada manual de mercadoria" : model.Motivo
                );

                TempData["MensagemSucesso"] = $"Entrada de {model.Quantidade} unidade(s) registrada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao processar entrada: {ex.Message}");
            }
        }

        return View(model);
    }

    // GET: Produtos/AjusteInventario/5
    public async Task<IActionResult> AjusteInventario(int? id)
    {
        if (id == null) return NotFound();

        var produto = await _context.Produtos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto == null) return NotFound();

        var viewModel = new AjusteInventarioViewModel
        {
            ProdutoId = produto.Id,
            NomeProduto = produto.Nome,
            CodigoBarras = produto.CodigoBarras,
            CategoriaNome = produto.Categoria?.Nome,
            SaldoSistema = produto.QuantidadeEstoque,
            NovoEstoqueFisico = produto.QuantidadeEstoque // Inicia preenchido com o saldo atual
        };

        return View(viewModel);
    }

    // POST: Produtos/AjusteInventario
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AjusteInventario(AjusteInventarioViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _estoqueService.RealizarAjusteInventarioAsync(
                    produtoId: model.ProdutoId,
                    novoEstoqueFisico: model.NovoEstoqueFisico,
                    motivo: model.Motivo
                );

                TempData["MensagemSucesso"] = $"Inventário do produto '{model.NomeProduto}' atualizado para {model.NovoEstoqueFisico} unidade(s)!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao processar o ajuste: {ex.Message}");
            }
        }

        return View(model);
    }
}