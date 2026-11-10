# BTG Banking API

API REST para gerenciamento de contas bancárias, desenvolvida em **.NET 8**, utilizando Clean Architecture, PostgreSQL, Redis, RabbitMQ e MassTransit.

O projeto foi desenvolvido como parte de um desafio técnico e contempla operações CRUD, validações de domínio, cache distribuído, comunicação assíncrona, Transactional Outbox e processamento de eventos por um Worker independente.

---

# 🚀 Como executar o projeto localmente

Esta seção apresenta o procedimento completo para executar a aplicação utilizando Docker.

O ambiente foi validado a partir de um clone limpo do repositório, sem dependência de arquivos de configuração locais não versionados.

## 1. Pré-requisitos

Certifique-se de possuir as seguintes ferramentas instaladas:

- **Docker Desktop**, com Docker Compose disponível.
- **.NET SDK 8.0**.
- **Entity Framework Core CLI (`dotnet-ef`)**, compatível com EF Core 8.
- **Git**.

Para verificar as instalações:

```bash
docker --version
docker compose version
dotnet --version
dotnet ef --version
git --version
```

Caso o `dotnet-ef` não esteja instalado:

```bash
dotnet tool install --global dotnet-ef --version 8.*
```

Caso já esteja instalado e precise atualizar para uma versão compatível:

```bash
dotnet tool update --global dotnet-ef --version 8.*
```

> Os comandos de configuração apresentados a seguir utilizam PowerShell, conforme o ambiente em que a aplicação foi validada.

## 2. Clonar o repositório

Clone o projeto:

```bash
git clone https://github.com/GAWK96/btgBakingTest.git
```

Acesse o diretório:

```bash
cd btgBankingTest
```

Substitua `https://github.com/GAWK96/btgBakingTest.git` pelo endereço do repositório no GitHub.

## 3. Configurar variáveis de ambiente

O projeto utiliza um arquivo `.env` para configurar as credenciais dos serviços executados pelo Docker Compose.

Crie o arquivo a partir do exemplo disponibilizado no repositório:

```powershell
Copy-Item .env.example .env
```


## 4. Inicializar os serviços de infraestrutura

Inicialmente, execute apenas PostgreSQL, Redis e RabbitMQ:

```bash
docker compose up -d postgres redis rabbitmq
```

Verifique se os containers estão em execução:

```bash
docker compose ps
```

Os serviços devem apresentar status de execução e, quando configurado, healthcheck saudável.

### Serviços utilizados

| Serviço | Tecnologia | Porta |
|---|---|---|
| Banco de dados | PostgreSQL 16 | 5432 |
| Cache | Redis 7 | 6379 |
| Mensageria | RabbitMQ 4.3 | 5672 |
| RabbitMQ Management | RabbitMQ Management UI | 15672 |

O RabbitMQ Management pode ser acessado em:

`http://localhost:15672`

Utilize as credenciais configuradas no `.env`.

## 5. Executar as migrations

A aplicação utiliza dois contextos do Entity Framework Core:

- `BtgBankDbContext`: responsável pelas entidades da aplicação e pelas tabelas utilizadas pelo Transactional Outbox.
- `WorkerDbContext`: responsável pela persistência dos identificadores das mensagens processadas.

As migrations devem ser executadas antes da inicialização da API e do Worker.

### 5.1 Configurar a connection string

No PowerShell, execute:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database='POSTGRES_DB';Username='POSTGRES_USER';Password='POSTGRES_PASSWORD'"
```
Substitua DATABASE, USERNAME e PASSWORD pelas credenciais encontradas no arquivo env
### 5.2 Aplicar as migrations da aplicação

```powershell
dotnet ef database update `
  --project src/btgBank.Infrastructure `
  --startup-project src/btgBank.Api `
  --context BtgBankDbContext
```

### 5.3 Aplicar as migrations do Worker

