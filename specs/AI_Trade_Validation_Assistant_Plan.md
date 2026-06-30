# Asistente de Validación Operativa con IA

## 1. Objetivo

Implementar dentro de **TradingBookApp** un módulo de asistencia para validar operativas de trading mediante:

- carga de múltiples imágenes del gráfico;
- extracción visual de estructura, zona, gatillo y contexto;
- evaluación contra las reglas objetivas de la estrategia;
- cálculo del `Trading Score` existente;
- consulta de estadísticas históricas y convergencias;
- explicación clara de condiciones cumplidas, incumplidas y no confirmables.

El módulo **no debe generar señales automáticas ni ejecutar operaciones**. Su función es ayudar al usuario a validar, documentar y mejorar una idea de trade.

---

## 2. Principio de diseño

Separar la solución en tres responsabilidades:

### 2.1 IA multimodal

La IA únicamente debe:

- analizar las imágenes;
- identificar evidencia visual;
- extraer datos estructurados;
- indicar niveles de confianza;
- marcar información faltante o contradictoria.

La IA no debe:

- decidir por sí sola si la operación es válida;
- calcular el score final;
- inventar estadísticas históricas;
- ejecutar SQL;
- registrar una orden automáticamente.

### 2.2 Motor determinístico de reglas

Las reglas de la estrategia deben implementarse en C# o SQL, no depender del criterio libre del modelo.

Ejemplos:

- ratio mínimo `1:1`;
- en `ETAPA 1` solo son válidos los escenarios `BA`, `B` y `BC`;
- determinadas figuras solo son válidas en ciertas etapas;
- dirección alineada con tendencia;
- zona válida obligatoria;
- gatillo obligatorio;
- confirmación compatible con continuación o reversión;
- zona pivote y ubicación estructural.

### 2.3 Evidencia histórica

El sistema debe consultar la base de datos y aportar métricas reales de operaciones similares:

- cantidad de operaciones;
- porcentaje de SL;
- porcentaje de TP1;
- porcentaje de TP2;
- porcentaje de TP3;
- score de convergencia;
- combinaciones con mejor rendimiento.

La IA puede explicar esos datos, pero nunca inventarlos.

---

## 3. Flujo funcional

```text
Usuario
  ↓
Carga imágenes y datos propuestos
  ↓
Validación y almacenamiento temporal de archivos
  ↓
Análisis visual multimodal
  ↓
Extracción estructurada del setup
  ↓
Normalización contra catálogos del sistema
  ↓
Motor de reglas de estrategia
  ↓
Trading Score Engine
  ↓
Consulta de convergencias y operaciones similares
  ↓
Composición de resultado
  ↓
Confirmación o corrección manual del usuario
  ↓
Guardar validación o crear operación
```

---

## 4. Datos de entrada

El formulario debe permitir capturar:

- instrumento;
- dirección propuesta;
- entrada;
- stop loss;
- take profit;
- temporalidad principal;
- escenario propuesto, opcional;
- etapa propuesta, opcional;
- gatillo propuesto, opcional;
- figura propuesta, opcional;
- comentario del usuario;
- entre 1 y 8 imágenes.

Cada imagen debe incluir metadatos:

- temporalidad;
- función dentro del análisis;
- orden;
- comentario opcional.

### Roles de imagen sugeridos

```csharp
public enum TradingImageRole
{
    GeneralContext = 1,
    HigherTimeframe = 2,
    MainTimeframe = 3,
    EntryTimeframe = 4,
    Trigger = 5,
    Confirmation = 6,
    UserMarkup = 7
}
```

Ejemplo:

| Imagen | Temporalidad | Rol |
|---|---|---|
| 1 | 4H | HigherTimeframe |
| 2 | 1H | MainTimeframe |
| 3 | 15m | Trigger |
| 4 | 15m | Confirmation |

---

## 5. Resultado esperado

La respuesta debe organizarse en bloques claros.

### 5.1 Veredicto

Valores sugeridos:

- `Valid`;
- `ConditionallyValid`;
- `Invalid`;
- `InsufficientEvidence`.

Debe incluir:

- score total;
- grado;
- confianza visual;
- estado general.

### 5.2 Condiciones cumplidas

Ejemplos:

