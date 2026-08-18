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

    public string Cpf { get; private set; } = null!;

    public DateOnly DataNascimento { get; set; }

    public StatusBeneficiario Status { get; set; }

    public Guid PlanoId { get; set; }

    public Plano? Plano { get; set; }

    public DateTime DataCadastro { get; private set; }
    public DateTime? ExcluidoEm { get; private set; }
    public void Excluir() => ExcluidoEm = DateTime.UtcNow;

    public void DefinirDados(string? nomeCompleto, string? cpf, DateOnly? dataNascimento, Guid? planoId)
    {
        cpf = cpf?.Trim() ?? string.Empty;

        var detalhes = ValidarDadosComuns(nomeCompleto, dataNascimento, planoId);

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

       
        if(detalhes.Count > 0)
        {
            if(detalhes.Count ==1 && detalhes[0].Campo == "data_nascimento" && detalhes[0].Regra == "data de nascimento não pode ser futura")
            {
                throw new EntidadeNaoProcessadaException("Data de nascimento não pode ser futura", detalhes);
            }
            throw new ValidacaoException("Dados do beneficiario inválidos", detalhes);
        }

        NomeCompleto = nomeCompleto!;
        Cpf = cpf;
        DataNascimento = dataNascimento!.Value;
        PlanoId = planoId!.Value;
        Status = StatusBeneficiario.ATIVO;
        DataCadastro = DateTime.UtcNow;

    }

    public void Atualizar(
        string? nomeCompleto,
        DateOnly? dataNascimento,
        Guid? planoId,
        StatusBeneficiario? status)
    {

        nomeCompleto = nomeCompleto?.Trim() ?? NomeCompleto;
        dataNascimento = dataNascimento ?? DataNascimento;
        planoId = planoId ?? PlanoId;
        status = status ?? Status;

        var detalhes = ValidarDadosComuns(
            nomeCompleto,
            dataNascimento,
            planoId);

        if (Status == StatusBeneficiario.INATIVO)
        {
            if (status == StatusBeneficiario.ATIVO){
                Status = StatusBeneficiario.ATIVO;
                return;
            }
            throw new ConflitoException("Beneficiário inativo não pode ter seus dados alterados");
        }

        if(detalhes.Count > 0)
        {
            if(detalhes.Count ==1 && detalhes[0].Campo == "data_nascimento" && detalhes[0].Regra == "data de nascimento não pode ser futura")
            {
                throw new EntidadeNaoProcessadaException("Data de nascimento não pode ser futura", detalhes);
            }
            throw new ValidacaoException("Dados do beneficiario inválidos", detalhes);
        }



        NomeCompleto = nomeCompleto!.Trim();
        DataNascimento = dataNascimento!.Value;
        PlanoId = planoId!.Value;
        Status = status!.Value;
    }


    private static List<DetalheErro> ValidarDadosComuns(
        string? nomeCompleto,
        DateOnly? dataNascimento,
        Guid? planoId)
    {
        nomeCompleto = nomeCompleto?.Trim() ?? string.Empty;
        var detalhes = new List<DetalheErro>();

        if(nomeCompleto.Length == 0)
        {
            detalhes.Add(new DetalheErro("nome_completo", "obrigatorio"));
        }
        else if (!FormatoDoNome().IsMatch(nomeCompleto))
        {
            detalhes.Add(new DetalheErro("nome_completo", "formato_invalido"));
        }

        if(dataNascimento is null)
        {
            detalhes.Add(new DetalheErro("data_nascimento", "obrigatorio"));
        }else if (dataNascimento.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            detalhes.Add(new DetalheErro("data_nascimento", "data de nascimento não pode ser futura"));
        }

        if(planoId is null)
        {
            detalhes.Add(new DetalheErro("plano_id", "obrigatorio"));
        }
        return detalhes;
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
