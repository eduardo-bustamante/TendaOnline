using System.ComponentModel.DataAnnotations;

namespace TendaOnline.Models.ViewModels;

public class MovimentacaoEstoqueViewModel
{
    [Required]
    public int ProdutoId { get; set; }

    public string NomeProduto { get; set; } = string.Empty;
    public int SaldoAtual { get; set; }

    [Required(ErrorMessage = "Informe a quantidade.")]
    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser de pelo menos 1.")]
    public int Quantidade { get; set; }

    [Required(ErrorMessage = "Informe o custo unitário.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O custo unitário deve ser maior que zero.")]
    public decimal CustoUnitario { get; set; }

    [MaxLength(250)]
    public string? Motivo { get; set; }
}