- dirección alineada con la estructura;
- zona válida;
- gatillo detectado;
- confirmación presente;
- zona pivote detectada.

### 5.3 Condiciones incumplidas

Ejemplos:

- ratio inferior a `1:1`;
- escenario incompatible con la etapa;
- ausencia de confirmación;
- entrada demasiado cercana a resistencia;
- figura no permitida.

### 5.4 Información no confirmable

Ejemplos:

- temporalidad ilegible;
- escenario ambiguo;
- no se distingue cierre de vela;
- niveles de entrada, SL o TP no visibles;
- contradicción entre imágenes.

### 5.5 Evidencia histórica

Mostrar:

- combinación encontrada;
- número de trades;
- SL, TP1, TP2 y TP3;
- score de convergencia;
- advertencia si la muestra es pequeña.

### 5.6 Acciones sugeridas

Las acciones deben expresar cómo cumplir mejor la estrategia, por ejemplo:

- esperar cierre de vela;
- ajustar entrada o TP para cumplir RR;
- seleccionar manualmente el escenario;
- añadir imagen de temporalidad superior;
- confirmar si la zona es soporte o resistencia.

---

## 6. Arquitectura propuesta

Mantener la estructura actual de Clean Architecture.

### Domain

```text
Domain/
├── Entities/
│   ├── AiTradeValidation.cs
│   ├── AiTradeValidationImage.cs
│   ├── AiTradeValidationRule.cs
│   └── AiConversationMessage.cs
├── Enums/
│   ├── TradingImageRole.cs
│   ├── AiValidationStatus.cs
│   ├── ValidationRuleResult.cs
│   └── ValidationSource.cs
└── ValueObjects/
    └── TradeProposal.cs
```

### Application

```text
Application/
├── DTOs/AiValidation/
│   ├── CreateAiValidationDto.cs
│   ├── AiVisionExtractionDto.cs
│   ├── AiValidationResultDto.cs
│   ├── HistoricalEvidenceDto.cs
│   └── ImageFindingDto.cs
├── Interfaces/
│   ├── IAiVisionClient.cs
│   ├── ITradeValidationOrchestrator.cs
│   ├── IStrategyRuleEngine.cs
│   ├── IHistoricalEvidenceService.cs
│   ├── IAiTradeValidationRepository.cs
│   └── ITradingImageStorage.cs
└── Services/
    ├── TradeValidationOrchestrator.cs
    ├── StrategyRuleEngine.cs
    ├── HistoricalEvidenceService.cs
    └── TradeSetupNormalizer.cs
```

### Infrastructure

```text
Infrastructure/
├── ArtificialIntelligence/
│   ├── OpenAiVisionClient.cs
│   ├── OpenAiOptions.cs
│   ├── PromptTemplateProvider.cs
│   └── StructuredOutputSchemas.cs
├── Persistence/Repositories/
│   └── AiTradeValidationRepository.cs
└── Storage/
    └── TradingImageStorage.cs
```

### Web

```text
Web/
├── Controllers/
│   └── TradeAssistantController.cs
├── Views/TradeAssistant/
│   ├── Index.cshtml
│   ├── Result.cshtml
│   └── History.cshtml
├── Models/
│   └── TradeAssistantViewModel.cs
└── wwwroot/js/
    └── trade-assistant.js
```

---

## 7. Entidades principales

### AiTradeValidation

Campos mínimos:

- `Id`;
- `UserId`;
- `OrderId`, nullable;
- `InstrumentId`;
- `ProposedDirection`;
- `ProposedEntry`;
- `ProposedStopLoss`;
- `ProposedTakeProfit`;
- `DetectedTriggerId`;
- `DetectedSceneryId`;
- `DetectedFigureId`;
- `DetectedFrameId`;
- `DetectedStage`;
- `DetectedLocation`;
- `DetectedConfirmation`;
- `IsTrendAligned`;
- `IsPivotZone`;
- `RiskRewardRatio`;
- `StructuralScore`;
- `TotalScore`;
- `Grade`;
- `VisualConfidence`;
- `ValidationStatus`;
- `ModelResponseJson`;
- `FinalSummary`;
- `ModelName`;
- `PromptVersion`;
- `CreatedAt`.

