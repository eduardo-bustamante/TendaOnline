using TendaOnline.Models.Enums;
using TendaOnline.Models;

public class MovimentacaoEstoque
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public TipoMovimentacao Tipo { get; set; }
    public int Quantidade { get; set; } // Sempre positivo; o Tipo define soma/subtração
    public decimal CustoUnitario { get; set; } // Valor pago na entrada
    public DateTime DataMovimentacao { get; set; } = DateTime.UtcNow;
    public string? Motivo { get; set; } // Ex: "NF 1234", "Quebra acidental", "Venda #102"
    public int? VendaId { get; set; }
    public Venda? Venda { get; set; }
}