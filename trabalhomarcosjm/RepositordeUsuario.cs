using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using trabalhomarcosjm;

public class RepositorioUsuarios
{
    private string caminhoArquivo = "usuarios.json";

    public List<Usuario> Carregar()
    {
        if (!File.Exists(caminhoArquivo))
            return new List<Usuario>();

        string json = File.ReadAllText(caminhoArquivo);
        return JsonSerializer.Deserialize<List<Usuario>>(json);
    }

    public void Salvar(List<Usuario> lista)
    {
        string json = JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(caminhoArquivo, json);
    }
}