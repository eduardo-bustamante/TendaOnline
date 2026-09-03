using TendaOnline.Models.Enums;

namespace TendaOnline.Services.Interfaces;

public interface IEstoqueService
{
    Task RegistrarEntradaAsync(int produtoId, int quantidade, decimal custoUnitario, string? motivo = null);
    Task RegistrarSaidaAsync(int produtoId, int quantidade, TipoMovimentacao tipo, string? motivo = null, int? vendaId = null);
    Task RealizarAjusteInventarioAsync(int produtoId, int novoEstoqueFisico, string motivo);
    Task<int> ObterSaldoAtualAsync(int produtoId);
}