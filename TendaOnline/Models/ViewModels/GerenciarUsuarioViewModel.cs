using System.ComponentModel.DataAnnotations;

namespace TendaOnline.Models.ViewModels;

public class EditarUsuarioViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione o perfil de acesso.")]
    public string Perfil { get; set; } = string.Empty;
}

public class ResetarSenhaViewModel
{
    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    [DataType(DataType.Password)]
    public string NovaSenha { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("NovaSenha", ErrorMessage = "As senhas não conferem.")]
    public string ConfirmarNovaSenha { get; set; } = string.Empty;
}