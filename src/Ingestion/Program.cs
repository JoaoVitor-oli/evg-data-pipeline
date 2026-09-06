// Esse código realiza o download do arquivo mais recente de matrículas da Escola Virtual disponível no portal de dados abertos da EVG, tentando nos últimos 14 dias.

var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"; // User agent utilizado para as requisições HTTP
using var client = new HttpClient(); // Cliente HTTP utilizado para realizar as requisições
client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent); // Adiciona o user agent às requisições HTTP

// Diretório onde os arquivos brutos serão armazenados

var rawDir = Path.Combine("..", "..", "data", "raw"); // Caminho completo para o diretório onde os arquivos brutos serão armazenados
Directory.CreateDirectory(rawDir); // Cria o diretório caso ele não exista

// Início da tentativa de download do arquivo mais recente

const int maxDiasParaTentar = 14; // Número máximo de dias para tentar baixar o arquivo mais recente
var baixado = false; // Indica se algum arquivo foi baixado com sucesso

// Loop para tentar baixar o arquivo dos últimos 14 dias, começando pelo mais recente

for (var diasAtras = 0; diasAtras < maxDiasParaTentar && !baixado; diasAtras++) // Itera sobre os últimos 14 dias tentando baixar o arquivo mais recente
{
    var data = DateTime.Today.AddDays(-diasAtras); // Calcula a data candidata para o download
    var nomeArquivo = $"{data:yyyyMMdd}_escolavirtual_dadosabertos_matriculas_utf8.rar"; // Nome do arquivo candidato para o download
    var urlCandidata = $"https://dadosaberto.evg.gov.br/{nomeArquivo}"; // URL candidata para o download do arquivo

    Console.WriteLine($"Tentando: {urlCandidata}"); // Informa que está tentando baixar o arquivo da URL candidata

    using var response = await client.GetAsync(urlCandidata, HttpCompletionOption.ResponseHeadersRead); // Realiza a requisição HTTP para a URL candidata

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"  -> {(int)response.StatusCode} {response.StatusCode}, tentando data anterior..."); // Se a resposta não foi bem-sucedida, tenta a data anterior
        continue; // Continua para a próxima data candidata
    }

    // So remove arquivos antigos depois de confirmar que o novo arquivo existe de verdade -
    // assim, se o download falhar logo em seguida, ainda sobra o arquivo antigo em data/raw
    foreach (var arquivoAntigo in Directory.GetFiles(rawDir, "*.rar"))
    {
        Console.WriteLine($"Removendo arquivo antigo: {arquivoAntigo}");
        File.Delete(arquivoAntigo);
    }

    var destinationPath = Path.Combine(rawDir, nomeArquivo);
    Console.WriteLine($"Encontrado! Baixando para {destinationPath}..."); // Informa que o download será iniciado

    await using var httpStream = await response.Content.ReadAsStreamAsync();
    await using var fileStream = File.Create(destinationPath);
    await httpStream.CopyToAsync(fileStream);

    Console.WriteLine("Download concluido."); // Informa que o download foi concluído com sucesso
    baixado = true;
}

// Verifica se algum arquivo foi baixado com sucesso nos últimos 14 dias
if (!baixado)
{
    Console.WriteLine($"Nenhum arquivo encontrado nos ultimos {maxDiasParaTentar} dias."); // Informa que nenhum arquivo foi baixado com sucesso nos últimos 14 dias
    return 1; // Retorna código de erro indicando que nenhum arquivo foi baixado com sucesso
}

return 0; // Retorna código de sucesso indicando que o arquivo foi baixado com sucesso