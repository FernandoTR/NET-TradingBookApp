# SPEC 36 — Modelo normativo, score contextual y métricas de validación

> **Estado:** Implementado · **Depende de:** SPEC 25, SPEC 27, SPEC 28, SPEC 32 · **Fecha:** 2026-10-23
>
> **Objetivo:** Alinear la evidencia de evaluación, el score contextual y las métricas de validación de setups con el checklist de la mentoría.
>
> **Referencia normativa:** `doc/mentoria/CHECKLIST_TRADING.md`.

---

## 1. Por qué existe esta especificación

La revisión de las SPEC 25–28 y 30–32, del plan maestro y de la implementación identificó diferencias entre el modelo actual y la metodología documentada en la mentoría.

- La validez gráfica depende de zona, gatillo y ratio, no de obtener una puntuación o un grado determinado.
- Tener un ID de gatillo o una ubicación de catálogo no demuestra todos los requisitos de esas condiciones.
- Los datos desconocidos necesitan una representación distinta de un incumplimiento o un error técnico.
- El score actual asigna pesos y límites que no reproducen las categorías contextuales acordadas.
- La actualización de una orden puede volver a ejecutar el score aunque sólo cambie su resultado operativo.
- Las métricas necesitan distinguir concordancia, corrección humana y resultado real.

Esta especificación establece los contratos de evidencia y desarrolla el nuevo score y sus métricas. Los evaluadores gráficos y el nuevo flujo de revisiones se desarrollarán en especificaciones posteriores.

Las definiciones de grados incluyen las aclaraciones operativas confirmadas por el usuario durante esta revisión. La representación numérica es una convención explícita de la aplicación, no una fórmula atribuida a la mentoría.

## 2. Alcance

### Incluye

**Contrato normativo y de evidencia versionado**

- Identificar reglas obligatorias, criterios de calidad y filtros adicionales del perfil.
- Representar contexto, referencias marcadas, límites, temporalidades y momento de evaluación.
- Cubrir etapa 1, etapa 2 y los siete gatillos con requisitos específicos por variante.
- Representar entrada, invalidación técnica, SL, primer obstáculo y objetivo.
- Distinguir datos propuestos, observados, confirmados y calculados.
- Representar contradicciones e información faltante.

**Estados de evaluación**

- Definir `Cumple`, `No cumple` y `Pendiente` para zona, gatillo y ratio.
- Definir su composición en `Setup gráfico válido`, `Setup no válido` o `Evaluación pendiente`.
- Distinguir evidencia desconocida, ausencia confirmada y requisitos no aplicables.
- Representar por separado la revisión manual de habilitación para real, con responsable, fecha y fundamento.

**Recalibración del score contextual**

- Sustituir la suma de pesos y los límites actuales por reglas contextuales versionadas.
- Conservar los grados A, AB, BA, B, BC, CB y C con los significados acordados.
- Identificar «línea pivote» mediante el catálogo de figuras y utilizar su base B, con ajustes a BA o BC.
- Considerar dirección, figura, estructuras y referencias al justificar una clasificación.
- Obtener posición contextual, score y Grade mediante una única evaluación.
- Mantener desconocidos los resultados que carezcan de evidencia requerida.
- Incorporar el nuevo score al resumen cuando cumpla los criterios de aceptación.
- Mantener la validez gráfica determinada por las tres condiciones obligatorias.

**Desarrollo de nuevas métricas**

- Concordancia IA–usuario por campo.
- Cobertura de extracción y datos completados manualmente.
- Frecuencia y motivos de corrección y descarte.
- Comparación entre Escenario manual y Grade calculado.
- Distribución y motivos de evaluaciones válidas, no válidas y pendientes, cuando exista esa evaluación identificada.
- Relación descriptiva entre score, Grade y resultado operativo.
- Segmentación por proveedor, modelo y versiones relevantes.
- Tamaños de muestra y denominadores comparables.
- Cálculo, persistencia y consulta mediante el resumen de métricas existente.

**Comparación histórica y trazabilidad**

- Utilizar órdenes históricas y nuevas evaluaciones revisadas, identificando ambas poblaciones.
- Comparar únicamente campos disponibles y compatibles.
- Conservar las puntuaciones y evaluaciones originales.
- Identificar cada recálculo con su versión y los datos utilizados.

### Fuera de alcance

- Implementar los nuevos evaluadores ejecutables de zona, gatillo y ratio.
- Activar el nuevo contrato en los proveedores IA.
- Implementar el nuevo flujo de revisiones y creación/vinculación de órdenes.
- Rediseñar integralmente las pantallas del asistente.
- Reconstruir automáticamente el mapa del mercado.
- Automatizar la habilitación para real.
- Desarrollar un score predictivo o ajustar sus reglas automáticamente.
- Medir falsos positivos/negativos de validez sin una revisión de referencia explícita.
- Almacenar imágenes, rutas, hashes o metadatos persistentes de archivos.

## 3. Modelo de datos

### 3.1. Ubicación y versiones

