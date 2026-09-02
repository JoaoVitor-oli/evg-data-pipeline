using SharpCompress.Archives;
using SharpCompress.Common;

var rawDir = Path.Combine("..", "..", "data", "raw");
var extractedDir = Path.Combine("..", "..", "data", "extracted");
Directory.CreateDirectory(extractedDir);

var rarFile = Directory.GetFiles(rawDir, "*.rar")
    .OrderByDescending(f => f)
    .FirstOrDefault();

if (rarFile is null)
{
    Console.WriteLine("Nenhum arquivo .rar encontrado em data/raw. Rode a Ingestão primeiro.");
    return;
}

Console.WriteLine($"Extraindo {rarFile}...");

using var archive = ArchiveFactory.OpenArchive(rarFile);
foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
{
    entry.WriteToDirectory(extractedDir, new ExtractionOptions
    {
        ExtractFullPath = false,
        Overwrite = true
    });
    Console.WriteLine($"Extraído: {entry.Key}");
}

Console.WriteLine($"Extração concluída em: {extractedDir}");