### AiTradeValidationImage

Campos mínimos:

- `Id`;
- `ValidationId`;
- `ImageRole`;
- `FrameId`;
- `OriginalFileName`;
- `StoragePath`;
- `ContentType`;
- `FileSize`;
- `SortOrder`;
- `ImageAnalysisJson`.

### AiTradeValidationRule

Campos mínimos:

- `Id`;
- `ValidationId`;
- `RuleCode`;
- `RuleName`;
- `Result`;
- `Weight`;
- `ScoreObtained`;
- `Evidence`;
- `Source`.

---

## 8. Contrato estructurado de IA

La respuesta del modelo debe ser JSON estricto y permitir valores `null` cuando no exista evidencia suficiente.

Ejemplo simplificado:

```json
{
  "instrument": "SUIUSDT",
  "direction": "Long",
  "detectedFrames": ["4H", "1H", "15m"],
  "marketStructure": {
    "trend": "Bullish",
    "stage": "Stage2",
    "higherHighs": true,
    "higherLows": true
  },
  "location": {
    "type": "Support",
    "pivotZoneDetected": true,
    "description": "Retest de soporte y línea de tendencia"
  },
  "trigger": {
    "type": "PerfectPullback",
    "confidence": 0.81
  },
  "figure": {
    "type": null,
    "confidence": 0.34
  },
  "confirmation": {
    "type": "ContinuationRetest",
    "detected": true
  },
  "visibleLevels": {
    "entry": 1.158,
    "stopLoss": 1.068,
    "takeProfit": 1.236
  },
  "evidence": [],
  "contradictions": [],
  "missingData": [],
  "confidence": {
    "overall": 0.81
  }
}
```

Reglas del contrato:

- no devolver texto fuera del JSON;
- no inventar catálogos;
- utilizar valores normalizables;
- permitir `null`;
- incluir evidencia por imagen;
- identificar contradicciones;
- separar dato visible, dato proporcionado e inferencia.

---

## 9. Prompt del sistema

El prompt debe estar versionado.

Principios obligatorios:

1. No generar señales.
2. No recomendar comprar o vender.
3. No inventar niveles, escenarios, figuras o temporalidades.
4. Usar `null` cuando no sea posible confirmar algo.
5. No calcular estadísticas históricas.
6. No otorgar el score definitivo.
7. No declarar válida una operación solo por apariencia.
8. Indicar la imagen que respalda cada hallazgo.
9. Registrar contradicciones entre temporalidades.
10. Devolver únicamente el esquema JSON solicitado.

El prompt debe incluir únicamente las reglas relevantes para el setup analizado, evitando enviar toda la estrategia en cada llamada.

---

## 10. Orquestador de validación

Responsabilidad principal:

```csharp
public interface ITradeValidationOrchestrator
{
    Task<AiValidationResultDto> ValidateAsync(
        CreateAiValidationDto request,
        CancellationToken cancellationToken);
}
```

Flujo interno:

```csharp
public async Task<AiValidationResultDto> ValidateAsync(
    CreateAiValidationDto request,
    CancellationToken cancellationToken)
{
    var extraction = await _visionClient.ExtractSetupAsync(
        request,
        cancellationToken);

    var setup = _normalizer.Normalize(request, extraction);

    var rules = _ruleEngine.Evaluate(setup);

    var historicalEvidence = await _evidenceService.GetEvidenceAsync(
        setup,
        cancellationToken);

    var score = _scoreEngine.Calculate(setup, rules);

    var result = _resultFactory.Create(
        setup,
        extraction,
        rules,
        historicalEvidence,
        score);

    await _repository.SaveAsync(result, cancellationToken);

    return result;
}
```

---

## 11. Cálculos determinísticos

El ratio siempre debe calcularse en código.

### Long

```text
Riesgo = Entrada - StopLoss
Beneficio = TakeProfit - Entrada
RR = Beneficio / Riesgo
```

### Short

```text
Riesgo = StopLoss - Entrada
Beneficio = Entrada - TakeProfit
RR = Beneficio / Riesgo
```

Validaciones mínimas:

- riesgo mayor que cero;
- beneficio mayor que cero;
- precios coherentes con la dirección;
- ratio mínimo configurable;
- evitar división entre cero.

