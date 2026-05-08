# Script de configuración automática para LocalStack - Inventory System
$endpoint = "http://localhost:4566"
$lambdaZip = "..\microservices\Inventory.LambdaProcessor\bin\Release\net8.0\publish\deploy-package.zip"

Write-Host "--- 🚀 Iniciando configuración de AWS Local ---" -ForegroundColor Cyan

# 1. Crear Cola SQS
Write-Host "1. Creando cola SQS..." -ForegroundColor Yellow
aws --endpoint-url=$endpoint sqs create-queue --queue-name inventory-orders-queue

# 2. Crear Tabla DynamoDB
Write-Host "2. Creando tabla DynamoDB..." -ForegroundColor Yellow
aws --endpoint-url=$endpoint dynamodb create-table `
    --table-name InventorySummary `
    --attribute-definitions AttributeName=ProductId,AttributeType=N `
    --key-schema AttributeName=ProductId,KeyType=HASH `
    --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5

# 3. Publicar Función Lambda (Si el ZIP existe)
if (Test-Path $lambdaZip) {
    Write-Host "3. Publicando AWS Lambda..." -ForegroundColor Yellow
    aws --endpoint-url=$endpoint lambda create-function `
        --function-name OrderProcessorLambda `
        --runtime dotnet8 `
        --handler Inventory.LambdaProcessor::Inventory.LambdaProcessor.Function::FunctionHandler `
        --role arn:aws:iam::000000000000:role/lambda-role `
        --zip-file fileb://$lambdaZip `
        --timeout 30
} else {
    Write-Host "⚠️ Advertencia: No se encontró el ZIP de la Lambda. Compila el proyecto antes de mapear." -ForegroundColor Red
}

# 4. Crear Event Source Mapping (Conexión SQS -> Lambda)
Write-Host "4. Conectando SQS con Lambda..." -ForegroundColor Yellow
$queueArn = (aws --endpoint-url=$endpoint sqs get-queue-attributes --queue-url "$endpoint/000000000000/inventory-orders-queue" --attribute-names QueueArn | ConvertFrom-Json).Attributes.QueueArn

aws --endpoint-url=$endpoint lambda create-event-source-mapping `
    --function-name OrderProcessorLambda `
    --event-source-arn $queueArn

Write-Host "--- ✅ Infraestructura local configurada con éxito ---" -ForegroundColor Green
Write-Host "ARN de la cola: $queueArn"