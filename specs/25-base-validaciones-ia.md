# SPEC 25 - Base de validaciones IA sin almacenamiento de imagenes

> **Estado:** Implementado - **Depende de:** Ninguno - **Fecha:** 2026-06-25
> **Objetivo:** Crear la base de dominio, aplicacion y persistencia para guardar validaciones de trade con IA solamente cuando exista un resultado completo, sin almacenar imagenes ni rutas de archivos.

---

## Alcance

**Incluye:**

- Crear las entidades necesarias para registrar una validacion IA completada.
- Guardar proveedor, modelo, version de prompt, version de esquema y respuesta JSON estructurada.
- Guardar datos propuestos por el usuario, datos detectados por IA y datos confirmados por el usuario.
- Guardar reglas evaluadas y resultado final de la validacion.
- Relacionar opcionalmente una validacion con una orden creada despues.
- Agregar interfaces y DTOs base en `Application`.
- Agregar repositorio de validaciones en `Infrastructure`.
- Mantener compatibilidad con el enfoque database-first del proyecto.

**Fuera de alcance:**

- Almacenar imagenes, rutas de imagen, hashes persistentes o metadatos persistentes de archivos.
- Enviar imagenes a proveedores IA.
- Implementar adaptadores OpenAI, MiniMax, DeepSeek, GLM o Kimi.
- Ejecutar reglas de estrategia, score o convergencias.
- Crear pantallas MVC.
- Crear ordenes desde una validacion.
- Chat de seguimiento.
- Metricas de aprendizaje operativo.

---

## Data model

Se agregan entidades en `Domain/Entities`:

```csharp
public partial class AiTradeValidation
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public int? OrderId { get; set; }
    public int InstrumentId { get; set; }
    public int DirectionId { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public string? UserComment { get; set; }
    public int? DetectedTriggerId { get; set; }
    public int? DetectedSceneryId { get; set; }
    public int? DetectedFigureId { get; set; }
    public int? DetectedFrameId { get; set; }
    public int? DetectedStageId { get; set; }
    public byte? DetectedLocationType { get; set; }
    public byte? DetectedConfirmationType { get; set; }
    public bool? DetectedIsTrendAligned { get; set; }
    public bool? DetectedIsPivotZone { get; set; }
    public int? ConfirmedTriggerId { get; set; }
    public int? ConfirmedSceneryId { get; set; }
    public int? ConfirmedFigureId { get; set; }
    public int? ConfirmedFrameId { get; set; }
    public int? ConfirmedStageId { get; set; }
    public byte? ConfirmedLocationType { get; set; }
    public byte? ConfirmedConfirmationType { get; set; }
    public bool? ConfirmedIsTrendAligned { get; set; }
    public bool? ConfirmedIsPivotZone { get; set; }
    public decimal? RiskRewardRatio { get; set; }
    public short? StructuralScore { get; set; }
    public int? TotalScore { get; set; }
    public string? Grade { get; set; }
    public decimal? VisualConfidence { get; set; }
    public string ValidationStatus { get; set; } = null!;
    public string ProviderName { get; set; } = null!;
    public string ModelName { get; set; } = null!;
    public string PromptVersion { get; set; } = null!;
    public string SchemaVersion { get; set; } = null!;
    public string ModelResponseJson { get; set; } = null!;
    public string FinalSummary { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}
```

```csharp
public partial class AiTradeValidationRule
{
    public int Id { get; set; }
    public int ValidationId { get; set; }
    public string RuleCode { get; set; } = null!;
    public string RuleName { get; set; } = null!;
    public string Result { get; set; } = null!;
    public decimal Weight { get; set; }
    public decimal ScoreObtained { get; set; }
    public string? Evidence { get; set; }
    public string Source { get; set; } = null!;
}
```

Se agregan enums en `Domain/Enums`:

```csharp
public enum AiValidationStatus
{
    Valid = 1,
    ConditionallyValid = 2,
    Invalid = 3,
    InsufficientEvidence = 4
}

public enum ValidationRuleResult
{
    Passed = 1,
    Failed = 2,
    NotConfirmable = 3,
    NotApplicable = 4
}

public enum ValidationSource
{
    UserInput = 1,
    AiVision = 2,
    DeterministicRule = 3,
    HistoricalEvidence = 4,
    UserConfirmation = 5
}
```

Se agregan DTOs en `Application/DTOs/AiValidation`:

- `CreateAiValidationDto`.
- `AiVisionExtractionDto`.
- `AiValidationResultDto`.
- `AiValidationRuleResultDto`.
- `ConfirmedAiValidationDto`.

Se agregan interfaces en `Application/Interfaces`:

```csharp
public interface IAiTradeValidationRepository
{
    Task<int> SaveCompletedAsync(AiTradeValidation validation, IEnumerable<AiTradeValidationRule> rules, CancellationToken cancellationToken);
    Task<AiTradeValidation?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AiTradeValidation>> GetByUserAsync(string userId, CancellationToken cancellationToken);
    Task<bool> LinkOrderAsync(int validationId, int orderId, string userId, CancellationToken cancellationToken);
}
```

