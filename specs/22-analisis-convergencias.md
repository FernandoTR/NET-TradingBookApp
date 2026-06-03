# SPEC 22 — Análisis de Convergencias

> **Estado:** Implementado · **Depende de:** Ninguno · **Fecha:** 2026-06-02
> **Objetivo:** Crear una página de análisis de convergencias que permita cruzar hasta 5 variables (Trigger, Scenery, Direction, Frame, Figure) sobre la tabla Orders para identificar qué combinaciones generan los mejores resultados operativos, rankeados por un Score compuesto calculado en SQL Server.

---

## Alcance

**Incluye:**

- Nueva página `AnalyticsConvergence/Index` con drawer de filtros globales (Category, AccountType, Instrument) más un panel de variables de cruce
- Variables de cruce disponibles: Trigger, Scenery, Direction, Frame, Figure
- **Comportamiento por defecto:** al abrir la página, solo Trigger y Scenery están activas. Direction, Frame y Figure están inactivas (no participan en el GROUP BY ni en el resultado)
- **Activación incremental:** el usuario puede activar variables adicionales mediante toggles/checkboxes. Al activar Direction, el cruce pasa a 3 variables; al activar Frame, 4; al activar Figure, 5
- Cada variable activa muestra su dropdown para seleccionar un valor específico o "Todos"
- Comportamiento de resultado:
  - Todas las variables activas con valor específico → **1 fila** de resultado
  - Alguna variable activa en "Todos" → **tabla rankeada** de combinaciones que matchean, ordenadas por Score descendente
- Nuevo stored procedure `usp_GetTBAnalyticsConvergence` que recibe los IDs de las variables activas y hace GROUP BY dinámico sobre `Orders`, calcula COUNT, SUM(TP1), SUM(TP2), SUM(TP3), SUM(SL), porcentajes y Score final
- Fórmula del Score: `(TP1Rate×10 + TP2Rate×20 + TP3Rate×70) × MIN(Trades/50, 1)`
- Umbral mínimo de trades configurable (parámetro del SP), valor por defecto N=10
- Nuevo permiso `AnalyticsConvergence = 15` en `Domain/Enums/Permissions.cs`
- Nuevo DTO `GetTBAnalyticsConvergenceDto` (keyless) registrado en `ApplicationDbContext.Custom.cs`
- Controller `AnalyticsConvergenceController` con sus acciones y `[Authorize]`
- Vista con DataTables server-side que consume `JsonDataTable` vía AJAX

**Fuera de alcance:**

- Variables Day y Stage (no participan en esta spec)
- Auto-descubrimiento de mejores combinaciones (spec futura, opción B)
- Persistencia de combinaciones guardadas o favoritas
- Exportación a CSV/Excel
- Gráficos o visualizaciones (solo tabla)
- Modificar los stored procedures de analytics existentes

---

## Data model

### Nuevo permiso

```csharp
// Domain/Enums/Permissions.cs
AnalyticsConvergence = 15
```

### Nuevo DTO de parámetros

```csharp
// Application/DTOs/ParametersTBAnalyticsConvergenceDto.cs
public class ParametersTBAnalyticsConvergenceDto : ParametersTBAnalyticsDto
{
    public bool TriggerActive { get; set; }
    public bool SceneryActive { get; set; }
    public bool DirectionActive { get; set; }
    public bool FrameActive { get; set; }
    public bool FigureActive { get; set; }
    
    public int? MinTrades { get; set; }  // default 10
}
```

### Nuevo DTO de resultado (keyless)

```csharp
// Application/DTOs/GetTBAnalyticsConvergenceDto.cs
public class GetTBAnalyticsConvergenceDto
{
    public string Setup { get; set; }
    public int Trades { get; set; }
    public decimal TP1Rate { get; set; }
    public decimal TP2Rate { get; set; }
    public decimal TP3Rate { get; set; }
    public decimal SLRate { get; set; }
    public decimal Score { get; set; }
}
```

### Registro en DbContext

```csharp
// Infrastructure/Persistence/Data/ApplicationDbContext.Custom.cs
modelBuilder.Entity<GetTBAnalyticsConvergenceDto>(entity =>
{
    entity.HasNoKey();
});
```

### Nuevo stored procedure

