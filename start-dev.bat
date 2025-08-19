@echo off
echo Iniciando ambiente de desenvolvimento...

echo.
echo === Verificando .NET SDK ===
dotnet --version
if %ERRORLEVEL% neq 0 (
    echo ERRO: .NET SDK nao encontrado!
    echo Por favor, instale o .NET 8 SDK em: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo.
echo === Verificando Docker ===
docker --version
if %ERRORLEVEL% neq 0 (
    echo ERRO: Docker nao encontrado!
    echo Por favor, instale o Docker em: https://docker.com/get-started
    pause
    exit /b 1
)

echo.
echo === Iniciando servicos de dependencia (PostgreSQL + Redis) ===
docker-compose -f docker-compose.dev.yml up -d

echo.
echo === Aguardando servicos iniciarem ===
timeout /t 10 /nobreak

echo.
echo === Compilando solucao ===
dotnet build DatariskMLOps.sln
if %ERRORLEVEL% neq 0 (
    echo ERRO: Falha na compilacao!
    pause
    exit /b 1
)

echo.
echo === Executando migrations ===
cd src\DatariskMLOps.API
dotnet ef database update
if %ERRORLEVEL% neq 0 (
    echo AVISO: Falha nas migrations - execute manualmente se necessario
)
cd ..\..

echo.
echo ✅ Ambiente pronto para desenvolvimento!
echo.
echo Para executar a API:
echo   cd src\DatariskMLOps.API
echo   dotnet run
echo.
echo URLs disponiveis:
echo   API: https://localhost:7000 ou http://localhost:5000
echo   Swagger: http://localhost:5000/swagger
echo   PostgreSQL: localhost:5432
echo   Redis: localhost:6379
echo.
pause
