# Deploy no Render

Este projeto foi preparado para subir no Render com:

- Web Service via Docker
- Banco gerenciado Render Postgres
- Migrations do EF Core executadas no startup
- Segredos via variáveis de ambiente

## Arquivos principais

- `Dockerfile`
- `render.yaml`
- `src/API/appsettings.json`
- `src/API/Program.cs`
- `src/Infrastructure/Migrations`

## Subida recomendada

1. Envie o repositório para GitHub.
2. No Render, crie um Blueprint apontando para o arquivo `render.yaml`.
3. Confirme a criação do serviço `erp-api` e do banco `erp-db`.
4. Aguarde o primeiro deploy.

## Variáveis importantes

O `render.yaml` já define:

- `ConnectionStrings__DefaultConnection` vindo do banco Render Postgres
- `Jwt__SecretKey` gerado automaticamente
- `ASPNETCORE_URLS=http://0.0.0.0:10000`

## Observações

- O projeto usa `Database.MigrateAsync()` ao iniciar, então o schema é aplicado automaticamente.
- O seed inicial cria:
  - `admin / admin123`
  - `funcionario / func123`
- Depois do primeiro deploy, troque essas senhas pelo fluxo da aplicação ou diretamente no banco.

## Deploy manual sem Blueprint

Se preferir criar tudo pelo painel:

1. Crie um `Postgres` no Render.
2. Crie um `Web Service` com runtime `Docker`.
3. Aponte para este repositório.
4. Configure as env vars:
   - `ConnectionStrings__DefaultConnection`
   - `Jwt__Issuer`
   - `Jwt__Audience`
   - `Jwt__SecretKey`
   - `Jwt__ExpirationMinutes`
   - `ExternalApis__ViaCepBaseUrl`
   - `ASPNETCORE_ENVIRONMENT`
   - `ASPNETCORE_URLS`
