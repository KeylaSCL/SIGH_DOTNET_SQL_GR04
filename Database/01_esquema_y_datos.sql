-- =============================================================================
-- SIGHO - Esquema SQL Server 
-- Ejecutar en SQL Server Management Studio (SSMS) con un usuario administrador.

-- =============================================================================
IF DB_ID(N'SIGH_GR04_Login') IS NULL
    CREATE DATABASE SIGH_GR04_Login;
GO

USE SIGH_GR04_Login;
GO

-- ---------------------------------------------------------------- Departamentos
IF OBJECT_ID(N'dbo.PEDEP_DEPAR', N'U') IS NULL
CREATE TABLE dbo.PEDEP_DEPAR (
    PEDEP_CODIGO CHAR(3)     NOT NULL,
    PEDEP_DESCRI VARCHAR(50) NOT NULL,
    CONSTRAINT PK_PEDEP_DEPAR PRIMARY KEY (PEDEP_CODIGO)
);
GO

-- ---------------------------------------------------------------- Cargos (suposición)
IF OBJECT_ID(N'dbo.PECAR_CARGO', N'U') IS NULL
CREATE TABLE dbo.PECAR_CARGO (
    PECAR_CODIGO CHAR(3)     NOT NULL,
    PEDEP_CODIGO CHAR(3)     NOT NULL,
    PECAR_DESCRI VARCHAR(50) NOT NULL,
    CONSTRAINT PK_PECAR_CARGO PRIMARY KEY (PECAR_CODIGO),
    CONSTRAINT FK_PECAR_PEDEP FOREIGN KEY (PEDEP_CODIGO)
        REFERENCES dbo.PEDEP_DEPAR (PEDEP_CODIGO)   -- sin ON DELETE CASCADE: el error 547 protege los datos
);
GO

-- ---------------------------------------------------------------- Usuarios
IF OBJECT_ID(N'dbo.XEUSU_USUAR', N'U') IS NULL
CREATE TABLE dbo.XEUSU_USUAR (
    XEUSU_CODIGO VARCHAR(20)  NOT NULL,
    PEPER_CODIGO VARCHAR(10)  NULL,
    XEEST_CODIGO CHAR(1)      NOT NULL,
    XEUSU_PASWD  VARCHAR(255) NOT NULL,   -- formato pbkdf2_sha256$iter$sal$hash
    XEUSU_INTFAL INT          NOT NULL CONSTRAINT DF_XEUSU_INTFAL DEFAULT 0,
    CONSTRAINT PK_XEUSU_USUAR PRIMARY KEY (XEUSU_CODIGO)
);
GO

-- ---------------------------------------------------------------- Datos iniciales
-- Usuario de prueba:  ADMIN  /  Admin123*   (CAMBIAR o borrar antes de usar en serio)
IF NOT EXISTS (SELECT 1 FROM dbo.XEUSU_USUAR WHERE XEUSU_CODIGO = 'ADMIN')
INSERT INTO dbo.XEUSU_USUAR (XEUSU_CODIGO, PEPER_CODIGO, XEEST_CODIGO, XEUSU_PASWD, XEUSU_INTFAL)
VALUES ('ADMIN', NULL, 'A',
        'pbkdf2_sha256$600000$oae98u+PXnnTTa9uUJReRQ==$sRxqrGfCwk135cb7B/Z7If2K0vFHUNEM7EXgUMd+Hys=',
        0);

IF NOT EXISTS (SELECT 1 FROM dbo.PEDEP_DEPAR)
INSERT INTO dbo.PEDEP_DEPAR (PEDEP_CODIGO, PEDEP_DESCRI) VALUES
    ('D01', 'Administración'),
    ('D02', 'Radiología'),
    ('D03', 'Emergencias');
GO
