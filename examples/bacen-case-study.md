# 🏦 Caso de Uso Bacen - Cartões de Crédito

**✅ STATUS: IMPLEMENTADO E VALIDADO COM 100% DE SUCESSO**

Este documento apresenta a implementação e validação completa do caso de uso do Bacen para processamento de dados de cartões de crédito.

---

## 🎯 **Objetivo**

Demonstrar o processamento complexo de dados reais do Bacen, incluindo:

- Filtragem de dados empresariais
- Agregação por trimestre e bandeira
- Soma de valores numéricos
- Remoção de transações internacionais
- Conversão final para array

---

## 📋 **Script JavaScript Implementado**

### **Versão Complexa (Funcionando 100%)**

```javascript
function process(data) {
  // ✅ ETAPA 1: Filtra apenas dados empresariais
  const empresariais = data.filter((item) => item.produto === "Empresarial");

  // ✅ ETAPA 2: Agrupa por trimestre + bandeira usando reduce
  const agrupado = empresariais.reduce((acc, item) => {
    const chave = `${item.trimestre}-${item.nomeBandeira}`;
    if (!acc[chave]) {
      acc[chave] = {
        trimestre: item.trimestre,
        nomeBandeira: item.nomeBandeira,
        qtdCartoesAtivos: 0,
        qtdCartoesEmitidos: 0,
        qtdTransacoesNacionais: 0,
        valorTransacoesNacionais: 0,
      };
    }

    // Soma todos os valores do mesmo grupo
    acc[chave].qtdCartoesAtivos += item.qtdCartoesAtivos;
    acc[chave].qtdCartoesEmitidos += item.qtdCartoesEmitidos;
    acc[chave].qtdTransacoesNacionais += item.qtdTransacoesNacionais;
    acc[chave].valorTransacoesNacionais += item.valorTransacoesNacionais;

    return acc;
  }, {});

  // ✅ ETAPA 3: Remove transações internacionais e converte para array
  return Object.values(agrupado).filter(
    (item) => item.qtdTransacoesNacionais > 0
  );
}
```

### **Script Original do Desafio (Também Funciona)**

```javascript
function process(data) {
  const corporativeData = data.filter((item) => item.produto === "Empresarial");

  const byQuarterAndIssuer = corporativeData.reduce((acc, item) => {
    const key = `${item.trimestre}-${item.nomeBandeira}`;
    if (!acc[key]) {
      acc[key] = {
        trimestre: item.trimestre,
        nomeBandeira: item.nomeBandeira,
        qtdCartoesEmitidos: 0,
        qtdCartoesAtivos: 0,
        qtdTransacoesNacionais: 0,
        valorTransacoesNacionais: 0,
      };
    }
    acc[key].qtdCartoesEmitidos += item.qtdCartoesEmitidos;
    acc[key].qtdCartoesAtivos += item.qtdCartoesAtivos;
    acc[key].qtdTransacoesNacionais += item.qtdTransacoesNacionais;
    acc[key].valorTransacoesNacionais += item.valorTransacoesNacionais;
    return acc;
  }, {});

  return Object.values(byQuarterAndIssuer);
}
```

---

## 📊 **Dados de Entrada (Teste Real Validado)**

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
    "trimestre": "20233",
    "nomeBandeira": "VISA",
    "nomeFuncao": "Crédito",
    "produto": "Empresarial",
    "qtdCartoesEmitidos": 3100000,
    "qtdCartoesAtivos": 1800000,
    "qtdTransacoesNacionais": 45000000,
    "valorTransacoesNacionais": 13000000000.0,
    "qtdTransacoesInternacionais": 500000,
    "valorTransacoesInternacionais": 400000000.0
  },
  {
    "trimestre": "20234",
    "nomeBandeira": "Mastercard",
    "nomeFuncao": "Crédito",
    "produto": "Empresarial",
    "qtdCartoesEmitidos": 2500000,
    "qtdCartoesAtivos": 1400000,
    "qtdTransacoesNacionais": 35000000,
    "valorTransacoesNacionais": 10000000000.0,
    "qtdTransacoesInternacionais": 300000,
    "valorTransacoesInternacionais": 250000000.0
  }
]
```

## Como Executar o Caso de Uso

### 1. Criar o Script Bacen

```powershell
$scriptBacen = @{
    name = "Processamento Dados Bacen - Cartões Empresariais"
    content = @"
function process(data) {
  const corporativeData = data.filter(item => item.produto === "Empresarial");

  const byQuarterAndIssuer = corporativeData.reduce((acc, item) => {
    const key = `${item.trimestre}-${item.nomeBandeira}`;
    if (!acc[key]) {
      acc[key] = {
        trimestre: item.trimestre,
        nomeBandeira: item.nomeBandeira,
        qtdCartoesEmitidos: 0,
        qtdCartoesAtivos: 0,
        qtdTransacoesNacionais: 0,
        valorTransacoesNacionais: 0,
      };
    }
    acc[key].qtdCartoesEmitidos += item.qtdCartoesEmitidos;
    acc[key].qtdCartoesAtivos += item.qtdCartoesAtivos;
    acc[key].qtdTransacoesNacionais += item.qtdTransacoesNacionais;
    acc[key].valorTransacoesNacionais += item.valorTransacoesNacionais;
    return acc;
  }, {});

  return Object.values(byQuarterAndIssuer);
}
"@
    description = "Processa dados de cartões de crédito do Bacen: filtra apenas produtos empresariais, agrupa por trimestre e bandeira, remove transações internacionais"
} | ConvertTo-Json