La IA nunca debe ser la fuente del ratio final.

---

## 12. Integración con estadísticas

Crear servicios controlados, sin permitir SQL libre generado por IA.

```csharp
Task<ConvergenceEvidenceDto> GetConvergenceAsync(
    int? triggerId,
    int? sceneryId,
    Direction? direction,
    int? frameId,
    int? figureId,
    CancellationToken cancellationToken);

Task<SimilarTradesDto> GetSimilarTradesAsync(
    TradeSetupFilter filter,
    CancellationToken cancellationToken);

Task<StrategyRulesDto> GetStrategyRulesAsync(
    Stage? stage,
    Direction? direction,
    int? figureId,
    CancellationToken cancellationToken);
```

Debe reutilizarse el módulo existente de convergencias cuando sea posible.

---

## 13. Carga y seguridad de imágenes

Configuración inicial sugerida:

```json
{
  "MaxImagesPerValidation": 8,
  "MaxImageSizeMb": 8,
  "MaxTotalUploadMb": 40,
  "AllowedFormats": [
    "image/jpeg",
    "image/png",
    "image/webp"
  ]
}
```

Medidas obligatorias:

- validar extensión, MIME y firma real del archivo;
- rechazar SVG;
- normalizar orientación;
- eliminar metadatos EXIF;
- generar nombre interno seguro;
- impedir path traversal;
- calcular hash SHA-256;
- detectar duplicados;
- aplicar autorización por usuario;
- no exponer rutas físicas;
- limitar cantidad y tamaño total;
- registrar errores con Serilog sin incluir secretos.

Estrategia de análisis:

- de 1 a 4 imágenes: una sola llamada multimodal;
- de 5 a 8 imágenes: análisis individual y posterior síntesis conjunta.

---

## 14. Confirmación humana

Antes de crear una orden, mostrar los datos detectados junto con campos editables.

Ejemplo:

| Campo | Detectado por IA | Confirmado por usuario |
|---|---|---|
| Trigger | Pullback Perfecto | Selector |
| Escenario | A | Selector |
| Etapa | 2 | Selector |
| Figura | Ninguna | Selector |
| Ubicación | Soporte | Selector |
| Confirmación | Retest | Selector |
| Tendencia alineada | Sí | Sí/No |
| Zona pivote | Sí | Sí/No |

Guardar:

- valor detectado;
- valor confirmado;
- fecha de modificación;
- usuario;
- motivo opcional.

El botón para crear la orden debe utilizar los valores confirmados, no directamente los valores de la IA.

---

## 15. Chat de seguimiento

La primera versión puede operar como un formulario de validación. El chat conversacional se agrega en una segunda fase.

Ejemplos de mensajes posteriores:

- “La imagen 2 es 4H, no 1H”.
- “Recalcula usando escenario AB”.
- “Muéstrame por qué falló la regla de etapa”.
- “Compara con mis operaciones similares”.

Cada mensaje debe asociarse con la validación original y conservar:

- mensaje del usuario;
- adjuntos;
- respuesta estructurada;
- versión de prompt;
- modelo utilizado;
- cambios realizados.

---

## 16. Métricas de aprendizaje

Guardar la comparación entre:

- clasificación de la IA;
- clasificación confirmada por el usuario;
- resultado real de la operación.

Métricas futuras:

- precisión del trigger;
- precisión del escenario;
- precisión de la etapa;
- precisión de la figura;
- tasa de corrección humana;
- relación entre score y TP alcanzado;
- falsos positivos;
- falsos negativos;
- confianza visual frente a precisión real.

---

## 17. Fases de implementación

### Fase 1 — MVP

Implementar:

1. formulario de validación;
2. carga de hasta 4 imágenes;
3. asignación de rol y temporalidad;
4. datos de entrada, SL, TP y dirección;
5. integración multimodal;
6. respuesta JSON estructurada;
7. normalización contra catálogos;
8. motor de reglas;
9. cálculo del score;
10. consulta de convergencias;
11. pantalla de resultado;
12. confirmación manual;
13. historial de validaciones;
14. botón para crear una operación.

### Fase 2 — Conversación y evidencia

Implementar:

