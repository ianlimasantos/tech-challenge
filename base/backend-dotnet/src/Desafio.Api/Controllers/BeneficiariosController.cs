using Desafio.Api.Api.Contratos;
using Desafio.Api.Aplicacao;
using Microsoft.AspNetCore.Mvc;


namespace Desafio.Api.Controllers;

[ApiController]
[Route("beneficiarios")]
[Produces("application/json")]
public class BeneficiariosController(BeneficiarioServico servico) : ControllerBase
{
    

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
                            // if (beneficiario.Cpf?.Length == 11)
                            // {
                            //     // Mesmo modelo do PlanoServico: a garantia de unicidade é o índice único da
                            //     // tabela, e esta consulta prévia existe só para recusar o pedido antes de ele
                            //     // chegar no banco.
                            //     var existe = _db.Beneficiarios.Any(b => b.Cpf == beneficiario.Cpf);

                            //     if (!existe)
                            //     {
                            //         _db.Beneficiarios.Add(beneficiario);
                            //         await _db.SaveChangesAsync();

                            //         return Ok(beneficiario);
                            //     }

                            //     return BadRequest("CPF ja cadastrado");
                            // }

        
        //return BadRequest("CPF invalido");
    }

    // [HttpGet]
    // public async Task<IActionResult> Listar()
    // {
        //var lista = await _db.Beneficiarios.ToListAsync();

        // O plano é resolvido aqui, e não na consulta principal, porque o FindAsync usa o
        // cache do contexto: a listagem continua fazendo uma única ida ao banco, qualquer
        // que seja o tamanho da página.
        
        
        // foreach (var b in lista)
        // {
        //     b.Plano = await _db.Planos.FindAsync(b.PlanoId);
        // }

        // return Ok(lista);
    //}
}
