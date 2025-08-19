@echo off
echo Verificando compilacao dos projetos...

echo.
echo === Compilando DatariskMLOps.Domain ===
dotnet build src\DatariskMLOps.Domain\DatariskMLOps.Domain.csproj
if %ERRORLEVEL% neq 0 (
    echo ERRO: Falha na compilacao do Domain
    pause
    exit /b 1
)

echo.
echo === Compilando DatariskMLOps.Infrastructure ===
dotnet build src\DatariskMLOps.Infrastructure\DatariskMLOps.Infrastructure.csproj
if %ERRORLEVEL% neq 0 (
    echo ERRO: Falha na compilacao da Infrastructure
    pause
    exit /b 1
)

echo.
echo === Compilando DatariskMLOps.API ===
dotnet build src\DatariskMLOps.API\DatariskMLOps.API.csproj
if %ERRORLEVEL% neq 0 (
    echo ERRO: Falha na compilacao da API
    pause
    exit /b 1
)

echo.
echo === Compilando Tests.Unit ===
dotnet build src\DatariskMLOps.Tests.Unit\DatariskMLOps.Tests.Unit.csproj
if %ERRORLEVEL% neq 0 (
    echo ERRO: Falha na compilacao dos Tests.Unit
    pause
    exit /b 1
)

echo.
echo === Compilando Tests.Integration ===
dotnet build src\DatariskMLOps.Tests.Integration\DatariskMLOps.Tests.Integration.csproj
if %ERRORLEVEL% neq 0 (
    echo ERRO: Falha na compilacao dos Tests.Integration
    pause
    exit /b 1
)

echo.
echo ✅ SUCESSO: Todos os projetos compilaram sem erros!
echo.
echo Pressione qualquer tecla para continuar...
pause
