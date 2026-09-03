using System.ComponentModel.DataAnnotations;
using TendaOnline.Models.Enums;

namespace TendaOnline.DTOs.Vendas;

public class CriarVendaDto
{
    [Required]
    public FormaPagamento FormaPagamento { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "O desconto não pode ser negativo.")]
    public decimal Desconto { get; set; } = 0m;

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "A venda deve conter pelo menos um item.")]
    public List<ItemVendaDto> Itens { get; set; } = new();
}

public class ItemVendaDto
{
    [Required]
    public int ProdutoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser de pelo menos 1.")]
    public int Quantidade { get; set; }
}