using TrabalhoMarcos.Application;
using TrabalhoMarcos.Domain;

namespace TrabalhoMarcos.Tests;

public class ServicoCadastroUsuarioTests
{
    private static ServicoCadastroUsuario CriarServico()
        => new(new RepositorioUsuarioEmMemoria());

    [Fact]
    public async Task CadastrarAsync_ComDadosValidos_PersisteERetornaUsuario()
    {
        var servico = CriarServico();

        var usuario = await servico.CadastrarAsync("Maria Silva", "maria@exemplo.com", "senhaSegura123");

        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal("Maria Silva", usuario.Nome.Valor);
        Assert.Equal("maria@exemplo.com", usuario.Email.Valor);
        Assert.True(usuario.VerificarSenha("senhaSegura123"));

        var encontrado = await servico.BuscarPorEmailAsync("MARIA@EXEMPLO.COM");
        Assert.NotNull(encontrado);
        Assert.Equal(usuario.Id, encontrado.Id);
    }

    [Fact]
    public async Task CadastrarAsync_ComEmailDuplicado_LancaEmailJaCadastradoException()
    {
        var servico = CriarServico();
        await servico.CadastrarAsync("Maria Silva", "maria@exemplo.com", "senhaSegura123");

        var excecao = await Assert.ThrowsAsync<EmailJaCadastradoException>(() =>
            servico.CadastrarAsync("Outra Maria", " MARIA@Exemplo.com ", "outraSenha456"));

        Assert.Equal("maria@exemplo.com", excecao.Email.Valor);
        Assert.Single(await servico.ListarAsync());
    }

    [Theory]
    [InlineData("ab", "valido@exemplo.com", "senhaSegura123")]
    [InlineData("Nome Valido", "invalido", "senhaSegura123")]
    [InlineData("Nome Valido", "valido@exemplo.com", "curta")]
    public async Task CadastrarAsync_ComDadosInvalidos_LancaDomainExceptionENaoPersiste(
        string nome,
        string email,
        string senha)
    {
        var servico = CriarServico();

        await Assert.ThrowsAsync<DomainException>(() =>
            servico.CadastrarAsync(nome, email, senha));

        Assert.Empty(await servico.ListarAsync());
    }

    [Fact]
    public async Task BuscarPorIdAsync_RetornaUsuarioQuandoExiste()
    {
        var servico = CriarServico();
        var usuario = await servico.CadastrarAsync("João Souza", "joao@exemplo.com", "senhaSegura123");

        var encontrado = await servico.BuscarPorIdAsync(usuario.Id);

        Assert.NotNull(encontrado);
        Assert.Equal(usuario.Id, encontrado.Id);
        Assert.Null(await servico.BuscarPorIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task BuscarPorEmailAsync_QuandoNaoExiste_RetornaNulo()
    {
        var servico = CriarServico();

        Assert.Null(await servico.BuscarPorEmailAsync("ninguem@exemplo.com"));
    }

    [Fact]
    public async Task ListarAsync_RetornaTodosOrdenadosPorDataENome()
    {
        var servico = CriarServico();
        var baseTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var repositorio = new RepositorioUsuarioEmMemoria();
        var servicoComDatas = new ServicoCadastroUsuario(repositorio);

        var ultimo = Usuario.Criar("Zoe Lima", "zoe@exemplo.com", "senhaSegura123", baseTime.AddDays(2));
        var primeiro = Usuario.Criar("Ana Costa", "ana@exemplo.com", "senhaSegura123", baseTime);
        var segundo = Usuario.Criar("Bruno Alves", "bruno@exemplo.com", "senhaSegura123", baseTime.AddDays(1));

        await repositorio.AdicionarAsync(ultimo);
        await repositorio.AdicionarAsync(primeiro);
        await repositorio.AdicionarAsync(segundo);

       var lista = await servicoComDatas.ListarAsync();

        Assert.Equal(3, lista.Count);
        Assert.Equal(new[] { primeiro.Id, segundo.Id, ultimo.Id }, lista.Select(u => u.Id));
        Assert.Empty(await servico.ListarAsync());
    }

    [Fact]
    public async Task RemoverAsync_RemoveUsuarioEPermiteReutilizarEmail()
    {
        var servico = CriarServico();
        var usuario = await servico.CadastrarAsync("Carla Dias", "carla@exemplo.com", "senhaSegura123");

        await servico.RemoverAsync(usuario.Id);

        Assert.Null(await servico.BuscarPorIdAsync(usuario.Id));
        Assert.Empty(await servico.ListarAsync());

        var novo = await servico.CadastrarAsync("Carla Nova", "carla@exemplo.com", "outraSenha456");
        Assert.NotEqual(usuario.Id, novo.Id);
    }

    [Fact]
    public async Task RemoverAsync_ComIdInexistente_LancaUsuarioNaoEncontradoException()
    {
        var servico = CriarServico();
        var id = Guid.NewGuid();

        var excecao = await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(() =>
            servico.RemoverAsync(id));

        Assert.Equal(id, excecao.Id);
    }
}
