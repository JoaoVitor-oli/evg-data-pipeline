-- Schema do EVG Data Pipeline
-- Modelo dimensional (star schema) para os dados de matriculas da Escola Virtual Gov

IF DB_ID('EvgDados') IS NULL
BEGIN
    CREATE DATABASE EvgDados;
END
GO

USE EvgDados;
GO

-- =========================
-- DIMENSAO: Aluno
-- Granularidade: uma linha por pessoa (codigo_pessoa).
-- Dados demograficos guardados como atributos, por simplicidade de consulta.
-- =========================
IF OBJECT_ID('dbo.DimAluno', 'U') IS NULL
CREATE TABLE dbo.DimAluno (
    AlunoSK         INT IDENTITY(1,1) PRIMARY KEY,
    CodigoPessoa    VARCHAR(20)  NOT NULL,
    Sexo            VARCHAR(20)  NULL,
    Idade           INT          NULL,
    Deficiencia     VARCHAR(50)  NULL,
    Raca            VARCHAR(30)  NULL,
    Nacionalidade   VARCHAR(10)  NULL,
    UfPessoa        CHAR(2)      NULL,
    MunicipioPessoa VARCHAR(100) NULL,
    CONSTRAINT UQ_DimAluno_CodigoPessoa UNIQUE (CodigoPessoa)
);
GO

-- =========================
-- DIMENSAO: Curso
-- Granularidade: uma linha por curso (cod_curso).
-- =========================
IF OBJECT_ID('dbo.DimCurso', 'U') IS NULL
CREATE TABLE dbo.DimCurso (
    CursoSK         INT IDENTITY(1,1) PRIMARY KEY,
    CodCurso        VARCHAR(20)  NOT NULL,
    NomeCurso       VARCHAR(300) NOT NULL,
    ModalidadeTurma VARCHAR(50)  NULL,
    CargaHoraria    INT          NULL,
    Conteudista     VARCHAR(200) NULL,
    Tematica        VARCHAR(200) NULL,
    CONSTRAINT UQ_DimCurso_CodCurso UNIQUE (CodCurso)
);
GO

-- =========================
-- DIMENSAO: Situacao da matricula
-- Granularidade: um valor distinto de sit_matricula (ex. "Concluido", "Nao Concluido").
-- =========================
IF OBJECT_ID('dbo.DimSituacao', 'U') IS NULL
CREATE TABLE dbo.DimSituacao (
    SituacaoSK      INT IDENTITY(1,1) PRIMARY KEY,
    SitMatricula    VARCHAR(50) NOT NULL,
    CONSTRAINT UQ_DimSituacao_Valor UNIQUE (SitMatricula)
);
GO

-- =========================
-- DIMENSAO: Tempo
-- Granularidade: um dia. Chave no formato AAAAMMDD.
-- =========================
IF OBJECT_ID('dbo.DimTempo', 'U') IS NULL
CREATE TABLE dbo.DimTempo (
    DataSK      INT PRIMARY KEY,      -- ex. 20260828
    DataCompleta DATE NOT NULL,
    Ano         INT NOT NULL,
    Mes         INT NOT NULL,
    Dia         INT NOT NULL,
    Trimestre   INT NOT NULL
);
GO

-- =========================
-- FATO: Matricula
-- Granularidade: uma linha por matricula (cod_matricula) — o grao mais fino da fonte.
-- =========================
IF OBJECT_ID('dbo.FatoMatricula', 'U') IS NULL
CREATE TABLE dbo.FatoMatricula (
    FatoMatriculaSK BIGINT IDENTITY(1,1) PRIMARY KEY,
    CodMatricula    VARCHAR(20) NOT NULL,
    AlunoSK         INT NOT NULL REFERENCES dbo.DimAluno(AlunoSK),
    CursoSK         INT NOT NULL REFERENCES dbo.DimCurso(CursoSK),
    SituacaoSK      INT NOT NULL REFERENCES dbo.DimSituacao(SituacaoSK),
    DataMatriculaSK INT NOT NULL REFERENCES dbo.DimTempo(DataSK),
    CONSTRAINT UQ_FatoMatricula_CodMatricula UNIQUE (CodMatricula)
);
GO