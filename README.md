# ERP API - Cadastro de Clientes

API REST para um sistema ERP focado em cadastro e gerenciamento de clientes, produtos e pedidos. O projeto foi desenvolvido com C#, ASP.NET Core e PostgreSQL, seguindo arquitetura em camadas para facilitar manutencao, evolucao e organizacao das regras de negocio.

## Visao Geral

Esta API atende um frontend web e oferece:

- autenticacao com JWT
- cadastro e consulta de clientes
- listagem de produtos
- criacao de pedidos e atualizacao de status
- dashboard administrativo
- integracao com ViaCEP para consulta de enderecos

## Stack

- C#
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- JWT
- Swagger
- Docker
- Railway

## Arquitetura

O backend segue separacao por camadas:

- `Domain`: entidades e enums do sistema
- `Application`: DTOs, servicos e contratos
- `Infrastructure`: persistencia, repositorios, seguranca e integracoes
- `API`: controllers, autenticacao, configuracao e endpoints

Essa estrutura ajuda a reduzir acoplamento, centralizar regras de negocio e facilitar manutencao em cenarios de crescimento do sistema.

## Funcionalidades

- Login com emissao de token JWT
- CRUD parcial de clientes
- Consulta de produtos
- Criacao de pedidos com validacoes de negocio
- Atualizacao de status de pedidos
- Dashboard restrito a perfil administrador
- Consulta de CEP por integracao externa
- Migrations e seed automatico no startup

## Endpoints Principais

Base URL local:

```txt
http://localhost:8080
```

Rotas:

- `POST /api/auth/login`
- `GET /api/customers`
- `GET /api/customers/{id}`
- `POST /api/customers`
- `PUT /api/customers/{id}`
- `GET /api/products`
- `GET /api/orders`
- `GET /api/orders/{id}`
- `POST /api/orders`
- `PATCH /api/orders/{id}/status`
- `GET /api/dashboard`
- `GET /api/integrations/cep/{cep}`
- `GET /health`

## Usuarios Iniciais

O seed inicial cria dois usuarios para testes:

- `admin / admin123`
- `funcionario / func123`

Recomendacao: alterar essas credenciais em ambiente real.

## Como Rodar Localmente

### 1. Clonar o repositorio

```bash
git clone <url-do-repositorio>
cd Customer-registration-ERP-api
```

### 2. Configurar o banco

Atualize a connection string em [appsettings.json](/E:/repositories/Customer-registration-ERP-api/src/API/appsettings.json:1) ou use variaveis de ambiente.

Exemplo local:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=erpdb;Username=postgres;Password=postgres"
}
```

### 3. Configurar JWT

Defina uma chave com pelo menos 32 caracteres:

```json
"Jwt": {
  "Issuer": "ERP.API",
  "Audience": "ERP.Client",
  "SecretKey": "dev-secret-key-erp-api-2026-123457890",
  "ExpirationMinutes": 120
}
```

### 4. Executar a API

```bash
dotnet restore src/API/ERP.API.csproj
dotnet run --project src/API/ERP.API.csproj
```

## Variaveis de Ambiente

Em producao, prefira configurar por variaveis de ambiente:

```env
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=erpdb;Username=postgres;Password=postgres
Jwt__Issuer=ERP.API
Jwt__Audience=ERP.Client
Jwt__SecretKey=sua-chave-com-32-ou-mais-caracteres
Jwt__ExpirationMinutes=120
ExternalApis__ViaCepBaseUrl=https://viacep.com.br/ws/
```

## Deploy

O projeto foi preparado para deploy em ambiente cloud com:

- Railway para backend e banco
- Docker para empacotamento da aplicacao
- startup com `Database.MigrateAsync()` para aplicar migrations automaticamente

## Diferenciais Tecnicos

- autenticacao JWT com validacao no startup
- seed automatico de usuarios e produtos
- arquitetura em camadas
- integracao externa desacoplada por servico
- endpoints protegidos por autenticacao e perfil
- healthcheck para monitoramento

## Estrutura do Projeto

```txt
src/
  API/
  Application/
  Domain/
  Infrastructure/
```

## Objetivo do Projeto

Este projeto foi construido para demonstrar experiencia com:

- desenvolvimento de APIs REST em .NET
- organizacao de backend corporativo
- autenticacao e seguranca
- integracao com banco relacional
- arquitetura voltada para manutencao e evolucao

## Licenca

Este projeto esta disponivel para fins de estudo e portfolio.
