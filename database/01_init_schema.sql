/*************************************************************
 * Project: Inventory & Analytics System (Cloud Native)
 * Author: Giovanne Macias
 * Date: April 2026
 * Description: Initial database schema for inventory management 
 * and Outbox pattern implementation.
 *************************************************************/
USE [master];
GO
CREATE DATABASE [InventoryCloudDB];
GO
USE [InventoryCloudDB];
-- Tabla principal de Inventario
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100),
    Stock INT,
    LastUpdate DATETIME DEFAULT GETDATE()
);

-- Tabla de Outbox (Para eventos asíncronos)
CREATE TABLE IntegrationEventOutbox (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    EventType NVARCHAR(100), -- Ejemplo: 'StockUpdated'
    Content NVARCHAR(MAX),   -- El JSON del evento
    OccurredOnUtc DATETIME,
    ProcessedOnUtc DATETIME NULL -- Si ya se mandó a AWS SQS
);