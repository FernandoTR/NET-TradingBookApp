# SPEC 28 - Normalizacion, reglas deterministicas y Trading Score

> **Estado:** Implementado - **Depende de:** SPEC 25, SPEC 27 - **Fecha:** 2026-06-25
> **Objetivo:** Convertir la extraccion visual de IA en un setup normalizado contra catalogos del sistema, evaluar reglas objetivas en codigo y reutilizar el `TradingScoreEngineService` existente.

---

## Alcance

**Incluye:**

- Crear `TradeSetupNormalizer` para mapear salida IA a catalogos existentes.
- Calcular RR en codigo para Long y Short.
- Validar coherencia de entrada, stop loss y take profit segun direccion.
- Crear `StrategyRuleEngine` con reglas deterministicas iniciales.
- Reutilizar `TradingScoreEngineService` para calcular `StructuralScore`, `TotalScore` y `Grade`.
- Separar condiciones cumplidas, incumplidas y no confirmables.
- Construir `AiValidationResultDto` con resultado compuesto.
- Guardar solo cuando IA, normalizacion, reglas y score terminen correctamente.

**Fuera de alcance:**

- Crear o modificar adaptadores IA.
- Consultar convergencias historicas.
- Crear UI MVC.
- Crear ordenes.
- Guardar imagenes.
- Chat de seguimiento.

---

## Data model

Se agregan servicios en `Application/Services`:

- `TradeSetupNormalizer`.
- `StrategyRuleEngine`.
- `TradeValidationOrchestrator`.
- `AiValidationResultFactory`.

Se agregan interfaces en `Application/Interfaces`:

```csharp
public interface ITradeSetupNormalizer
{
    Task<NormalizedTradeSetupDto> NormalizeAsync(
        CreateAiValidationDto request,
        AiVisionExtractionDto extraction,
        CancellationToken cancellationToken);
}

public interface IStrategyRuleEngine
{
    IReadOnlyList<AiValidationRuleResultDto> Evaluate(NormalizedTradeSetupDto setup);
}

public interface ITradeValidationOrchestrator
{
    Task<AiValidationResultDto> ValidateAsync(
        CreateAiValidationDto request,
        IReadOnlyList<AiValidationImageInputDto> images,
        CancellationToken cancellationToken);
}
```

Se agrega DTO normalizado:

```csharp
public sealed class NormalizedTradeSetupDto
{
    public int InstrumentId { get; set; }
    public int DirectionId { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public int? TriggerId { get; set; }
    public int? SceneryId { get; set; }
    public int? FigureId { get; set; }
    public int? FrameId { get; set; }
    public int? StageId { get; set; }
    public byte? LocationType { get; set; }
    public byte? ConfirmationType { get; set; }
    public bool? IsTrendAligned { get; set; }
    public bool? IsPivotZone { get; set; }
    public decimal? RiskRewardRatio { get; set; }
    public decimal VisualConfidence { get; set; }
}
```

No se agrega un nuevo score engine. Se reutiliza `Application/Services/TradingScoreEngineService.cs`.

---

## Plan de implementacion

**Paso 1 - Crear normalizador**

1. Crear `ITradeSetupNormalizer` y `TradeSetupNormalizer`.
2. Resolver IDs usando servicios de catalogos existentes.
3. Preferir valores del usuario cuando existan y sean validos.
4. Marcar como no confirmable lo que no pueda normalizarse.
5. Compilar para validar.

**Paso 2 - Calcular RR en codigo**

1. Implementar calculo Long: beneficio igual `TakeProfit - EntryPrice` y riesgo igual `EntryPrice - StopLoss`.
2. Implementar calculo Short: beneficio igual `EntryPrice - TakeProfit` y riesgo igual `StopLoss - EntryPrice`.
3. Rechazar riesgo menor o igual a cero.
4. Rechazar beneficio menor o igual a cero.
5. Evitar division entre cero.
6. Compilar para validar.

**Paso 3 - Crear motor de reglas**