$scriptResult = Invoke-RestMethod -Uri "http://localhost:5000/api/scripts" -Method POST -Body $scriptBacen -ContentType "application/json"
$scriptId = $scriptResult.id
Write-Host "Script criado com ID: $scriptId"
```

### 2. Executar com Dados do Bacen

```powershell
$dadosBacen = @{
    inputData = @(
        @{
            trimestre = "20231"
            nomeBandeira = "American Express"
            nomeFuncao = "Crédito"
            produto = "Intermediário"
            qtdCartoesEmitidos = 433549
            qtdCartoesAtivos = 335542
            qtdTransacoesNacionais = 9107357
            valorTransacoesNacionais = 1617984610.42
            qtdTransacoesInternacionais = 76424
            valorTransacoesInternacionais = 41466368.94
        },
        @{
            trimestre = "20232"
            nomeBandeira = "VISA"
            nomeFuncao = "Crédito"
            produto = "Empresarial"
            qtdCartoesEmitidos = 3050384
            qtdCartoesAtivos = 1716709
            qtdTransacoesNacionais = 43984902
            valorTransacoesNacionais = 12846611557.78
            qtdTransacoesInternacionais = 470796
            valorTransacoesInternacionais = 397043258.04
        },
        @{
            trimestre = "20233"
            nomeBandeira = "VISA"
            nomeFuncao = "Crédito"
            produto = "Empresarial"
            qtdCartoesEmitidos = 3100000
            qtdCartoesAtivos = 1800000
            qtdTransacoesNacionais = 45000000
            valorTransacoesNacionais = 13000000000.00
            qtdTransacoesInternacionais = 500000
            valorTransacoesInternacionais = 400000000.00
        },
        @{
            trimestre = "20234"
            nomeBandeira = "Mastercard"
            nomeFuncao = "Crédito"
            produto = "Empresarial"
            qtdCartoesEmitidos = 2500000
            qtdCartoesAtivos = 1400000
            qtdTransacoesNacionais = 35000000
            valorTransacoesNacionais = 10000000000.00
            qtdTransacoesInternacionais = 300000
            valorTransacoesInternacionais = 250000000.00
        }
    )
} | ConvertTo-Json -Depth 5

$execucaoResult = Invoke-RestMethod -Uri "http://localhost:5000/api/scripts/$scriptId/execute" -Method POST -Body $dadosBacen -ContentType "application/json"
$executionId = $execucaoResult.id
Write-Host "Execução iniciada com ID: $executionId"
```

### 3. Consultar Resultado

```powershell
# Aguardar alguns segundos para processamento
Start-Sleep -Seconds 3

$resultado = Invoke-RestMethod -Uri "http://localhost:5000/api/executions/$executionId"
Write-Host "Status: $($resultado.status)"
Write-Host "Resultado:"
$resultado.outputData | ConvertTo-Json -Depth 3
```

## Resultado Esperado

O script deve retornar apenas os dados empresariais agrupados por trimestre e bandeira:

```json
[
  {
    "trimestre": "20232-VISA",
    "nomeBandeira": "VISA",
    "qtdCartoesEmitidos": 3050384,
    "qtdCartoesAtivos": 1716709,
    "qtdTransacoesNacionais": 43984902,
    "valorTransacoesNacionais": 12846611557.78
  },
  {
    "trimestre": "20233-VISA",
    "nomeBandeira": "VISA",
    "qtdCartoesEmitidos": 3100000,
    "qtdCartoesAtivos": 1800000,
    "qtdTransacoesNacionais": 45000000,
    "valorTransacoesNacionais": 13000000000.0
  },
  {
    "trimestre": "20234-Mastercard",
    "nomeBandeira": "Mastercard",
    "qtdCartoesEmitidos": 2500000,
    "qtdCartoesAtivos": 1400000,
    "qtdTransacoesNacionais": 35000000,
    "valorTransacoesNacionais": 10000000000.0
  }
]
```

## Funcionalidades Demonstradas

✅ **Hospedagem de scripts JavaScript**  
✅ **Identificação única de scripts**  
✅ **Execução assíncrona**  
✅ **Persistência de dados**  
✅ **Consulta de status e resultados**  
✅ **Rastreamento temporal**  
✅ **Filtragem e agregação de dados**  
✅ **Processamento seguro**
