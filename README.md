# Datarisk MLOps API

API REST para gerenciamento e execução de scripts de pré-processamento de dados no contexto de MLOps.

## 🚀 Tecnologias

- **Backend**: ASP.NET Core 8.0 com C#
- **Banco de Dados**: PostgreSQL 15
- **Cache/Queue**: Redis 7
- **Engine JavaScript**: Jint (execução segura)
- **Background Jobs**: Hangfire
- **Containerização**: Docker e Docker Compose
- **Documentação**: OpenAPI/Swagger
- **Logs**: Serilog

## 📁 Estrutura do Projeto

```
src/
├── DatariskMLOps.API/          # Camada de apresentação (Controllers, DTOs, Middleware)
├── DatariskMLOps.Domain/       # Camada de domínio (Entidades, Serviços, Interfaces)
├── DatariskMLOps.Infrastructure/ # Camada de infraestrutura (Repositórios, DbContext, Jobs)
├── DatariskMLOps.Tests.Unit/   # Testes unitários
└── DatariskMLOps.Tests.Integration/ # Testes de integração
```

## 🐳 Como Executar

### Pré-requisitos

- Docker e Docker Compose instalados
- .NET 8 SDK (opcional, para desenvolvimento)

### Execução com Docker Compose

1. **Clone o repositório**

```bash
git clone <repository-url>
cd datarisk-test
```

### Opções de Execução

#### ⚡ Opção 1: Execução Completa (Recomendado)

**Windows:**

```batch
start.bat
```

**Linux/Mac:**

```bash
chmod +x start.sh
./start.sh
```

**Docker Compose Manual:**

```bash
docker-compose up -d --build
```

#### 🔧 Opção 2: Desenvolvimento Local

**Windows:**

```batch
# Inicia apenas PostgreSQL e Redis
start-dev.bat
```

**Linux/Mac:**

```bash
# Inicia apenas dependências
docker-compose -f docker-compose.dev.yml up -d

# Execute a API localmente
cd src/DatariskMLOps.API
dotnet run
```

## 🌐 Endpoints Disponíveis

Após inicialização (aguarde ~30 segundos):

| Serviço                    | URL                            | Descrição                                 |
| -------------------------- | ------------------------------ | ----------------------------------------- |
| **🚀 Interface Principal** | http://localhost:5000/swagger  | **ACESSE AQUI** - Documentação interativa |
| **💚 Health Check**        | http://localhost:5000/health   | Status dos serviços                       |
| **📊 Dashboard Jobs**      | http://localhost:5000/hangfire | Jobs em background                        |
| **🔌 API REST**            | http://localhost:5000/api/\*   | Endpoints da API                          |

> **⚠️ Nota**: O endpoint raiz `http://localhost:5000/` não está configurado (404 é normal).  
> **✅ Use**: `http://localhost:5000/swagger` como interface principal!

## 📚 Como Usar a API

### 1. 📝 Criar Script JavaScript

**PowerShell:**

```powershell
$script = @{
    name = "Filtro Básico"
    content = "function process(data) { return data.filter(item => item.ativo === true); }"
    description = "Filtra apenas itens ativos"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/scripts" -Method POST -Body $script -ContentType "application/json"
```

**cURL:**

```bash
curl -X POST "http://localhost:5000/api/scripts" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Filtro Básico",
    "content": "function process(data) { return data.filter(item => item.ativo === true); }",
    "description": "Filtra apenas itens ativos"
  }'
```

### 2. 📋 Listar Scripts

```powershell
# PowerShell
Invoke-RestMethod -Uri "http://localhost:5000/api/scripts"
```

```bash
# cURL
curl "http://localhost:5000/api/scripts"
```

### 3. ▶️ Executar Script

```powershell
# PowerShell (substitua {scriptId} pelo ID retornado)
$data = @{
    inputData = @(
        @{ nome = "Item 1"; ativo = $true },
        @{ nome = "Item 2"; ativo = $false }
    )
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "http://localhost:5000/api/scripts/{scriptId}/execute" -Method POST -Body $data -ContentType "application/json"
```

### 4. 🔍 Consultar Execução

```powershell
# PowerShell (substitua {executionId})
Invoke-RestMethod -Uri "http://localhost:5000/api/executions/{executionId}"
```

## 💾 Sistema de Backup Automatizado

### Backups Manuais

**Backup do Banco:**

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/backup/database" -Method POST
```

**Backup de Logs:**

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/backup/logs" -Method POST
```