- Los nuevos enums se ubican en `Domain/Enums`.
- Los nuevos contratos se ubican en `Application/DTOs/AiValidation/V2`.
- Las nuevas entidades persistentes se ubican en `Domain/Entities`.
- Los mapeos personalizados se incorporan a `Infrastructure/Persistence/Data/ApplicationDbContext.Custom.cs`.
- Los cambios SQL se entregan mediante scripts versionados compatibles con database-first.

Se distinguen las versiones de estrategia, evidencia, reglas de clasificación, score, métricas, prompt y esquema de extracción del proveedor.

Una versión de origen desconocida permanece desconocida. Recalcular un histórico con una definición nueva no significa que su evaluación original utilizara esa definición.

### 3.2. Evidencia e incertidumbre

En `Domain/Enums/EvidenceAvailability.cs`:

```csharp
public enum EvidenceAvailability
{
    Unknown = 0,
    Present = 1,
    Absent = 2,
    NotApplicable = 3
}
```

En `Application/DTOs/AiValidation/V2/EvidenceValueDto.cs`:

```csharp
public sealed record EvidenceValueDto<T>(
    EvidenceAvailability Availability,
    T? Value,
    ValidationSource? Source,
    DateTimeOffset? OccurredAtUtc,
    DateTimeOffset? AvailableAtUtc,
    decimal? VisualConfidence,
    string? ReasonCode
) where T : struct;
```

| Estado | Significado | Ejemplo |
|---|---|---|
| `Unknown` | No se puede determinar el dato. | No se distingue si la vela cerró. |
| `Present` | Existe un valor conocido. | Se observa un SL o se confirma que una vela sigue abierta. |
| `Absent` | Se confirma la ausencia de una referencia o elemento. | No existe una figura concreta en el contexto revisado. |
| `NotApplicable` | El requisito no corresponde al caso. | Strikes de etapa 1 para una zona de etapa 2. |

Reglas del contrato:

- `Present` requiere un valor. `false` y `0` son valores conocidos.
- Los demás estados llevan `Value = null` y una causa identificable.
- Una ausencia confirmada no significa automáticamente incumplimiento. Depende del requisito evaluado.
- `OccurredAtUtc` identifica cuándo ocurrió el hecho.
- `AvailableAtUtc` identifica cuándo podía conocerse.
- Las fechas desconocidas permanecen desconocidas. Una revisión posterior no permite retrotraer artificialmente la disponibilidad de la información.
- `VisualConfidence` expresa confianza de extracción, puede ser desconocida y no representa probabilidad de éxito del trade.
- Se reutiliza `ValidationSource` para identificar datos del usuario, IA, confirmación humana y cálculo determinístico.
- La confirmación humana se conserva como una observación diferenciada de la extracción original.

### 3.3. Contrato principal y componentes

En `Application/DTOs/AiValidation/V2/TradeSetupEvidenceDto.cs`:

```csharp
public sealed record TradeSetupEvidenceDto(
    string StrategyVersion,
    string EvidenceSchemaVersion,
    DateTimeOffset EvaluatedAtUtc,
    EvaluationContextDto Context,
    ZoneEvidenceDto Zone,
    TriggerEvidenceDto Trigger,
    RiskPlanEvidenceDto RiskPlan,
    ContextualEvidenceDto ContextualEvidence,
    IReadOnlyList<EvidenceConflictDto> Conflicts
);
```

Los valores numéricos, booleanos, temporales y de enum observados utilizan `EvidenceValueDto<T>`.

| Estructura | Campos y responsabilidad |
|---|---|
| `EvaluationContextDto` | `InstrumentId: int`, `DirectionId: int`, `StageId: EvidenceValueDto<int>`, `TriggerFrameId: EvidenceValueDto<int>`, momento del setup y modalidad temporal. |
| `ZoneEvidenceDto` | Referencia de negocio, tipo de zona, límites de influencia, anclajes precio/tiempo, frame y etapa de origen, vigencia, confirmación y strikes cuando correspondan. |
| `PriceTimeAnchorDto` | Precio y momento del anclaje, con disponibilidad y procedencia. |
| `TriggerEvidenceDto` | Concepto de gatillo, familia, velas/extremos relevantes, cierre, extremo del movimiento y evidencia de los requisitos de la variante. |
| `TriggerPhaseEvidenceDto` | Fase de ruptura, retesteo o continuación, con precios, tiempos y procedencia de la evidencia. |
| `RiskPlanEvidenceDto` | Entrada, SL, invalidación técnica, margen, primer obstáculo y objetivo. |
| `ContextualEvidenceDto` | Figura principal, referencias adicionales, estructuras y sus temporalidades, convergencias y factores favorables o contrarios. |
| `EvidenceConflictDto` | Campo afectado, observaciones contradictorias, estado de resolución y fundamento de la resolución manual. |

El contrato cubre envolvente, stars, pinzas, fake out y pullbacks perfecto, largo y corto.

Las referencias de catálogo distinguen el concepto identificado del ID normalizado. La extracción no depende de que la IA adivine IDs de la base de datos.

La figura principal se diferencia de las referencias adicionales que forman una convergencia. Una única selección de Figura no acredita por sí sola todo el contexto compuesto.

