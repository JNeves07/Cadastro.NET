using TrabalhoMarcos.Application;
using TrabalhoMarcos.Domain;

var repositorio = new RepositorioUsuarioEmMemoria();
var servico = new ServicoCadastroUsuario(repositorio);

if (args.Contains("--demo", StringComparer.OrdinalIgnoreCase))
{
    await ExecutarDemonstracaoAsync(servico);
    return;
}

await ExecutarMenuAsync(servico);

static async Task ExecutarMenuAsync(ServicoCadastroUsuario servico)
{
    Console.WriteLine("=== Sistema de Cadastro de Usuários (.NET 10) ===");

    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("1 - Cadastrar usuário");
        Console.WriteLine("2 - Listar usuários");
        Console.WriteLine("3 - Buscar por e-mail");
        Console.WriteLine("4 - Remover usuário");
        Console.WriteLine("0 - Sair");
        Console.Write("Escolha: ");

        var opcao = Console.ReadLine();

        try
        {
            switch (opcao)
            {
                case "1":
                    Console.Write("Nome: ");
                    var nome = Console.ReadLine() ?? string.Empty;
                    Console.Write("E-mail: ");
                    var email = Console.ReadLine() ?? string.Empty;
                    Console.Write("Senha (mínimo 8 caracteres): ");
                    var senha = Console.ReadLine() ?? string.Empty;

                    var usuario = await servico.CadastrarAsync(nome, email, senha);
                    Console.WriteLine($"Usuário cadastrado: {usuario} ({usuario.Id})");
                    break;

                case "2":
                    ImprimirUsuarios(await servico.ListarAsync());
                    break;

                case "3":
                    Console.Write("E-mail: ");
                    var encontrado = await servico.BuscarPorEmailAsync(Console.ReadLine() ?? string.Empty);
                    Console.WriteLine(encontrado is null ? "Usuário não encontrado." : encontrado.ToString());
                    break;

                case "4":
                    Console.Write("Identificador do usuário: ");
                    if (Guid.TryParse(Console.ReadLine(), out var id))
                    {
                        await servico.RemoverAsync(id);
                        Console.WriteLine("Usuário removido.");
                    }
                    else
                    {
                        Console.WriteLine("Identificador inválido.");
                    }

                    break;

                case "0":
                    Console.WriteLine("Até logo!");
                    return;

                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }
        }
        catch (Exception ex) when (ex is DomainException or EmailJaCadastradoException or UsuarioNaoEncontradoException)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }
}

static async Task ExecutarDemonstracaoAsync(ServicoCadastroUsuario servico)
{
    Console.WriteLine("=== Demonstração não interativa ===");

    var maria = await servico.CadastrarAsync("Maria Silva", "MARIA@exemplo.com", "senhaSegura123");
    var joao = await servico.CadastrarAsync("João Souza", "joao@exemplo.com", "outraSenha456");

    Console.WriteLine($"Cadastrados: {maria} e {joao}");
    Console.WriteLine();
    ImprimirUsuarios(await servico.ListarAsync());

    Console.WriteLine();
    Console.WriteLine("Tentativa de cadastro duplicado (e-mail com caixa diferente):");
    try
    {
        await servico.CadastrarAsync("Maria Duplicada", "maria@exemplo.com", "senhaSegura123");
    }
    catch (EmailJaCadastradoException ex)
    {
        Console.WriteLine($"Rejeitado: {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine($"Senha correta para Maria? {maria.VerificarSenha("senhaSegura123")}");
    Console.WriteLine($"Senha incorreta para Maria? {maria.VerificarSenha("errada")}");

    await servico.RemoverAsync(joao.Id);
    Console.WriteLine();
    Console.WriteLine("Após remover João:");
    ImprimirUsuarios(await servico.ListarAsync());
}

static void ImprimirUsuarios(IReadOnlyList<Usuario> usuarios)
{
    Console.WriteLine($"{"Nome",-20} | {"E-mail",-28} | {"Criado em (UTC)",-30} | Id");
    Console.WriteLine(new string('-', 110));

    foreach (var usuario in usuarios)
    {
        Console.WriteLine(
            $"{usuario.Nome.Valor,-20} | {usuario.Email.Valor,-28} | " +
            $"{usuario.CriadoEm,-30:u} | {usuario.Id}");
    }

    if (usuarios.Count == 0)
    {
        Console.WriteLine("(nenhum usuário cadastrado)");
    }
}
