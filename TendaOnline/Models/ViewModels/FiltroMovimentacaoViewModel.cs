using Microsoft.AspNetCore.Mvc.Rendering;
using TendaOnline.Models;
using TendaOnline.Models.Enums;

namespace TendaOnline.Models.ViewModels;

public class FiltroMovimentacaoViewModel
{
    public int? ProdutoId { get; set; }
    public TipoMovimentacao? Tipo { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }

    public List<MovimentacaoEstoque> Movimentacoes { get; set; } = new();

    // Dados para os dropdowns de filtro
    public SelectList? ListaProdutos { get; set; }
    public SelectList? ListaTipos { get; set; }
}