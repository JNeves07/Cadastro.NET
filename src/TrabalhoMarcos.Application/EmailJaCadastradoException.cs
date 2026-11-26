using TrabalhoMarcos.Domain.ValueObjects;

namespace TrabalhoMarcos.Application;

/// <summary>
/// Indica uma tentativa de cadastro com e-mail já existente.
/// </summary>
public sealed class EmailJaCadastradoException : Exception
{
    public EmailJaCadastradoException(Email email)
        : base($"O e-mail '{email}' já está cadastrado.")
    {
        Email = email;
    }

    public Email Email { get; }
}