**Limpeza de Backups Antigos:**

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/backup/cleanup" -Method POST
```

### Backup Automático

- **Frequência**: A cada 6 horas
- **Retenção**: 30 dias
- **Localização**: `/backups` (dentro do container)
- **Monitoramento**: Via Hangfire Dashboard

## 🧪 Caso de Uso - Dados Bacen

### Script de Processamento (Versão Simples)

```javascript
function process(data) {
  // Filtra dados empresariais
  const empresariais = data.filter((item) => item.produto === "Empresarial");

  // Conta por bandeira
  const resultado = {};
  empresariais.forEach((item) => {
    const bandeira = item.nomeBandeira;
    if (!resultado[bandeira]) {
      resultado[bandeira] = 0;
    }
    resultado[bandeira] += item.qtdCartoesAtivos;
  });

  return resultado;
}
```

### Dados de Teste

```json
[
  {
    "trimestre": "20231",
    "nomeBandeira": "VISA",
    "nomeFuncao": "Crédito",
    "produto": "Empresarial",
    "qtdCartoesEmitidos": 3050384,
    "qtdCartoesAtivos": 1716709,
    "qtdTransacoesNacionais": 43984902,
    "valorTransacoesNacionais": 12846611557.78
  },
  {
    "trimestre": "20231",
    "nomeBandeira": "Mastercard",
    "nomeFuncao": "Crédito",
    "produto": "Empresarial",
    "qtdCartoesEmitidos": 1500000,
    "qtdCartoesAtivos": 800000,
    "qtdTransacoesNacionais": 25000000,
    "valorTransacoesNacionais": 8000000000.0
  }
]
```

## 🔍 Monitoramento e Logs

### Health Check

```powershell
# Verificar saúde dos serviços
Invoke-RestMethod -Uri "http://localhost:5000/health"
# Resposta esperada: "Healthy"
```

### Logs em Tempo Real

```bash
# Logs da API
docker logs datarisk-test-api-1 -f

# Logs do PostgreSQL
docker logs datarisk-test-postgres-1 -f

# Status dos containers
docker-compose ps
```

### Hangfire Dashboard

- **URL**: http://localhost:5000/hangfire
- **Funcionalidades**:
  - Visualizar jobs de backup automático
  - Monitorar execuções de scripts
  - Estatísticas de performance

## 🛑 Gerenciamento do Ambiente

### Parar Serviços

```bash
# Parar todos os containers
docker-compose down

# Parar mantendo volumes (dados preservados)
docker-compose stop
```

### Reiniciar com Rebuild

```bash
# Rebuild completo
docker-compose down
docker-compose up -d --build --force-recreate
```

### Reset Completo (CUIDADO!)

```bash
# Remove TODOS os dados permanentemente
docker-compose down -v
docker-compose up -d --build
```

## ❗ Troubleshooting

### Problema: Porta em Uso

```bash
# Windows - Verificar processo na porta 5000
netstat -ano | findstr :5000

# Linux/Mac
lsof -i :5000
```

### Problema: Container não inicia

```bash
# Verificar logs de erro
docker-compose logs

# Verificar recursos do sistema
docker system df
```

### Problema: API retorna 500

```bash
# Verificar logs detalhados
docker logs datarisk-test-api-1 --tail 50

# Verificar conectividade com banco
docker exec -it datarisk-test-postgres-1 psql -U postgres -d datarisk_mlops -c "SELECT 1;"
```

### Problema: Scripts não executam

- **Validação de Segurança**: O sistema possui validações rígidas
- **Loops Complexos**: Evite muitos loops aninhados
- **Funções Externas**: Apenas JavaScript vanilla é permitido

## 🎯 Fluxo de Trabalho Recomendado

1. **Iniciar Ambiente**

   ```bash
   docker-compose up -d --build
   ```

2. **Verificar Saúde**

   ```powershell
   Invoke-RestMethod -Uri "http://localhost:5000/health"
   ```

3. **Explorar API via Swagger**

   - Acesse: http://localhost:5000/swagger

4. **Criar Script de Teste**

   - Use exemplos da documentação

5. **Executar Pré-processamento**

   - Teste com dados simples primeiro

6. **Monitorar via Hangfire**
   - Acesse: http://localhost:5000/hangfire

## 📊 Métricas e Performance

- **Tempo de Inicialização**: ~30 segundos
- **Tempo de Backup**: ~5 segundos (banco vazio)
- **Execução de Scripts**: ~100ms (scripts simples)
- **Capacidade**: Limitada pela validação de segurança

---

**🚀 Ambiente DataRisk MLOps pronto para produção!**

_Para mais detalhes técnicos, consulte a documentação Swagger em http://localhost:5000/swagger_

# Ou manualmente

docker-compose up -d --build

````

3. **Verifique os serviços**

```bash
docker-compose ps
````

