using CsvHelper.Configuration.Attributes;

public class Matricula
{
    [Name("cod_matricula")]
    public string CodMatricula { get; set; } = "";

    [Name("dt_matricula")]
    public DateTime DtMatricula { get; set; }

    [Name("cod_curso")]
    public string CodCurso { get; set; } = "";

    [Name("nome_curso")]
    public string NomeCurso { get; set; } = "";

    [Name("modalidade_turma")]
    public string ModalidadeTurma { get; set; } = "";

    [Name("carga_horaria")]
    public int CargaHoraria { get; set; }

    [Name("sit_matricula")]
    public string SitMatricula { get; set; } = "";

    [Name("codigo_pessoa")]
    public string CodigoPessoa { get; set; } = "";

    [Name("sexo")]
    public string? Sexo { get; set; }

    [Name("idade")]
    public int? Idade { get; set; }

    [Name("raca")]
    public string? Raca { get; set; }

    [Name("uf_pessoa")]
    public string? UfPessoa { get; set; }

    [Name("municipio_pessoa")]
    public string? MunicipioPessoa { get; set; }
}