Las referencias marcadas en las imágenes permiten identificar elementos del mapa. La vigencia, el cierre y la secuencia del gatillo necesitan evidencia propia.

Los límites de una línea inclinada deben referirse al momento evaluado. Esta especificación representa la información aportada; no introduce una geometría automática ni tolerancias ausentes en la metodología.

Los factores de volumen, RSI u otros elementos contextuales pueden registrarse cuando formen parte de la evidencia aportada o de una revisión explícita. No se convierten en requisitos gráficos universales.

### 3.4. Resultado gráfico y revisión para real

En `Domain/Enums`:

```csharp
public enum SetupConditionState
{
    Pending = 0,
    Passed = 1,
    Failed = 2
}

public enum SetupVerdict
{
    Pending = 0,
    Valid = 1,
    Invalid = 2
}
```

`SetupConditionResultDto` contiene:

- Condición: zona, gatillo o ratio.
- Estado.
- Requisitos evaluados y referencias normativas.
- Motivos y referencias a la evidencia utilizada.

`SetupEvaluationResultDto` contiene las tres condiciones, el veredicto y las versiones utilizadas.

La composición contractual es:

1. Alguna condición no cumple: `Setup no válido`.
2. Ninguna falla y alguna está pendiente: `Evaluación pendiente`.
3. Las tres cumplen: `Setup gráfico válido`.

El estado predeterminado es pendiente. Los requisitos internos pueden ser no aplicables; las tres condiciones principales conservan sus tres estados.

`RealTradingReviewDto` representa:

- Estado: pendiente, habilitado o no habilitado.
- Usuario revisor y fecha.
- Fundamento de la decisión.
- Revisión de la evaluación a la que corresponde.

Una evaluación técnicamente terminada puede contener requisitos incumplidos o pendientes. Un timeout o una respuesta malformada del proveedor pertenece al manejo de errores técnicos.

La persistencia del nuevo flujo de evaluaciones y revisiones se implementará en la especificación posterior correspondiente. Este bloque define sus contratos.

### 3.5. Escenario manual y Grade calculado

`SceneryId` conserva la clasificación manual del usuario.

`Grade` es el resultado del motor de score. Su cálculo utiliza evidencia y no toma el Escenario manual como respuesta de entrada.

Se reutiliza `Domain/Enums/GradeType.cs` sin cambiar sus valores existentes.

| Grade | Significado contextual |
|---|---|
| A | Zona de trabajo con convergencia de estructuras que favorecen la dirección del trade. Un ejemplo es una figura de etapa 2 que converge con una zona de etapa 1. |
| AB | Contexto favorable con algún elemento contrario, o entrada favorable sin convergencia suficiente para A. |
| BA | Contexto neutral con algún elemento favorable, como una observación de volumen o RSI respaldada por evidencia. |
| B | Contexto neutral, sin una expectativa direccional clara. Una línea pivote, considerada como referencia principal, parte de esta base. |
| BC | Contexto neutral con algún elemento contrario. |
| CB | Contexto contrario a la estructura, con divergencia u otro elemento favorable. |
| C | Operación contra la estructura, aunque pueda cumplir zona, gatillo y ratio. |

Reglas adicionales:

- La figura «línea pivote» se identifica desde `CatFigure` mediante un mapeo semántico verificable.
- Su base B admite BA o BC según los factores adicionales acordados.
- No se deduce la figura exclusivamente del campo Etapa.
- `IsPivotZone` y la figura «línea pivote» son referencias distintas del modelo.
- Una base B con factores favorables y contrarios simultáneos queda pendiente hasta resolver manualmente el contexto.
- La resolución manual conserva ambos argumentos y su fundamento. Después, el motor vuelve a calcular el resultado.
- Aclaración aprobada durante la implementación del Paso 6: la resolución indica predominio favorable, contrario o neutral, con usuario responsable, fecha y fundamento. Sobre una base B, el motor obtiene BA, BC o B, respectivamente; el revisor no introduce un Grade manual.
- La resolución identifica las observaciones favorables y contrarias consideradas. Ambas se conservan; una resolución que no corresponda a los argumentos actuales no resuelve el contexto revisado.
- La estructura utilizada para clasificar se identifica junto con su dirección y temporalidad.
- Un requisito gráfico obligatorio incumplido no se reduce a un simple modificador del grado.
- Un grado C no invalida automáticamente el setup gráfico.
- Un grado A no compensa una condición gráfica incumplida.
- La neutralidad B no se traduce automáticamente a una efectividad del 50%.

Referencias documentales relacionadas:

- `doc/mentoria/CHECKLIST_TRADING.md`, apartados A, B, C y D.
- `doc/mentoria/transcripciones/15 PM - Replay.md`, líneas 130–153 y 185–210: categorías base y contexto.
- `doc/mentoria/transcripciones/15 PM - Replay.md`, líneas 493–511: categorías intermedias y entrada favorable sin convergencia suficiente para A.
- `doc/mentoria/transcripciones/17 PM.md`, líneas 95–110: contextos intermedios y neutrales con elementos contrarios.
- `doc/mentoria/transcripciones/19 PM - Convergencia de frames, RSI, VYK.md`, líneas 18–87 y 557–560: contexto estructural y factores adicionales.

