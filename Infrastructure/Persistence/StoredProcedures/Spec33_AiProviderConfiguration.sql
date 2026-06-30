-- =============================================
-- SPEC 33 - Gestion de proveedores IA y API keys
-- Crea la tabla de configuracion, semillas base y menu.
-- =============================================

IF OBJECT_ID(N'dbo.AiProviderConfiguration', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AiProviderConfiguration
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AiProviderConfiguration PRIMARY KEY,
        ProviderName NVARCHAR(100) NOT NULL,
        ModelName NVARCHAR(150) NOT NULL,
        Endpoint NVARCHAR(500) NULL,
        ApiKeyEnvironmentVariable NVARCHAR(150) NOT NULL,
        SupportsVision BIT NOT NULL CONSTRAINT DF_AiProviderConfiguration_SupportsVision DEFAULT (1),
        TimeoutSeconds INT NOT NULL CONSTRAINT DF_AiProviderConfiguration_TimeoutSeconds DEFAULT (60),
        IsActive BIT NOT NULL CONSTRAINT DF_AiProviderConfiguration_IsActive DEFAULT (0),
        IsEnabled BIT NOT NULL CONSTRAINT DF_AiProviderConfiguration_IsEnabled DEFAULT (1),
        CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_AiProviderConfiguration_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(7) NULL,
        DeactivatedAt DATETIME2(7) NULL
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_AiProviderConfiguration_ProviderName'
      AND object_id = OBJECT_ID(N'dbo.AiProviderConfiguration')
)
BEGIN
    CREATE UNIQUE INDEX UX_AiProviderConfiguration_ProviderName
        ON dbo.AiProviderConfiguration (ProviderName);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_AiProviderConfiguration_Active'
      AND object_id = OBJECT_ID(N'dbo.AiProviderConfiguration')
)
BEGIN
    CREATE UNIQUE INDEX UX_AiProviderConfiguration_Active
        ON dbo.AiProviderConfiguration (IsActive)
        WHERE IsActive = 1;
END
GO

MERGE dbo.AiProviderConfiguration AS target
USING
(
    VALUES
        (N'OpenAI', N'gpt-4.1-mini', N'https://api.openai.com/v1/responses', N'OPENAI_API_KEY', 1, 60, 0, 1),
        (N'MiniMax', N'minimax-vision', N'__CHANGE_ME__', N'MINIMAX_API_KEY', 1, 60, 0, 1),
        (N'DeepSeek', N'deepseek-vision', N'__CHANGE_ME__', N'DEEPSEEK_API_KEY', 1, 60, 0, 1),
        (N'GLM', N'glm-vision', N'__CHANGE_ME__', N'GLM_API_KEY', 1, 60, 0, 1),
        (N'Kimi', N'kimi-vision', N'__CHANGE_ME__', N'KIMI_API_KEY', 1, 60, 0, 1)
) AS source
(
    ProviderName,
    ModelName,
    Endpoint,
    ApiKeyEnvironmentVariable,
    SupportsVision,
    TimeoutSeconds,
    IsActive,
    IsEnabled
)
ON target.ProviderName = source.ProviderName
WHEN NOT MATCHED THEN
    INSERT
    (
        ProviderName,
        ModelName,
        Endpoint,
        ApiKeyEnvironmentVariable,
        SupportsVision,
        TimeoutSeconds,
        IsActive,
        IsEnabled,
        CreatedAt
    )
    VALUES
    (
        source.ProviderName,
        source.ModelName,
        source.Endpoint,
        source.ApiKeyEnvironmentVariable,
        source.SupportsVision,
        source.TimeoutSeconds,
        source.IsActive,
        source.IsEnabled,
        SYSUTCDATETIME()
    );
GO

DECLARE @ApplicationId INT = 1;
DECLARE @ParentMenuId INT = NULL;

IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE PermissionNumber = 21 AND ApplicationId = @ApplicationId)
BEGIN
    INSERT INTO dbo.Menu (Name, URL, Icon, ParentMenuId, Position, PermissionNumber, Visible, Comment, ApplicationId)
    VALUES
    (
        N'Proveedores IA',
        N'~/AiProviders',
        N'ki-filled ki-key',
        @ParentMenuId,
        (SELECT ISNULL(MAX(Position), 0) + 1 FROM dbo.Menu WHERE ApplicationId = @ApplicationId AND (ParentMenuId = @ParentMenuId OR (ParentMenuId IS NULL AND @ParentMenuId IS NULL))),
        21,
        1,
        N'SPEC 33 - Gestion de proveedores IA y API keys',
        @ApplicationId
    );
END
GO
