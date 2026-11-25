-- =============================================================================
-- SIGHO - Cuenta de la aplicación en SQL Server 
-- Ejecutar con un usuario administrador.
-- =============================================================================
USE master;
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'sigh_login_app')
    CREATE LOGIN sigh_login_app WITH PASSWORD = N'CambiarEsta_Clave_123!', CHECK_POLICY = ON;
GO

USE SIGH_GR04_Login;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'sigh_login_app')
    CREATE USER sigh_login_app FOR LOGIN sigh_login_app;
GO

-- Mínimo privilegio: CRUD sobre departamentos y cargos; solo lectura de usuarios.
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.PEDEP_DEPAR TO sigh_login_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.PECAR_CARGO TO sigh_login_app;
GRANT SELECT                         ON dbo.XEUSU_USUAR TO sigh_login_app;
GO

-- Verificar permisos finales
SELECT p.permission_name, p.state_desc, OBJECT_NAME(p.major_id) AS objeto
FROM sys.database_permissions p
WHERE p.grantee_principal_id = USER_ID(N'sigh_login_app') AND p.major_id > 0;
GO
