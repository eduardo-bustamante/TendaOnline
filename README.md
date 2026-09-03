# 🛒 TendaOnline - Sistema de Gestão Comercial, Estoque e PDV

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=c-sharp)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-blue?style=flat)
![Entity Framework Core](https://img.shields.io/badge/EF_Core-ORM-blueviolet?style=flat)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC292B?style=flat&logo=microsoft-sql-server)
![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat&logo=bootstrap)
![Status](https://img.shields.io/badge/Status-Em_Desenvolvimento-brightgreen)

Sistema web completo para controle e gestão comercial de lojas físicas e varejo, abrangendo frente de caixa (PDV), controle transacional de estoque, auditoria de movimentações, conciliação física de prateleira e governança com controle de acesso baseado em perfis (RBAC).

---

## 📌 Principais Funcionalidades

### 🏬 Frente de Caixa (PDV)
- **Registro Ágil de Vendas:** Seleção e adição dinâmica de itens com conferência de saldo em tempo real.
- **Múltiplas Formas de Pagamento:** Suporte a Dinheiro, Cartão de Crédito, Cartão de Débito e PIX.
- **Cancelamento e Estorno Seguro:** Cancelamento de vendas restrito a gestores, com estorno automático e atômico das quantidades ao saldo em estoque.
- **Comprovantes de Venda:** Emissão e visualização detalhada de comprovantes fiscais/recibos de operação.

### 📦 Controle e Auditoria de Estoque
- **Gestão de Ciclo de Vida do Produto:** Entradas por compra de fornecedor, saídas por venda, devoluções e descarte por avaria.
- **Trilha de Auditoria (Extrato Completo):** Histórico imutável de todas as movimentações com data/hora, tipo de operação, quantidade movimentada, custo unitário e motivo.
- **Ajuste Físico de Inventário / Balanço:** Interface com cálculo dinâmico de divergência (sobra/falta) entre o saldo do sistema e a contagem real na prateleira, gerando lançamento auditado de correção.
- **Monitoramento de Reposição:** Sinalização visual de níveis de estoque (normal, em atenção e crítico abaixo do mínimo).

### 📊 Painel de Controle (Dashboard)
- Indicadores em tempo real: faturamento diário, quantidade de vendas concluídas, ticket médio e contagem de itens em situação crítica.
- Tabela rápida de produtos com necessidade urgente de reposição com atalho para entrada de mercadoria.
- Gráfico de distribuição percentual das vendas por modalidade de pagamento.

### 🔐 Segurança e Governança (Identity & RBAC)
- **Autenticação Obrigatória:** Tela de login customizada com validações, bloqueio global de rotas anônimas e controle por cookies.
- **Perfis de Acesso:**
  - **Administrador:** Acesso irrestrito a cadastros, preços de custo, conciliações, auditoria, cancelamentos e gestão de acessos.
  - **Operador de Caixa:** Acesso direcionado ao PDV, emissão de comprovantes e consulta básica de histórico.
- **Gestão de Usuários:** Cadastro de novos operadores, edição de e-mails, redefinição forçada de senhas por administradores e revogação de acessos com proteção contra autoexclusão.

---

## 🛠️ Tecnologias Utilizadas

- **Back-end:** C# com ASP.NET Core MVC (.NET 8)
- **Acesso a Dados:** Entity Framework Core (Code-First) com Migrations
- **Banco de Dados:** Microsoft SQL Server
- **Autenticação & Autorização:** ASP.NET Core Identity Core com Roles (RBAC)
- **Front-end:** Razor Views, Bootstrap 5, Bootstrap Icons, JavaScript (Vanilla ES6)

---

## 🏗️ Arquitetura e Boas Práticas

- **Arquitetura em Camadas:** Separação entre Controllers, Camada de Serviços (`IEstoqueService`, `IVendaService`), Data Access (DbContext) e ViewModels.
- **Atomicidade Transacional:** Movimentações de estoque e registros de vendas executados de forma transacional para garantir integridade referencial e consistência de saldos.
- **Segurança:** Proteção contra Cross-Site Request Forgery (`[ValidateAntiForgeryToken]`), injeção de dependência desacoplada e políticas de autorização em granularidade de endpoints.

---

## 🚀 Como Executar o Projeto Localmente

### Pré-requisitos
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) ou superior instalado.
- [SQL Server](https://www.microsoft.com/sql-server/) (LocalDB, Express ou Standard) ativo.
- Git instalado.

### Passo a Passo

1. **Clone o repositório:**
   ```bash
   git clone [https://github.com/SEU-USUARIO/TendaOnline.git](https://github.com/SEU-USUARIO/TendaOnline.git)
   cd TendaOnline
