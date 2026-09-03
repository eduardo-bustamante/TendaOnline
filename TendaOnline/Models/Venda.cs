using TendaOnline.Models.Enums;
namespace TendaOnline.Models;

public class Venda
{
    public int Id { get; set; }
    public DateTime DataVenda { get; set; } = DateTime.UtcNow;
    public decimal ValorTotal { get; set; }
    public decimal Desconto { get; set; } = 0m;
    public FormaPagamento FormaPagamento { get; set; }
    public string? Observacoes { get; set; }

    public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
}