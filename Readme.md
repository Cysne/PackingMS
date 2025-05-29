
# PackingApi - Loja do Seu Manoel

## Descrição

PackingApi é um microserviço desenvolvido em .NET 9 para automatizar o processo de embalagem de pedidos da loja do Seu Manoel. Dada uma lista de pedidos com produtos e dimensões, a API retorna quais caixas devem ser usadas para cada pedido e quais produtos vão em cada caixa, otimizando o uso do espaço e minimizando o número de caixas.

## Funcionalidades

- Recebe pedidos via JSON, cada um com uma lista de produtos e dimensões.
- Calcula a melhor forma de empacotar os produtos nas caixas disponíveis.
- Retorna, para cada pedido, as caixas utilizadas e os produtos em cada caixa.
- Persistência dos dados em SQL Server.
- API documentada e testável via Swagger.

## Caixas Disponíveis

- **Caixa 1:** 30 x 40 x 80 cm
- **Caixa 2:** 80 x 50 x 40 cm
- **Caixa 3:** 50 x 80 x 60 cm

## Tecnologias Utilizadas

- .NET 9
- SQL Server 2022
- Docker & Docker Compose
- Entity Framework Core
- Swagger

## Pré-requisitos

- [Docker](https://www.docker.com/) instalado na máquina

## Como rodar o projeto

1. **Clone o repositório:**
   ```bash
   git clone <seu-repositorio>
   cd PackingService.Api
   ```

2. **Suba os containers com Docker Compose:**
   ```bash
   docker-compose up --build
   ```

   Isso irá:
   - Subir o banco SQL Server na porta 1433
   - Subir a API na porta 8080

3. **Acesse o Swagger para testar a API:**
   - Abra o navegador em: [http://localhost:8080/swagger](http://localhost:8080/swagger)

## Exemplo de Requisição

**Endpoint:**  
`POST /api/packing/pack-orders`

**Body:**
```json
[
  {
    "orderId": 1,
    "products": [
      { "name": "Jogo 1", "height": 10, "width": 20, "length": 30 },
      { "name": "Jogo 2", "height": 15, "width": 25, "length": 35 }
    ]
  }
]
```

**Resposta:**
```json
[
  {
    "order_id": 1,
    "boxes": [
      {
        "box_id": "Caixa 1",
        "products": ["Jogo 1", "Jogo 2"],
        "observation": null
      }
    ]
  }
]
```

## Observações

- O banco de dados é criado e migrado automaticamente ao subir os containers.
- O projeto já está pronto para ser testado via Swagger.
- Para customizações, altere as configurações em `PackingService.Api/appsettings.json` ou no `docker-compose.yml`.