1. Crear `IStrategyRuleEngine` y `StrategyRuleEngine`.
2. Agregar regla de RR minimo configurable con default `1:1`.
3. Agregar regla de etapa 1 permitiendo escenarios `BA`, `B` y `BC` segun catalogo existente.
4. Agregar regla de zona valida obligatoria.
5. Agregar regla de gatillo obligatorio.
6. Agregar regla de direccion alineada cuando el dato sea confirmable.
7. Agregar regla de confirmacion compatible cuando exista dato.
8. Compilar para validar.

**Paso 4 - Reutilizar Trading Score existente**

1. Construir un `Order` temporal con los campos estructurales confirmables.
2. Llamar `ITradingScoreEngineService.Evaluate(order)`.
3. Leer `StructuralScore`, `TotalScore` y `Grade` del `Order` temporal.
4. No guardar el `Order` temporal.
5. Compilar para validar.

**Paso 5 - Crear orquestador**

1. Crear `ITradeValidationOrchestrator` y `TradeValidationOrchestrator`.
2. Llamar `IAiVisionClientFactory.CreateActiveClient()`.
3. Enviar request e imagenes temporales al cliente IA.
4. Normalizar la extraccion.
5. Ejecutar reglas.
6. Calcular score.
7. Crear resultado final.
8. Guardar con `IAiTradeValidationRepository.SaveCompletedAsync` solo si todo termino correctamente.
9. Compilar para validar.

**Paso 6 - Agregar pruebas unitarias**

1. Crear proyecto de pruebas si no existe.
2. Probar RR Long valido.
3. Probar RR Short valido.
4. Probar SL invalido.
5. Probar TP invalido.
6. Probar campos `null` no confirmables.
7. Probar regla de etapa 1.
8. Probar uso de `TradingScoreEngineService`.

**Paso 7 - Verificacion final**

1. Ejecutar `dotnet build "TradingBookApp.sln"`.
2. Ejecutar las pruebas disponibles.
3. Confirmar que no existe logica de reglas en controladores.

---

## Criterios de aceptacion

- [ ] Existe `TradeSetupNormalizer`.
- [ ] Existe `StrategyRuleEngine`.
- [ ] Existe `TradeValidationOrchestrator`.
- [ ] RR Long se calcula en codigo.
- [ ] RR Short se calcula en codigo.
- [ ] Riesgo cero o negativo se rechaza.
- [ ] Beneficio cero o negativo se rechaza.
- [ ] La IA no calcula el RR final.
- [ ] La IA no decide el score final.
- [ ] `TradingScoreEngineService` se reutiliza.
- [ ] Las reglas devuelven cumplidas, incumplidas y no confirmables.
- [ ] Una validacion no se guarda si falla IA, normalizacion, reglas o score.
- [ ] Existen pruebas unitarias para RR y reglas iniciales.
- [ ] `dotnet build "TradingBookApp.sln"` termina sin errores.

---

## Decisiones tomadas y descartadas

- **Si:** reglas en C#. El modelo solo extrae evidencia visual.
- **Si:** reutilizar `TradingScoreEngineService`. Evita dos fuentes de verdad para `Grade` y score.
- **Si:** usar un `Order` temporal para el score. El servicio existente trabaja con `Order`.
- **Si:** valores `null` cuando la evidencia no alcance. Evita inventar catalogos o condiciones.
- **No:** permitir que la IA declare una operacion valida por criterio propio. La validez sale de reglas y score.
- **No:** guardar el `Order` temporal usado para score. La orden solo se crea despues de confirmacion humana.

---

## Riesgos identificados

| Riesgo | Mitigacion |
|--------|------------|
| Catalogos con nombres distintos a los que devuelve la IA | Normalizador con aliases controlados y fallback a no confirmable |
| `TradingScoreEngineService` puede requerir campos no detectados | Pasar solo valores confirmables y tratar faltantes como neutros segun reglas actuales |
| Reglas de estrategia pueden crecer rapido | Mantener reglas pequenas, testeables y con `RuleCode` estable |
| Un resultado IA parcial puede parecer util | No guardar validacion hasta completar normalizacion, reglas y score |

---

## Lo que **no** esta en esta spec

- Adaptadores IA.
- Carga de imagenes.
- Persistencia de imagenes.
- Convergencias historicas.
- UI MVC.
- Creacion de ordenes.
- Chat de seguimiento.
