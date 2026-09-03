namespace TendaOnline.Models;

public class Produto
{
    public int Id { get; set; }
    public string? CodigoBarras { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PrecoCusto { get; set; }
    public decimal PrecoVenda { get; set; }
    public int EstoqueMinimo { get; set; } = 5;
    public int QuantidadeEstoque { get; set; } = 0; // Atualizado via movimentações
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    // Chave estrangeira
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    // Histórico de movimentações
    public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = new List<MovimentacaoEstoque>();
}