Las referencias no convierten las distancias numéricas entre grados en diferencias demostradas de efectividad.

### 3.6. Score ordinal y disponibilidad

Un único cálculo produce posición contextual, score y Grade.

| Grade | Posición interna | Score mostrado sobre 100 |
|---|---:|---:|
| A | 6 | 100 |
| AB | 5 | 83,33 |
| BA | 4 | 66,67 |
| B | 3 | 50 |
| BC | 2 | 33,33 |
| CB | 1 | 16,67 |
| C | 0 | 0 |

```text
Score = 100 × posición interna / 6
```

La posición interna es independiente de los valores numéricos de `GradeType`.

El redondeo se aplica a la presentación. Grade se obtiene desde la posición, no desde el valor mostrado.

Esta versión representa orden contextual. Dos setups con el mismo Grade tienen la misma puntuación. El score no expresa calidad adicional dentro de una categoría ni probabilidad de ganar.

En `Domain/Enums/ContextualScoreState.cs`:

```csharp
public enum ContextualScoreState
{
    NotEvaluable = 0,
    Partial = 1,
    Complete = 2
}
```

El resultado tipado es:

```csharp
public sealed record ContextualScoreResultDto(
    string ScoreVersion,
    ContextualScoreState State,
    int? ContextRank,
    decimal? Value,
    GradeType? Grade,
    int ExpectedComponents,
    int EvaluatedComponents,
    IReadOnlyList<ContextualScoreComponentDto> Components
);
```

Las estructuras del cálculo son:

| Estructura | Contenido |
|---|---|
| `ContextualScoreInputDto` | Figura principal normalizada, contexto base, referencias estructurales, convergencias y factores adicionales con evidencia. Excluye el Escenario manual como respuesta del cálculo. |
| `ScenarioGradeResultDto` | Contexto base, clasificación final cuando sea determinable, versión, factores, referencias y motivos. Es un resultado interno del mismo cálculo. |
| `ContextualScoreDefinitionDto` | Versión, reglas de clasificación, tabla explícita de posiciones y sus justificaciones. |
| `ContextualScoreComponentDto` | Código del requisito o factor, disponibilidad, procedencia, motivo y referencias a la evidencia. |
| `ContextualPredominanceResolutionDto` | Predominio favorable, contrario o neutral, responsable, fecha, fundamento e instantáneas de ambos grupos de argumentos con sus referencias. Permite comprobar que la resolución corresponde a las observaciones actuales. No contiene un Grade manual. |

Las definiciones anteriores sustituyen la propuesta de factores con pesos iguales y `GradeBands`. No se utilizan bandas genéricas para inventar categorías A–C.

Los contadores de componentes representan los datos y comprobaciones requeridos por la regla aplicable, no tres componentes numéricos con pesos.

- `Complete` requiere una posición determinable y todos los datos necesarios para esa regla.
- `Partial` conserva el contexto conocido y los requisitos faltantes. El total y Grade permanecen desconocidos.
- `NotEvaluable` indica que no se puede establecer la base necesaria para clasificar.
- Sólo se excluyen los requisitos justificadamente no aplicables.
- Una lista vacía o un campo desconocido no demuestran ausencia de modificadores.
- Una puntuación 0 puede ser un resultado completo C. No equivale a falta de datos.

### 3.7. Instantánea persistente del score

`ContextualScoreSnapshot`, en `Domain/Entities`, contiene:

| Campo | Tipo o significado |
|---|---|
| `Id` | Identificador de la instantánea. |
| `UserId` | Propietario de los datos evaluados. |
| `SourceKind` | Orden histórica o evaluación revisada. |
| `OrderId` | Orden de origen o vinculada, cuando corresponda. |
| `ValidationId` | Validación de origen, cuando corresponda. |
| `EvaluationRevision` | Revisión de origen cuando exista. No se inventa una revisión para fuentes previas sin ese dato. |
| `ScoreVersion` | Versión de la definición aplicada. |
| `State` | Disponibilidad del resultado. |
| `ContextRank` | Posición interna nullable. |
| `Value` | Score decimal nullable. |
| `Grade` | Grado nullable. |
| `DefinitionJson` | Definición utilizada, incluidas reglas y tabla de posiciones. |
| `InputSnapshotJson` | Valores y procedencia utilizados. |
| `ComponentsJson` | Desglose y motivos del cálculo. |
| `CalculatedAtUtc` | Momento del cálculo. |

La instantánea conserva la relación con su fuente y con el usuario propietario. Los identificadores relacionados deben pertenecer a ese mismo usuario.

Una recalibración genera otra instantánea. Los resultados originales y las nuevas puntuaciones permanecen diferenciados.

Se conservan hechos de negocio extraídos y confirmados. Los JSON no incluyen archivos, contenido base64, rutas, nombres de archivo, hashes ni metadatos persistentes de archivos.

### 3.8. Métricas por campo y evaluación

