-- =============================================
-- usp_GetTBAnalyticsConvergence
-- 
-- Analisis de convergencias: cruza hasta 5 
-- variables (Trigger, Scenery, Direction, 
-- Frame, Figure) sobre Orders y calcula el 
-- Score compuesto.
-- 
-- =============================================

CREATE OR ALTER PROCEDURE usp_GetTBAnalyticsConvergence
    @CategoryId INT = NULL,
    @AccountTypeId INT = NULL,
    @InstrumentId INT = NULL,
    @TriggerId INT = NULL,
    @SceneryId INT = NULL,
    @DirectionId INT = NULL,
    @FrameId INT = NULL,
    @FigureId INT = NULL,
    @TriggerActive BIT = 1,
    @SceneryActive BIT = 1,
    @DirectionActive BIT = 0,
    @FrameActive BIT = 0,
    @FigureActive BIT = 0,
    @MinTrades INT = 10,
    @SearchValue NVARCHAR(500) = NULL,
    @OrderByColumn NVARCHAR(100) = NULL,
    @SortColumnDir NVARCHAR(10) = NULL,
    @Skip INT = 0,
    @Take INT = 10,
    @Count INT = 0 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sqlSelect NVARCHAR(MAX) = '';
    DECLARE @sqlGroupBy NVARCHAR(MAX) = '';
    DECLARE @sqlWhere NVARCHAR(MAX) = 'WHERE 1=1 ';
    DECLARE @sql NVARCHAR(MAX);
    DECLARE @orderBy NVARCHAR(MAX) = 'Score DESC';
    DECLARE @params NVARCHAR(MAX);
    DECLARE @hasSetup BIT = 0;

    -- =============================================
    -- Build Setup name (CONCAT parts) and GROUP BY
    -- =============================================

    SET @sqlSelect = 'CONCAT(';

    IF @TriggerActive = 1
    BEGIN
        SET @sqlSelect = @sqlSelect + 'TRG.Code + '' '', TRG.Description, '' + '', ';
        SET @sqlGroupBy = @sqlGroupBy + 'O.Cat_TriggerId, ';
        SET @hasSetup = 1;
    END

    IF @SceneryActive = 1
    BEGIN
        SET @sqlSelect = @sqlSelect + 'SC.Code + '' '', SC.Description, '' + '', ';
        SET @sqlGroupBy = @sqlGroupBy + 'O.Cat_SceneryId, ';
        SET @hasSetup = 1;
    END

    IF @DirectionActive = 1
    BEGIN
        SET @sqlSelect = @sqlSelect + 'DIR.Code + '' '', DIR.Description, '' + '', ';
        SET @sqlGroupBy = @sqlGroupBy + 'O.Cat_DirectionId, ';
        SET @hasSetup = 1;
    END

    IF @FrameActive = 1
    BEGIN
        SET @sqlSelect = @sqlSelect + 'FR.Code + '' '', FR.Description, '' + '', ';
        SET @sqlGroupBy = @sqlGroupBy + 'O.Cat_FrameId, ';
        SET @hasSetup = 1;
    END

    IF @FigureActive = 1
    BEGIN
        SET @sqlSelect = @sqlSelect + 'FIG.Code + '' '', FIG.Description, '' + '', ';
        SET @sqlGroupBy = @sqlGroupBy + 'O.Cat_FigureId, ';
        SET @hasSetup = 1;
    END

    IF @hasSetup = 0
    BEGIN
        SET @sqlSelect = @sqlSelect + '''Sin variables'', ';
        SET @sqlGroupBy = @sqlGroupBy + '0, ';
    END

    SET @sqlSelect = LEFT(@sqlSelect, LEN(@sqlSelect) - 1);
    SET @sqlGroupBy = LEFT(@sqlGroupBy, LEN(@sqlGroupBy) - 1);

    SET @sqlSelect = @sqlSelect + ')';

    -- =============================================
    -- Build WHERE clause for filters
    -- =============================================

    IF @CategoryId IS NOT NULL
        SET @sqlWhere = @sqlWhere + 'AND O.Cat_CategoryId = @CategoryId ';

    IF @AccountTypeId IS NOT NULL
        SET @sqlWhere = @sqlWhere + 'AND ACC.Cat_AccountTypeId = @AccountTypeId ';

    IF @InstrumentId IS NOT NULL
        SET @sqlWhere = @sqlWhere + 'AND O.Cat_InstrumentId = @InstrumentId ';

    IF @TriggerId IS NOT NULL
        SET @sqlWhere = @sqlWhere + 'AND O.Cat_TriggerId = @TriggerId ';

    IF @SceneryId IS NOT NULL
        SET @sqlWhere = @sqlWhere + 'AND O.Cat_SceneryId = @SceneryId ';

    IF @DirectionId IS NOT NULL
        SET @sqlWhere = @sqlWhere + 'AND O.Cat_DirectionId = @DirectionId ';

    IF @FrameId IS NOT NULL
        SET @sqlWhere = @sqlWhere + 'AND O.Cat_FrameId = @FrameId ';

    IF @FigureId IS NOT NULL
        SET @sqlWhere = @sqlWhere + 'AND O.Cat_FigureId = @FigureId ';

    -- =============================================
    -- Build ORDER BY
    -- =============================================

    IF @OrderByColumn IS NOT NULL AND @OrderByColumn <> ''
    BEGIN
        IF @OrderByColumn = 'Setup'
            SET @orderBy = 'Setup ' + CASE WHEN @SortColumnDir = 'asc' THEN 'ASC' ELSE 'DESC' END;
        ELSE IF @OrderByColumn = 'Trades'
            SET @orderBy = 'Trades ' + CASE WHEN @SortColumnDir = 'asc' THEN 'ASC' ELSE 'DESC' END;
        ELSE IF @OrderByColumn = 'TP1Rate'
            SET @orderBy = 'TP1Rate ' + CASE WHEN @SortColumnDir = 'asc' THEN 'ASC' ELSE 'DESC' END;
        ELSE IF @OrderByColumn = 'TP2Rate'
            SET @orderBy = 'TP2Rate ' + CASE WHEN @SortColumnDir = 'asc' THEN 'ASC' ELSE 'DESC' END;
        ELSE IF @OrderByColumn = 'TP3Rate'
            SET @orderBy = 'TP3Rate ' + CASE WHEN @SortColumnDir = 'asc' THEN 'ASC' ELSE 'DESC' END;
        ELSE IF @OrderByColumn = 'SLRate'
            SET @orderBy = 'SLRate ' + CASE WHEN @SortColumnDir = 'asc' THEN 'ASC' ELSE 'DESC' END;
        ELSE IF @OrderByColumn = 'Score'
            SET @orderBy = 'Score ' + CASE WHEN @SortColumnDir = 'asc' THEN 'ASC' ELSE 'DESC' END;
    END

    -- =============================================
    -- Build and execute dynamic query
    -- =============================================

    SET @sql = '
    WITH AggData AS (
        SELECT
            ' + @sqlSelect + ' AS Setup,
            COUNT(*) AS Trades,
            CAST(SUM(CAST(ISNULL(O.TP1, 0) AS INT)) AS DECIMAL(10,2)) / NULLIF(COUNT(*), 0) * 100 AS TP1Rate,
            CAST(SUM(CAST(ISNULL(O.TP2, 0) AS INT)) AS DECIMAL(10,2)) / NULLIF(COUNT(*), 0) * 100 AS TP2Rate,
            CAST(SUM(CAST(ISNULL(O.TP3, 0) AS INT)) AS DECIMAL(10,2)) / NULLIF(COUNT(*), 0) * 100 AS TP3Rate,
            CAST(SUM(CAST(ISNULL(O.SL, 0) AS INT)) AS DECIMAL(10,2)) / NULLIF(COUNT(*), 0) * 100 AS SLRate,
            (
                CAST(SUM(CAST(ISNULL(O.TP1, 0) AS INT)) AS DECIMAL(10,2)) / NULLIF(COUNT(*), 0) * 100 * 10 +
                CAST(SUM(CAST(ISNULL(O.TP2, 0) AS INT)) AS DECIMAL(10,2)) / NULLIF(COUNT(*), 0) * 100 * 20 +
                CAST(SUM(CAST(ISNULL(O.TP3, 0) AS INT)) AS DECIMAL(10,2)) / NULLIF(COUNT(*), 0) * 100 * 70
            )
            * CASE WHEN COUNT(*) >= 50 THEN 1 ELSE CAST(COUNT(*) AS DECIMAL(10,2)) / 50 END AS Score
        FROM Orders O
        JOIN Accounts ACC ON O.AccountId = ACC.Id
        LEFT JOIN Cat_Trigger TRG ON O.Cat_TriggerId = TRG.Id
        LEFT JOIN Cat_Scenery SC ON O.Cat_SceneryId = SC.Id
        LEFT JOIN Cat_Direction DIR ON O.Cat_DirectionId = DIR.Id
        LEFT JOIN Cat_Frame FR ON O.Cat_FrameId = FR.Id
        LEFT JOIN Cat_Figure FIG ON O.Cat_FigureId = FIG.Id
        ' + @sqlWhere + '
        GROUP BY ' + @sqlGroupBy + '
        HAVING COUNT(*) >= @MinTrades
    )
    SELECT *
    INTO #TempAgg
    FROM AggData
    WHERE (@SearchValue IS NULL OR Setup LIKE ''%'' + @SearchValue + ''%'');

    SELECT @Count = COUNT(*) FROM #TempAgg;

    SELECT Setup, Trades, TP1Rate, TP2Rate, TP3Rate, SLRate, Score
    FROM #TempAgg
    ORDER BY ' + @orderBy + '
    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

    DROP TABLE #TempAgg;
    ';

    SET @params = N'@CategoryId INT, @AccountTypeId INT, @InstrumentId INT,
        @TriggerId INT, @SceneryId INT, @DirectionId INT, @FrameId INT, @FigureId INT,
        @MinTrades INT, @SearchValue NVARCHAR(500), @Skip INT, @Take INT,
        @Count INT OUTPUT';

    EXEC sp_executesql @sql, @params,
        @CategoryId, @AccountTypeId, @InstrumentId,
        @TriggerId, @SceneryId, @DirectionId, @FrameId, @FigureId,
        @MinTrades, @SearchValue,
        @Skip, @Take,
        @Count OUTPUT;
END