No existe entidad `AiTradeValidationImage` en esta spec porque las imagenes no se persisten.

---

## Plan de implementacion

**Paso 1 - Crear contratos de dominio**

1. Agregar `AiTradeValidation` y `AiTradeValidationRule` en `Domain/Entities`.
2. Agregar enums de estado, resultado de regla y fuente en `Domain/Enums`.
3. Compilar para validar dependencias.

**Paso 2 - Crear DTOs base**

1. Crear carpeta `Application/DTOs/AiValidation`.
2. Agregar DTOs de solicitud, extraccion, resultado, regla y confirmacion.
3. Mantener los DTOs libres de `IFormFile` para que la base no dependa de MVC.
4. Compilar para validar.

**Paso 3 - Crear interfaz de repositorio**

1. Agregar `IAiTradeValidationRepository` en `Application/Interfaces`.
2. Definir metodos para guardar resultado completo, consultar historial y vincular orden.
3. Compilar para validar.

**Paso 4 - Preparar persistencia database-first**

1. Crear script SQL versionado para tablas `AiTradeValidation` y `AiTradeValidationRule`.
2. No usar EF migrations salvo decision futura del proyecto.
3. Agregar `DbSet` y relaciones en `ApplicationDbContext` o archivo parcial seguro si aplica.
4. Compilar para validar.

**Paso 5 - Crear repositorio**

1. Implementar `AiTradeValidationRepository` en `Infrastructure/Persistence/Repositories`.
2. Guardar validacion y reglas en una transaccion.
3. Rechazar guardado si falta `ModelResponseJson`, `ProviderName`, `ModelName` o `FinalSummary`.
4. Registrar el repositorio en `Infrastructure/DependencyInjection.cs`.
5. Compilar para validar.

**Paso 6 - Verificacion final**

1. Ejecutar `dotnet build "TradingBookApp.sln"`.
2. Confirmar que no se agrego tabla ni entidad de imagenes.
3. Confirmar que la validacion solo puede persistirse como resultado completo.

---

## Criterios de aceptacion

- [ ] Existe `AiTradeValidation` sin campos de ruta, hash o binario de imagen.
- [ ] Existe `AiTradeValidationRule` relacionada con `AiTradeValidation`.
- [ ] Existe `IAiTradeValidationRepository` con guardado atomico de validacion y reglas.
- [ ] El repositorio no guarda registros incompletos.
- [ ] El modelo guarda proveedor, modelo, prompt version y schema version.
- [ ] El modelo permite relacionar una validacion con una orden existente.
- [ ] El modelo permite diferenciar valores detectados y valores confirmados.
- [ ] No existe `AiTradeValidationImage` ni equivalente persistente.
- [ ] No se persisten imagenes, rutas, nombres internos, hashes ni metadatos de archivo.
- [ ] `dotnet build "TradingBookApp.sln"` termina sin errores.

---

## Decisiones tomadas y descartadas

- **Si:** guardar solo resultados completos. Evita historiales parciales cuando falla el proveedor IA o la normalizacion.
- **Si:** guardar proveedor y modelo usados. Permite auditar cambios entre OpenAI, MiniMax, DeepSeek, GLM y Kimi.
- **Si:** separar valores detectados y confirmados. La IA no debe crear ordenes directamente.
- **Si:** mantener database-first. Respeta la arquitectura actual del proyecto.
- **No:** almacenar imagenes. El usuario confirmo que las imagenes solo se cargan para enviarlas al modelo.
- **No:** guardar rutas, hashes o metadatos persistentes de imagen. Aunque sean menos sensibles que el archivo, siguen describiendo archivos que el usuario no quiere conservar.
- **No:** guardar validaciones fallidas. Los errores tecnicos se registran por logging, no como validaciones de negocio.

---

## Riesgos identificados

| Riesgo | Mitigacion |
|--------|------------|
| Guardar JSON demasiado grande puede crecer la base de datos | Guardar solo la respuesta estructurada necesaria y evitar incluir imagenes codificadas en base64 |
| Una llamada IA exitosa pero normalizacion fallida podria perder trazabilidad funcional | Registrar el error tecnico con Serilog sin crear validacion de negocio incompleta |
| Cambios futuros del esquema IA pueden romper deserializacion | Guardar `SchemaVersion` y `PromptVersion` en cada validacion |
| Mezclar valores detectados y confirmados puede crear ordenes incorrectas | Mantener columnas separadas y usar solo confirmados para crear ordenes |

---

## Lo que **no** esta en esta spec

- Almacenamiento persistente de imagenes.
- Adaptadores IA.
- Carga temporal de archivos.
- Motor de reglas.
- Score.
- Convergencias.
- Interfaz web.
- Creacion de ordenes.
- Chat de seguimiento.
- Metricas de aprendizaje.
