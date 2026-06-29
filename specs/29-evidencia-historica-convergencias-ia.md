# SPEC 29 - Evidencia historica y convergencias para validaciones IA

> **Estado:** Implementado - **Depende de:** SPEC 28 - **Fecha:** 2026-06-25
> **Objetivo:** Agregar evidencia historica real a la validacion IA reutilizando el modulo existente de convergencias, sin permitir SQL generado por modelos.

---

## Alcance

**Incluye:**

- Crear `IHistoricalEvidenceService` en `Application`.
- Reutilizar `ICatConvergenceService.GetTBAnalyticsConvergenceAsync`.
- Construir filtros de convergencia a partir del setup normalizado.
- Mostrar cantidad de trades, TP1, TP2, TP3, SL y score de convergencia.
- Marcar muestra insuficiente cuando `Trades` sea menor al minimo configurado.
- Incluir evidencia historica en `AiValidationResultDto`.
- Guardar el resumen historico dentro del resultado completo de validacion.

**Fuera de alcance:**

- Crear SQL dinamico desde texto generado por IA.
- Modificar el procedimiento `usp_GetTBAnalyticsConvergence` salvo que una spec futura lo requiera.
- Crear nuevas estadisticas que no existan en los DTOs actuales.
- Buscar operaciones similares fuera del modulo de convergencias.
- Crear dashboard analitico nuevo.
- Chat de seguimiento.

---

## Data model

Se agrega DTO en `Application/DTOs/AiValidation`:

```csharp
public sealed class HistoricalEvidenceDto
{
    public string? Setup { get; set; }
    public int Trades { get; set; }
    public decimal TP1Rate { get; set; }
    public decimal TP2Rate { get; set; }
    public decimal TP3Rate { get; set; }
    public decimal SLRate { get; set; }
    public decimal Score { get; set; }
    public bool IsSampleSmall { get; set; }
    public int MinTrades { get; set; }
}
```

Se agrega interfaz:

```csharp
public interface IHistoricalEvidenceService
{
    Task<HistoricalEvidenceDto?> GetEvidenceAsync(
        NormalizedTradeSetupDto setup,
        CancellationToken cancellationToken);
}
```

Se reutilizan tipos existentes:

- `ParametersTBAnalyticsConvergenceDto`.
- `GetTBAnalyticsConvergenceDto`.
- `ICatConvergenceService`.
- `CatConvergenceService`.
- `CatConvergenceRepository`.

---

## Plan de implementacion

**Paso 1 - Crear contrato de evidencia historica**

1. Agregar `HistoricalEvidenceDto`.
2. Agregar `IHistoricalEvidenceService`.
3. Compilar para validar.

**Paso 2 - Implementar servicio con convergencias existentes**

1. Crear `HistoricalEvidenceService` en `Application/Services`.
2. Inyectar `ICatConvergenceService`.
3. Mapear `NormalizedTradeSetupDto` a `ParametersTBAnalyticsConvergenceDto`.
4. Activar filtros solo cuando el dato normalizado exista.
5. Usar `MinTrades` configurable con default `10`.
6. Compilar para validar.

**Paso 3 - Seleccionar evidencia principal**

1. Consultar convergencias con filtros de trigger, escenario, direccion, frame y figura.
2. Si existen varias filas, usar la de mayor score o mayor cantidad de trades segun regla documentada.
3. Marcar `IsSampleSmall` cuando `Trades < MinTrades`.
4. Devolver `null` cuando no haya evidencia.
5. Compilar para validar.

**Paso 4 - Integrar con el orquestador**

1. Inyectar `IHistoricalEvidenceService` en `TradeValidationOrchestrator`.
2. Consultar evidencia despues de normalizar y antes de crear resultado final.
3. Incluir evidencia en `AiValidationResultDto`.
4. Incluir resumen historico en `FinalSummary`.
5. Compilar para validar.

**Paso 5 - Agregar pruebas**

1. Probar mapeo de setup normalizado a parametros de convergencia.
2. Probar muestra insuficiente.
3. Probar ausencia de evidencia.
4. Probar que no existe SQL dinamico.

**Paso 6 - Verificacion final**

1. Ejecutar `dotnet build "TradingBookApp.sln"`.
2. Probar manualmente una validacion con convergencia existente.
3. Probar manualmente una validacion sin evidencia historica.

---

## Criterios de aceptacion

- [x] Existe `IHistoricalEvidenceService`.
- [x] Existe `HistoricalEvidenceService`.
- [x] El servicio reutiliza `ICatConvergenceService.GetTBAnalyticsConvergenceAsync`.
- [x] La IA no genera SQL.
- [x] La IA no inventa estadisticas.
- [x] El resultado muestra trades, TP1, TP2, TP3, SL y score.
- [x] La muestra se marca insuficiente cuando `Trades < MinTrades`.
- [x] La ausencia de evidencia no rompe la validacion.
- [x] La evidencia se incluye en `AiValidationResultDto`.
- [x] `dotnet build "TradingBookApp.sln"` termina sin errores.

---

## Decisiones tomadas y descartadas

- **Si:** reutilizar convergencias existentes. El proyecto ya tiene `CatConvergenceService` y `usp_GetTBAnalyticsConvergence`.
- **Si:** no permitir SQL libre. El modelo no debe consultar ni construir queries.
- **Si:** marcar muestra insuficiente. Evita sobrerrepresentar estadisticas con pocos trades.
- **No:** crear un modulo nuevo de analytics para el MVP. Aumentaria alcance sin necesidad.
- **No:** pedir a la IA que estime resultados historicos. Las estadisticas deben salir de la base de datos.

---

## Riesgos identificados

| Riesgo | Mitigacion |
|--------|------------|
| La base de datos puede no tener el procedimiento esperado | Fallar de forma controlada y mostrar evidencia no disponible |
| Pocos trades pueden producir conclusion engañosa | Mostrar `IsSampleSmall` y `MinTrades` |
| Filtros demasiado estrictos pueden devolver cero resultados | Activar filtros solo con datos normalizados existentes |
| Cambios en `GetTBAnalyticsConvergenceDto` pueden romper el servicio | Mantener pruebas de mapeo y compilacion |

---

## Lo que **no** esta en esta spec

- SQL generado por IA.
- Nuevos procedimientos almacenados.
- Operaciones similares fuera de convergencias.
- Dashboard analitico nuevo.
- UI final del asistente.
- Creacion de ordenes.
- Chat de seguimiento.