```sql
usp_GetTBAnalyticsConvergence
    @CategoryId INT,
    @AccountTypeId INT,
    @InstrumentId INT,
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
    @Count INT OUTPUT
```

### Interfaces y servicios

```csharp
// Application/Interfaces/ICatConvergenceService.cs
// Application/Services/CatConvergenceService.cs
// Application/Interfaces/ICatConvergenceRepository.cs
// Infrastructure/Persistence/Repositories/CatConvergenceRepository.cs
```

---

## Plan de implementación

**Paso 1 — Agregar permiso y DTOs**
1. En `Domain/Enums/Permissions.cs`: agregar `AnalyticsConvergence = 15`
2. Crear `Application/DTOs/ParametersTBAnalyticsConvergenceDto.cs` que hereda de `ParametersTBAnalyticsDto`, agrega 5 flags booleanos (`TriggerActive`, `SceneryActive`, `DirectionActive`, `FrameActive`, `FigureActive`) más `MinTrades` nullable
3. Crear `Application/DTOs/GetTBAnalyticsConvergenceDto.cs` con propiedades: `Setup` (string), `Trades` (int), `TP1Rate`, `TP2Rate`, `TP3Rate`, `SLRate`, `Score` (decimal)
4. Compilar para validar

**Paso 2 — Registrar DTO en DbContext**
1. En `Infrastructure/Persistence/Data/ApplicationDbContext.Custom.cs`: agregar `modelBuilder.Entity<GetTBAnalyticsConvergenceDto>(e => e.HasNoKey());`
2. Compilar para validar

**Paso 3 — Crear repositorio e interfaz**
1. Crear `Application/Interfaces/ICatConvergenceRepository.cs` con método:
   `Task<(List<GetTBAnalyticsConvergenceDto> data, int totalCount)> GetTBAnalyticsConvergenceAsync(ParametersTBAnalyticsConvergenceDto parameters)`
2. Crear `Infrastructure/Persistence/Repositories/CatConvergenceRepository.cs` que ejecute el SP vía `FromSqlRaw` con `SqlParameter` para cada parámetro, capturando `@Count OUTPUT`
3. Compilar para validar

**Paso 4 — Crear servicio e interfaz**
1. Crear `Application/Interfaces/ICatConvergenceService.cs` con método `GetTBAnalyticsConvergenceAsync`
2. Crear `Application/Services/CatConvergenceService.cs` que recibe `ICatConvergenceRepository` vía constructor y delega la llamada
3. Compilar para validar

**Paso 5 — Registrar en DI**
1. En `Application/DependencyInjection.cs`: agregar `services.AddScoped<ICatConvergenceService, CatConvergenceService>();`
2. En `Infrastructure/DependencyInjection.cs`: agregar `services.AddScoped<ICatConvergenceRepository, CatConvergenceRepository>();`
3. Compilar para validar

**Paso 6 — Crear el script del stored procedure**
1. Crear script SQL `usp_GetTBAnalyticsConvergence` con parámetros detallados en la sección Data Model
2. El SP construye GROUP BY dinámico según los flags activos. Cada variable inactiva se fuerza a un valor constante (ej: `-1`) para no afectar la agregación
3. Calcula: `TP1Rate = CAST(SUM(CAST(TP1 AS INT)) AS DECIMAL) / COUNT(*) * 100`, análogo para TP2, TP3, SL
4. Calcula: `RawScore = (TP1Rate * 10 + TP2Rate * 20 + TP3Rate * 70)`
5. Calcula: `FinalScore = RawScore * (CASE WHEN COUNT(*) >= 50 THEN 1 ELSE CAST(COUNT(*) AS DECIMAL) / 50 END)`
6. Filtra `HAVING COUNT(*) >= @MinTrades`
7. Soporta búsqueda, ordenamiento y paginación con `OFFSET/FETCH`
8. Devuelve `@Count OUTPUT` con el total de filas (sin paginación)
9. Ejecutar script contra la base de datos