- chat de seguimiento;
- hasta 8 imágenes;
- análisis por temporalidad;
- búsqueda de operaciones similares;
- recuperación de documentación de estrategia;
- feedback del usuario;
- comparación IA vs usuario.

### Fase 3 — Aprendizaje operativo

Implementar:

- métricas de precisión;
- análisis de falsos positivos;
- detección de sesgos recurrentes;
- setups fuera del plan;
- alertas de sobreoperación;
- comparación entre score y resultado real;
- revisión posterior al cierre de la operación.

---

## 18. Fuera de alcance del MVP

No implementar inicialmente:

- ejecución automática de órdenes;
- generación de señales en tiempo real;
- acceso directo del modelo a SQL;
- entrenamiento o fine-tuning;
- interpretación de video;
- análisis automático de todo el mercado;
- scraping de TradingView;
- recomendaciones financieras autónomas.

---

## 19. Criterios de aceptación del MVP

El MVP se considera terminado cuando:

- permite cargar al menos 4 imágenes;
- cada imagen tiene rol y temporalidad;
- valida formatos y tamaños;
- obtiene una respuesta JSON válida;
- soporta campos no confirmables con `null`;
- calcula RR en código;
- ejecuta reglas objetivas;
- reutiliza el `Trading Score Engine`;
- consulta estadísticas históricas reales;
- diferencia condiciones cumplidas, fallidas y no confirmables;
- permite editar los datos detectados;
- guarda la validación completa;
- permite crear una orden con datos confirmados;
- aplica autorización por usuario;
- no expone secretos ni rutas internas;
- registra errores y ejecuciones relevantes;
- existen pruebas unitarias para el motor de reglas y cálculos.

---

## 20. Pruebas mínimas

Agregar un proyecto de pruebas si todavía no existe.

### Unitarias

- cálculo RR Long;
- cálculo RR Short;
- SL o TP inválidos;
- regla de Etapa 1;
- regla de figura por etapa;
- tendencia alineada;
- ausencia de zona;
- ausencia de gatillo;
- normalización de nombres del modelo;
- manejo de campos `null`;
- cálculo del resultado general.

### Integración

- persistencia completa de validación;
- consulta de convergencias;
- carga segura de imágenes;
- autorización por usuario;
- deserialización de Structured Output;
- rechazo de JSON inválido;
- manejo de error de proveedor de IA.

### Manuales

- una imagen clara;
- múltiples temporalidades;
- imágenes contradictorias;
- texto ilegible;
- escenario ambiguo;
- RR menor a `1:1`;
- operación válida con buena muestra histórica;
- operación con muestra histórica insuficiente.

---

## 21. Instrucciones para Codex

1. Respetar Clean Architecture y la dirección de dependencias actual.
2. No colocar lógica de negocio en controladores ni vistas.
3. No permitir que la IA determine reglas que deben ser determinísticas.
4. No generar SQL dinámico desde texto del modelo.
5. Mantener compatibilidad con el enfoque database-first.
6. Colocar configuraciones sensibles en `appsettings.{Environment}.json` o variables de entorno.
7. Versionar el prompt y el esquema de salida.
8. Utilizar DTOs específicos y validaciones de entrada.
9. Añadir pruebas unitarias antes de conectar la interfaz final.
10. Implementar el MVP en cambios pequeños y revisables.
11. Actualizar `CHANGELOG.md` al completar cada bloque funcional.
12. Ejecutar al final:

```bash
dotnet build "TradingBookApp.sln"
```

---

## 22. Resultado arquitectónico esperado

```text
OpenAI Vision
    ↓
Extrae evidencia visual estructurada
    ↓
TradeSetupNormalizer
    ↓
Normaliza catálogos y valores
    ↓
StrategyRuleEngine
    ↓
Aplica reglas objetivas
    ↓
TradingScoreEngine
    ↓
Calcula score
    ↓
HistoricalEvidenceService
    ↓
Consulta convergencias y operaciones similares
    ↓
TradeValidationOrchestrator
    ↓
Compone y guarda el resultado
    ↓
Usuario confirma o corrige
    ↓
Se crea la operación opcionalmente
```

La IA observa y explica. El código valida. La base de datos aporta evidencia. El usuario toma la decisión final.