```powershell
dotnet ef database update `
  --project src/btgBank.Worker `
  --startup-project src/btgBank.Worker `
  --context WorkerDbContext
```

O Worker utiliza uma tabela independente para o histórico de migrations:

`__EFMigrationsHistory_Worker`

Essa configuração permite que os dois contextos mantenham seus históricos de migrations separadamente, mesmo utilizando o mesmo banco de dados.

## 6. Inicializar API e Worker

Após a aplicação das migrations, execute:

```bash
docker compose up --build -d api worker
```

O Docker construirá as imagens da API e do Worker e iniciará os serviços.

Para verificar os containers:

```bash
docker compose ps
```

Para acompanhar os logs:

**API:**

```bash
docker compose logs -f api
```

**Worker:**

```bash
docker compose logs -f worker
```

Para acompanhar ambos:

```bash
docker compose logs -f api worker
```

## 7. Acessar a API

Após a inicialização, a documentação interativa da API estará disponível pelo Swagger:

`http://localhost:8080/swagger`

A partir dessa interface, é possível executar as operações de criação, consulta, atualização e exclusão de contas.

### Exemplo de criação de conta

Endpoint:

```http
POST /api/accounts
```

Request body:

```json
{
  "holderName": "João da Silva",
  "cpf": "52998224725"
}
```

O retorno esperado para uma criação bem-sucedida é:

```http
201 Created
```

O identificador da conta é gerado automaticamente.

## 8. Verificar o processamento assíncrono

Após criar uma conta, consulte os logs do Worker:

```bash
docker compose logs --tail=50 worker
```

O Worker deverá receber e processar o evento `AccountCreatedEvent`.

O mesmo fluxo de mensageria é utilizado para os eventos de atualização e exclusão.

O MassTransit utiliza RabbitMQ como broker e o Worker registra os identificadores das mensagens processadas no PostgreSQL.

## 9. Verificar os dados no PostgreSQL

Acesse o terminal do PostgreSQL:

```bash
docker exec -it btgBank-postgres psql -U btgBank -d btgBankdb
```

Liste as tabelas:

```sql
\dt
```

Consulte as contas cadastradas:

```sql
SELECT * FROM "Accounts";
```

Consulte os registros de mensagens processadas:

```sql
SELECT * FROM "ProcessedMessages";
```

Para sair do PostgreSQL:

```sql
\q
```

## 10. Verificar o Redis

Acesse o Redis:

```bash
docker exec -it btgBank-redis redis-cli
```

Verifique a comunicação:

```redis
PING
```

Resposta esperada:

```text
PONG
```

Após realizar consultas pela API, é possível verificar as chaves armazenadas:

```redis
SCAN 0
```

O cache utiliza chaves para consultas individuais e para a listagem completa de contas.

## 11. Executar os testes unitários

Na raiz do repositório, execute:

```bash
dotnet test
```

O projeto possui testes unitários para validar regras de domínio e comportamentos dos serviços da aplicação.

Na última validação realizada durante o desenvolvimento, **59 testes unitários foram executados com sucesso**.

## 12. Encerrar a aplicação

Para interromper os containers:

```bash
docker compose down
```

---

# 📋 Visão geral

A BTG Banking API é uma aplicação REST desenvolvida para gerenciar contas bancárias.

Além das funcionalidades CRUD, o projeto incorpora recursos de infraestrutura e padrões arquiteturais frequentemente utilizados em sistemas distribuídos.

### Funcionalidades

- Criação de contas bancárias.
- Consulta de conta por identificador.
- Listagem de contas.
- Atualização dos dados de uma conta.
- Exclusão de contas.
- Validação de CPF e nome do titular.
- Prevenção de CPF duplicado.
- Cache de consultas com Redis.
- Invalidação de cache após alterações.
- Publicação de eventos de domínio.
- Processamento assíncrono por Worker.
- Transactional Outbox.
- Controle de mensagens duplicadas no Worker.
- Testes unitários.
- Containerização com Docker Compose.

