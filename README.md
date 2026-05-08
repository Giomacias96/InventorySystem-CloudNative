# Inventory & Analytics System (Cloud Native & Event-Driven)

Sistema integral de gestión de inventarios y analíticos en tiempo real, diseñado bajo una arquitectura de microservicios y patrones de resiliencia para entornos de nube.

## 🏗️ Arquitectura del Sistema
El proyecto implementa un flujo de datos asíncrono para garantizar la integridad y escalabilidad:
1. **API (.NET 8):** Registra pedidos en SQL Server y utiliza el **Outbox Pattern** para asegurar la consistencia.
2. **Worker Service:** Procesa la tabla Outbox y publica eventos en **Amazon SQS**.
3. **Serverless (AWS Lambda):** Función en C# que consume mensajes de SQS.
4. **NoSQL (DynamoDB):** Almacena proyecciones de analíticos (ventas totales) para consultas de alto rendimiento.



## 🛠️ Stack Tecnológico
* **Backend:** .NET 8 (C#) con Entity Framework Core.
* **Mensajería:** Amazon SQS (Simulado con LocalStack).
* **Serverless:** AWS Lambda (.NET 8).
* **Bases de Datos:** * Relacional: SQL Server (Transaccional).
    * NoSQL: DynamoDB (Analíticos/Lectura rápida).
* **Infraestructura:** Docker & LocalStack.

## 🚀 Cómo Ejecutar (Entorno Local)

### Requisitos Previos
* **Docker Desktop** con soporte para WSL 2.
* **AWS CLI** instalado y configurado (usar `test`/`test` para credenciales).
* **.NET 8 SDK**.

### Paso 1: Levantar Infraestructura (LocalStack)
Ejecuta el contenedor con soporte para Docker-in-Docker (necesario para Lambdas):
```powershell
docker run -d --name inventoryapi -p 4566:4566 -e SERVICES=sqs,dynamodb,lambda -e LAMBDA_RUNTIME_ENVIRONMENT_TIMEOUT=120 -v /var/run/docker.sock:/var/run/docker.sock localstack/localstack:3.4.0
```

### Paso 2: Compilar y Empaquetar la Lambda
Navega a la carpeta de la Lambda y genera el paquete de despliegue:
```powershell
cd microservices/Inventory.LambdaProcessor
dotnet lambda package -c Release -f net8.0 -o bin/Release/net8.0/publish/deploy-package.zip
cd ../..
```

### Paso 3: Configuración Automática de AWS
Ejecuta el script de automatización para crear la cola, la tabla y el mapeo de la Lambda:
```powershell
./scripts/setup-local-aws.ps1
```

### Paso 4: Iniciar la Solución
Abre Inventory.API.slnx en Visual Studio y arranca los proyectos Inventory.API e Inventory.OutboxProcessor.