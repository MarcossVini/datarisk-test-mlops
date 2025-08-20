@echo off
REM start.bat

echo Iniciando ambiente Datarisk MLOps API...

REM Build e start dos containers
docker-compose up -d --build

echo Aguardando inicializacao dos servicos...
timeout /t 30 /nobreak

REM Verificar status dos serviços
docker-compose ps

echo Ambiente iniciado com sucesso!
echo API disponivel em: http://localhost:5000 
echo Swagger UI: http://localhost:5000/swagger
echo Hangfire Dashboard: http://localhost:5000/hangfire
echo Health Check: http://localhost:5000/health
