-- =============================================
-- SPEC 34 - Modulo ErrorLogs
-- Agrega la entrada de menu para consultar errores de aplicacion.
-- No crea tabla nueva ni permiso nuevo.
-- =============================================

DECLARE @ApplicationId INT = 1;
DECLARE @ParentMenuId INT = NULL;
DECLARE @PermissionNumber INT = 4; -- Domain.Enums.Permissions.Logs
DECLARE @Position INT = NULL;

IF @Position IS NULL
BEGIN
    SELECT @Position = ISNULL(MAX(Position), 0) + 1
    FROM dbo.Menu
    WHERE ApplicationId = @ApplicationId
      AND (ParentMenuId = @ParentMenuId OR (ParentMenuId IS NULL AND @ParentMenuId IS NULL));
END

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Menu
    WHERE URL = N'~/ErrorLogs'
      AND ApplicationId = @ApplicationId
)
BEGIN
    INSERT INTO dbo.Menu (Name, URL, Icon, ParentMenuId, Position, PermissionNumber, Visible, Comment, ApplicationId)
    VALUES
    (
        N'Errores de Aplicación',
        N'~/ErrorLogs',
        N'ki-filled ki-information-2',
        @ParentMenuId,
        @Position,
        @PermissionNumber,
        1,
        N'SPEC 34 - Modulo ErrorLogs para errores de aplicacion',
        @ApplicationId
    );
END
GO
