# Script para testar a API
Write-Host "Verificando containers..."
docker ps

Write-Host "`nTestando conectividade com a API..."
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -TimeoutSec 5
    Write-Host "API respondeu com status: $($response.StatusCode)"
    Write-Host "Conteúdo: $($response.Content)"
} catch {
    Write-Host "Erro ao conectar: $($_.Exception.Message)"
}

Write-Host "`nTestando Swagger..."
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/swagger" -UseBasicParsing -TimeoutSec 5
    Write-Host "Swagger respondeu com status: $($response.StatusCode)"
} catch {
    Write-Host "Erro ao acessar Swagger: $($_.Exception.Message)"
}
