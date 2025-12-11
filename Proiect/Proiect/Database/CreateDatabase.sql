-- Script SQL pentru crearea bazei de date PSSCProiectDb
-- Acest script creează toate tabelele necesare pentru proiect

USE master;
GO

-- Creează baza de date dacă nu există
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PSSCProiectDb')
BEGIN
    CREATE DATABASE PSSCProiectDb;
END
GO

USE PSSCProiectDb;
GO

-- Tabel pentru produse
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL UNIQUE,
    StockQuantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Tabel pentru comenzi
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    CustomerName NVARCHAR(200) NOT NULL,
    CustomerEmail NVARCHAR(200) NOT NULL,
    DeliveryStreet NVARCHAR(200) NOT NULL,
    DeliveryCity NVARCHAR(100) NOT NULL,
    DeliveryPostalCode NVARCHAR(20) NOT NULL,
    DeliveryCountry NVARCHAR(100) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Unvalidated',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ConfirmedAt DATETIME2 NULL,
    PaidAt DATETIME2 NULL
);
GO

-- Tabel pentru articolele din comenzi
CREATE TABLE OrderItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

-- Tabel pentru facturi
CREATE TABLE Invoices (
    Id INT PRIMARY KEY IDENTITY(1,1),
    InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
    OrderId INT NOT NULL,
    SubtotalAmount DECIMAL(18,2) NOT NULL,
    VatAmount DECIMAL(18,2) NOT NULL,
    TotalWithVat DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Unvalidated',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    GeneratedAt DATETIME2 NULL,
    CONSTRAINT FK_Invoices_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
);
GO

-- Tabel pentru colete
CREATE TABLE Packages (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    AWBNumber NVARCHAR(50) NULL UNIQUE,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Unvalidated',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ShippedAt DATETIME2 NULL,
    DeliveredAt DATETIME2 NULL,
    CONSTRAINT FK_Packages_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
);
GO

-- Inserare date inițiale pentru produse
INSERT INTO Products (Name, StockQuantity, UnitPrice, CreatedAt, UpdatedAt) VALUES
('Laptop', 10, 999.99, GETUTCDATE(), GETUTCDATE()),
('Mouse', 50, 29.99, GETUTCDATE(), GETUTCDATE()),
('Keyboard', 30, 79.99, GETUTCDATE(), GETUTCDATE()),
('Monitor', 15, 299.99, GETUTCDATE(), GETUTCDATE()),
('Headphones', 25, 149.99, GETUTCDATE(), GETUTCDATE());
GO

-- Creare indexuri pentru performanță optimă
CREATE INDEX IX_Orders_OrderNumber ON Orders(OrderNumber);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);
CREATE INDEX IX_Invoices_OrderId ON Invoices(OrderId);
CREATE INDEX IX_Packages_OrderId ON Packages(OrderId);
GO

PRINT 'Baza de date PSSCProiectDb a fost creată cu succes!';
GO

