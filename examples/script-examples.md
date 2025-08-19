# Exemplos de Scripts para a Datarisk MLOps API

## Script 1: Processamento de Cartões Corporativos

### Criar o Script

```bash
curl -X POST http://localhost:5000/api/scripts \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Processamento Cartões Corporativos",
    "description": "Filtra cartões empresariais e agrupa por trimestre e bandeira",
    "content": "function process(data) { const corporativeData = data.filter(item => item.produto === \"Empresarial\"); const byQuarterAndIssuer = corporativeData.reduce((acc, item) => { const key = `${item.trimestre}-${item.nomeBandeira}`; if (!acc[key]) { acc[key] = { trimestre: item.trimestre, nomeBandeira: item.nomeBandeira, qtdCartoesEmitidos: 0, qtdCartoesAtivos: 0, qtdTransacoesNacionais: 0, valorTransacoesNacionais: 0 }; } acc[key].qtdCartoesEmitidos += item.qtdCartoesEmitidos; acc[key].qtdCartoesAtivos += item.qtdCartoesAtivos; acc[key].qtdTransacoesNacionais += item.qtdTransacoesNacionais; acc[key].valorTransacoesNacionais += item.valorTransacoesNacionais; return acc; }, {}); return Object.values(byQuarterAndIssuer); }"
  }'
```

### Dados de Exemplo

```json
[
  {
    "trimestre": "20231",
    "nomeBandeira": "American Express",
    "nomeFuncao": "Crédito",
    "produto": "Intermediário",
    "qtdCartoesEmitidos": 433549,
    "qtdCartoesAtivos": 335542,
    "qtdTransacoesNacionais": 9107357,
    "valorTransacoesNacionais": 1617984610.42,
    "qtdTransacoesInternacionais": 76424,
    "valorTransacoesInternacionais": 41466368.94
  },
  {
    "trimestre": "20232",
    "nomeBandeira": "VISA",
    "nomeFuncao": "Crédito",
    "produto": "Empresarial",
    "qtdCartoesEmitidos": 3050384,
    "qtdCartoesAtivos": 1716709,
    "qtdTransacoesNacionais": 43984902,
    "valorTransacoesNacionais": 12846611557.78,
    "qtdTransacoesInternacionais": 470796,
    "valorTransacoesInternacionais": 397043258.04
  },
  {
    "trimestre": "20232",
    "nomeBandeira": "Mastercard",
    "nomeFuncao": "Crédito",
    "produto": "Empresarial",
    "qtdCartoesEmitidos": 2150000,
    "qtdCartoesAtivos": 1200000,
    "qtdTransacoesNacionais": 30000000,
    "valorTransacoesNacionais": 8000000000.0,
    "qtdTransacoesInternacionais": 300000,
    "valorTransacoesInternacionais": 250000000.0
  }
]
```

## Script 2: Análise de Fraudes

### Criar o Script

```bash
curl -X POST http://localhost:5000/api/scripts \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Detecção de Padrões de Fraude",
    "description": "Identifica transações suspeitas baseado em padrões",
    "content": "function process(data) { const suspiciousThreshold = 5000; const frequencyThreshold = 10; const suspiciousTransactions = data.filter(transaction => { const isHighValue = transaction.valor > suspiciousThreshold; const isFrequent = transaction.frequenciaDiaria > frequencyThreshold; const isNightTime = transaction.horaTransacao >= 22 || transaction.horaTransacao <= 6; return isHighValue || (isFrequent && isNightTime); }); const summary = { totalTransactions: data.length, suspiciousCount: suspiciousTransactions.length, suspiciousPercentage: (suspiciousTransactions.length / data.length * 100).toFixed(2), highValueCount: suspiciousTransactions.filter(t => t.valor > suspiciousThreshold).length, nightTimeCount: suspiciousTransactions.filter(t => t.horaTransacao >= 22 || t.horaTransacao <= 6).length }; return { summary, suspiciousTransactions }; }"
  }'
```

### Dados de Exemplo para Fraude

```json
[
  {
    "id": "TXN001",
    "valor": 2500.0,
    "horaTransacao": 14,
    "frequenciaDiaria": 3,
    "localizacao": "São Paulo",
    "tipoCartao": "Crédito"
  },
  {
    "id": "TXN002",
    "valor": 8500.0,
    "horaTransacao": 23,
    "frequenciaDiaria": 1,
    "localizacao": "Rio de Janeiro",
    "tipoCartao": "Débito"
  },
  {
    "id": "TXN003",
    "valor": 150.0,
    "horaTransacao": 2,
    "frequenciaDiaria": 15,
    "localizacao": "Belo Horizonte",
    "tipoCartao": "Crédito"
  }
]
```

## Script 3: Relatório de Performance por Região

### Criar o Script

