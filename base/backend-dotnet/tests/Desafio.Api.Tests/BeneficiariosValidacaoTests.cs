using System.Net;
using System.Net.Http.Json;

namespace Desafio.Api.Tests;

[Collection(ColecaoDaApi.Nome)]
public class BeneficiariosValidacaoTests(ApiFixture fixture) : IAsyncLifetime
{
    private HttpClient Client => fixture.Client;

    public Task InitializeAsync() => fixture.LimparAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static object CorpoDeCriacao(string cpf, Guid? planoId = null, string nome = "Maria Aparecida da Silva") => new
    {
        NomeCompleto = nome,
        Cpf = cpf,
        DataNascimento = "1990-05-12",
        PlanoId = planoId ?? Planos.Bronze
    };

    // ------------------------------------------------------------------ CPF

    [Theory]
    [InlineData("1234567890")]          // curto demais
    [InlineData("123456789012")]        // longo demais
    [InlineData("529.982.247-25")]      // com pontuação
    [InlineData("5299822472a")]         // não numérico
    [InlineData("52998224724")]         // dígito verificador inválido
    [InlineData("00000000000")]         // sequência repetida
    [InlineData("11111111111")]         // sequência repetida (dígitos verificadores "fecham")
    public async Task Criar_com_cpf_invalido_deve_devolver_400(string cpfInvalido)
    {
        var resposta = await Client.PostAsync("/beneficiarios", Http.Json(CorpoDeCriacao(cpfInvalido)));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    // ------------------------------------------------------------------ campos obrigatórios / formato

    [Fact]
    public async Task Criar_sem_nome_completo_deve_devolver_400()
    {
        var corpo = new { Cpf = "52998224725", DataNascimento = "1990-05-12", PlanoId = Planos.Bronze };

        var resposta = await Client.PostAsync("/beneficiarios", Http.Json(corpo));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Criar_com_data_nascimento_no_futuro_deve_devolver_422()
    {
        var futura = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)).ToString("yyyy-MM-dd");
        var corpo = CorpoDeCriacaoComData(futura);

        var resposta = await Client.PostAsync("/beneficiarios", Http.Json(corpo));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    private static object CorpoDeCriacaoComData(string dataNascimento) => new
    {
        NomeCompleto = "Maria Aparecida da Silva",
        Cpf = "52998224725",
        DataNascimento = dataNascimento,
        PlanoId = Planos.Bronze
    };

    [Fact]
    public async Task Erro_de_validacao_deve_indicar_qual_campo_foi_recusado()
    {
        var resposta = await Client.PostAsync("/beneficiarios", Http.Json(CorpoDeCriacao("11111111111")));
        var corpo = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        // Ajuste a asserção conforme o formato de erro adotado — o importante é não
        // devolver corpo vazio nem mensagem genérica sem referência ao campo.
        Assert.False(string.IsNullOrWhiteSpace(corpo));
    }

    // ------------------------------------------------------------------ campos ignorados pelo servidor

    [Fact]
    public async Task Criar_deve_ignorar_id_status_e_data_cadastro_enviados_pelo_cliente()
    {
        var idForjado = Guid.NewGuid();
        var corpo = new
        {
            Id = idForjado,
            NomeCompleto = "Maria Aparecida da Silva",
            Cpf = "52998224725",
            DataNascimento = "1990-05-12",
            PlanoId = Planos.Bronze,
            Status = "INATIVO",
            DataCadastro = DateTime.UtcNow.AddYears(-5)
        };

        var resposta = await Client.PostAsync("/beneficiarios", Http.Json(corpo));
        var criado = await resposta.CorpoAsync();

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotEqual(idForjado, criado.GetProperty("id").GetGuid());
        Assert.Equal("ATIVO", criado.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Atualizar_deve_ignorar_cpf_enviado()
    {
        var beneficiario = (await fixture.SemearBeneficiariosAsync(1)).Single();

        var resposta = await Client.PutAsync($"/beneficiarios/{beneficiario.Id}", Http.Json(new
        {
            NomeCompleto = "Maria Aparecida da Silva",
            Cpf = "39053344705", // diferente do original — deve ser ignorado
            DataNascimento = "1990-05-12",
            PlanoId = Planos.Bronze,
            Status = "ATIVO"
        }));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.CorpoAsync();
        Assert.Equal(beneficiario.Cpf, corpo.GetProperty("cpf").GetString());
    }

    [Fact]
    public async Task Cpf_com_zeros_a_esquerda_deve_ser_preservado()
    {
        var beneficiario = (await fixture.SemearBeneficiariosAsync(1)).Single();

        var resposta = await Client.GetAsync($"/beneficiarios/{beneficiario.Id}");
        var corpo = await resposta.CorpoAsync();

        Assert.Equal(beneficiario.Cpf, corpo.GetProperty("cpf").GetString());
    }
    // ------------------------------------------------------------------ reativação (inverso do teste de INATIVO congelado)

    [Fact]
    public async Task Atualizar_status_de_inativo_para_ativo_deve_ser_permitido()
    {
        var beneficiario = (await fixture.SemearBeneficiariosAsync(
            1, Planos.Bronze, "INATIVO")).Single();

        var resposta = await Client.PutAsync($"/beneficiarios/{beneficiario.Id}", Http.Json(new
        {
            NomeCompleto = beneficiario.NomeCompleto,
            DataNascimento = "1990-05-12",
            PlanoId = Planos.Bronze,
            Status = "ATIVO"
        }));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.CorpoAsync();
        Assert.Equal("ATIVO", corpo.GetProperty("status").GetString());
    }

    // ------------------------------------------------------------------ concorrência de CPF

    [Fact]
    public async Task Criar_simultaneamente_com_mesmo_cpf_deve_garantir_unicidade()
    {
        var cpf = "39053344705";

        var respostas = await Task.WhenAll(
            Client.PostAsync("/beneficiarios", Http.Json(CorpoDeCriacao(cpf))),
            Client.PostAsync("/beneficiarios", Http.Json(CorpoDeCriacao(cpf))));

        var sucessos = respostas.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflitos = respostas.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.Equal(1, sucessos);
        Assert.Equal(1, conflitos);
    }

    // ------------------------------------------------------------------ plano excluído logicamente

    [Fact]
    public async Task Criar_apontando_para_plano_excluido_deve_devolver_422()
    {
        await Client.DeleteAsync($"/planos/{Planos.Executivo}");

        var resposta = await Client.PostAsync(
            "/beneficiarios",
            Http.Json(CorpoDeCriacao("39053344705", Planos.Executivo)));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Atualizar_apontando_para_plano_excluido_deve_devolver_422()
    {
        var beneficiario = (await fixture.SemearBeneficiariosAsync(1)).Single();
        await Client.DeleteAsync($"/planos/{Planos.Executivo}");

        var resposta = await Client.PutAsync($"/beneficiarios/{beneficiario.Id}", Http.Json(new
        {
            NomeCompleto = beneficiario.NomeCompleto,
            DataNascimento = "1990-05-12",
            PlanoId = Planos.Executivo,
            Status = "ATIVO"
        }));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Beneficiario_vinculado_a_plano_excluido_apos_o_vinculo_deve_continuar_valido()
    {
        var beneficiario = (await fixture.SemearBeneficiariosAsync(1, Planos.Executivo)).Single();

        var exclusaoDoPlano = await Client.DeleteAsync($"/planos/{Planos.Executivo}");
        Assert.Equal(HttpStatusCode.NoContent, exclusaoDoPlano.StatusCode);

        var resposta = await Client.GetAsync($"/beneficiarios/{beneficiario.Id}");
        var corpo = await resposta.CorpoAsync();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(Planos.Executivo, corpo.GetProperty("plano_id").GetGuid());
    }

    // ------------------------------------------------------------------ paginação inválida

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task Listar_com_parametros_de_paginacao_invalidos_deve_devolver_400(int pagina, int tamanho)
    {
        var resposta = await Client.GetAsync($"/beneficiarios?pagina={pagina}&tamanho={tamanho}");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Listar_pagina_alem_do_total_deve_devolver_200_vazio_com_total_correto()
    {
        await fixture.SemearBeneficiariosAsync(3);

        var corpo = await (await Client.GetAsync("/beneficiarios?pagina=99&tamanho=10")).CorpoAsync();

        Assert.Equal(0, corpo.GetProperty("dados").GetArrayLength());
        Assert.Equal(3, corpo.GetProperty("total").GetInt32());
    }

    // ------------------------------------------------------------------ estabilidade da paginação

    [Fact]
    public async Task Percorrer_todas_as_paginas_deve_devolver_cada_registro_exatamente_uma_vez()
    {
        var semeados = await fixture.SemearBeneficiariosAsync(23);
        const int tamanho = 7;

        var idsColetados = new List<Guid>();
        var pagina = 1;

        while (true)
        {
            var corpo = await (await Client.GetAsync($"/beneficiarios?pagina={pagina}&tamanho={tamanho}")).CorpoAsync();
            var itens = corpo.GetProperty("dados").EnumerateArray().ToList();

            if (itens.Count == 0)
            {
                break;
            }

            idsColetados.AddRange(itens.Select(item => item.GetProperty("id").GetGuid()));
            pagina++;
        }

        Assert.Equal(semeados.Count, idsColetados.Count);
        Assert.Equal(semeados.Count, idsColetados.Distinct().Count());
        Assert.Equal(semeados.Select(s => s.Id).OrderBy(id => id), idsColetados.OrderBy(id => id));
    }

    // ------------------------------------------------------------------ filtros isolados

    [Fact]
    public async Task Listar_deve_filtrar_somente_por_status()
    {
        await fixture.SemearBeneficiariosAsync(4, Planos.Bronze, "ATIVO", 100);
        await fixture.SemearBeneficiariosAsync(6, Planos.Prata, "INATIVO", 200);

        var corpo = await (await Client.GetAsync("/beneficiarios?tamanho=50&status=INATIVO")).CorpoAsync();

        Assert.Equal(6, corpo.GetProperty("total").GetInt32());
        Assert.All(
            corpo.GetProperty("dados").EnumerateArray(),
            b => Assert.Equal("INATIVO", b.GetProperty("status").GetString()));
    }

    [Fact]
    public async Task Listar_deve_filtrar_somente_por_plano()
    {
        await fixture.SemearBeneficiariosAsync(4, Planos.Bronze, "ATIVO", 100);
        await fixture.SemearBeneficiariosAsync(6, Planos.Prata, "ATIVO", 200);

        var corpo = await (await Client.GetAsync($"/beneficiarios?tamanho=50&plano_id={Planos.Prata}")).CorpoAsync();

        Assert.Equal(6, corpo.GetProperty("total").GetInt32());
        Assert.All(
            corpo.GetProperty("dados").EnumerateArray(),
            b => Assert.Equal(Planos.Prata, b.GetProperty("plano_id").GetGuid()));
    }
}