using TendaOnline.DTOs.Vendas;

namespace TendaOnline.Services.Interfaces;

public interface IVendaService
{
    Task<VendaResponseDto> RealizarVendaAsync(CriarVendaDto dto);
    Task<VendaResponseDto?> ObterPorIdAsync(int vendaId);
    Task CancelarVendaAsync(int vendaId, string motivoCancelamento);
}