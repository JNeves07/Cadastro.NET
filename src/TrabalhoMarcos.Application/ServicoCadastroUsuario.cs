using TrabalhoMarcos.Domain;
using TrabalhoMarcos.Domain.ValueObjects;

namespace TrabalhoMarcos.Application;

/// <summary>
/// Casos de uso de cadastro, consulta, listagem e remoção de usuários.
/// </summary>
public sealed class ServicoCadastroUsuario
{
    private readonly IRepositorioUsuario _repositorio;

    public ServicoCadastroUsuario(IRepositorioUsuario repositorio)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
    }

    public async Task<Usuario> CadastrarAsync(
        string nome,
        string email,
        string senha,
        CancellationToken cancellationToken = default)
    {
        var emailNormalizado = Email.Criar(email);

        if (await _repositorio.ExisteEmailAsync(emailNormalizado, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new EmailJaCadastradoException(emailNormalizado);
        }

        var usuario = Usuario.Criar(Nome.Criar(nome), emailNormalizado, senha);

        await _repositorio.AdicionarAsync(usuario, cancellationToken).ConfigureAwait(false);

        return usuario;
    }

    public Task<Usuario?> BuscarPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => _repositorio.ObterPorEmailAsync(Email.Criar(email), cancellationToken);

    public Task<Usuario?> BuscarPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _repositorio.ObterPorIdAsync(id, cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        var usuarios = await _repositorio.ListarAsync(cancellationToken).ConfigureAwait(false);

        return usuarios
            .OrderBy(u => u.CriadoEm)
            .ThenBy(u => u.Nome.Valor, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removido = await _repositorio.RemoverAsync(id, cancellationToken).ConfigureAwait(false);

        if (!removido)
        {
            throw new UsuarioNaoEncontradoException(id);
        }
    }
}