| Estructura | Contenido |
|---|---|
| `AiValidationFieldMetricDto` | Código del campo, disponibilidad IA y humana, concordancia nullable, tipo de cambio y motivo de corrección. |
| `AiTradeValidationMetricSnapshot` | Validación y revisión de origen, usuario, versiones, proveedor/modelo, detalle de campos, resultados gráficos disponibles y referencia a la instantánea de score. |
| `AiValidationMetricSummaryV2Dto` | Métricas agregadas, denominadores, grupos comparables y relación entre score y resultados operativos. |
| `AiValidationMetricFilterV2Dto` | Población, proveedor/modelo, versiones, fechas, instrumento, etapa y gatillo. |

El tipo de cambio distingue sin cambios, corrección, dato completado, dato descartado y sin revisión.

- Si la IA devuelve desconocido y el usuario completa el campo, se cuenta como completado manualmente.
- Una ausencia conocida por ambas partes puede ser comparable y coincidente.
- Una afirmación de ausencia corregida por el usuario pertenece a correcciones, no a completados de información desconocida.
- Un dato descartado como no determinable no se fuerza a un valor para obtener concordancia.
- La comparación Escenario–Grade es una medición diferenciada de la concordancia IA–usuario por campo.
- Las órdenes históricas sin información IA–usuario no aportan observaciones a ese denominador.
- Los estados de validación legados conservan su identificación de origen. No se presentan como evaluaciones del nuevo modelo normativo si no se han evaluado con él.

### 3.9. Denominadores y agregación

```csharp
public sealed record RateMetricDto(
    int Numerator,
    int Denominator,
    decimal? Percentage
);
```

| Métrica | Numerador / denominador |
|---|---|
| Cobertura de extracción | Campos determinados por IA / campos esperados aplicables. |
| Concordancia | Campos coincidentes / campos comparables revisados. |
| Corrección humana | Afirmaciones IA corregidas o descartadas / afirmaciones IA revisadas. |
| Completado manual | Campos desconocidos completados / campos desconocidos revisados. |
| Concordancia Escenario–Grade | Clasificaciones coincidentes / pares con Escenario normalizado y Grade calculado disponible. |
| Distribución de estados | Evaluaciones de cada estado / evaluaciones incluidas con estado identificado y comparable. |

Si el denominador es cero, `Percentage = null` y se muestra `Sin datos`.

Los agregados conservan numeradores y denominadores. No se obtiene una tasa conjunta mediante un promedio ciego de porcentajes con muestras distintas.

Reglas de comparabilidad:

- Identificar la población de órdenes históricas y la de evaluaciones revisadas.
- Identificar proveedor/modelo y versiones relevantes cuando existan.
- Mantener desconocidas las versiones de origen no registradas.
- Contar por defecto la última revisión de cada validación.
- Contar órdenes distintas en el análisis operativo, evitando duplicarlas por revisiones o recálculos.
- Identificar la versión y la instantánea de score utilizadas en la relación con resultados operativos.
- Conservar el momento en que se observó el resultado de la orden.
- Conservar los indicadores SL/TP disponibles sin inferir su secuencia cuando no esté registrada.
- Presentar concordancia y efectividad operativa como mediciones diferentes.

## 4. Plan de implementación

Cada paso mantiene la solución compilable. Los cambios se entregan en unidades revisables y las comprobaciones se incorporan junto al comportamiento correspondiente.

### Bloque 1 — Contratos de evidencia

**Paso 1 — Crear estados y contrato básico**

- Agregar los nuevos enums en `Domain/Enums`.
- Crear `EvidenceValueDto<T>` y `EvidenceConflictDto` en `Application/DTOs/AiValidation/V2`.
- Verificar desconocido, valores conocidos, ausencias y requisitos no aplicables.

**Paso 2 — Incorporar contexto y referencias del mapa**

- Crear `EvaluationContextDto`, `ZoneEvidenceDto` y los anclajes precio/tiempo.
- Representar límites, temporalidades, vigencia y procedencia.
- Incorporar las referencias estructurales que justifican la clasificación contextual.

**Paso 3 — Incorporar gatillo y plan de riesgo**

- Crear `TriggerEvidenceDto`, `TriggerPhaseEvidenceDto` y `RiskPlanEvidenceDto`.
- Representar las siete alternativas y sus requisitos específicos.
- Incorporar entrada, invalidación, SL, primer obstáculo y objetivo.

**Paso 4 — Completar resultados y revisión**

- Crear `TradeSetupEvidenceDto`, `SetupConditionResultDto` y `SetupEvaluationResultDto`.
- Incorporar `RealTradingReviewDto`.
- Definir las versiones y referencias que identifican la evidencia de cada resultado.

Estos contratos quedan disponibles para los evaluadores y el nuevo flujo de revisión de las siguientes especificaciones.

### Bloque 2 — Recalibración del score

**Paso 5 — Resolver figuras contra el catálogo**

- Identificar el registro real de «línea pivote» en `CatFigure`.
- Establecer un mapeo semántico verificable sin depender de un ID fijo.
- Diferenciar la figura de campos como `IsPivotZone`.

**Paso 6 — Implementar reglas contextuales versionadas**