---

# 🛠️ Tecnologias utilizadas

| Tecnologia | Finalidade |
|---|---|
| .NET 8 | Plataforma de desenvolvimento |
| ASP.NET Core | API REST |
| C# | Linguagem de programação |
| Entity Framework Core 8 | ORM e migrations |
| PostgreSQL 16 | Persistência relacional |
| Npgsql | Integração entre EF Core e PostgreSQL |
| Redis 7 | Cache distribuído |
| RabbitMQ 4.3 | Broker de mensagens |
| MassTransit 8.4.1 | Framework de mensageria |
| Docker | Containerização |
| Docker Compose | Orquestração local dos serviços |
| Swagger / OpenAPI | Documentação e testes dos endpoints |
| xUnit | Testes unitários |

---

# 🏗️ Arquitetura

O projeto utiliza **Clean Architecture**, separando regras de negócio, casos de uso, infraestrutura e pontos de entrada da aplicação.

A organização busca reduzir o acoplamento entre componentes e facilitar a implementação de testes automatizados.

## Estrutura da solução

```text
btgBankingTest/
│
├── src/
│   ├── btgBank.Api/
│   ├── btgBank.Application/
│   ├── btgBank.Domain/
│   ├── btgBank.Infrastructure/
│   └── btgBank.Worker/
│
├── tests/
│   └── btgBank.UnitTests/
│
├── docker-compose.yml
├── .env.example
├── .gitignore
└── README.md
```

## Responsabilidades das camadas

### Domain

Contém as entidades, Value Objects e regras de negócio.

A entidade `Account` representa uma conta bancária e contém informações como:

- `Id`
- `HolderName`
- `Cpf`
- `IsActive`

As validações de domínio incluem a normalização do CPF e a validação do nome do titular.

### Application

Contém os casos de uso e os contratos utilizados pela aplicação.

O `AccountService` coordena as operações de criação, consulta, atualização e exclusão.

Também utiliza abstrações para persistência, cache e publicação de eventos.

### Infrastructure

Contém as implementações das integrações externas:

- Entity Framework Core.
- PostgreSQL.
- Repositórios.
- Unit of Work.
- Redis.
- MassTransit.
- Transactional Outbox.

### API

Expõe os endpoints HTTP e configura os serviços necessários à execução da aplicação.

### Worker

Executa o processamento assíncrono dos eventos recebidos pelo RabbitMQ.

Também realiza o registro dos identificadores das mensagens processadas para auxiliar no controle de idempotência.

---

# 🔌 Endpoints

A API disponibiliza as seguintes operações:

| Método | Endpoint | Descrição |
|---|---|---|
| POST | `/api/accounts` | Criar uma conta |
| GET | `/api/accounts` | Listar contas |
| GET | `/api/accounts/{id}` | Consultar conta por ID |
| PUT | `/api/accounts/{id}` | Atualizar uma conta |
| DELETE | `/api/accounts/{id}` | Excluir uma conta |

Os contratos de entrada e saída podem ser consultados diretamente pelo Swagger.

---

# 💾 Persistência de dados

A aplicação utiliza PostgreSQL como banco de dados principal.

O acesso aos dados é realizado por meio do Entity Framework Core.

## Integridade

A persistência utiliza mecanismos de integridade, incluindo um índice único para o CPF.

Essa restrição complementa as validações realizadas pela aplicação e impede que duas contas sejam persistidas com o mesmo CPF normalizado.

## Unit of Work

O padrão Unit of Work é utilizado para coordenar a persistência das alterações.

A solução também utiliza o Transactional Outbox do MassTransit para registrar mensagens de saída juntamente com as alterações da aplicação.

---

# ⚡ Estratégia de cache

O Redis é utilizado como mecanismo de cache distribuído.

A estratégia implementada segue o padrão **Cache-Aside**.

