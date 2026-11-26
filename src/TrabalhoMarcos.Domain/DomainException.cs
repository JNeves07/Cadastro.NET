namespace TrabalhoMarcos.Domain;

/// <summary>
/// Exceção base para violações das regras de negócio do domínio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