### URLs Disponíveis

- **API Base**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Hangfire Dashboard**: http://localhost:5000/hangfire
- **Health Check**: http://localhost:5000/health

## 📊 Banco de Dados

### Scripts Table

```sql
CREATE TABLE scripts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

### Executions Table

```sql
CREATE TABLE executions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    script_id UUID NOT NULL REFERENCES scripts(id),
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    input_data JSONB NOT NULL,
    output_data JSONB,
    error_message TEXT,
    started_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP WITH TIME ZONE,
    execution_time_ms INTEGER
);
```

## 🔧 Endpoints da API

### Scripts

- `POST /api/scripts` - Criar novo script
- `GET /api/scripts/{id}` - Buscar script por ID
- `GET /api/scripts` - Listar scripts (paginado)
- `PUT /api/scripts/{id}` - Atualizar script
- `DELETE /api/scripts/{id}` - Deletar script

### Executions

- `POST /api/executions` - Executar script
- `GET /api/executions/{id}` - Buscar execução por ID
- `GET /api/executions/by-script/{scriptId}` - Buscar execuções por script

## 📝 Exemplo de Uso

### 1. Criar um Script

```bash
curl -X POST http://localhost:5000/api/scripts \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Processamento Cartões Corporativos",
    "description": "Filtra cartões empresariais e agrupa por trimestre e bandeira",
    "content": "function process(data) { const corporativeData = data.filter(item => item.produto === \"Empresarial\"); const byQuarterAndIssuer = corporativeData.reduce((acc, item) => { const key = `${item.trimestre}-${item.nomeBandeira}`; if (!acc[key]) { acc[key] = { trimestre: item.trimestre, nomeBandeira: item.nomeBandeira, qtdCartoesEmitidos: 0, qtdCartoesAtivos: 0, qtdTransacoesNacionais: 0, valorTransacoesNacionais: 0 }; } acc[key].qtdCartoesEmitidos += item.qtdCartoesEmitidos; acc[key].qtdCartoesAtivos += item.qtdCartoesAtivos; acc[key].qtdTransacoesNacionais += item.qtdTransacoesNacionais; acc[key].valorTransacoesNacionais += item.valorTransacoesNacionais; return acc; }, {}); return Object.values(byQuarterAndIssuer); }"
  }'
```

### 2. Executar o Script

```bash
curl -X POST http://localhost:5000/api/executions \
  -H "Content-Type: application/json" \
  -d '{
    "scriptId": "<script-id-retornado>",
    "data": [
      {
        "trimestre": "20231",
        "nomeBandeira": "American Express",
        "nomeFuncao": "Crédito",
        "produto": "Intermediário",
        "qtdCartoesEmitidos": 433549,
        "qtdCartoesAtivos": 335542,
        "qtdTransacoesNacionais": 9107357,
        "valorTransacoesNacionais": 1617984610.42
      }
    ]
  }'
```

### 3. Consultar Status da Execução

```bash
curl -X GET http://localhost:5000/api/executions/<execution-id>
```

## 🧪 Testes

### Executar Testes Unitários

```bash
dotnet test src/DatariskMLOps.Tests.Unit/
```

### Executar Testes de Integração

```bash
dotnet test src/DatariskMLOps.Tests.Integration/
```

### Executar Todos os Testes com Cobertura

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 🔒 Segurança

- **Isolamento de Scripts**: Jint executa JavaScript em ambiente controlado
- **Limitações de Recursos**: Timeout, máximo de statements, limite de recursão
- **Sanitização**: Remoção de APIs perigosas (require, import, eval)
- **Validação de Entrada**: Validação rigorosa de todos os inputs
- **Logs de Auditoria**: Registro completo de todas as execuções

## 🔄 Desenvolvimento

### Executar em Desenvolvimento

```bash
# Executar apenas o banco e Redis
docker-compose up postgres redis -d

# Executar a API em desenvolvimento
cd src/DatariskMLOps.API
dotnet run
```

### Migrations

```bash
# Adicionar nova migration
dotnet ef migrations add <MigrationName> -p src/DatariskMLOps.Infrastructure -s src/DatariskMLOps.API

# Aplicar migrations
dotnet ef database update -p src/DatariskMLOps.Infrastructure -s src/DatariskMLOps.API
```

## 📈 Monitoramento

- **Logs**: Arquivos em `/logs` (quando executando via Docker)
- **Health Checks**: `/health` endpoint
- **Hangfire Dashboard**: `/hangfire` para monitorar jobs
- **Métricas**: Logs estruturados com Serilog

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.
