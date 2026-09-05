using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;

const string connectionString =
    "Server=localhost,1433;Database=EvgDados;User Id=sa;Password=TrocarEsta$enha123;TrustServerCertificate=True;";
const int tamanhoLote = 1000;

var samplePath = Path.Combine("..", "..", "data", "sample.csv");

var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    Delimiter = "|",
    HasHeaderRecord = true
};

using var conn = new SqlConnection(connectionString);
await conn.OpenAsync();

using var reader = new StreamReader(samplePath, System.Text.Encoding.UTF8);
using var csv = new CsvReader(reader, csvConfig);

var alunoCache = new Dictionary<string, int>();
var cursoCache = new Dictionary<string, int>();
var situacaoCache = new Dictionary<string, int>();
var tempoCache = new HashSet<int>();

// Carrega de uma unica vez os fatos ja existentes, em vez de checar linha a linha
var existentes = new HashSet<string>(
    await conn.QueryAsync<string>("SELECT CodMatricula FROM dbo.FatoMatricula"));

Console.WriteLine($"Fatos ja existentes no banco (carregados em memoria): {existentes.Count}");

var loteAtual = CriarTabelaDeLote();
var inseridos = 0;
var ignorados = 0;
var processados = 0;

await foreach (var m in csv.GetRecordsAsync<Matricula>())
{
    processados++;

    if (existentes.Contains(m.CodMatricula))
    {
        ignorados++;
        continue;
    }

    var alunoSk = await GetOrCreateAlunoAsync(conn, m, alunoCache);
    var cursoSk = await GetOrCreateCursoAsync(conn, m, cursoCache);
    var situacaoSk = await GetOrCreateSituacaoAsync(conn, m.SitMatricula, situacaoCache);
    var dataSk = await GetOrCreateTempoAsync(conn, m.DtMatricula, tempoCache);

    loteAtual.Rows.Add(m.CodMatricula, alunoSk, cursoSk, situacaoSk, dataSk);
    existentes.Add(m.CodMatricula); // evita duplicata dentro do mesmo lote/arquivo
    inseridos++;

    if (loteAtual.Rows.Count >= tamanhoLote)
    {
        await EnviarLoteAsync(connectionString, loteAtual);
        loteAtual = CriarTabelaDeLote();
    }

    if (processados % 500 == 0)
    {
        Console.WriteLine($"Processados: {processados}");
    }
}

// Envia o que sobrou no ultimo lote (provavelmente menor que 1000)
if (loteAtual.Rows.Count > 0)
{
    await EnviarLoteAsync(connectionString, loteAtual);
}

Console.WriteLine($"\nTotal processado: {processados}");
Console.WriteLine($"Fatos inseridos: {inseridos}");
Console.WriteLine($"Fatos ja existentes (ignorados): {ignorados}");

// ---------- Funcoes auxiliares ----------

static DataTable CriarTabelaDeLote()
{
    var tabela = new DataTable();
    tabela.Columns.Add("CodMatricula", typeof(string));
    tabela.Columns.Add("AlunoSK", typeof(int));
    tabela.Columns.Add("CursoSK", typeof(int));
    tabela.Columns.Add("SituacaoSK", typeof(int));
    tabela.Columns.Add("DataMatriculaSK", typeof(int));
    return tabela;
}

static async Task EnviarLoteAsync(string connectionString, DataTable lote)
{
    using var bulk = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.KeepNulls)
    {
        DestinationTableName = "dbo.FatoMatricula",
        BatchSize = lote.Rows.Count
    };

    // Mapeamento explicito por nome, para nao depender da ordem das colunas
    bulk.ColumnMappings.Add("CodMatricula", "CodMatricula");
    bulk.ColumnMappings.Add("AlunoSK", "AlunoSK");
    bulk.ColumnMappings.Add("CursoSK", "CursoSK");
    bulk.ColumnMappings.Add("SituacaoSK", "SituacaoSK");
    bulk.ColumnMappings.Add("DataMatriculaSK", "DataMatriculaSK");

    await bulk.WriteToServerAsync(lote);
}

