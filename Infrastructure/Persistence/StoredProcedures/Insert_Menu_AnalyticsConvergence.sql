-- =============================================
-- Referencia: INSERT para entrada de menú
-- AnalyticsConvergence (Permiso 18)
-- =============================================
-- 
-- Ajustar @ApplicationId según el ID de la aplicación
-- en la tabla Application (normalmente 1).
-- Ajustar @ParentMenuId si hay un menú padre "Analytics".
--
-- =============================================

DECLARE @ApplicationId INT = 1;
DECLARE @ParentMenuId INT = NULL;

IF NOT EXISTS (SELECT 1 FROM Menu WHERE PermissionNumber = 18 AND ApplicationId = @ApplicationId)
BEGIN
    INSERT INTO Menu (Name, URL, Icon, ParentMenuId, Position, PermissionNumber, Visible, Comment, ApplicationId)
    VALUES (
        N'Análisis de Convergencias',
        N'~/AnalyticsConvergence',
        N'ki-filled ki-abstract-41',
        @ParentMenuId,
        (SELECT ISNULL(MAX(Position), 0) + 1 FROM Menu WHERE ApplicationId = @ApplicationId AND (ParentMenuId = @ParentMenuId OR (ParentMenuId IS NULL AND @ParentMenuId IS NULL))),
        18,
        1,
        N'SPEC 22 — Análisis de Convergencias',
        @ApplicationId
    );
END
