# EVG Data Pipeline

Pipeline de dados em C#/.NET que ingere, transforma e carrega os dados abertos
da [Escola Virtual Gov](https://emnumeros.escolavirtual.gov.br/dados-abertos/)
(capacitação de servidores públicos federais) em um modelo dimensional para análise.

> Projeto de estudo em engenharia de dados — construído passo a passo como
> prática de pipelines, orquestração e modelagem dimensional.

## Por quê

A EVG disponibiliza semanalmente uma base pública com dados de alunos, cursos
e situações de matrícula, grande demais para abrir no Excel. Este projeto
constrói um pipeline reprodutível — não uma extração manual — para transformar
esse dado bruto em um modelo pronto para análise (ex. Power BI).

## Arquitetura

_(diagrama a ser adicionado no passo de Orquestração)_

Ingestão (raw) → Extração (descompactação) → Transformação (parsing/limpeza)
→ Carga (modelo dimensional) → Orquestração (execução semanal) → Consumo (BI)

## Como rodar localmente

1. `docker-compose up -d` — sobe SQL Server e Azurite
2. Copie `appsettings.example.json` para `appsettings.json` e ajuste se necessário
3. _(demais passos serão adicionados conforme o projeto evolui)_

## Estrutura

```
src/Ingestion/       # download automatizado do dataset
src/Extraction/      # descompactação RAR/TAR.GZ
src/Transformation/  # parsing e validação (streaming)
src/Load/             # carga no modelo dimensional
src/Orchestration/    # encadeamento do pipeline
sql/schema.sql         # DDL do modelo dimensional
docs/                   # dicionário de dados e diagramas
```

## Decisões técnicas

- **Streaming em vez de carregar tudo em memória**: a base é maior que o
  limite de linhas do Excel, então o parsing é feito linha a linha.
- **Containers para dependências locais**: SQL Server e Azurite via Docker,
  para não poluir a máquina e manter o setup reprodutível.

## Fonte dos dados

[Dados Abertos — Escola Virtual Gov](https://emnumeros.escolavirtual.gov.br/dados-abertos/)
(ver também o [Dicionário de Dados](https://emnumeros.escolavirtual.gov.br/dados-abertos/dicionario/)).
O dataset bruto não é versionado neste repositório — veja `.gitignore`.

## Próximos passos

- [ ] Ingestão automatizada (download semanal)
- [ ] Extração RAR/TAR.GZ
- [ ] Parsing e validação contra o dicionário de dados
- [ ] Modelo dimensional e carga
- [ ] Orquestração (Azure Function / Airflow local)
- [ ] Dashboard Power BI
- [ ] Evoluir para Azure real (Blob Storage, Data Factory, Azure SQL)
