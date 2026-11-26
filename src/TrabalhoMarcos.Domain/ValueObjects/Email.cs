using System.Text.RegularExpressions;

namespace TrabalhoMarcos.Domain.ValueObjects;

/// <summary>
/// Endereço de e-mail normalizado (minúsculas e sem espaços nas bordas).
/// </summary>
public sealed record Email
{
    public const int TamanhoMaximo = 254;

    private static readonly Regex Formato =
        new(@"^[^@\s]+@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z]{2,}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Valor { get; }

    private Email(string valor) => Valor = valor;

    public static Email Criar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DomainException("O e-mail é obrigatório.");
        }

        var normalizado = valor.Trim().ToLowerInvariant();

        if (normalizado.Length > TamanhoMaximo)
        {
            throw new DomainException($"O e-mail não pode exceder {TamanhoMaximo} caracteres.");
        }

        if (!Formato.IsMatch(normalizado))
        {
            throw new DomainException("O e-mail informado não é válido.");
        }

        return new Email(normalizado);
    }

    public override string ToString() => Valor;
}