## Consultas

Ao receber uma requisição de consulta:

1. A aplicação verifica se o dado está armazenado no Redis.
2. Em caso de cache hit, retorna o valor armazenado.
3. Em caso de cache miss, consulta o PostgreSQL.
4. Armazena o resultado no Redis.
5. Retorna a resposta ao cliente.

## Expiração

As entradas possuem TTL configurado para expirar no próximo encerramento do dia.

## Invalidação

As operações de criação, atualização e exclusão invalidam as entradas de cache afetadas.

Essa estratégia busca reduzir a possibilidade de retornar informações desatualizadas após alterações realizadas pela aplicação.

---

# 📨 Mensageria e processamento assíncrono

A aplicação utiliza RabbitMQ como broker de mensagens e MassTransit como framework de mensageria.

As alterações nas contas produzem eventos que são processados por um Worker independente.

## Eventos publicados

| Evento | Operação |
|---|---|
| `AccountCreatedEvent` | Criação de conta |
| `AccountUpdatedEvent` | Atualização de conta |
| `AccountDeletedEvent` | Exclusão de conta |

## Fluxo de processamento

```text
Cliente
   |
   v
ASP.NET Core API
   |
   v
AccountService
   |
   +--------------------+
   |                    |
   v                    v
PostgreSQL         MassTransit
   |                    |
   |              Transactional
   |                 Outbox
   |                    |
   +--------------------+
                        |
                        v
                     RabbitMQ
                        |
                        v
                      Worker
                        |
                        v
                AccountEventConsumer
                        |
                        v
                  PostgreSQL
                ProcessedMessages
```

## Transactional Outbox

O Transactional Outbox é utilizado para reduzir o risco de inconsistências entre a persistência de dados e a publicação de eventos.

A alteração da conta e a mensagem de saída são persistidas na mesma transação do banco.

Posteriormente, o MassTransit realiza a entrega da mensagem ao RabbitMQ.

Isso permite que mensagens persistidas sejam encaminhadas mesmo quando o broker estiver temporariamente indisponível.

## Retry

O Worker utiliza uma política de retry para lidar com falhas temporárias.

A configuração atual contempla três tentativas adicionais, com intervalo de cinco segundos.

## Tratamento de falhas

O MassTransit possui mecanismos para encaminhar mensagens que não puderam ser processadas para filas de erro.

Essas mensagens podem ser inspecionadas para identificar falhas e orientar eventuais procedimentos de recuperação.

## Idempotência

O Worker utiliza a tabela `ProcessedMessages` para registrar os identificadores das mensagens processadas.

Uma restrição de unicidade permite detectar tentativas de registro duplicadas.

Essa abordagem auxilia na prevenção de processamento repetido, considerando que sistemas de mensageria podem realizar entregas duplicadas.

A garantia de idempotência dos efeitos depende da coordenação entre o registro da mensagem e as operações executadas pelo consumidor.

---

# 🧪 Testes

O projeto possui testes unitários destinados a validar as regras de domínio e o comportamento dos serviços da aplicação.

Os testes permitem verificar cenários de sucesso e falha sem depender da execução completa da infraestrutura.

## Executar todos os testes

```bash
dotnet test
```

## Executar apenas o projeto de testes

```bash
dotnet test tests/btgBank.UnitTests
```

Durante o desenvolvimento, a suíte foi validada com **59 testes unitários aprovados**.

Além dos testes unitários, o fluxo principal da aplicação foi verificado em um ambiente Docker iniciado a partir de um clone limpo do repositório.

Essa validação incluiu:

- Inicialização dos serviços de infraestrutura.
- Aplicação das migrations.
- Inicialização da API e do Worker.
- Criação de conta pela API.
- Persistência no PostgreSQL.
- Publicação de eventos.
- Consumo de eventos pelo Worker.
- Registro de mensagens processadas.

---

