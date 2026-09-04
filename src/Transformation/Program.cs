using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

var samplePath = Path.Combine("..", "..", "data", "sample.csv");

var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    Delimiter = "|",
    HasHeaderRecord = true
};

using var reader = new StreamReader(samplePath, System.Text.Encoding.UTF8);
using var csv = new CsvReader(reader, config);

var total = 0;
var semIdade = 0;

foreach (var matricula in csv.GetRecords<Matricula>())
{
    total++;
    if (matricula.Idade is null) semIdade++;

    if (total <= 3)
    {
        Console.WriteLine($"{matricula.CodMatricula} | {matricula.NomeCurso} | {matricula.SitMatricula}");
    }
}

Console.WriteLine($"\nTotal de registros lidos: {total}");
Console.WriteLine($"Registros sem idade preenchida: {semIdade}");