- Crear los contratos tipados de entrada y definición del score.
- Clasificar contexto base y factores favorables o contrarios.
- Implementar las siete categorías y el caso AB sin convergencia suficiente para A.
- Mantener pendiente una base B con factores en ambos sentidos sin resolución.
- Aplicar la resolución manual por predominio aprobada: favorable → BA, contrario → BC y neutral → B, conservando ambos grupos de argumentos y los datos de la decisión.
- Registrar los argumentos que producen cada resultado.

**Paso 7 — Calcular score y Grade mediante una evaluación**

- Utilizar la posición interna de 0 a 6.
- Calcular `100 × posición / 6` y obtener Grade antes del redondeo.
- Devolver resultado parcial o no evaluable cuando falten datos requeridos.
- Incorporar pruebas de clasificación, simetría Long/Short e independencia del Escenario manual.

**Paso 8 — Integrar consumidores existentes**

- Adaptar `Application/Interfaces/ITradingScoreEngineService.cs` y `Application/Services/TradingScoreEngineService.cs` para la evaluación tipada y su transición compatible.
- Revisar las llamadas desde `Application/Services/OrdersService.cs` y `Application/Services/AiValidationResultFactory.cs`.
- Transportar el resultado contextual decimal sin convertirlo al entero del contrato legado.
- Los adaptadores sólo aportan información realmente disponible. No completan evidencia para forzar una puntuación.
- Separar actualizaciones operativas y reevaluaciones contextuales.
- Conservar la evaluación original al producir resultados con la nueva definición.

### Bloque 3 — Persistencia y trazabilidad

**Paso 9 — Crear tablas de instantáneas**

- Preparar un script SQL versionado para `ContextualScoreSnapshot` y `AiTradeValidationMetricSnapshot` bajo `Infrastructure/Persistence/Scripts`.
- Agregar entidades y mapeos EF compatibles con database-first.
- Registrar origen, usuario, versiones, datos utilizados y fecha de cálculo.
- Incorporar pruebas de persistencia y recuperación de instantáneas.

**Paso 10 — Implementar repositorios y recálculo histórico**

- Agregar los contratos de repositorio en `Application/Interfaces` y las implementaciones en `Infrastructure/Persistence/Repositories`.
- Guardar cada recálculo con su entrada y definición.
- Conservar el resultado original y diferenciar las dos poblaciones.
- Producir resultados parciales cuando un histórico no permita completar la clasificación.
- Registrar las dependencias en los contenedores correspondientes.

### Bloque 4 — Métricas y presentación

**Paso 11 — Implementar mediciones por campo**

- Calcular concordancia, cobertura, correcciones, completados y descartes.
- Separar casos comparables y no comparables.
- Devolver porcentaje desconocido ante denominador cero.
- Incorporar pruebas de ausencias conocidas, completados y correcciones.

**Paso 12 — Implementar agregaciones y seguimiento operativo**

- Ampliar `Application/Services/AiTradeValidationMetricService.cs` y los contratos de consulta necesarios.
- Agrupar por población y versiones relevantes.
- Comparar Escenario manual y Grade calculado mediante códigos normalizados.
- Relacionar score y Grade con resultados operativos observados.
- Evitar duplicados por revisiones y recálculos.
- Limitar las consultas a los datos del usuario correspondiente.

**Paso 13 — Actualizar el resumen existente**

- Adaptar `Web/Controllers/TradeAssistantMetricsController.cs`.
- Adaptar `Web/Models/TradeAssistantMetricsViewModel.cs` y `Web/Views/TradeAssistantMetrics/Index.cshtml`.
- Mostrar muestras, denominadores, versiones y población.
- Incorporar el resultado contextual al resumen del asistente con grado, puntuación, disponibilidad y justificación.
- Mostrar la comparación entre Escenario manual y Grade calculado.

## 5. Criterios de aceptación

### Contrato de evidencia

- [ ] Los contratos V2 representan contexto, zona, gatillo, plan de riesgo y factores contextuales.
- [ ] Se distinguen datos desconocidos, valores conocidos —incluidos `false` y `0`—, ausencias confirmadas y requisitos no aplicables.
- [ ] Cada observación permite identificar procedencia y momentos de ocurrencia y disponibilidad cuando se conozcan.
- [ ] Los contratos cubren ambas etapas y los siete gatillos, sin exigir una figura de etapa 2 para representar un setup de etapa 1.
- [ ] Cada condición principal permite expresar estado, motivos, requisitos y referencias a la evidencia utilizada.

### Clasificación y score

- [ ] Escenario conserva la selección manual del usuario.
- [ ] Grade no utiliza el Escenario manual como respuesta de entrada.
- [ ] Cambiar únicamente el Escenario manual no modifica el score ni el Grade calculado.
- [ ] «Línea pivote» se identifica mediante el catálogo y su mapeo semántico verificable.
- [ ] Las pruebas del motor cubren todos los casos de la siguiente tabla.

