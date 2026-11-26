using System.Security.Cryptography;

namespace TrabalhoMarcos.Domain.ValueObjects;

/// <summary>
/// Hash de senha com salt aleatório, derivado com PBKDF2/SHA-256.
/// Substitui o SHA-256 sem salt do código original.
/// </summary>
public sealed record SenhaHash
{
    public const int TamanhoSalt = 16;
    public const int TamanhoHash = 32;
    public const int IteracoesPadrao = 100_000;
    public const int SenhaTamanhoMinimo = 8;
    public const int SenhaTamanhoMaximo = 128;

    public string Salt { get; }
    public string Hash { get; }
    public int Iteracoes { get; }

    private SenhaHash(string salt, string hash, int iteracoes)
    {
        Salt = salt;
        Hash = hash;
        Iteracoes = iteracoes;
    }

    public static SenhaHash Criar(string? senha)
    {
        ValidarSenha(senha);

        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            senha!, salt, IteracoesPadrao, HashAlgorithmName.SHA256, TamanhoHash);

        return new SenhaHash(
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash),
            IteracoesPadrao);
    }

    /// <summary>
    /// Reconstrói um hash previamente persistido.
    /// </summary>
    public static SenhaHash Restaurar(string salt, string hash, int iteracoes)
    {
        if (string.IsNullOrWhiteSpace(salt) || string.IsNullOrWhiteSpace(hash))
        {
            throw new DomainException("Salt e hash da senha são obrigatórios.");
        }

        if (iteracoes <= 0)
        {
            throw new DomainException("O número de iterações do hash deve ser positivo.");
        }

        return new SenhaHash(salt, hash, iteracoes);
    }

    public bool Verificar(string? senha)
    {
        if (string.IsNullOrEmpty(senha))
        {
            return false;
        }

        byte[] salt;
        byte[] hashEsperado;

        try
        {
            salt = Convert.FromBase64String(Salt);
            hashEsperado = Convert.FromBase64String(Hash);
        }
        catch (FormatException)
        {
            return false;
        }

        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
            senha, salt, Iteracoes, HashAlgorithmName.SHA256, hashEsperado.Length);

        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }

    public override string ToString() => $"{Iteracoes}.{Salt}.{Hash}";

    private static void ValidarSenha(string? senha)
    {
        if (string.IsNullOrEmpty(senha))
        {
            throw new DomainException("A senha é obrigatória.");
        }

        if (senha.Length is < SenhaTamanhoMinimo or > SenhaTamanhoMaximo)
        {
            throw new DomainException(
                $"A senha deve ter entre {SenhaTamanhoMinimo} e {SenhaTamanhoMaximo} caracteres.");
        }
    }
}