**Paso 7 — Crear controller**
1. Crear `Web/Controllers/AnalyticsConvergenceController.cs`
2. Heredar de `Controller`, inyectar `ICatConvergenceService`, `ICatCategoryService`, `ICatAccountTypeService`, `ICatInstrumentsService`, `ICatTriggerService`, `ICatSceneryService`, `ICatDirectionService`, `ICatFrameService`, `ICatFigureService`
3. Decorar con `[Authorize(Policy = "AnalyticsConvergence")]`
4. Acción `Index()`: GET, retorna vista con modelo que incluye listas para llenar dropdowns de filtros globales
5. Acción `JsonDataTable(ParametersTBAnalyticsConvergenceDto)`: POST, recibe parámetros de DataTables, llama al servicio, retorna JSON `{ draw, recordsFiltered, recordsTotal, data }`
6. Acciones helper para recargar selects vía AJAX: `GetTriggerListSelect`, `GetSceneryListSelect`, `GetDirectionListSelect`, `GetFrameListSelect`, `GetFigureListSelect` (mismo patrón que los controllers existentes)
7. Compilar para validar

**Paso 8 — Crear vista**
1. Crear `Web/Views/AnalyticsConvergence/Index.cshtml`
2. Seguir patrón Metronic Tailwind de las vistas analytics existentes:
   - `kt-container-fixed` con título "Análisis de Convergencias"
   - `kt-app-toolbar` con botón "Filtros" que abre el drawer
3. Drawer (`kt-drawer`) con dos secciones:
   - **Filtros globales:** Category, AccountType, Instrument (kt-select, mismo patrón existente)
   - **Panel de convergencia:** 5 filas, cada una con toggle/checkbox + dropdown:
     - Trigger (toggle ON por defecto) + select de triggers
     - Scenery (toggle ON por defecto) + select de sceneries
     - Direction (toggle OFF por defecto) + select de directions
     - Frame (toggle OFF por defecto) + select de frames
     - Figure (toggle OFF por defecto) + select de figures
   - Campo `MinTrades` (input numérico, default 10)
   - Botones "Limpiar" y "Aplicar"
4. Tabla DataTables con columnas: Setup, Trades, TP1%, TP2%, TP3%, SL%, Score
5. Render de Score con barra de progreso coloreada (verde > 80, amarillo > 50, rojo ≤ 50)
6. Configuración JS: AJAX POST a `~/AnalyticsConvergence/JsonDataTable`, server-side processing, orden por Score descendente por defecto
7. Compilar para validar

**Paso 9 — Configurar menú y política de autorización**
1. Ejecutar INSERT en base de datos para agregar entrada de menú vinculada al permiso `AnalyticsConvergence = 15`, URL `~/AnalyticsConvergence`, padre "Analytics" (si existe) o standalone
2. En `Web/DependencyInjection.cs` o donde se configuren las policies: agregar policy para el permiso 15, siguiendo el mismo patrón de las policies existentes (basadas en `Permissions` enum)
3. Compilar para validar

**Paso 10 — Verificación final**
1. Ejecutar `dotnet build "TradingBookApp.sln"` — debe compilar sin errores
2. Ejecutar `dotnet run --project Web/Web.csproj` — la app inicia sin errores
3. Navegar a la página de AnalyticsConvergence, verificar que carga con Trigger+Scenery activos por defecto
4. Verificar que al activar más variables el resultado cambia
5. Verificar que el Score se calcula correctamente según la fórmula

---

## Criterios de aceptación

- [ ] `Domain/Enums/Permissions.cs` contiene `AnalyticsConvergence = 15`
- [ ] `ParametersTBAnalyticsConvergenceDto` y `GetTBAnalyticsConvergenceDto` existen en `Application/DTOs/`
- [ ] `GetTBAnalyticsConvergenceDto` está registrado como keyless en `ApplicationDbContext.Custom.cs`
- [ ] `ICatConvergenceRepository` y `CatConvergenceRepository` existen y ejecutan `usp_GetTBAnalyticsConvergence` vía `FromSqlRaw`
- [ ] `ICatConvergenceService` y `CatConvergenceService` existen y están registrados en DI
- [ ] El stored procedure `usp_GetTBAnalyticsConvergence` existe en la base de datos
- [ ] `AnalyticsConvergenceController` existe con `Index`, `JsonDataTable` y helpers de select
- [ ] `AnalyticsConvergenceController` tiene `[Authorize(Policy = "AnalyticsConvergence")]`
- [ ] La vista `Index.cshtml` existe en `Web/Views/AnalyticsConvergence/`
- [ ] Al abrir la página, solo Trigger y Scenery están activos
- [ ] Activar Direction → GROUP BY de 3 variables; Frame → 4; Figure → 5
- [ ] Todas las variables activas con valor específico → 1 fila
- [ ] Alguna variable activa en "Todos" → tabla rankeada por Score descendente
- [ ] Score: `(TP1Rate×10 + TP2Rate×20 + TP3Rate×70) × MIN(Trades/50, 1)`
- [ ] Combinaciones con menos de `MinTrades` trades no aparecen
- [ ] Filtros globales (Category, AccountType, Instrument) limitan correctamente
- [ ] DataTables con server-side processing funciona
- [ ] `dotnet build "TradingBookApp.sln"` compila sin errores
- [ ] `dotnet run --project Web/Web.csproj` inicia sin errores