| Contexto evaluado | Grade | Score mostrado |
|---|---|---:|
| Favorable, con convergencia suficiente y sin elementos contrarios | A | 100 |
| Favorable, con algún elemento contrario | AB | 83,33 |
| Favorable, sin convergencia suficiente para A | AB | 83,33 |
| Neutral, con elementos favorables | BA | 66,67 |
| Neutral, sin modificadores favorables o contrarios | B | 50 |
| Neutral, con elementos contrarios | BC | 33,33 |
| Contrario a la estructura, con algún elemento favorable | CB | 16,67 |
| Contrario a la estructura, sin elementos favorables | C | 0 |
| Neutral, con elementos favorables y contrarios sin resolver | Pendiente | Sin total |
| Neutral, con elementos favorables y contrarios; predominio favorable resuelto manualmente | BA | 66,67 |
| Neutral, con elementos favorables y contrarios; predominio contrario resuelto manualmente | BC | 33,33 |
| Neutral, con elementos favorables y contrarios; predominio neutral resuelto manualmente | B | 50 |

- [ ] Para «línea pivote» se comprueban la base B y los ajustes a BA o BC.
- [ ] Los elementos desconocidos requeridos no se interpretan como ausencia de modificadores.
- [ ] Una clasificación pendiente conserva sus motivos y permite recalcular después de resolver el contexto.
- [ ] La resolución por predominio exige responsable, fecha, fundamento y referencias a ambos grupos de argumentos actuales; conserva las observaciones originales y no acepta un Grade manual.
- [ ] Grade se obtiene antes del redondeo de presentación y se conservan los valores existentes de `GradeType`.
- [ ] La posición interna, el score y Grade son coherentes con una única versión de las reglas.
- [ ] Las pruebas Long/Short verifican que soporte y resistencia se interpreten según dirección y contexto.

### Interpretación de resultados

- [ ] La presentación distingue validez gráfica, Escenario manual, Grade calculado y score.
- [ ] El score se muestra sobre 100 sin presentarlo como porcentaje de efectividad.
- [ ] Un grado alto no compensa una condición gráfica incumplida.
- [ ] Un grado C no modifica por sí solo la validez gráfica.
- [ ] La revisión manual de habilitación para real dispone de estado, responsable, fecha y fundamento diferenciados.

### Persistencia y datos históricos

- [ ] Cada instantánea conserva origen, usuario, versión, entrada, reglas utilizadas y desglose.
- [ ] Se pueden guardar instantáneas de score parcial o no evaluable con total y Grade desconocidos.
- [ ] Un recálculo histórico conserva el resultado original y genera un resultado identificado por su versión.
- [ ] Los históricos insuficientes producen un resultado parcial o no evaluable sin completar evidencia retrospectivamente.
- [ ] Actualizar únicamente SL/TP conserva la evaluación contextual utilizada.
- [ ] Las versiones históricas desconocidas permanecen identificadas como desconocidas.

### Métricas

- [ ] Se distinguen concordancia, corrección, completado manual, descarte y falta de revisión.
- [ ] Un campo desconocido completado por el usuario se contabiliza como completado manualmente.
- [ ] Cada tasa expone numerador, denominador y porcentaje.
- [ ] Un denominador cero produce porcentaje desconocido, mostrado como `Sin datos`.
- [ ] Escenario y Grade se comparan mediante códigos normalizados y el resultado se presenta como concordancia.
- [ ] Los grupos identifican población, versiones y tamaño de muestra.
- [ ] Las agregaciones evitan duplicar validaciones u órdenes por revisiones y recálculos.
- [ ] Las consultas sólo incluyen datos del usuario correspondiente.
- [ ] Los resultados SL/TP se utilizan como observaciones operativas sin inferir una secuencia no registrada.

### Integración y comprobaciones

- [ ] El resumen existente muestra las nuevas métricas, sus muestras y filtros.
- [ ] El resultado contextual muestra puntuación, Grade, disponibilidad y justificación.
- [ ] Existen pruebas del clasificador, normalización numérica, incertidumbre, métricas y preservación histórica.
- [ ] `dotnet build "TradingBookApp.sln"` termina sin errores.
- [ ] `dotnet test "Tests/Application.Tests/Application.Tests.csproj"` termina sin pruebas fallidas.

## 6. Decisiones tomadas y descartadas

### Decisiones tomadas