# 📐 Architecture Decision Records (ADRs)

Os ADRs documentam as principais decisões arquiteturais adotadas no desenvolvimento da solução.

Cada registro apresenta o contexto, a decisão, as justificativas, as alternativas consideradas e suas consequências.

Todos os ADRs possuem status **Accepted**.

---

## ADR-001 — Adoção de banco de dados relacional (SQL)

### Contexto

A aplicação é responsável pelo gerenciamento de contas bancárias, realizando operações de criação, consulta, atualização e exclusão.

O domínio exige consistência dos dados, integridade das informações e prevenção de registros duplicados.

Além disso, a solução utiliza o padrão Transactional Outbox, que depende da persistência transacional dos dados da aplicação e das mensagens de saída.

### Decisão

Utilizar um banco de dados relacional baseado em SQL como mecanismo principal de persistência.

### Justificativas

- **Consistência transacional:** suporte a transações ACID.
- **Integridade dos dados:** utilização de constraints e índices únicos, especialmente para o CPF.
- **Transactional Outbox:** possibilidade de persistir alterações de domínio e mensagens na mesma transação.
- **Consultas estruturadas:** suporte a filtros, ordenação e consultas relacionais.
- **Integração com EF Core:** mapeamento de entidades, migrations e consultas LINQ.

### Alternativas consideradas
Não foram consideradas alternativas para esse caso, visto que um banco de dados relacional atende bem ao que foi pedido na avliação técnica

### Consequências

**Positivas:**
- Maior facilidade para garantir integridade e unicidade.
- Suporte consolidado a transações.
- Integração direta com o Entity Framework Core.

**Negativas:**
- Menor flexibilidade para alterações frequentes de esquema.
- Necessidade de gerenciar migrations.
- Escalabilidade horizontal pode exigir estratégias adicionais.

---

## ADR-002 — PostgreSQL como banco de dados principal

**Status:** Accepted

### Contexto

Após a decisão de utilizar um banco relacional, foi necessário selecionar um SGBD compatível com .NET 8, Entity Framework Core e execução em containers Docker.

A solução também precisava oferecer suporte ao Transactional Outbox do MassTransit.

### Decisão

Adotar **PostgreSQL 16** como banco de dados principal.

### Justificativas

- **Open source:** não exige licenciamento comercial para utilização.
- **Confiabilidade:** oferece recursos maduros de transações, índices e constraints.
- **Integração com .NET:** utilização do provider Npgsql para Entity Framework Core.
- **Compatibilidade com MassTransit:** suporte ao Entity Framework Outbox com PostgreSQL.
- **Containerização:** imagem oficial disponível para execução via Docker.
- **Portabilidade:** possibilidade de executar a solução em diferentes ambientes.

### Alternativas consideradas

**SQL Server:**

Possui excelente integração com o ecossistema Microsoft, suporte consolidado ao Entity Framework Core e recursos avançados de segurança, transações e administração.

Entretanto, PostgreSQL foi escolhido por oferecer uma alternativa open source, multiplataforma e sem custos obrigatórios de licenciamento, atendendo aos requisitos do desafio.

### Consequências

**Positivas:**
- Ambiente reproduzível utilizando Docker.
- Ausência de dependência de licenças comerciais.
- Suporte aos recursos transacionais necessários.

**Negativas:**
- Necessidade de administrar migrations e conexões.
- Dependência do provider Npgsql.
- Particularidades de sintaxe SQL em relação a outros SGBDs.

---

## ADR-003 — Redis como mecanismo de cache

### Contexto

A aplicação possui endpoints para consultar contas individualmente e recuperar a lista de contas cadastradas.

Embora essas operações sejam relativamente simples, consultas repetidas podem gerar acessos desnecessários ao banco de dados.

O desafio também contempla armazenamento temporário de informações consultadas durante o dia.

### Decisão

Utilizar **Redis 7** como mecanismo de cache distribuído, seguindo a estratégia **Cache-Aside**.

