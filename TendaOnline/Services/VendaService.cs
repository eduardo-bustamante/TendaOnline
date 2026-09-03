using Microsoft.EntityFrameworkCore;
using TendaOnline.Data;
using TendaOnline.DTOs.Vendas;
using TendaOnline.Models;
using TendaOnline.Models.Enums;
using TendaOnline.Services.Interfaces;

namespace TendaOnline.Services;

public class VendaService : IVendaService
{
    private readonly AppDbContext _context;
    private readonly IEstoqueService _estoqueService;

    public VendaService(AppDbContext context, IEstoqueService estoqueService)
    {
        _context = context;
        _estoqueService = estoqueService;
    }

    public async Task<VendaResponseDto> RealizarVendaAsync(CriarVendaDto dto)
    {
        if (dto.Itens == null || !dto.Itens.Any())
            throw new ArgumentException("A venda deve conter pelo menos um item.", nameof(dto));

        // Usa transação de banco para garantir que tudo seja comitado junto
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var idsProdutos = dto.Itens.Select(i => i.ProdutoId).Distinct().ToList();
            var produtos = await _context.Produtos
                .Where(p => idsProdutos.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // 1. Valida existência e status dos produtos
            foreach (var itemDto in dto.Itens)
            {
                if (!produtos.TryGetValue(itemDto.ProdutoId, out var produto))
                {
                    throw new InvalidOperationException($"Produto ID {itemDto.ProdutoId} não foi encontrado.");
                }

                if (!produto.Ativo)
                {
                    throw new InvalidOperationException($"O produto '{produto.Nome}' está inativo e não pode ser vendido.");
                }
            }

            // 2. Instancia a venda
            var venda = new Venda
            {
                DataVenda = DateTime.UtcNow,
                FormaPagamento = dto.FormaPagamento,
                Desconto = dto.Desconto,
                Observacoes = dto.Observacoes,
                Itens = new List<ItemVenda>()
            };

            decimal valorBruto = 0m;

            // 3. Adiciona itens com preço congelado no momento da venda
            foreach (var itemDto in dto.Itens)
            {
                var produto = produtos[itemDto.ProdutoId];

                var itemVenda = new ItemVenda
                {
                    ProdutoId = produto.Id,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = produto.PrecoVenda // Garante histórico de preço
                };

                venda.Itens.Add(itemVenda);
                valorBruto += itemVenda.Subtotal;
            }

            if (dto.Desconto > valorBruto)
                throw new InvalidOperationException("O desconto não pode ser superior ao valor total dos produtos.");

            venda.ValorTotal = valorBruto - dto.Desconto;

            // Salva a venda para gerar o Venda.Id
            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();

            // 4. Baixa no estoque via IEstoqueService para cada item vendido
            foreach (var item in venda.Itens)
            {
                await _estoqueService.RegistrarSaidaAsync(
                    produtoId: item.ProdutoId,
                    quantidade: item.Quantidade,
                    tipo: TipoMovimentacao.SaidaVenda,
                    motivo: $"Saída automática referente à Venda #{venda.Id}",
                    vendaId: venda.Id
                );
            }

            // Confirma a transação com tudo consistente
            await transaction.CommitAsync();

            return new VendaResponseDto
            {
                Id = venda.Id,
                DataVenda = venda.DataVenda,
                ValorTotal = venda.ValorTotal,
                Desconto = venda.Desconto,
                FormaPagamento = venda.FormaPagamento,
                Observacoes = venda.Observacoes,
                Itens = venda.Itens.Select(i => new ItemVendaResponseDto
                {
                    ProdutoId = i.ProdutoId,
                    NomeProduto = produtos[i.ProdutoId].Nome,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario
                }).ToList()
            };
        }
        catch
        {
            // Qualquer exceção (ex: saldo insuficiente no estoque) desfaz tudo
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<VendaResponseDto?> ObterPorIdAsync(int vendaId)
    {
        var venda = await _context.Vendas
            .AsNoTracking()
            .Include(v => v.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(v => v.Id == vendaId);

        if (venda == null) return null;

        return new VendaResponseDto
        {
            Id = venda.Id,
            DataVenda = venda.DataVenda,
            ValorTotal = venda.ValorTotal,
            Desconto = venda.Desconto,
            FormaPagamento = venda.FormaPagamento,
            Observacoes = venda.Observacoes,
            Itens = venda.Itens.Select(i => new ItemVendaResponseDto
            {
                ProdutoId = i.ProdutoId,
                NomeProduto = i.Produto?.Nome ?? "Produto não encontrado",
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario
            }).ToList()
        };
    }

    public async Task CancelarVendaAsync(int vendaId, string motivoCancelamento)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var venda = await _context.Vendas
                .Include(v => v.Itens)
                .FirstOrDefaultAsync(v => v.Id == vendaId)
                ?? throw new InvalidOperationException($"Venda ID {vendaId} não foi encontrada.");

            // Devolve os itens para o estoque como devolução
            foreach (var item in venda.Itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);
                decimal custo = produto?.PrecoCusto ?? 0m;

                await _estoqueService.RegistrarEntradaAsync(
                    produtoId: item.ProdutoId,
                    quantidade: item.Quantidade,
                    custoUnitario: custo,
                    motivo: $"Devolução/Cancelamento da Venda #{vendaId}. Motivo: {motivoCancelamento}"
                );
            }

            venda.Observacoes = string.IsNullOrWhiteSpace(venda.Observacoes)
                ? $"[CANCELADA] {motivoCancelamento}"
                : $"{venda.Observacoes} | [CANCELADA] {motivoCancelamento}";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}