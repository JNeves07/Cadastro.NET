using TrabalhoMarcos.Domain;
using TrabalhoMarcos.Domain.ValueObjects;

namespace TrabalhoMarcos.Application;

/// <summary>
/// Repositório em memória, seguro para uso concorrente.
/// Mantém índices por identificador e por e-mail normalizado.
/// </summary>
public sealed class RepositorioUsuarioEmMemoria : IRepositorioUsuario
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, Usuario> _porId = new();
    private readonly Dictionary<string, Guid> _porEmail = new(StringComparer.Ordinal);

    public Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            if (_porId.ContainsKey(usuario.Id))
            {
                throw new InvalidOperationException("Já existe um usuário com este identificador.");
            }

            if (_porEmail.ContainsKey(usuario.Email.Valor))
            {
                throw new InvalidOperationException("Já existe um usuário com este e-mail.");
            }

            _porId.Add(usuario.Id, usuario);
            _porEmail.Add(usuario.Email.Valor, usuario.Id);
        }

        return Task.CompletedTask;
    }

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            return Task.FromResult(_porId.GetValueOrDefault(id));
        }
    }

    public Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            if (!_porEmail.TryGetValue(email.Valor, out var id))
            {
                return Task.FromResult<Usuario?>(null);
            }

            return Task.FromResult(_porId.GetValueOrDefault(id));
        }
    }

    public Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            IReadOnlyList<Usuario> copia = _porId.Values.ToList();
            return Task.FromResult(copia);
        }
    }

    public Task<bool> RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            if (!_porId.TryGetValue(id, out var usuario))
            {
                return Task.FromResult(false);
            }

            _porId.Remove(id);
            _porEmail.Remove(usuario.Email.Valor);
            return Task.FromResult(true);
        }
    }

    public Task<bool> ExisteEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            return Task.FromResult(_porEmail.ContainsKey(email.Valor));
        }
    }
}