### Estratégia implementada

**Consulta:**

1. A aplicação verifica se os dados estão disponíveis no Redis.
2. Se estiverem, retorna os dados armazenados.
3. Caso contrário, consulta o PostgreSQL.
4. Armazena o resultado no Redis com tempo de expiração.
5. Retorna os dados ao cliente.

**Alterações:**

Nas operações de criação, atualização e exclusão, as chaves de cache afetadas são invalidadas após a persistência.

As entradas também possuem expiração configurada para o próximo encerramento do dia.

### Justificativas

- **Performance:** redução de consultas repetidas ao PostgreSQL.
- **Cache distribuído:** possibilidade de compartilhamento entre múltiplas instâncias da API.
- **TTL:** suporte nativo à expiração automática.
- **Integração com .NET:** utilização de `IDistributedCache`.
- **Docker:** facilidade para executar e testar localmente.

### Alternativas consideradas

**IMemoryCache:**

Possui configuração simples, baixa latência e não exige infraestrutura adicional.

Entretanto, os dados ficam associados à memória de cada instância da aplicação, dificultando o compartilhamento em ambientes distribuídos.

### Consequências

**Positivas:**
- Redução de acessos ao banco em cache hits.
- Expiração automática dos registros.
- Suporte a cenários com múltiplas instâncias.

**Negativas:**
- Inclusão de um serviço adicional na infraestrutura.
- Necessidade de controlar a invalidação do cache.
- Possibilidade de dados temporariamente desatualizados em determinadas condições de concorrência.

---

## ADR-004 — Adoção de Clean Architecture

### Contexto

Embora o requisito funcional principal seja um CRUD de contas, a solução também incorpora persistência, cache, publicação de eventos e processamento assíncrono.

A concentração dessas responsabilidades diretamente nos controllers aumentaria o acoplamento e dificultaria os testes automatizados.

### Decisão

Adotar **Clean Architecture**, organizando a solução em camadas com responsabilidades bem definidas.

### Estrutura

**Domain**

Contém as entidades, os Value Objects e as regras de negócio.

**Application**

Contém os serviços de aplicação, DTOs, contratos e interfaces utilizados pelos casos de uso.

**Infrastructure**

Contém as implementações de persistência, cache, mensageria e integração com serviços externos.

**API**

Expõe os endpoints HTTP e configura os serviços necessários à execução da aplicação.

**Worker**

Executa o processamento assíncrono dos eventos recebidos por mensageria.

### Justificativas

- **Separação de responsabilidades:** regras de negócio isoladas de detalhes de infraestrutura.
- **Inversão de dependência:** utilização de interfaces para abstrair integrações externas.
- **Testabilidade:** possibilidade de testar regras e serviços utilizando mocks.
- **Manutenibilidade:** alterações na infraestrutura com menor impacto nas regras de negócio.
- **Evolução:** organização adequada para incorporar novas funcionalidades.

### Alternativas consideradas

**Vertical Slice Architecture:**

Organiza a aplicação por funcionalidades ou casos de uso, agrupando os componentes necessários para executar cada operação.

Essa abordagem favorece a coesão por funcionalidade e permite que diferentes casos de uso evoluam de maneira relativamente independente.

Entretanto, considerando o escopo do desafio e a intenção de demonstrar explicitamente a separação entre domínio, aplicação e infraestrutura, Clean Architecture foi considerada mais adequada.

As duas abordagens não são necessariamente incompatíveis e podem ser combinadas em aplicações maiores.

### Consequências

**Positivas:**
- Maior organização do código.
- Facilidade de implementação de testes unitários.
- Redução do acoplamento com tecnologias específicas.
- Maior clareza na distribuição das responsabilidades.

**Negativas:**
- Maior quantidade de projetos, interfaces e abstrações.
- Complexidade inicial superior à necessária para um CRUD simples.
- Necessidade de manter as dependências entre camadas corretamente organizadas.

