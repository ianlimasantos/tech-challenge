using System.Text.RegularExpressions;
namespace Desafio.Api.Dominio;

public enum StatusBeneficiario
{
    ATIVO,
    INATIVO
}

public partial class Beneficiario
{

    private Beneficiario()
    {
    }

    public Beneficiario(string? nomeCompleto, string? cpf, DateOnly? dataNascimento, Guid? planoId) 
        : this(Guid.NewGuid(), nomeCompleto, cpf, dataNascimento, planoId )
    { 
    }

    public Beneficiario(Guid id, string? nomeCompleto, string? cpf, DateOnly? dataNascimento, Guid? planoId)
    {
        Id = id;
        DefinirDados(nomeCompleto, cpf, dataNascimento, planoId);
    }

    public Guid Id { get; set; }

    public string NomeCompleto { get; set; } = null!;

    public string Cpf { get; set; } = null!;

    public DateOnly DataNascimento { get; set; }

    public StatusBeneficiario Status { get; private set; }

    public Guid PlanoId { get; set; }

    public Plano? Plano { get; set; }

    public DateTime DataCadastro { get; private set; }

    public void DefinirDados(string? nomeCompleto, string? cpf, DateOnly? dataNascimento, Guid? planoId)
    {
        nomeCompleto = nomeCompleto?.Trim() ?? string.Empty;
        cpf = cpf?.Trim() ?? string.Empty;

        var detalhes = new List<DetalheErro>();

        if(nomeCompleto.Length == 0)
        {
            detalhes.Add(new DetalheErro("nome_completo", "obrigatorio"));
        }
        else if (!FormatoDoNome().IsMatch(nomeCompleto))
        {
            detalhes.Add(new DetalheErro("nome_completo", "formato_invalido"));
        }

        if(cpf.Length == 0)
        {
            detalhes.Add(new DetalheErro("cpf", "obrigatorio"));
        }
        else if (!FormatoDoCpf().IsMatch(cpf) || cpf.All(c => c == cpf[0]))
        {
            detalhes.Add(new DetalheErro("cpf", "formato_invalido"));
        }
        else if (!CpfValido(cpf))
        {
            detalhes.Add(new DetalheErro("cpf", "formato_invalido"));
        }

        if(dataNascimento is null)
        {
            detalhes.Add(new DetalheErro("data_nascimento", "obrigatorio"));
        }else if (dataNascimento.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            detalhes.Add(new DetalheErro("data_nascimento", "data_nascimento_nao_pode_ser_futura"));
        }


        if(planoId is null)
        {
            detalhes.Add(new DetalheErro("plano_id", "obrigatorio"));
        }

        if(detalhes.Count > 0)
        {
            if(detalhes.Count ==1 && detalhes[0].Campo == "data_nascimento" && detalhes[0].Regra == "data_nascimento_nao_pode_ser_futura")
            {
                throw new EntidadeNaoProcessadaException("Data de nascimento não pode ser futura", detalhes);
            }
            throw new ValidacaoException("Dados do beneficiario inválidos", detalhes);
        }

        NomeCompleto = nomeCompleto;
        Cpf = cpf;
        DataNascimento = dataNascimento!.Value;
        PlanoId = planoId!.Value;
        Status = StatusBeneficiario.ATIVO;
        DataCadastro = DateTime.UtcNow;




    }

    private static bool CpfValido(string cpf)
    {
        
        
        var digitos = cpf
            .Select(c => c - '0')
            .ToArray();

        var primeiroDigito = CalcularDigito(digitos, 9);

        if (digitos[9] != primeiroDigito)
            return false;

        var segundoDigito = CalcularDigito(digitos, 10);

        return digitos[10] == segundoDigito;
    }

    private static int CalcularDigito(int[] digitos, int quantidade)
    {
        var soma = 0;

        for (var i = 0; i < quantidade; i++)
        {
            soma += digitos[i] * (quantidade + 1 - i);
        }

        var resto = soma % 11;

        return resto < 2 ? 0 : 11 - resto;
    }

    [GeneratedRegex("^[0-9]{11}$")]
    private static partial Regex FormatoDoCpf();

    [GeneratedRegex(@"^[\p{L}\s]+$")]
    private static partial Regex FormatoDoNome();
}
