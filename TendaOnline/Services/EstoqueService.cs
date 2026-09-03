using Microsoft.EntityFrameworkCore;
using TendaOnline.Data;
using TendaOnline.Models;
using TendaOnline.Models.Enums;
using TendaOnline.Services.Interfaces;

namespace TendaOnline.Services;

public class EstoqueService : IEstoqueService
{
    private readonly AppDbContext _context;

    public EstoqueService(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarEntradaAsync(int produtoId, int quantidade, decimal custoUnitario, string? motivo = null)
    {
        if (quantidade <= 0)
            throw new ArgumentException("A quantidade de entrada deve ser maior que zero.", nameof(quantidade));

        if (custoUnitario < 0)
            throw new ArgumentException("O custo unitário não pode ser negativo.", nameof(custoUnitario));

        var produto = await _context.Produtos.FindAsync(produtoId)
            ?? throw new InvalidOperationException($"Produto com ID {produtoId} não foi encontrado.");

        // 1. Cria a movimentação de auditoria
        var movimentacao = new MovimentacaoEstoque
        {
            ProdutoId = produtoId,
            Tipo = TipoMovimentacao.EntradaCompra,
            Quantidade = quantidade,
            CustoUnitario = custoUnitario,
            DataMovimentacao = DateTime.UtcNow,
            Motivo = motivo ?? "Entrada de mercadoria"
        };

        // 2. Atualiza o saldo e o custo de reposição no produto
        produto.QuantidadeEstoque += quantidade;
        produto.PrecoCusto = custoUnitario; // Mantém o custo de reposição atualizado

        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync();
    }

    public async Task RegistrarSaidaAsync(int produtoId, int quantidade, TipoMovimentacao tipo, string? motivo = null, int? vendaId = null)
    {
        if (quantidade <= 0)
            throw new ArgumentException("A quantidade de saída deve ser maior que zero.", nameof(quantidade));

        if (tipo == TipoMovimentacao.EntradaCompra)
            throw new ArgumentException("O tipo informado não representa uma saída de estoque.", nameof(tipo));

        var produto = await _context.Produtos.FindAsync(produtoId)
            ?? throw new InvalidOperationException($"Produto com ID {produtoId} não foi encontrado.");

        // Bloqueia saída se não houver saldo suficiente
        if (produto.QuantidadeEstoque < quantidade)
        {
            throw new InvalidOperationException(
                $"Saldo insuficiente para o produto '{produto.Nome}'. Disponível: {produto.QuantidadeEstoque}, Solicitado: {quantidade}.");
        }

        // 1. Cria a movimentação
        var movimentacao = new MovimentacaoEstoque
        {
            ProdutoId = produtoId,
            Tipo = tipo,
            Quantidade = quantidade,
            CustoUnitario = produto.PrecoCusto, // Registra o custo contábil do item no momento da saída
            DataMovimentacao = DateTime.UtcNow,
            Motivo = motivo ?? $"Saída por {tipo}",
            VendaId = vendaId
        };

        // 2. Deduz do estoque
        produto.QuantidadeEstoque -= quantidade;

        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync();
    }

    public async Task RealizarAjusteInventarioAsync(int produtoId, int novoEstoqueFisico, string motivo)
    {
        if (novoEstoqueFisico < 0)
            throw new ArgumentException("O saldo físico de estoque não pode ser negativo.", nameof(novoEstoqueFisico));

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("É obrigatório informar o motivo de um ajuste de inventário.", nameof(motivo));

        var produto = await _context.Produtos.FindAsync(produtoId)
            ?? throw new InvalidOperationException($"Produto com ID {produtoId} não foi encontrado.");

        int diferenca = novoEstoqueFisico - produto.QuantidadeEstoque;

        // Se a contagem física for igual ao sistema, nada precisa ser feito
        if (diferenca == 0) return;

        var movimentacao = new MovimentacaoEstoque
        {
            ProdutoId = produtoId,
            Tipo = TipoMovimentacao.AjusteInventario,
            Quantidade = Math.Abs(diferenca),
            CustoUnitario = produto.PrecoCusto,
            DataMovimentacao = DateTime.UtcNow,
            Motivo = $"Ajuste de inventário ({diferenca:+0;-0}): {motivo}"
        };

        produto.QuantidadeEstoque = novoEstoqueFisico;

        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync();
    }

    public async Task<int> ObterSaldoAtualAsync(int produtoId)
    {
        var produto = await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == produtoId)
            ?? throw new InvalidOperationException($"Produto com ID {produtoId} não foi encontrado.");

        return produto.QuantidadeEstoque;
    }
}