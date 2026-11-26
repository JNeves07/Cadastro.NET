using System.Text.RegularExpressions;

namespace TrabalhoMarcos.Domain.ValueObjects;

/// <summary>
/// Nome civil de uma pessoa. Value object imutável que garante um nome válido.
/// </summary>
public sealed record Nome
{
    public const int TamanhoMinimo = 3;
    public const int TamanhoMaximo = 120;

    private static readonly Regex CaracteresPermitidos =
        new(@"^[\p{L}\s'-]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Valor { get; }

    private Nome(string valor) => Valor = valor;

    public static Nome Criar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DomainException("O nome é obrigatório.");
        }

        var normalizado = Regex.Replace(valor.Trim(), @"\s+", " ");

        if (normalizado.Length is < TamanhoMinimo or > TamanhoMaximo)
        {
            throw new DomainException(
                $"O nome deve ter entre {TamanhoMinimo} e {TamanhoMaximo} caracteres.");
        }

        if (!CaracteresPermitidos.IsMatch(normalizado))
        {
            throw new DomainException(
                "O nome deve conter apenas letras, espaços, hífens ou apóstrofos.");
        }

        return new Nome(normalizado);
    }

    public override string ToString() => Valor;
}
