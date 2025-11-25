// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using trabalhomarcosjm;

class Program
{
    static void Main(string[] args)
    {
        var repo = new RepositorioUsuarios();
        List<Usuario> usuarios = repo.Carregar();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Sistema de Cadastro (sem SQL) ===");
            Console.WriteLine("1 - Cadastrar usuário");
            Console.WriteLine("2 - Listar usuários");
            Console.WriteLine("3 - Sair");
            Console.Write("Escolha: ");

            string opc = Console.ReadLine();

            if (opc == "1")
            {
                Console.Write("Nome: ");
                string nome = Console.ReadLine();

                Console.Write("Email: ");
                string email = Console.ReadLine();

                Console.Write("Senha: ");
                string senha = Console.ReadLine();
                string senhaHash = Hash(senha);

                usuarios.Add(new Usuario
                {
                    Nome = nome,
                    Email = email,
                    SenhaHash = senhaHash,
                    CriadoEm = DateTime.Now
                });

                repo.Salvar(usuarios);
                Console.WriteLine("Usuário salvo com sucesso!");
                Console.ReadKey();
            }
            else if (opc == "2")
            {
                Console.WriteLine("\n=== Lista de usuários cadastrados ===");
                foreach (var u in usuarios)
                {
                    Console.WriteLine($"Nome: {u.Nome} | Email: {u.Email} | Criado em: {u.CriadoEm}");
                }
                Console.ReadKey();
            }
            else if (opc == "3")
            {
                break;
            }
        }
    }

    // Hash simples da senha
    static string Hash(string texto)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
            return Convert.ToBase64String(bytes);
        }
    }
}