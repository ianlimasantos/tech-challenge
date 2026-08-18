using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Desafio.Api.Dominio;
using Microsoft.AspNetCore.Mvc;

namespace Desafio.Api.Api.Contratos
{
    public sealed record BeneficiarioFiltro(
    StatusBeneficiario? Status,
    [param: FromQuery(Name = "plano_id")]
    Guid? PlanoId,
    int Pagina = 1,
    int Tamanho = 10);

    public sealed record PaginaResponse<T>(
    IReadOnlyList<T> Dados,
    int Pagina,
    int Tamanho,
    int Total);
}