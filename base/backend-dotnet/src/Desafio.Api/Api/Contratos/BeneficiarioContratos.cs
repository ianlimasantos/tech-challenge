using Desafio.Api.Dominio;

namespace Desafio.Api.Api.Contratos;

public sealed record BeneficiarioRequest(string? NomeCompleto, string? Cpf, DateOnly? DataNascimento, Guid? PlanoId);

public sealed record BeneficiarioResponse(Guid Id, string NomeCompleto, string Cpf, DateOnly DataNascimento, Guid? PlanoId)
{
    public static BeneficiarioResponse De(Beneficiario beneficiario) =>
        new(beneficiario.Id, beneficiario.NomeCompleto, beneficiario.Cpf, beneficiario.DataNascimento, beneficiario.PlanoId);
}
