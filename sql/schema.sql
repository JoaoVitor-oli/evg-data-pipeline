-- Schema inicial do EVG Data Pipeline
-- Modelagem dimensional (star schema): será detalhado no passo "Modelagem e carga"
-- Por enquanto, só a criação do banco e um placeholder das tabelas.

IF DB_ID('EvgDados') IS NULL
BEGIN
    CREATE DATABASE EvgDados;
END
GO

USE EvgDados;
GO

-- TODO: DimAluno, DimCurso, DimSituacao, DimTempo, FatoMatricula
-- Serão criadas junto com o passo de modelagem, com base no dicionário de dados oficial.
