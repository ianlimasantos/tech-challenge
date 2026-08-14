using Desafio.Api.Api.Contratos;
using Desafio.Api.Dominio;
using Desafio.Api.Infraestrutura;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Desafio.Api.Aplicacao;

public class BeneficiarioServico(AppDbContext db, PlanoServico planoServico)
{
  
    private const string CodigoViolacaoDeUnicidade = "23505";


    public async Task<Beneficiario> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db.Beneficiarios.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
               ?? throw new NaoEncontradoException("Beneficiario não encontrado");
    }

    public async Task<Beneficiario> CriarAsync(BeneficiarioRequestDados dados, CancellationToken cancellationToken)
    {
        await planoServico.ObterAsync(dados.PlanoId ?? Guid.Empty, cancellationToken); 
        var beneficiario = new Beneficiario(dados.NomeCompleto, dados.Cpf, dados.DataNascimento, dados.PlanoId);

        await GarantirUnicidadeAsync(beneficiario, cancellationToken);
        db.Beneficiarios.Add(beneficiario);
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

    private static bool EhViolacaoDeUnicidade(DbUpdateException excecao) =>
        excecao.InnerException is PostgresException postgres &&
        postgres.SqlState == CodigoViolacaoDeUnicidade;
}

public sealed record BeneficiarioRequestDados(string? NomeCompleto, string? Cpf, DateOnly? DataNascimento, Guid? PlanoId);