---

## ADR-005 — MassTransit para mensageria e processamento assíncrono

### Contexto

A aplicação publica eventos relacionados ao ciclo de vida das contas bancárias:

- `AccountCreatedEvent`
- `AccountUpdatedEvent`
- `AccountDeletedEvent`

Esses eventos são enviados ao RabbitMQ e processados por um Worker independente.

A solução também precisa lidar com falhas temporárias, evitar perda de mensagens durante a persistência e reduzir os efeitos de entregas duplicadas.

### Decisão

Utilizar **MassTransit 8.4.1** como framework de mensageria, integrado ao **RabbitMQ** e ao **Entity Framework Core**.

### Justificativas

**Abstração de mensageria**

O MassTransit simplifica a configuração de publishers, consumers e endpoints.

**Integração com RabbitMQ**

Permite utilizar o RabbitMQ como broker sem implementar manualmente toda a infraestrutura de comunicação.

**Transactional Outbox**

A integração com EF Core permite armazenar alterações de domínio e mensagens de saída na mesma transação do banco.

Isso reduz o risco de inconsistência entre a persistência da conta e o registro do evento que será publicado.

**Retry**

O Worker utiliza uma política de tentativas para lidar com falhas temporárias.

**Tratamento de falhas**

O MassTransit disponibiliza mecanismos de encaminhamento de mensagens que não puderam ser processadas para filas de erro.

**Integração com Dependency Injection**

A configuração de consumers e serviços utiliza o mecanismo de injeção de dependência do .NET.

### Estratégia de idempotência

Como sistemas de mensageria podem realizar entregas duplicadas, o Worker utiliza a tabela `ProcessedMessages`.

O identificador da mensagem é registrado no PostgreSQL, com uma restrição de unicidade.

Isso permite identificar mensagens previamente processadas e evitar a repetição de seus efeitos, desde que o registro de processamento e os efeitos relevantes sejam coordenados corretamente.

### Alternativas consideradas

**RabbitMQ.Client:**

Biblioteca oficial do RabbitMQ para .NET, que permite comunicação direta com o broker por meio do protocolo AMQP.

Oferece maior controle sobre os detalhes de publicação, consumo, acknowledgements e configuração das mensagens.

Entretanto, sua utilização exigiria implementar manualmente mecanismos adicionais de retry, tratamento de falhas, gerenciamento de consumidores e integração com o padrão Transactional Outbox.

MassTransit foi escolhido por fornecer abstrações e funcionalidades prontas que reduzem a complexidade da implementação e a quantidade de código de infraestrutura.

### Consequências

**Positivas:**
- Redução de código repetitivo de infraestrutura.
- Integração com o padrão Transactional Outbox.
- Configuração simplificada de consumers.
- Suporte a retry e tratamento de mensagens com falha.
- Separação entre processamento HTTP e processamento assíncrono.

**Negativas:**
- Dependência de um framework externo.
- Necessidade de compreender suas convenções e configurações.
- Complexidade operacional adicional devido ao broker e ao Worker.
- Necessidade de implementar idempotência no processamento de mensagens.

---

# 📝 Considerações finais

O projeto foi desenvolvido com o objetivo de atender aos requisitos funcionais do desafio técnico e demonstrar a aplicação de conceitos de engenharia de software.

A implementação contempla persistência relacional, separação de responsabilidades, cache distribuído, comunicação assíncrona, Transactional Outbox e controle de idempotência.

Embora algumas dessas escolhas introduzam complexidade adicional em comparação a um CRUD tradicional, elas permitem explorar padrões relevantes para sistemas distribuídos.

A execução da solução foi validada em um ambiente Docker configurado a partir de um clone limpo do repositório, incluindo a inicialização dos serviços, a aplicação das migrations e a verificação do fluxo de publicação e consumo de eventos.
