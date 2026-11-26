using TrabalhoMarcos.Domain;
using TrabalhoMarcos.Domain.ValueObjects;

namespace TrabalhoMarcos.Application;

/// <summary>
/// Contrato de persistência de usuários. A aplicação depende desta abstração,
/// não de uma implementação concreta.
/// </summary>
public interface IRepositorioUsuario
{
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);

    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken = default);

    Task<bool> RemoverAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExisteEmailAsync(Email email, CancellationToken cancellationToken = default);
}
