using Desafio.Api.Api.Contratos;
using Desafio.Api.Dominio;
using Desafio.Api.Infraestrutura;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Desafio.Api.Aplicacao;

public class BeneficiarioServico(AppDbContext db, PlanoServico planoServico)
{
  
    private const string CodigoViolacaoDeUnicidade = "23505";

    public async Task<PaginaResponse<BeneficiarioResponse>> ListarAsync(
        BeneficiarioFiltro filtro,
        CancellationToken cancellationToken)
    {
        ValidarFiltro(filtro);

        var query = db.Beneficiarios
            .AsNoTracking()
            .AsQueryable();

        if (filtro.Status is not null)
        {
            query = query.Where(
                b => b.Status == filtro.Status.Value);
        }

        if (filtro.PlanoId is not null)
        {
            query = query.Where(
                b => b.PlanoId == filtro.PlanoId.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var beneficiarios = await query
            .OrderBy(b => b.NomeCompleto)
            .ThenBy(b => b.Id)
            .Skip((filtro.Pagina - 1) * filtro.Tamanho)
            .Take(filtro.Tamanho)
            .ToListAsync(cancellationToken);

        var dados = beneficiarios
            .Select(BeneficiarioResponse.De)
            .ToList();

        return new PaginaResponse<BeneficiarioResponse>(
            dados,
            filtro.Pagina,
            filtro.Tamanho,
            total);
    }
    public async Task<Beneficiario> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db.Beneficiarios.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
               ?? throw new NaoEncontradoException("Beneficiario não encontrado");
    }

    public async Task<Beneficiario> CriarAsync(BeneficiarioRequestDados dados, CancellationToken cancellationToken)
    {
        await planoServico.ValidarPlanoAsync(dados.PlanoId, cancellationToken);
        var beneficiario = new Beneficiario(dados.NomeCompleto, dados.Cpf, dados.DataNascimento, dados.PlanoId);

        await GarantirUnicidadeAsync(beneficiario, cancellationToken);
        db.Beneficiarios.Add(beneficiario);
        await SalvarAsync(cancellationToken);

        return beneficiario;
    }

    public async Task<Beneficiario> AtualizarAsync(Guid id, BeneficiarioRequestAtualizarDados dados, CancellationToken cancellationToken)
    {
        
        var beneficiario = await ObterAsync(id, cancellationToken);
        await planoServico.ValidarPlanoAsync(dados.PlanoId, cancellationToken);
        beneficiario.Atualizar(dados.NomeCompleto, dados.DataNascimento, dados.PlanoId, dados.Status);
        await GarantirUnicidadeAsync(beneficiario, cancellationToken);
        await SalvarAsync(cancellationToken);

        return beneficiario;
    }

    private async Task SalvarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException excecao) when (EhViolacaoDeUnicidade(excecao))
        {
            throw new ConflitoException("Já existe beneficiario cadastrado com esse valor");
        }
    }

    private async Task GarantirUnicidadeAsync(Beneficiario beneficiario, CancellationToken cancellationToken)
    {
        var conflito = await db.Beneficiarios
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(b => b.Id != beneficiario.Id)
            .Where(b => b.Cpf == beneficiario.Cpf)
            .FirstOrDefaultAsync(cancellationToken);

        if (conflito is null)
        {
            return;
        }

        var campo = "cpf";
        throw new ConflitoException(
            "Já existe beneficiario cadastrado com esse valor",
            [new DetalheErro(campo, "duplicado")]);
    }

  public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken)
  {
      var beneficiario = await ObterAsync(id, cancellationToken);

      beneficiario.Excluir();
      await SalvarAsync(cancellationToken);
  }

  private static bool EhViolacaoDeUnicidade(DbUpdateException excecao) =>
      excecao.InnerException is PostgresException postgres &&
      postgres.SqlState == CodigoViolacaoDeUnicidade;

  private static void ValidarFiltro(BeneficiarioFiltro filtro)
  {
      var detalhes = new List<DetalheErro>();

      if (filtro.Pagina < 1)
      {
        detalhes.Add(new DetalheErro("pagina", "valor_invalido"));
      }

      if (filtro.Tamanho < 1 || filtro.Tamanho > 100)
      {
        detalhes.Add(new DetalheErro("tamanho", "valor_invalido"));
      }

      if (detalhes.Count > 0)
      {
        throw new ValidacaoException("Parâmetros de paginação inválidos", detalhes);
      }
  }

}
public sealed record BeneficiarioRequestDados(string? NomeCompleto, string? Cpf, DateOnly? DataNascimento, Guid? PlanoId);
public sealed record BeneficiarioRequestAtualizarDados(string? NomeCompleto, StatusBeneficiario? Status, DateOnly? DataNascimento, Guid? PlanoId);