1. **Checklist como base normativa, con aclaraciones operativas documentadas.** Las tres condiciones gráficas son zona, gatillo y ratio. Las definiciones A–C incluyen las aclaraciones confirmadas en esta revisión. Se distinguen metodología y convenciones de la aplicación.
2. **Separar validez gráfica, clasificación contextual y habilitación para real.** Responden preguntas diferentes. La habilitación inicial es una revisión manual registrada.
3. **Mantener Escenario manual y Grade calculado.** El motor utiliza evidencia. La selección manual permite comparar clasificaciones sin convertirse en la respuesta del cálculo.
4. **Utilizar Figura para identificar «línea pivote».** Se resuelve contra el catálogo. Su base B admite BA o BC según los factores acordados.
5. **Un único cálculo produce posición, score y Grade.** La versión inicial utiliza las siete posiciones y su normalización. Dos setups con el mismo Grade tienen la misma puntuación.
6. **Conservar `GradeType` y utilizar una posición independiente.** Los valores actuales del enum no representan la escala acordada. El mapeo es explícito y tiene pruebas.
7. **Representar incertidumbre sin resultados por defecto.** Los datos desconocidos conservan esa condición. La falta de evidencia requerida produce un resultado parcial o no evaluable con motivos.
8. **Resolver explícitamente contextos neutrales con factores opuestos.** La clasificación queda pendiente hasta una resolución manual fundamentada y su recálculo. La aclaración aprobada en el Paso 6 permite registrar predominio favorable, contrario o neutral para obtener BA, BC o B mediante el motor, con responsable, fecha y referencias a ambos grupos de argumentos conservados.
9. **Versionar reglas y conservar instantáneas reproducibles.** Cada cálculo registra entrada, procedencia, versión y justificación. Los recálculos se diferencian del original.
10. **Separar reevaluación contextual y actualización operativa.** Modificar SL/TP actualiza resultados. Reevaluar contexto identifica los datos que cambiaron y conserva trazabilidad.
11. **Medir concordancia y resultados con denominadores explícitos.** Se distinguen coincidencias, correcciones, completados, descartes y casos no comparables.
12. **Integrar sobre la arquitectura y el resumen existentes.** Se mantiene database-first con SQL versionado y mapeos protegidos frente al scaffolding.

### Decisiones descartadas

- **Obtener A–C mediante bandas numéricas genéricas.** No conserva el significado contextual acordado.
- **Utilizar tres componentes con pesos iguales.** La clasificación inicial utiliza reglas contextuales y modificadores.
- **Sustituir los grados por Bajo/Medio/Alto.** Se conserva la nomenclatura utilizada en la mentoría y en la aplicación.
- **Aplicar un techo universal por etapa o penalizar una etiqueta de ubicación sin contexto.** Se consideran figura, dirección y evidencia.
- **Interpretar el score como efectividad.** La efectividad procede de resultados observados y muestras identificadas.
- **Tratar un SL como prueba de setup inválido o un TP como prueba de setup válido.** Resultado operativo y validez gráfica se miden por separado.
- **Completar retrospectivamente evidencia desconocida.** Los históricos utilizan únicamente información disponible y comparable.
- **Modificar automáticamente Escenario para hacerlo coincidir con Grade.** Las discrepancias permanecen disponibles para revisión y medición.

## 7. Riesgos identificados

| Riesgo | Mitigación |
|---|---|
| Las imágenes no acreditan toda la información necesaria. Una marca puede identificar una zona sin demostrar vigencia, cierre o secuencia. | Representar cada hecho por separado, con procedencia y disponibilidad temporal. Mantener pendiente lo no comprobable y permitir resolución manual fundamentada. |
| Los criterios cualitativos pueden interpretarse de manera diferente. | Documentar reglas con ejemplos de entrada y resultado esperado. Identificar estructura, temporalidad y evidencia de cada factor. Mantener pendientes los conflictos sin regla resolutiva. |
| Una única figura puede ocultar un contexto compuesto. | Distinguir figura principal y referencias adicionales. Registrar convergencias explícitas sin deducirlas sólo del campo Figura. |
| El catálogo puede no identificar inequívocamente «línea pivote». Los IDs o nombres pueden variar. | Validar el mapeo contra el catálogo real. Una referencia inexistente o ambigua produce resultado no evaluable con motivo identificable. |
| Los históricos pueden carecer de datos para el nuevo cálculo. | Utilizar información disponible y comparable. Conservar resultados parciales, mostrar cobertura y aplicar denominadores correspondientes. |
| Cambiar el motor compartido puede alterar históricos durante una actualización operativa. | Conservar originales y versiones. Diferenciar cambios de contexto, recálculos y actualizaciones SL/TP. Probar la preservación de la evaluación contextual. |
| Revisiones y poblaciones pueden duplicar observaciones. | Aplicar la selección de revisión y el conteo de órdenes distintas. Identificar población, versión e instantánea en cada agregado. |
| Score o concordancia pueden interpretarse como efectividad demostrada. | Etiquetar el score como ordinal sobre 100 y la comparación como concordancia. Presentar resultados operativos por separado con muestras y denominadores. |
| La auditoría visual posterior está limitada al no conservar imágenes. | Conservar evidencia de negocio, procedencia, confirmaciones, motivos y versiones. La trazabilidad no afirma reproducir la inspección visual original. |

## 8. Lo que no está en esta especificación

- Implementación de los nuevos evaluadores gráficos de zona, gatillo y ratio.
- Activación del contrato V2 en proveedores IA.
- Nuevo flujo de revisiones y creación/vinculación de órdenes.
- Rediseño integral de pantallas.
- Reconstrucción automática del mapa.
- Habilitación automática para real.
- Score predictivo, ajuste automático de reglas o probabilidades inferidas desde el grado.
- Falsos positivos/negativos de validez sin revisión de referencia.
- Persistencia de imágenes, rutas, hashes o metadatos de archivos.

Estas capacidades requieren sus especificaciones correspondientes. La SPEC 36 queda en estado Borrador hasta la revisión y aprobación explícita del documento consolidado.