```bash
curl -X POST http://localhost:5000/api/scripts \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Performance por Região",
    "description": "Agrupa métricas de performance por região geográfica",
    "content": "function process(data) { const regionStats = data.reduce((acc, item) => { const region = item.regiao; if (!acc[region]) { acc[region] = { regiao: region, totalVendas: 0, totalTransacoes: 0, ticketMedio: 0, conversao: 0, totalClientes: new Set() }; } acc[region].totalVendas += item.valorVenda; acc[region].totalTransacoes += item.numeroTransacoes; acc[region].totalClientes.add(item.clienteId); return acc; }, {}); const result = Object.values(regionStats).map(region => ({ regiao: region.regiao, totalVendas: region.totalVendas, totalTransacoes: region.totalTransacoes, ticketMedio: (region.totalVendas / region.totalTransacoes).toFixed(2), totalClientes: region.totalClientes.size, vendaPorCliente: (region.totalVendas / region.totalClientes.size).toFixed(2) })); return result.sort((a, b) => b.totalVendas - a.totalVendas); }"
  }'
```

### Dados de Exemplo para Performance

```json
[
  {
    "regiao": "Sudeste",
    "valorVenda": 15000.0,
    "numeroTransacoes": 25,
    "clienteId": "CLI001",
    "mes": "2023-01"
  },
  {
    "regiao": "Sul",
    "valorVenda": 8500.0,
    "numeroTransacoes": 12,
    "clienteId": "CLI002",
    "mes": "2023-01"
  },
  {
    "regiao": "Sudeste",
    "valorVenda": 12000.0,
    "numeroTransacoes": 18,
    "clienteId": "CLI003",
    "mes": "2023-01"
  },
  {
    "regiao": "Nordeste",
    "valorVenda": 6500.0,
    "numeroTransacoes": 15,
    "clienteId": "CLI004",
    "mes": "2023-01"
  },
  {
    "regiao": "Sul",
    "valorVenda": 9200.0,
    "numeroTransacoes": 14,
    "clienteId": "CLI005",
    "mes": "2023-01"
  }
]
```

## Script 4: Limpeza e Normalização de Dados

### Criar o Script

```bash
curl -X POST http://localhost:5000/api/scripts \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Limpeza de Dados de Cliente",
    "description": "Normaliza e limpa dados de clientes",
    "content": "function process(data) { const cleanData = data.map(cliente => { const nome = cliente.nome ? cliente.nome.trim().toUpperCase() : \"NOME_NAO_INFORMADO\"; const email = cliente.email ? cliente.email.toLowerCase().trim() : null; const telefone = cliente.telefone ? cliente.telefone.replace(/\\D/g, \"\") : null; const cpf = cliente.cpf ? cliente.cpf.replace(/\\D/g, \"\") : null; const isValidEmail = email && /^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$/.test(email); const isValidCPF = cpf && cpf.length === 11; const isValidPhone = telefone && telefone.length >= 10; return { id: cliente.id, nome, email: isValidEmail ? email : null, telefone: isValidPhone ? telefone : null, cpf: isValidCPF ? cpf : null, status: \"PROCESSADO\", dataProcessamento: new Date().toISOString(), validacoes: { emailValido: isValidEmail, cpfValido: isValidCPF, telefoneValido: isValidPhone } }; }); const resumo = { totalRegistros: data.length, emailsValidos: cleanData.filter(c => c.validacoes.emailValido).length, cpfsValidos: cleanData.filter(c => c.validacoes.cpfValido).length, telefonesValidos: cleanData.filter(c => c.validacoes.telefoneValido).length }; return { resumo, dados: cleanData }; }"
  }'
```

### Dados de Exemplo para Limpeza

```json
[
  {
    "id": 1,
    "nome": "  João Silva  ",
    "email": "JOAO@EMAIL.COM",
    "telefone": "(11) 99999-9999",
    "cpf": "123.456.789-00"
  },
  {
    "id": 2,
    "nome": "maria santos",
    "email": "maria.email.invalido",
    "telefone": "11888888888",
    "cpf": "98765432100"
  },
  {
    "id": 3,
    "nome": "",
    "email": "carlos@teste.com.br",
    "telefone": "123",
    "cpf": "000.000.000-00"
  }
]
```

## Como Executar um Exemplo

1. **Escolha um script** dos exemplos acima
2. **Crie o script** usando o comando curl
3. **Copie o ID retornado** do script criado
4. **Execute o script** com os dados de exemplo:

```bash
curl -X POST http://localhost:5000/api/executions \
  -H "Content-Type: application/json" \
  -d '{
    "scriptId": "SEU_SCRIPT_ID_AQUI",
    "data": [DADOS_DE_EXEMPLO_AQUI]
  }'
```

5. **Monitore a execução** usando o ID retornado:

```bash
curl -X GET http://localhost:5000/api/executions/SEU_EXECUTION_ID_AQUI
```
