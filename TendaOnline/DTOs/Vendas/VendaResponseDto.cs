using TendaOnline.Models.Enums;

namespace TendaOnline.DTOs.Vendas;

public class VendaResponseDto
{
    public int Id { get; set; }
    public DateTime DataVenda { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal Desconto { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public string? Observacoes { get; set; }
    public List<ItemVendaResponseDto> Itens { get; set; } = new();
}

public class ItemVendaResponseDto
{
    public int ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal => Quantidade * PrecoUnitario;
}