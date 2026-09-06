# EVG Data Pipeline

Pipeline de dados em C#/.NET que ingere, transforma e carrega os dados abertos
da [Escola Virtual Gov](https://emnumeros.escolavirtual.gov.br/dados-abertos/)
(capacitação de servidores públicos federais) em um modelo dimensional para análise.

> Projeto de estudo em engenharia de dados — construído passo a passo como
> prática de pipelines, orquestração e modelagem dimensional.

## Por quê

A EVG disponibiliza periodicamente (observado: aproximadamente semanal) uma
base pública com dados de alunos, cursos e situações de matrícula, grande
demais para abrir no Excel (~8,6 GB descompactado). Este projeto constrói um
pipeline reprodutível e idempotente — não uma extração manual — para
transformar esse dado bruto em um modelo pronto para análise (ex. Power BI).

## Arquitetura

```
Ingestion  -> baixa o .rar mais recente (busca por data, ultimos 14 dias)
Extraction -> descompacta o .rar em CSV (SharpCompress)
Load       -> parseia o CSV em streaming (CsvHelper) e carrega no SQL Server
              (upsert nas dimensoes + SqlBulkCopy no fato)
Orchestration -> executa as tres etapas acima em sequencia, como processos
                 separados, interrompendo o pipeline se alguma etapa falhar
```

A pasta `Transformation/` guarda o modelo de dados (`Matricula.cs`) usado
tanto para teste isolado de parsing quanto pelo `Load`, via referência de
projeto.

## Status atual

- [x] Ingestão automatizada — busca dinamicamente o arquivo do dia (ou dos
      últimos 14 dias, caso o de hoje ainda não tenha sido publicado), com
      limpeza do arquivo antigo só após confirmar o novo download
- [x] Extração automatizada do `.rar` (SharpCompress)
- [x] Parsing tipado e em streaming do CSV (CsvHelper, delimitador `|`,
      encoding UTF-8 explícito)
- [x] Modelo dimensional (star schema) criado no SQL Server: `DimAluno`,
      `DimCurso`, `DimSituacao`, `DimTempo`, `FatoMatricula`
- [x] Carga idempotente (upsert nas dimensões, `SqlBulkCopy` no fato,
      checagem de fatos já existentes antes de inserir)
- [x] Orquestração das três etapas como processos sequenciais
- [ ] Testar a carga contra o arquivo completo (~8,6 GB) — atualmente o
      `Load` está apontando para uma amostra (`data/sample.csv`, 5 mil linhas)
- [ ] Dashboard Power BI
- [ ] Evoluir para Azure real (Blob Storage, Data Factory, Azure SQL) —
      hoje tudo roda local via Docker (SQL Server + Azurite)

## Como rodar localmente

1. `docker compose up -d` — sobe SQL Server e Azurite
2. Copie `appsettings.example.json` para `appsettings.json` e ajuste se necessário
3. Aplique o schema no banco:
   ```powershell
   docker cp .\sql\schema.sql evg-sqlserver:/tmp/schema.sql
   docker exec -i evg-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '<senha>' -C -i /tmp/schema.sql
   ```
4. Rode o pipeline completo:
   ```powershell
   cd src/Orchestration
   dotnet run
   ```

## Estrutura

```
src/Ingestion/       # download automatizado do dataset (busca por data)
src/Extraction/      # descompactação RAR (SharpCompress)
src/Transformation/  # modelo Matricula.cs (parsing/validação)
src/Load/             # upsert nas dimensões + bulk insert no fato
src/Orchestration/    # encadeia as etapas acima como processos
sql/schema.sql         # DDL do modelo dimensional
docs/                   # dicionário de dados e diagramas
```

## Decisões técnicas

- **Streaming em vez de carregar tudo em memória**: a base é maior que o
  limite de linhas do Excel (~8,6 GB), então o parsing é feito linha a linha
  (`CsvReader.GetRecordsAsync`), nunca `.ToList()`.
- **Containers para dependências locais**: SQL Server e Azurite via Docker,
  para não poluir a máquina e manter o setup reprodutível.
- **Modelo dimensional (star schema)**: dimensões (`DimAluno`, `DimCurso`,
  `DimSituacao`, `DimTempo`) evitam repetir texto (ex. nome de curso) a cada
  linha de fato; chaves substitutas (`IDENTITY`) conectam as tabelas.
- **Idempotência**: chaves naturais têm restrição `UNIQUE`; a carga verifica
  o que já existe (dimensões via cache em memória, fatos via `HashSet`
  carregado uma vez) antes de inserir — rodar o pipeline várias vezes não
  duplica dados.
- **`SqlBulkCopy` no fato**: inserir linha a linha era o gargalo de
  performance (round-trip de rede por linha); lotes de 1000 registros via
  `SqlBulkCopy` resolveram isso.
- **Ingestão resiliente por data**: como a EVG nomeia o arquivo com a data de
  publicação (não há endpoint fixo), a Ingestão tenta a data de hoje e volta
  no tempo até 14 dias, em vez de depender de um link fixo que expira.
- **Orquestração via processos separados** (`Process.Start` + `dotnet run
  --project`), não chamadas de função no mesmo processo — mais próximo de
  como orquestradores reais (Airflow, Data Factory) tratam cada etapa.

## Fonte dos dados

[Dados Abertos — Escola Virtual Gov](https://emnumeros.escolavirtual.gov.br/dados-abertos/)
(ver também o Dicionário de Dados, linkado na mesma página).
O dataset bruto não é versionado neste repositório — veja `.gitignore`.

## Próximos passos

- [ ] Apontar o `Load` para o arquivo completo extraído (não o sample) e
      validar performance/tempo em escala real
- [ ] Dashboard Power BI sobre o modelo dimensional
- [ ] Mover a connection string para configuração externa
      (`appsettings.json` + `Microsoft.Extensions.Configuration`)
- [ ] Avaliar publicar via Azure real (Blob Storage no lugar do Azurite,
      Azure SQL no lugar do container local, Function com Timer Trigger no
      lugar da execução manual do Orchestration)