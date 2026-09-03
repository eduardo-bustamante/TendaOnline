using System.ComponentModel.DataAnnotations;

namespace TendaOnline.Models.ViewModels;

public class AjusteInventarioViewModel
{
    [Required]
    public int ProdutoId { get; set; }

    public string NomeProduto { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public string? CategoriaNome { get; set; }
    public int SaldoSistema { get; set; }

    [Required(ErrorMessage = "Informe a quantidade física encontrada na prateleira.")]
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade física não pode ser negativa.")]
    public int NovoEstoqueFisico { get; set; }

    [Required(ErrorMessage = "Informe o motivo da divergência / conferência.")]
    [MaxLength(200, ErrorMessage = "O motivo não pode passar de 200 caracteres.")]
    public string Motivo { get; set; } = string.Empty;

    public int Divergencia => NovoEstoqueFisico - SaldoSistema;
}