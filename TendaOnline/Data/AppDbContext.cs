using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using TendaOnline.Models;

namespace TendaOnline.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================
        // 1. Configuração: Categoria
        // ========================
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nome)
                  .IsRequired()
                  .HasMaxLength(100);

            // 1 Categoria -> N Produtos
            entity.HasMany(c => c.Produtos)
                  .WithOne(p => p.Categoria)
                  .HasForeignKey(p => p.CategoriaId)
                  .OnDelete(DeleteBehavior.Restrict); // Evita apagar categoria se tiver produtos nela
        });

        // ========================
        // 2. Configuração: Produto
        // ========================
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nome)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(p => p.CodigoBarras)
                  .HasMaxLength(50)
                  .IsRequired(false);

            entity.Property(p => p.Descricao)
                  .HasMaxLength(500);

            // Definição de precisão para valores monetários
            entity.Property(p => p.PrecoCusto)
                  .HasPrecision(18, 2);

            entity.Property(p => p.PrecoVenda)
                  .HasPrecision(18, 2);

            // Índice no código de barras para busca rápida
            entity.HasIndex(p => p.CodigoBarras);
        });

        // ========================
        // 3. Configuração: MovimentacaoEstoque
        // ========================
        modelBuilder.Entity<MovimentacaoEstoque>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Motivo)
                  .HasMaxLength(250);

            entity.Property(m => m.CustoUnitario)
                  .HasPrecision(18, 2);

            // 1 Produto -> N Movimentacoes
            entity.HasOne(m => m.Produto)
                  .WithMany(p => p.Movimentacoes)
                  .HasForeignKey(m => m.ProdutoId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Venda opcional (pode ser compra ou ajuste avulso)
            entity.HasOne(m => m.Venda)
                  .WithMany()
                  .HasForeignKey(m => m.VendaId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ========================
        // 4. Configuração: Venda
        // ========================
        modelBuilder.Entity<Venda>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.ValorTotal)
                  .HasPrecision(18, 2);

            entity.Property(v => v.Desconto)
                  .HasPrecision(18, 2);

            entity.Property(v => v.Observacoes)
                  .HasMaxLength(500);

            // 1 Venda -> N ItensVenda
            entity.HasMany(v => v.Itens)
                  .WithOne(i => i.Venda)
                  .HasForeignKey(i => i.VendaId)
                  .OnDelete(DeleteBehavior.Cascade); // Excluir a venda limpa seus itens
        });

        // ========================
        // 5. Configuração: ItemVenda
        // ========================
        modelBuilder.Entity<ItemVenda>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.Property(i => i.PrecoUnitario)
                  .HasPrecision(18, 2);

            // 1 Produto -> N ItensVenda
            entity.HasOne(i => i.Produto)
                  .WithMany()
                  .HasForeignKey(i => i.ProdutoId)
                  .OnDelete(DeleteBehavior.Restrict); // Não deixa deletar produto que já foi vendido
        });
    }
}