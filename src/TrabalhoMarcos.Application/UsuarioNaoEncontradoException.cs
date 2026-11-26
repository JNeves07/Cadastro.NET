namespace TrabalhoMarcos.Application;

/// <summary>
/// Indica que o identificador informado não corresponde a nenhum usuário.
/// </summary>
public sealed class UsuarioNaoEncontradoException : Exception
{
    public UsuarioNaoEncontradoException(Guid id)
        : base($"Nenhum usuário foi encontrado para o identificador '{id}'.")
    {
        Id = id;
    }

    public Guid Id { get; }
}
