# Frontend React para teste da API

## Requisitos

- Node.js 20+
- API rodando localmente em `http://localhost:5111` ou outra URL configurada

## Como rodar

1. Entre na pasta `frontend`
2. Instale as dependências com `npm install`
3. Copie `.env.example` para `.env` se quiser alterar a URL da API
4. Rode `npm run dev`

## Login seed

- `admin / admin123`
- `funcionario / func123`

## Fluxos disponíveis

- Login JWT
- Dashboard para `Admin`
- Cadastro e edição de clientes
- Busca automática de endereço por CEP
- Criação de pedidos com cálculo visual do total
- Atualização de status dos pedidos
- Consulta manual do endpoint de integração ViaCEP
