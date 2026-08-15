using Desafio.Api.Api.Contratos;
using Desafio.Api.Aplicacao;
using Microsoft.AspNetCore.Mvc;

namespace Desafio.Api.Controllers;

[ApiController]
[Route("beneficiarios")]
[Produces("application/json")]
public class BeneficiariosController(BeneficiarioServico servico) : ControllerBase
{
    
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] BeneficiarioFiltro filtro,
        CancellationToken cancellationToken)
    {
        var resultado = await servico.ListarAsync(filtro,cancellationToken);

        return Ok(resultado);
    }


    [HttpGet("{id:guid}")]
    [ProducesResponseType<PlanoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(Guid id, CancellationToken cancellationToken)
    {
        var plano = await servico.ObterAsync(id, cancellationToken);

        return Ok(BeneficiarioResponse.De(plano));
    }

    [HttpPost]
    [ProducesResponseType<BeneficiarioResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar([FromBody] BeneficiarioRequest requisicao, CancellationToken cancellationToken)
    {
        var dados = new BeneficiarioRequestDados(requisicao.NomeCompleto, requisicao.Cpf, requisicao.DataNascimento, requisicao.PlanoId);
        var beneficiario = await servico.CriarAsync(dados, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = beneficiario.Id }, BeneficiarioResponse.De(beneficiario));     
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<PlanoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] BeneficiarioAtualizarRequest requisicao,
        CancellationToken cancellationToken)
    {
        BeneficiarioRequestAtualizarDados dados = new BeneficiarioRequestAtualizarDados(requisicao.NomeCompleto, requisicao.Status, requisicao.DataNascimento, requisicao.PlanoId);
        var beneficiario = await servico.AtualizarAsync(id, dados, cancellationToken);

        return Ok(BeneficiarioResponse.De(beneficiario));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await servico.ExcluirAsync(id, cancellationToken);

        return NoContent();
    }

}
