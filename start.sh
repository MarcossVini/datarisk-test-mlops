#!/bin/bash
# start.sh

echo "Iniciando ambiente Datarisk MLOps API..."

# Build e start dos containers
docker-compose up -d --build

echo "Aguardando inicialização dos serviços..."
sleep 30

# Verificar status dos serviços
docker-compose ps

echo "Ambiente iniciado com sucesso!"
echo "API disponível em: http://localhost:5000"
echo "Swagger UI: http://localhost:5000/swagger"
echo "Hangfire Dashboard: http://localhost:5000/hangfire"
echo "Health Check: http://localhost:5000/health"
