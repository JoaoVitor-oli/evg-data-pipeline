using System.Diagnostics;

var raizDoRepo = Path.Combine("..", "..");

var etapas = new (string Nome, string CaminhoProjeto)[]
{
    ("Ingestion", Path.Combine(raizDoRepo, "src", "Ingestion")),
    ("Extraction", Path.Combine(raizDoRepo, "src", "Extraction")),
    ("Load", Path.Combine(raizDoRepo, "src", "Load")),
};

foreach (var etapa in etapas)
{
    Console.WriteLine($"\n=== Iniciando etapa: {etapa.Nome} ===");

    var sucesso = await ExecutarEtapaAsync(etapa.CaminhoProjeto);

    if (!sucesso)
    {
        Console.WriteLine($"=== Etapa {etapa.Nome} falhou. Pipeline interrompido. ===");
        return 1;
    }

    Console.WriteLine($"=== Etapa {etapa.Nome} concluida com sucesso ===");
}

Console.WriteLine("\nPipeline completo executado com sucesso.");
return 0;

static async Task<bool> ExecutarEtapaAsync(string caminhoProjeto)
{
    var processInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = $"run --project \"{caminhoProjeto}\"",
        UseShellExecute = false,
        RedirectStandardOutput = false,
        RedirectStandardError = false
    };

    using var processo = Process.Start(processInfo);
    if (processo is null)
    {
        Console.WriteLine("Falha ao iniciar o processo.");
        return false;
    }

    await processo.WaitForExitAsync();
    return processo.ExitCode == 0;
}