---

## Decisiones tomadas y descartadas

- **Sí:** Stored procedure en SQL Server para GROUP BY dinámico. La tabla Orders puede tener miles de registros; agrupar en C# requeriría cargarlos todos en memoria. El SP lo hace eficientemente en el motor de base de datos.
- **Sí:** Toggles para activar/desactivar variables. Por defecto solo Trigger y Scenery están activas, lo que da un punto de entrada simple. El usuario agrega complejidad incrementalmente.
- **Sí:** Heredar `ParametersTBAnalyticsDto` para el DTO de parámetros. Reutiliza los campos de paginación, búsqueda y ordenamiento ya definidos, más los filtros globales de `ParametersAnalyticsDto`.
- **Sí:** Seguir el patrón de controllers analytics existentes (Drawer + DataTables + helpers de select AJAX). Consistencia visual y de código con los 7 módulos ya migrados.
- **Sí:** Nuevo permiso `AnalyticsConvergence = 15`. Es una funcionalidad nueva e independiente; no corresponde reutilizar permisos de analytics individuales.
- **Sí:** Score como métrica compuesta ponderada. TP3 tiene peso 70 porque representa el target más ambicioso y rentable; TP2 peso 20; TP1 peso 10. El factor de confianza penaliza muestras pequeñas.
- **Sí:** Umbral mínimo de trades configurable vía parámetro del SP. N=10 por defecto evita mostrar combinaciones con porcentajes no representativos.
- **No:** No usar LINQ/EF para el GROUP BY. La complejidad del GROUP BY dinámico (columnas variables según toggles) es impráctica en LINQ y pierde performance.
- **No:** No incluir Day y Stage. El usuario confirmó que quedan fuera de esta spec. Se pueden agregar en una spec futura si se requiere.
- **No:** No hacer auto-descubrimiento (opción B). Se deja para spec futura; esta spec se enfoca en análisis interactivo.
- **No:** No modificar los stored procedures de analytics existentes. Esta spec solo agrega, no modifica.

---

## Riesgos identificados

| Riesgo | Mitigación |
|--------|------------|
| **GROUP BY dinámico complejo en T-SQL** | El SP debe construirse con SQL dinámico (`sp_executesql`) para armar la cláusula GROUP BY según los flags activos. Validar con casos de prueba (2, 3, 4, 5 variables activas) antes de integrar con la app. |
| **Performance con muchas combinaciones** | Si el usuario selecciona "Todos" en varias variables activas, el número de combinaciones puede ser alto. El `HAVING COUNT(*) >= @MinTrades` y la paginación con `OFFSET/FETCH` mitigan el volumen. Considerar un límite máximo de filas en el SP (ej: `TOP 1000`). |
| **El SP no existe en la base de datos** | Sin el SP, `FromSqlRaw` falla en runtime. El script SQL debe ejecutarse manualmente. Documentar la dependencia en el spec y verificar durante el paso 10. |
| **Permiso no configurado en menú** | Si no se inserta la entrada en la tabla de menú vía DB, la página es inaccesible aunque el controller exista. Incluir el INSERT de ejemplo en el spec para referencia. |
| **Colisiones con scaffolding futuro** | El DTO y su registro en `ApplicationDbContext.Custom.cs` están en el archivo protegido contra scaffolding, reduciendo el riesgo. |