static async Task<int> GetOrCreateAlunoAsync(SqlConnection conn, Matricula m, Dictionary<string, int> cache)
{
    if (cache.TryGetValue(m.CodigoPessoa, out var cached)) return cached;

    var existente = await conn.QuerySingleOrDefaultAsync<int?>(
        "SELECT AlunoSK FROM dbo.DimAluno WHERE CodigoPessoa = @CodigoPessoa", new { m.CodigoPessoa });

    var sk = existente ?? await conn.QuerySingleAsync<int>(
        @"INSERT INTO dbo.DimAluno (CodigoPessoa, Sexo, Idade, Deficiencia, Raca, Nacionalidade, UfPessoa, MunicipioPessoa)
          OUTPUT INSERTED.AlunoSK
          VALUES (@CodigoPessoa, @Sexo, @Idade, @Deficiencia, @Raca, @Nacionalidade, @UfPessoa, @MunicipioPessoa)",
        m);

    cache[m.CodigoPessoa] = sk;
    return sk;
}

static async Task<int> GetOrCreateCursoAsync(SqlConnection conn, Matricula m, Dictionary<string, int> cache)
{
    if (cache.TryGetValue(m.CodCurso, out var cached)) return cached;

    var existente = await conn.QuerySingleOrDefaultAsync<int?>(
        "SELECT CursoSK FROM dbo.DimCurso WHERE CodCurso = @CodCurso", new { m.CodCurso });

    var sk = existente ?? await conn.QuerySingleAsync<int>(
        @"INSERT INTO dbo.DimCurso (CodCurso, NomeCurso, ModalidadeTurma, CargaHoraria, Conteudista, Tematica)
          OUTPUT INSERTED.CursoSK
          VALUES (@CodCurso, @NomeCurso, @ModalidadeTurma, @CargaHoraria, @Conteudista, @Tematica)",
        m);

    cache[m.CodCurso] = sk;
    return sk;
}

static async Task<int> GetOrCreateSituacaoAsync(SqlConnection conn, string sitMatricula, Dictionary<string, int> cache)
{
    if (cache.TryGetValue(sitMatricula, out var cached)) return cached;

    var existente = await conn.QuerySingleOrDefaultAsync<int?>(
        "SELECT SituacaoSK FROM dbo.DimSituacao WHERE SitMatricula = @sitMatricula", new { sitMatricula });

    var sk = existente ?? await conn.QuerySingleAsync<int>(
        @"INSERT INTO dbo.DimSituacao (SitMatricula) OUTPUT INSERTED.SituacaoSK VALUES (@sitMatricula)",
        new { sitMatricula });

    cache[sitMatricula] = sk;
    return sk;
}

static async Task<int> GetOrCreateTempoAsync(SqlConnection conn, DateTime data, HashSet<int> cache)
{
    var dataSk = int.Parse(data.ToString("yyyyMMdd"));
    if (cache.Contains(dataSk)) return dataSk;

    var existente = await conn.QuerySingleOrDefaultAsync<int?>(
        "SELECT DataSK FROM dbo.DimTempo WHERE DataSK = @dataSk", new { dataSk });

    if (!existente.HasValue)
    {
        var trimestre = (data.Month - 1) / 3 + 1;
        await conn.ExecuteAsync(
            @"INSERT INTO dbo.DimTempo (DataSK, DataCompleta, Ano, Mes, Dia, Trimestre)
              VALUES (@dataSk, @DataCompleta, @Ano, @Mes, @Dia, @Trimestre)",
            new { dataSk, DataCompleta = data.Date, Ano = data.Year, Mes = data.Month, Dia = data.Day, Trimestre = trimestre });
    }

    cache.Add(dataSk);
    return dataSk;
}