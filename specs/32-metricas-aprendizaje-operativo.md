# SPEC 32 - Metricas de aprendizaje operativo

> **Estado:** Implementado - **Depende de:** SPEC 25, SPEC 31 - **Fecha:** 2026-06-25
> **Objetivo:** Registrar metricas que comparen clasificacion de IA, confirmacion del usuario y resultado real de la orden para medir precision operativa del asistente.

---

## Alcance

**Incluye:**

- Crear estructura para guardar comparacion IA vs usuario vs resultado real.
- Medir precision de trigger, escenario, etapa, figura, frame, direccion y contexto estructural.
- Medir tasa de correccion humana.
- Medir relacion entre score calculado y resultado real de la orden.
- Medir falsos positivos y falsos negativos cuando exista orden vinculada.
- Crear servicio de actualizacion de metricas cuando cambie el resultado de una orden.
- Crear vista simple de metricas agregadas por proveedor y modelo.

**Fuera de alcance:**

- Entrenar o fine-tunear modelos.
- Cambiar prompts automaticamente.
- Crear recomendaciones automaticas de compra o venta.
- Alertas de sobreoperacion.
- Chat de seguimiento.
- Guardar imagenes.
- Dashboard avanzado con graficas complejas.

---

## Data model

Se agrega entidad en `Domain/Entities`:

```csharp
public partial class AiTradeValidationMetric
{
    public int Id { get; set; }
    public int ValidationId { get; set; }
    public int? OrderId { get; set; }
    public string ProviderName { get; set; } = null!;
    public string ModelName { get; set; } = null!;
    public bool? TriggerMatchedUser { get; set; }
    public bool? SceneryMatchedUser { get; set; }
    public bool? StageMatchedUser { get; set; }
    public bool? FigureMatchedUser { get; set; }
    public bool? FrameMatchedUser { get; set; }
    public bool? DirectionMatchedUser { get; set; }
    public bool? TrendMatchedUser { get; set; }
    public bool? LocationMatchedUser { get; set; }
    public bool? ConfirmationMatchedUser { get; set; }
    public bool? PivotZoneMatchedUser { get; set; }
    public decimal HumanCorrectionRate { get; set; }
    public int? TotalScore { get; set; }
    public string? Grade { get; set; }
    public bool? ReachedSl { get; set; }
    public bool? ReachedTp1 { get; set; }
    public bool? ReachedTp2 { get; set; }
    public bool? ReachedTp3 { get; set; }
    public string? OutcomeClassification { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

Se agregan DTOs en `Application/DTOs/AiValidation`:

- `AiValidationMetricDto`.
- `AiValidationMetricSummaryDto`.
- `AiValidationMetricFilterDto`.

Se agregan interfaces:

```csharp
public interface IAiTradeValidationMetricService
{
    Task CreateInitialMetricAsync(int validationId, string userId, CancellationToken cancellationToken);
    Task RefreshOrderOutcomeAsync(int orderId, CancellationToken cancellationToken);
    Task<AiValidationMetricSummaryDto> GetSummaryAsync(AiValidationMetricFilterDto filter, CancellationToken cancellationToken);
}
```

---

## Plan de implementacion

**Paso 1 - Crear modelo de metricas**

1. Agregar `AiTradeValidationMetric`.
2. Crear script SQL versionado para la tabla correspondiente.
3. Agregar relacion con `AiTradeValidation` y `Order` cuando exista.
4. Compilar para validar.

**Paso 2 - Crear servicio de metricas**

1. Crear `IAiTradeValidationMetricService`.
2. Crear `AiTradeValidationMetricService`.
3. Calcular coincidencias entre campos detectados por IA y campos confirmados por usuario.
4. Calcular `HumanCorrectionRate` como porcentaje de campos corregidos.
5. Compilar para validar.

**Paso 3 - Crear metrica inicial al confirmar validacion**

1. Llamar `CreateInitialMetricAsync` cuando el usuario confirme la validacion.
2. Guardar proveedor, modelo, score y grado.
3. Guardar coincidencias disponibles aunque aun no exista orden.
4. Compilar para validar.

**Paso 4 - Actualizar metricas con resultado real**

1. Llamar `RefreshOrderOutcomeAsync` cuando una orden vinculada cambie SL, TP1, TP2 o TP3.
2. Clasificar resultado como `SL`, `TP1`, `TP2`, `TP3`, `Open` o `Unknown`.
3. No inventar resultado si la orden no esta cerrada o no tiene flags suficientes.
4. Compilar para validar.

**Paso 5 - Crear resumen agregado**

1. Agregar consulta por proveedor, modelo y rango de fechas.
2. Mostrar precision por campo.
3. Mostrar tasa promedio de correccion humana.
4. Mostrar relacion entre grade y resultado real.
5. Compilar para validar.

**Paso 6 - Crear vista simple de metricas**

1. Crear `TradeAssistantMetricsController` o accion `Metrics` en `TradeAssistantController`.
2. Crear vista con filtros basicos.
3. Mostrar tabla agregada por proveedor y modelo.
4. Evitar graficas complejas en esta spec.
5. Compilar para validar.

**Paso 7 - Verificacion final**

1. Ejecutar `dotnet build "TradingBookApp.sln"`.
2. Crear validacion completa.
3. Confirmar valores con al menos una correccion.
4. Crear orden desde validacion.
5. Cambiar resultado de la orden.
6. Confirmar que la metrica se actualiza.

---

## Criterios de aceptacion

- [ ] Existe `AiTradeValidationMetric`.
- [ ] Existe servicio de metricas.
- [ ] Se crea metrica inicial al confirmar una validacion.
- [ ] La metrica compara campos IA contra campos confirmados por usuario.
- [ ] La metrica calcula tasa de correccion humana.
- [ ] La metrica guarda proveedor y modelo.
- [ ] La metrica puede vincularse con una orden.
- [ ] La metrica se actualiza cuando cambia resultado de orden vinculada.
- [ ] La metrica no inventa resultado si la orden no tiene outcome claro.
- [ ] Existe vista simple de resumen por proveedor y modelo.
- [ ] No se usan imagenes para calcular metricas.
- [ ] `dotnet build "TradingBookApp.sln"` termina sin errores.

---

## Decisiones tomadas y descartadas

- **Si:** crear metricas desde ahora. El usuario confirmo que quiere incluir aprendizaje operativo.
- **Si:** comparar IA contra confirmacion humana. Mide utilidad del asistente sin esperar cierre de orden.
- **Si:** actualizar con resultado real cuando exista. Permite medir relacion entre score y outcome.
- **Si:** agrupar por proveedor y modelo. Es necesario porque el modulo soporta multiples IAs.
- **No:** fine-tuning automatico. Las metricas primero deben observar calidad antes de entrenar modelos.
- **No:** alertas de sobreoperacion. Es una capacidad futura separada.
- **No:** dashboard avanzado. La primera version requiere tabla agregada verificable.

---

## Riesgos identificados

| Riesgo | Mitigacion |
|--------|------------|
| Una orden puede quedar abierta mucho tiempo | Permitir outcome `Open` o `Unknown` y actualizar despues |
| Cambios manuales posteriores pueden alterar metricas | Guardar `UpdatedAt` y recalcular desde datos actuales |
| Pocas validaciones por modelo pueden inducir conclusiones falsas | Mostrar conteo de muestra en el resumen |
| Diferentes modelos pueden tener campos no comparables | Usar el mismo schema estructurado para todos los proveedores |

---

## Lo que **no** esta en esta spec

- Fine-tuning.
- Cambio automatico de prompts.
- Alertas de sobreoperacion.
- Recomendaciones financieras autonomas.
- Chat de seguimiento.
- Persistencia de imagenes.
- Dashboard avanzado.
