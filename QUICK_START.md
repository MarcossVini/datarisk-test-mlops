# 🚀 GUIA RÁPIDO - DataRisk MLOps API

## ⚡ Início em 3 Passos

### 1. Executar o Ambiente

```bash
# Windows
start.bat

# Linux/Mac
chmod +x start.sh && ./start.sh

# Manual
docker-compose up -d --build
```

### 2. Verificar se Funcionou

```bash
# Aguarde ~30 segundos, depois teste:
curl http://localhost:5000/health
# Deve retornar: "Healthy"
```

### 3. Acessar Documentação

- **Swagger UI**: http://localhost:5000/swagger
- **API REST**: http://localhost:5000
- **Hangfire**: http://localhost:5000/hangfire

---

## 📝 Teste Rápido da API

### PowerShell (Windows)

```powershell
# 1. Criar script
$script = @{
    name = "Teste Simples"
    content = "function process(data) { return data.length; }"
    description = "Conta elementos"
} | ConvertTo-Json

$resultado = Invoke-RestMethod -Uri "http://localhost:5000/api/scripts" -Method POST -Body $script -ContentType "application/json"
$scriptId = $resultado.id

# 2. Executar script
$execucao = @{
    inputData = @(1, 2, 3, 4, 5)
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/scripts/$scriptId/execute" -Method POST -Body $execucao -ContentType "application/json"
```

### cURL (Linux/Mac)

```bash
# 1. Criar script
SCRIPT_ID=$(curl -s -X POST "http://localhost:5000/api/scripts" \
  -H "Content-Type: application/json" \
  -d '{"name":"Teste Simples","content":"function process(data){return data.length;}","description":"Conta elementos"}' \
  | jq -r '.id')

# 2. Executar script
curl -X POST "http://localhost:5000/api/scripts/$SCRIPT_ID/execute" \
  -H "Content-Type: application/json" \
  -d '{"inputData":[1,2,3,4,5]}'
```

---

## 💾 Teste do Backup

```powershell
# Backup manual
Invoke-RestMethod -Uri "http://localhost:5000/api/backup/database" -Method POST
```

---

## 🛑 Parar Ambiente

```bash
docker-compose down
```

**✅ Pronto! API funcionando em http://localhost:5000**
