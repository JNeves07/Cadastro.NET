using TrabalhoMarcos.Domain;
using TrabalhoMarcos.Domain.ValueObjects;

namespace TrabalhoMarcos.Tests;

public class UsuarioTests
{
    [Fact]
    public void Nome_Criar_NormalizaEspacosERetornaValor()
    {
        var nome = Nome.Criar("  Maria   Clara  ");

        Assert.Equal("Maria Clara", nome.Valor);
        Assert.Equal("Maria Clara", nome.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")]
    [InlineData("Jo@o")]
    [InlineData("123")]
    public void Nome_Criar_ComValorInvalido_LancaDomainException(string? valor)
    {
        Assert.Throws<DomainException>(() => Nome.Criar(valor));
    }

    [Fact]
    public void Nome_Criar_ComMaisDe120Caracteres_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => Nome.Criar(new string('a', 121)));
    }

    [Fact]
    public void Email_Criar_NormalizaParaMinusculas()
    {
        var email = Email.Criar("  usuario@Exemplo.COM ");

        Assert.Equal("usuario@exemplo.com", email.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sem-arroba")]
    [InlineData("a@b")]
    [InlineData("a @b.com")]
    [InlineData("a@b..com")]
    public void Email_Criar_ComValorInvalido_LancaDomainException(string? valor)
    {
        Assert.Throws<DomainException>(() => Email.Criar(valor));
    }

    [Fact]
    public void SenhaHash_Criar_GeraSaltDiferenteParaMesmaSenhaEVerificaCorretamente()
    {
        var primeiro = SenhaHash.Criar("senhaSegura123");
        var segundo = SenhaHash.Criar("senhaSegura123");

        Assert.NotEqual(primeiro.Salt, segundo.Salt);
        Assert.NotEqual(primeiro.Hash, segundo.Hash);
        Assert.Equal(100_000, primeiro.Iteracoes);

        Assert.True(primeiro.Verificar("senhaSegura123"));
        Assert.False(primeiro.Verificar("senhaErrada"));
        Assert.False(primeiro.Verificar(string.Empty));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("curta")]
    public void SenhaHash_Criar_ComSenhaInvalida_LancaDomainException(string? senha)
    {
        Assert.Throws<DomainException>(() => SenhaHash.Criar(senha));
    }

    [Fact]
    public void SenhaHash_Restaurar_ReconstroiHashPersistido()
    {
        var original = SenhaHash.Criar("senhaSegura123");

        var restaurado = SenhaHash.Restaurar(original.Salt, original.Hash, original.Iteracoes);

        Assert.True(restaurado.Verificar("senhaSegura123"));
        Assert.Equal(original.ToString(), restaurado.ToString());
    }

    [Fact]
    public void Usuario_Criar_PreencheDadosEValoresPadrao()
    {
        var criadoEm = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var id = Guid.NewGuid();

        var usuario = Usuario.Criar("Ana Costa", "ANA@exemplo.com", "senhaSegura123", criadoEm, id);

        Assert.Equal(id, usuario.Id);
        Assert.Equal("Ana Costa", usuario.Nome.Valor);
        Assert.Equal("ana@exemplo.com", usuario.Email.Valor);
        Assert.Equal(criadoEm, usuario.CriadoEm);
        Assert.True(usuario.VerificarSenha("senhaSegura123"));
        Assert.False(usuario.VerificarSenha("outraSenha"));
    }

    [Fact]
    public void Usuario_Criar_ComIdVazio_LancaDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Usuario.Criar("Ana Costa", "ana@exemplo.com", "senhaSegura123", id: Guid.Empty));
    }

    [Fact]
    public void Usuario_Alteracoes_AplicamValidacaoENovosValores()
    {
        var usuario = Usuario.Criar("Ana Costa", "ana@exemplo.com", "senhaSegura123");

        usuario.AlterarNome("  AnaMaria  Costa ");
        usuario.AlterarEmail("NOVO@exemplo.com");
        usuario.AlterarSenha("novaSenha789");

        Assert.Equal("AnaMaria Costa", usuario.Nome.Valor);
        Assert.Equal("novo@exemplo.com", usuario.Email.Valor);
        Assert.True(usuario.VerificarSenha("novaSenha789"));
        Assert.False(usuario.VerificarSenha("senhaSegura123"));
        Assert.Throws<DomainException>(() => usuario.AlterarEmail("invalido"));
    }

    [Fact]
    public void Usuario_Restaurar_PreservaIdentidadeEHash()
    {
        var original = Usuario.Criar("Ana Costa", "ana@exemplo.com", "senhaSegura123");

        var restaurado = Usuario.Restaurar(
            original.Id,
            original.Nome,
            original.Email,
            original.SenhaHash,
            original.CriadoEm);

        Assert.Equal(original.Id, restaurado.Id);
        Assert.Equal(original.CriadoEm, restaurado.CriadoEm);
        Assert.True(restaurado.VerificarSenha("senhaSegura123"));
    }
}
