# Archdraw — Backend

API REST em .NET 8 / C# da plataforma Archdraw (engenharia e arquitetura de software): projetos, diagramas arquiteturais, componentes reutilizáveis e documentação em Markdown, com autenticação JWT e PostgreSQL.

O padrão técnico segue o backend Kerdos (Clean Architecture + DDD + CQRS com MediatR + Repository/Unit of Work + ErrorOr); nenhum conceito de negócio do Kerdos foi reaproveitado.

## Sumário

- [Stack](#stack)
- [Arquitetura](#arquitetura)
- [Estrutura de projetos](#estrutura-de-projetos)
- [Design patterns](#design-patterns)
- [Modelo de domínio](#modelo-de-domínio)
- [Módulos e endpoints](#módulos-e-endpoints)
- [Salvamento de diagramas e concorrência](#salvamento-de-diagramas-e-concorrência)
- [Banco de dados](#banco-de-dados)
- [Segurança](#segurança)
- [Configuração](#configuração)
- [Como rodar](#como-rodar)
- [Migrations](#migrations)
- [Testes](#testes)
- [Docker](#docker)
- [Convenções](#convenções)
- [Decisões e trade-offs](#decisões-e-trade-offs)

## Stack

| Camada | Tecnologia | Versão |
|---|---|---|
| Runtime / Web | .NET / ASP.NET Core Web API | 8.0 |
| ORM / banco | EF Core + Npgsql / PostgreSQL | 8.0.11 / 16 |
| CQRS | MediatR | 14.0.0 |
| Validação | FluentValidation | 12.1.1 |
| Erros | ErrorOr | 2.0.1 |
| Identity / JWT | ASP.NET Core Identity, JwtBearer | 8.0.11 |
| Versionamento | Asp.Versioning | 8.1.0 |
| Docs | Swashbuckle | 6.6.2 |
| Logs | Serilog (console + arquivo) | 4.3.1 |
| E-mail | Brevo (API HTTP) | — |
| Storage de fotos | S3 / compatível (AWSSDK.S3) | 4.0.x |
| Cache (opcional) | Redis via `IDistributedCache` | 8.0.11 |
| Testes | xUnit + Testcontainers (PostgreSQL real) | 2.5.3 / 4.1.0 |

> **Licença do MediatR:** a versão 14 exige licença comercial em produção (o log avisa em desenvolvimento). É a mesma versão do Kerdos; se isso for um problema, fixe a versão 12.x (última gratuita), pois só a API `IMediator`/`IRequestHandler`/`IPipelineBehavior` é usada.

## Arquitetura

```
Presentation  →  Application  →  Domain  ←  BuildingBlocks
Infrastructure → (implementa interfaces do Domain / Application)
CrossCutting  →  (monta o DI de todas as camadas)
```

```
Controller → IMediator.Send(Command|Query)
  → ValidationBehavior (FluentValidation)
    → DomainExceptionBehavior (invariantes e conflitos → ErrorOr)
      → Handler (fino)
        → UseCase (regra de aplicação)
          → IUnitOfWork → Repository → EF Core / PostgreSQL
```

## Estrutura de projetos

```
archdraw.sln
├── Archdraw.Domain/            Modules/{Users,Projects,Diagrams,Components,Documents}, Catalogs,
│                               Interfaces/{Repositories,Services,Caching,UnitOfWork}, Exceptions
├── Archdraw.Application/       Modules/{Auth,Projects,Diagrams,Components,Documents,Catalogs}
│   │                             Commands/ Queries/ UseCases/ Requests/ Responses/ Mappers/ Services/
│   └── Behaviors/, Common/, Interfaces/
├── Archdraw.Infrastructure/    Context/AppDbContext, Configuration/ (IEntityTypeConfiguration),
│                               Repositories/, UnitOfWork/, Services/, Interceptor/, Migrations/
├── Archdraw.Presentation/      Controllers/V1, Extensions/, Middlewares/, Swagger/, Settings/, Program.cs
├── Archdraw.BuildingBlocks/    Entity<T>, AggregateRoot<T>, ValueObject, eventos de domínio, paginação
├── Archdraw.CrossCutting/      IoC/{Application,Infrastructure,BackgroundJobs}
├── Archdraw.BackgroundJobs/    estrutura de jobs agendados (desligada por padrão)
└── Archdraw.Tests/             Domain/ Application/ Integration/ (PostgreSQL real)
```

Desvios deliberados em relação ao esboço da especificação:

- Namespaces/pastas de módulo no **plural** (`Projects`, `Diagrams`, `Documents`…): `Project` e `Diagram` são também nomes de agregado, e um namespace homônimo quebra a resolução de tipos em C#.
- O agregado de documentação chama-se `Document` (escopo `Project | Diagram | Component`).
- Requests/Responses ficam agrupados em um arquivo por módulo (`Requests/DiagramRequests.cs`), em vez de uma pasta por ação.

## Design patterns

- **CQRS:** commands alteram estado, queries leem. Todo handler é fino e delega ao `UseCase`.
- **ErrorOr:** erros esperados nunca são exceções. `NotFound`→404, `Validation`→400, `Conflict`→409, `Unauthorized`→401, `Forbidden`→403.
- **Repository + Unit of Work:** `UseCase`s só enxergam `IUnitOfWork`; o `CommitAsync` é explícito nas escritas.
- **Invariantes no domínio:** violações lançam `DomainException`, convertida em 400 pelo `DomainExceptionBehavior`.
- **Retry/Caching:** `RetryOnConcurrencyBehavior` **não** é usado — um conflito de snapshot precisa chegar ao cliente. `CachingBehavior` e `ICacheService` existem como estrutura, fora do pipeline.

## Modelo de domínio

```
User (AppUser : IdentityUser<Guid>) 1—N Project 1—N Diagram 1—N DiagramNode
                                         │              └—N DiagramConnection
                                         ├—N ArchitecturalComponent ←(0..1)— DiagramNode.ComponentId
                                         └—N Document (escopo projeto | diagrama | componente)
```

| Agregado | Observações |
|---|---|
| `Project` | Raiz de propriedade (`OwnerId`). Estilo principal + estilos secundários coexistem; padrões são conceito separado. |
| `Diagram` | Raiz do grafo visual. `Revision` é o token de concorrência. Nós e conexões só existem dentro dele. |
| `DiagramNode` | Representação **visual** (posição, tamanho, propriedades em jsonb). `ComponentId?` (identidade semântica) e `LinkedDiagramId?` (navegação). |
| `DiagramConnection` | Origem/destino **do mesmo diagrama** (FK composta). Waypoints em jsonb. |
| `ArchitecturalComponent` | Identidade semântica reutilizável **entre diagramas do projeto**. |
| `Document` | README em Markdown vinculado por **id** (renomear não quebra o vínculo). |

**Regras de exclusão e integridade**

| Operação | Efeito |
|---|---|
| Excluir projeto | Cascata: diagramas, nós, conexões, componentes, documentos. |
| Excluir diagrama | Remove seus nós/conexões e README; `LinkedDiagramId` de outros diagramas vira `NULL`; **componentes compartilhados não são afetados**. |
| Excluir componente em uso | `409 Component.InUse` (com `usageCount`). Com `?detach=true`, desvincula os nós (que mantêm uma cópia das propriedades) e exclui. |
| Conexão com nó inexistente/de outro diagrama | Rejeitada no domínio **e** pelo banco (FK composta `(DiagramId, NodeId)`). |
| Vínculo para diagrama/componente de outro projeto | `400` (validado no `SnapshotPreparer`). |

**Catálogos** (estilos, padrões, tipos de componente e de conexão) são constantes tipadas com ids estáveis em `Domain/Catalogs`, sem tabelas nem CRUD; `GET /api/v1/catalogs` os expõe. O catálogo cobre 12 estilos, 22 padrões arquiteturais e os tipos de componente, incluindo componentes internos de DDD/CQRS/Hexagonal e uma seção de Design Patterns (ids com prefixo `dp-`). Ao adicionar um item, atualize também `archdraw-frontend/src/data` — os ids precisam coincidir.

**Componente vs. nó:** nós vinculados (`componentId`) exibem sempre as propriedades **atuais do componente**; salvar um snapshot não sobrescreve o componente com dados antigos do nó (evita sobrescrita silenciosa entre diagramas). Edita-se o componente por `PUT /components/{id}`.

## Módulos e endpoints

Base: `/api/v1`. Tudo exige `Authorization: Bearer <jwt>`, exceto autenticação e catálogos.

**Authentication** (`/authentication`)

| Método | Rota | Descrição |
|---|---|---|
| POST | `/register` · `/login` | Cadastro/login. Devolve o access token; o refresh token vai em cookie HttpOnly. |
| POST | `/refresh` · `/logout` | Renova (rotaciona) / encerra a sessão. Exigem `Origin` na lista do CORS (proteção CSRF). |
| GET/PUT | `/profile` | Perfil. |
| PUT | `/password` | Altera a senha (encerra as demais sessões). |
| POST | `/password-reset/request` · `/password-reset/confirm` | Recuperação por código de 6 dígitos por e-mail (15 min, 5 tentativas). |
| POST/DELETE | `/profile/photo` | Foto de perfil (JPEG/PNG/WebP, 2 MB). |

**Projects**

| Método | Rota |
|---|---|
| GET / POST | `/projects` (paginado, `search`) |
| GET / PUT / DELETE | `/projects/{id}` |
| GET | `/projects/summary` (totais do dashboard) |
| POST | `/projects/import` (bundle `archdraw.project` v1, migração/importação) |
| GET / POST | `/projects/{id}/diagrams` · `/projects/{id}/components` |
| GET / PUT | `/projects/{id}/documentation` |

**Diagrams**

| Método | Rota |
|---|---|
| GET / PUT / DELETE | `/diagrams/{id}` |
| POST | `/diagrams/{id}/duplicate` |
| GET / PUT | `/diagrams/{id}/snapshot` |
| GET | `/diagrams/recent` · `/diagrams/{id}/parents` |
| GET / PUT | `/diagrams/{id}/documentation` |

**Components:** `GET/PUT/DELETE /components/{id}` (`?detach=true`), `GET/PUT /components/{id}/documentation`.
**Catalogs:** `GET /catalogs`. **Saúde:** `GET /health`.

O Swagger fica em `/swagger` (em `Production`, com Basic Auth: defina `Swagger__Password`).

## Salvamento de diagramas e concorrência

`PUT /diagrams/{id}/snapshot` recebe o canvas inteiro + `baseRevision` e grava **em uma transação**:

1. valida dono, limites (2000 nós, 4000 conexões, 10 MB), catálogos, ids únicos e integridade das conexões;
2. se `baseRevision` ≠ revisão atual → `409 Diagram.RevisionConflict` com `metadata.currentRevision` (nada é gravado);
3. senão faz o upsert por id (ids estáveis), remove o que sumiu e incrementa `Revision`. `Revision` também é `IsConcurrencyToken`, o que cobre a corrida entre a leitura e o commit (apenas um de N salvamentos concorrentes vence — coberto por teste).

Metadados (`PUT /diagrams/{id}`) **não** incrementam `Revision`. Projetos e componentes usam `xmin` (Postgres) como token; documentos usam `Revision` (`409 Document.RevisionConflict`).

## Banco de dados

Modelagem híbrida: tabelas relacionais com FKs, índices e *check constraints* para a estrutura; **jsonb** para dados visuais/dinâmicos (`Viewport`, `Settings`, `Properties` do nó, `Waypoints`, `Links`, `Metadata`). Estilos/padrões são `text[]`. Todos os ids são `uuid` gerados pelo cliente (`ValueGeneratedNever`), o que mantém identificadores estáveis entre edições, cópias e importações.

Índices principais: `(OwnerId, UpdatedAtUtc)` em projetos, `(ProjectId, UpdatedAtUtc)` em diagramas, `(ComponentId)` e `(LinkedDiagramId)` em nós, `(Scope, OwnerId)` único em documentos.

## Segurança

- Identidade **sempre** do token (`HttpContext.GetUserId()`); nenhum `userId` vem do cliente.
- Toda leitura/escrita filtra por propriedade (`Project.OwnerId`); recursos de terceiros respondem **404** (não vazam existência).
- Access token curto (15 min) + refresh token rotativo com detecção de reuso; o JWT só vale com a sessão ativa.
- Senhas: Identity (hash), política (8+, maiúscula, minúscula, dígito), bloqueio progressivo por tentativas.
- Rate limiting: `auth-anonymous` (10/min por IP) em login/cadastro/reset; `auth-refresh` (60/min) no refresh.
- CORS por lista (`Cors:AllowedOrigins`), cookie `HttpOnly; Secure; SameSite=None` fora de dev, validação de `Origin` nas rotas de cookie.
- Limites de corpo (10 MB global; 1 MB documentos; 3 MB foto), `ErrorMiddleware` sem detalhes internos em produção.
- Nenhum segredo versionado: `Jwt:key` é obrigatória (≥ 32 caracteres) e vem de variável de ambiente.

## Configuração

Hierarquia `appsettings.json` → `appsettings.{Localhost|Development|Staging|Production}.json` → variáveis de ambiente (`Secao__Chave`).

| Chave | Descrição |
|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL (ou env `CONNECTION_STRING`). |
| `Jwt:key` | Segredo do JWT (≥ 32 chars). **Obrigatório.** |
| `TokenConfiguration:*` | `Issuer`, `Audience`, `AccessTokenExpireMinutes` (15), `RefreshTokenExpireDays` (30). |
| `Cors:AllowedOrigins` | Origens do frontend. |
| `BrevoSettings:*` | `ApiKey`, `FromEmail`, `FromName`. Sem isso o e-mail é **gravado no log** (dev). |
| `AwsSettings:*` | S3/compatível para fotos. Sem isso grava em disco (`LocalStorage`, servido em `/uploads`). |
| `ConnectionStrings:Redis` | Opcional. Sem isso usa cache distribuído em memória. |
| `BackgroundJobs:Enabled` | Liga a limpeza de sessões expiradas (estrutura de job). |
| `Database:MigrateOnStartup` | Aplica migrations ao subir (ligado em Localhost/Development). |
| `RateLimit:*` | Limites por minuto. |

`appsettings.Localhost.json` traz apenas valores locais inofensivos (chave de dev, senha `archdraw` do Postgres do compose). **Nunca versione credenciais reais** — use variáveis de ambiente; veja `.env.example`.

## Como rodar

Pré-requisitos: .NET 8 SDK e Docker.

```bash
cd archdraw-backend
docker compose up -d postgres                 # PostgreSQL 16 em localhost:5432
export ASPNETCORE_ENVIRONMENT=Localhost
dotnet run --project Archdraw.Presentation    # http://localhost:5080  (Swagger: /swagger)
```

As migrations são aplicadas automaticamente em Localhost. (No macOS a porta 5000 é do AirPlay, por isso 5080.)

## Migrations

```bash
dotnet ef migrations add <Nome> --project Archdraw.Infrastructure --startup-project Archdraw.Presentation --output-dir Migrations
dotnet ef database update      --project Archdraw.Infrastructure --startup-project Archdraw.Presentation
dotnet ef database update <MigrationAnterior> --project Archdraw.Infrastructure --startup-project Archdraw.Presentation
```

(Use `ASPNETCORE_ENVIRONMENT=Localhost` para o design-time encontrar a connection string.)

## Testes

```bash
dotnet test Archdraw.Tests          # requer Docker em execução
```

- **Domain/**: invariantes de `Diagram` (snapshot, duplicação, limites) e `Project`.
- **Application/**: behaviors do pipeline e UUID v5.
- **Integration/**: a API real contra **PostgreSQL de verdade** (Testcontainers) — jsonb, FKs, `xmin` e concorrência não são validáveis com EF InMemory. Cobrem autenticação, propriedade/autorização em todos os módulos, snapshots (persistência, remoção, ids estáveis, 409, 6 salvamentos concorrentes → 1 vence), integridade de conexões, duplicação, regras de exclusão, componentes compartilhados, documentação (conflito de revisão), importação idempotente, recuperação de senha e foto de perfil.

## Docker

```bash
docker build -f Archdraw.Presentation/Dockerfile -t archdraw-api .
JWT_KEY="$(openssl rand -hex 32)" docker compose --profile api up -d     # API + Postgres
```

Perfis opcionais do compose: `api` (a própria API) e `redis`. Imagem multi-stage (SDK → runtime `aspnet:8.0`, usuário não root).

## Convenções

| Tipo | Padrão |
|---|---|
| Command / Query | `<Ação><Módulo>Command` · `<Ação>Query` (ex.: `SaveDiagramSnapshotCommand`, `GetProjectsQuery`) |
| Handler / Validator / UseCase | `<Nome>CommandHandler` · `<Nome>CommandValidator` · `<Nome>UseCase` |
| Repositório | `I<Agregado>Repository` / `<Agregado>Repository` (acessados só via `IUnitOfWork`) |
| Rotas | `api/v{version}/<recurso>` (REST, plural) |

Mensagens de validação em **português**. Entidades de domínio nunca saem pela API (sempre DTOs `*Response`).

## Decisões e trade-offs

- **Snapshot completo vs. patches:** simples, transacional e idempotente; custo é o tamanho do payload (limitado a 2000 nós). Evolução possível: PATCH incremental com a mesma `Revision`.
- **PK global dos nós** (em vez de `(DiagramId, Id)`): impede reaproveitar ids entre diagramas e simplifica FKs; cópias geram ids novos.
- **FK `ComponentId` com `ON DELETE SET NULL`:** `RESTRICT/NO ACTION` impediria excluir um projeto (componentes e diagramas somem na mesma operação). O bloqueio de componente em uso é feito na aplicação (409).
- **Viewport só viaja junto com alterações de conteúdo:** evita 409 por pan/zoom entre abas.
- **Redis opcional:** `IDistributedCache` cai em memória sem Redis; o `ICacheService` é estrutura pronta, sem uso na v1.
- **Importação idempotente:** ids derivados por UUID v5 `(userId, "tipo:idLegado")`; reimportar não duplica, e duas contas importando o mesmo arquivo não colidem.
