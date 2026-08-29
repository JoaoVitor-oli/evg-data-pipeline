var fileUrl = "https://dadosaberto.evg.gov.br/20260829_escolavirtual_dadosabertos_matriculas_utf8.rar";
var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);

var rawDir = Path.Combine("..", "..", "data", "raw");
Directory.CreateDirectory(rawDir);

var destinationPath = Path.Combine(rawDir, fileName);

using var client = new HttpClient();

Console.WriteLine($"Downloading {fileUrl}...");

using var response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
response.EnsureSuccessStatusCode();

await using var httpStream = await response.Content.ReadAsStreamAsync();
await using var fileStream = File.Create(destinationPath);
await httpStream.CopyToAsync(fileStream);

Console.WriteLine($"File saved in: {destinationPath}");