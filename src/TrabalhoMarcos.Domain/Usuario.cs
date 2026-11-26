using TrabalhoMarcos.Domain.ValueObjects;

namespace TrabalhoMarcos.Domain;

/// <summary>
/// Entidade de usuário do sistema de cadastro.
/// </summary>
public sealed class Usuario
{
    private Usuario(
        Guid id,
        Nome nome,
        Email email,
        SenhaHash senhaHash,
        DateTimeOffset criadoEm)
    {
        Id = id;
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        CriadoEm = criadoEm;
    }

    public Guid Id { get; }

    public Nome Nome { get; private set; }

    public Email Email { get; private set; }

    public SenhaHash SenhaHash { get; private set; }

    public DateTimeOffset CriadoEm { get; }

    public static Usuario Criar(
        string nome,
        string email,
        string senha,
        DateTimeOffset? criadoEm = null,
        Guid? id = null)
        => Criar(Nome.Criar(nome), Email.Criar(email), senha, criadoEm, id);

    public static Usuario Criar(
        Nome nome,
        Email email,
        string senha,
        DateTimeOffset? criadoEm = null,
        Guid? id = null)
    {
        ArgumentNullException.ThrowIfNull(nome);
        ArgumentNullException.ThrowIfNull(email);

        var identificador = id ?? Guid.NewGuid();
        if (identificador == Guid.Empty)
        {
            throw new DomainException("O identificador do usuário não pode ser vazio.");
        }

        return new Usuario(
            identificador,
            nome,
            email,
            SenhaHash.Criar(senha),
            criadoEm ?? DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Reconstrói um usuário a partir de dados já persistidos.
    /// </summary>
    public static Usuario Restaurar(
        Guid id,
        Nome nome,
        Email email,
        SenhaHash senhaHash,
        DateTimeOffset criadoEm)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("O identificador do usuário não pode ser vazio.");
        }

        ArgumentNullException.ThrowIfNull(nome);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(senhaHash);

        return new Usuario(id, nome, email, senhaHash, criadoEm);
    }

    public void AlterarNome(string nome) => Nome = Nome.Criar(nome);

    public void AlterarEmail(string email) => Email = Email.Criar(email);

    public void AlterarSenha(string senha) => SenhaHash = SenhaHash.Criar(senha);

    public bool VerificarSenha(string senha) => SenhaHash.Verificar(senha);

    public override string ToString() => $"{Nome} <{Email}>";
}
