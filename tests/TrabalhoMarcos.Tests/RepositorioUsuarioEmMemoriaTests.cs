using TrabalhoMarcos.Application;
using TrabalhoMarcos.Domain;
using TrabalhoMarcos.Domain.ValueObjects;

namespace TrabalhoMarcos.Tests;

public class RepositorioUsuarioEmMemoriaTests
{
    [Fact]
    public async Task AdicionarAsync_ComMesmoId_LancaInvalidOperationException()
    {
        var repositorio = new RepositorioUsuarioEmMemoria();
        var id = Guid.NewGuid();
        var usuario = Usuario.Criar("Ana Costa", "ana@exemplo.com", "senhaSegura123", id: id);
        var outro = Usuario.Criar("Outra Pessoa", "outra@exemplo.com", "senhaSegura123", id: id);

        await repositorio.AdicionarAsync(usuario);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repositorio.AdicionarAsync(outro));
    }

    [Fact]
    public async Task AdicionarAsync_ComMesmoEmail_LancaInvalidOperationException()
    {
        var repositorio = new RepositorioUsuarioEmMemoria();
        await repositorio.AdicionarAsync(
            Usuario.Criar("Ana Costa", "ana@exemplo.com", "senhaSegura123"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repositorio.AdicionarAsync(
                Usuario.Criar("Outra Ana", "ANA@EXEMPLO.COM", "senhaSegura123")));
    }

    [Fact]
    public async Task ListarAsync_RetornaCopia_AlteracoesNaListaNaoAfetamRepositorio()
    {
        var repositorio = new RepositorioUsuarioEmMemoria();
        await repositorio.AdicionarAsync(
            Usuario.Criar("Ana Costa", "ana@exemplo.com", "senhaSegura123"));

        var lista = (List<Usuario>)await repositorio.ListarAsync();
        lista.Clear();

        Assert.Single(await repositorio.ListarAsync());
    }

    [Fact]
    public async Task ExisteEmailAsync_ConsideraEmailNormalizado()
    {
        var repositorio = new RepositorioUsuarioEmMemoria();
        await repositorio.AdicionarAsync(
            Usuario.Criar("Ana Costa", "ana@exemplo.com", "senhaSegura123"));

        Assert.True(await repositorio.ExisteEmailAsync(Email.Criar(" ANA@exemplo.com ")));
        Assert.False(await repositorio.ExisteEmailAsync(Email.Criar("outro@exemplo.com")));
    }

    [Fact]
    public async Task RemoverAsync_ComUsuarioInexistente_RetornaFalse()
    {
        var repositorio = new RepositorioUsuarioEmMemoria();

        Assert.False(await repositorio.RemoverAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Operacoes_ComTokenCancelado_LancamOperationCanceledException()
    {
        var repositorio = new RepositorioUsuarioEmMemoria();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            repositorio.ListarAsync(cts.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            repositorio.RemoverAsync(Guid.NewGuid(), cts.Token));
    }
}
