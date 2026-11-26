# Cadastro.NET

Refatoração de um pequeno projeto de console (cadastro de usuários sem banco de dados) para uma solução **C# / .NET 10** organizada em camadas, com validação de domínio, value objects, abstração de persistência, testes automatizados e integração contínua.

Os arquivos originais foram preservados integralmente em [`docs/original/`](docs/original/) (`Usuario.cs`, `RepositordeUsuario.cs`, `using System;.cs`, além do `.csproj` e da `.sln` antigos).

## Tecnologias

| Item | Versão / uso |
| --- | --- |
| C# | linguagem principal (nullable enabled, implicit usings) |
| .NET SDK | 10.0.402 (`net10.0`) |
| xUnit | testes de unidade |
| Microsoft.NET.Test.Sdk + xUnit runner | execução dos testes |
| coverlet.collector | coleta de cobertura (já no template de testes) |
| GitHub Actions | CI com `dotnet test` |
| PBKDF2/SHA-256 | derivação da senha com salt aleatório |

## O que mudou em relação ao original

- **Entidade rica:** `Usuario` agora é imutável na identidade (`Id`, `CriadoEm`) e expõe operações (`AlterarNome`, `AlterarEmail`, `AlterarSenha`, `VerificarSenha`) em vez de propriedades públicas graváveis.
- **Value objects:** `Nome`, `Email` e `SenhaHash` encapsulam as regras de validação e a normalização (e-mail em minúsculas, espaços do nome compactados).
- **Senha:** o `SHA256` simples e sem salt foi substituído por **PBKDF2 com salt aleatório de 16 bytes, 100.000 iterações e comparação em tempo constante**.
- **Persistência desacoplada:** `IRepositorioUsuario` defini o contrato; `RepositorioUsuarioEmMemoria` é uma implementação thread-safe com índices por `Id` e por e-mail. O serviço de cadastro não conhece detalhes de armazenamento.
- **Regras de negócio centralizadas:** e-mail duplicado (case-insensitive) é rejeitado antes da gravação; identificadores e dados inválidos geram exceções específicas.
- **CLI redesenhada:** menu interativo com cadastro, listagem, busca e remoção, além de um modo `--demo` não interativo (útil para CI e demonstração).

## Arquitetura

```text
Cadastro.NET/
├── docs/
│   └── original/                       # código-fonte original preservado
├── src/
│   ├── TrabalhoMarcos.Domain/          # entidades, value objects e exceções de domínio
│   │   ├── DomainException.cs
│   │   ├── Usuario.cs
│   │   └── ValueObjects/{Nome,Email,SenhaHash}.cs
│   ├── TrabalhoMarcos.Application/     # casos de uso e contrato de persistência
│   │   ├── IRepositorioUsuario.cs
│   │   ├── RepositorioUsuarioEmMemoria.cs
│   │   ├── ServicoCadastroUsuario.cs
│   │   ├── EmailJaCadastradoException.cs
│   │   └── UsuarioNaoEncontradoException.cs
│   └── TrabalhoMarcos.Cli/             # interface de console (top-level statements)
├── tests/
│   └── TrabalhoMarcos.Tests/           # 40 testes xUnit
├── .github/workflows/ci.yml
├── TrabalhoMarcos.sln
└── README.md
```

### Dependências entre projetos

```text
Cli  ──▶ Application ──▶ Domain
Tests ─▶ Application ──▶ Domain
```

A seta indica `ProjectReference`. O domínio não referencia nenhuma outra camada.

## Regras de validação

| Regra | Detalhe |
| --- | --- |
| Nome | obrigatório; de 3 a 120 caracteres; apenas letras, espaços, hífen e apóstrofo; espaços repetidos são compactados |
| E-mail | obrigatório; até 254 caracteres; formato `local@dominio.tld`; normalizado para minúsculas |
| Senha | de 8 a 128 caracteres; armazenada apenas como hash PBKDF2 com salt |
| Duplicidade | e-mail já cadastrado (comparação normalizada) → `EmailJaCadastradoException` |
| Remoção | id inexistente → `UsuarioNaoEncontradoException` |

## Como executar

Pré-requisito: **.NET SDK 10.0.402** (ou qualquer 10.0.x).

```powershell
# Restaurar, compilar e executar a demonstração automática
dotnet run --project src/TrabalhoMarcos.Cli -- --demo

# Menu interativo
dotnet run --project src/TrabalhoMarcos.Cli
```

No menu interativo:

1. Cadastrar usuário
2. Listar usuários
3. Buscar por e-mail
4. Remover usuário
0. Sair

## Testes

```powershell
dotnet test TrabalhoMarcos.sln --configuration Release
```

Os testes cobrem:

- validação e normalização de **nome** e **e-mail**;
- geração/verificação do **hash da senha** (salt diferente para a mesma senha, comparação em tempo constante);
- criação, restauração e alteração da entidade `Usuario`;
- **cadastro duplicado** (inclusive com caixa diferente no e-mail);
- **busca** por id e por e-mail;
- **remoção** (inclusive id inexistente e reutilização do e-mail);
- **listagem** ordenada por data e nome;
- comportamento do repositório em memória (índices, cópias de listagem, token cancelado).

Resultado esperado: **40 testes aprovados**.

## Integração contínua

O workflow [`.github/workflows/ci.yml`](.github/workflows/ci.yml) instala o .NET 10, restaura, compila em `Release` e executa `dotnet test` em `push`/`pull request` nas branches `main` e `master`.

## Histórico

- Código original preservado em `docs/original/`.
- Refatoração completa para domínio/application/cli, com testes e CI.
