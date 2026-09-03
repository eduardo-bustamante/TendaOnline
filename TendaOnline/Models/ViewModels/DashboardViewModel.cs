using TendaOnline.Models;

namespace TendaOnline.Models.ViewModels;

public class DashboardViewModel
{
    public decimal FaturamentoHoje { get; set; }
    public int TotalVendasHoje { get; set; }
    public decimal TicketMedioHoje => TotalVendasHoje > 0 ? FaturamentoHoje / TotalVendasHoje : 0m;

    public int TotalProdutosCadastrados { get; set; }
    public int TotalItensAbaixoMinimo { get; set; }

    public List<Produto> ProdutosEstoqueCritico { get; set; } = new();
    public List<Venda> UltimasVendas { get; set; } = new();
    public Dictionary<string, decimal> FaturamentoPorPagamento { get; set; } = new();
}