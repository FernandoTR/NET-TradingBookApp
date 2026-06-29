# SPEC 30 - UI Web del asistente de validacion IA

> **Estado:** Implementado - **Depende de:** SPEC 25, SPEC 26, SPEC 27, SPEC 28, SPEC 29 - **Fecha:** 2026-06-25
> **Objetivo:** Crear el modulo MVC del asistente para capturar propuesta de trade, enviar imagenes temporales a IA, mostrar resultado, permitir confirmacion manual e historial de validaciones completas.

---

## Alcance

**Incluye:**

- Crear `TradeAssistantController`.
- Crear formulario de validacion con datos propuestos e imagenes temporales.
- Cargar catalogos existentes para instrumento, direccion, frame, escenario, etapa, figura y gatillo.
- Mostrar resultado con veredicto, score, reglas, informacion no confirmable y evidencia historica.
- Permitir editar y confirmar valores detectados antes de crear orden.
- Mostrar historial de validaciones completas del usuario actual.
- Aplicar autorizacion por usuario.
- No mostrar ni enlazar imagenes despues de la validacion.

**Fuera de alcance:**

- Crear ordenes desde la validacion. Eso pertenece a SPEC 31.
- Chat de seguimiento.
- Metricas de aprendizaje.
- Pantalla administrativa para proveedores IA.
- Guardar imagenes.
- Cambiar CSS global del tema.

---

## Data model

Se agregan view models en `Web/Models`:

```csharp
public sealed class TradeAssistantCreateViewModel
{
    public int InstrumentId { get; set; }
    public int DirectionId { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public int? FrameId { get; set; }
    public int? SceneryId { get; set; }
    public int? StageId { get; set; }
    public int? TriggerId { get; set; }
    public int? FigureId { get; set; }
    public string? UserComment { get; set; }
    public List<TradeAssistantImageViewModel> Images { get; set; } = new();
}
```

```csharp
public sealed class TradeAssistantImageViewModel
{
    public IFormFile File { get; set; } = null!;
    public int ImageRole { get; set; }
    public int? FrameId { get; set; }
    public int SortOrder { get; set; }
    public string? Comment { get; set; }
}
```

```csharp
public sealed class TradeAssistantResultViewModel
{
    public int ValidationId { get; set; }
    public AiValidationResultDto Result { get; set; } = null!;
    public ConfirmedAiValidationDto Confirmation { get; set; } = null!;
}
```

Se agregan vistas en `Web/Views/TradeAssistant`:

- `Index.cshtml`.
- `Result.cshtml`.
- `History.cshtml`.

Se agrega JS local:

- `Web/wwwroot/js/trade-assistant.js`.

---

## Plan de implementacion

**Paso 1 - Crear controlador y rutas**

1. Crear `TradeAssistantController` en `Web/Controllers`.
2. Agregar accion `Index` para formulario.
3. Agregar accion `Validate` para POST de validacion.
4. Agregar accion `Result` para consultar resultado guardado.
5. Agregar accion `History` para historial del usuario.
6. Compilar para validar.

**Paso 2 - Cargar catalogos**

1. Inyectar servicios de catalogos ya existentes.
2. Preparar listas `SelectListItem` para los campos del formulario.
3. Reutilizar patrones existentes en `OrdersController` cuando aplique.
4. Compilar para validar.

**Paso 3 - Crear formulario**

1. Crear `Index.cshtml`.
2. Capturar instrumento, direccion, entrada, stop loss, take profit y comentario.
3. Permitir escenario, etapa, gatillo, figura y frame como campos opcionales.
4. Permitir 1 a 4 imagenes con rol, frame, orden y comentario.
5. Mostrar restricciones de tamanio y formato.
6. Compilar para validar.

**Paso 4 - Conectar validacion**

1. Convertir el view model a `CreateAiValidationDto`.
2. Validar imagenes con el flujo del SPEC 26.
3. Llamar `ITradeValidationOrchestrator.ValidateAsync`.
4. Redirigir a `Result` cuando exista validacion completa.
5. Mostrar error controlado cuando no exista resultado completo.
6. Compilar para validar.

**Paso 5 - Crear vista de resultado**

1. Mostrar veredicto, score, grado y confianza visual.
2. Mostrar condiciones cumplidas.
3. Mostrar condiciones incumplidas.
4. Mostrar informacion no confirmable.
5. Mostrar evidencia historica.
6. No mostrar imagenes ni enlaces de imagen.
7. Compilar para validar.

**Paso 6 - Crear confirmacion manual**

1. Mostrar valores detectados por IA junto a campos editables.
2. Permitir confirmar trigger, escenario, etapa, figura, frame, ubicacion, confirmacion, tendencia y zona pivote.
3. Guardar confirmacion en `AiTradeValidation`.
4. Compilar para validar.

**Paso 7 - Crear historial**

1. Mostrar solo validaciones del usuario actual.
2. Incluir fecha, instrumento, direccion, estado, score, proveedor y modelo.
3. Agregar enlace a resultado.
4. No mostrar ni intentar recuperar imagenes.
5. Compilar para validar.

**Paso 8 - Verificacion final**

1. Ejecutar `dotnet build "TradingBookApp.sln"`.
2. Ejecutar `dotnet run --project Web/Web.csproj --launch-profile https`.
3. Validar manualmente una propuesta completa con imagen valida.
4. Validar manualmente una propuesta rechazada por imagen invalida.
5. Confirmar que el historial solo muestra validaciones propias.

---

## Criterios de aceptacion

- [ ] Existe `TradeAssistantController`.
- [ ] Existe `Index.cshtml` para crear validacion.
- [ ] Existe `Result.cshtml` para mostrar resultado.
- [ ] Existe `History.cshtml` para historial.
- [ ] El formulario acepta datos propuestos de trade.
- [ ] El formulario acepta 1 a 4 imagenes temporales.
- [ ] La vista de resultado muestra reglas cumplidas, fallidas y no confirmables.
- [ ] La vista de resultado muestra evidencia historica cuando exista.
- [ ] El usuario puede editar y confirmar valores detectados.
- [ ] El historial solo muestra validaciones completas del usuario actual.
- [ ] Ninguna vista muestra imagenes despues de la validacion.
- [ ] Ninguna vista contiene rutas de imagenes persistidas.
- [ ] `dotnet build "TradingBookApp.sln"` termina sin errores.

---

## Decisiones tomadas y descartadas

- **Si:** MVC convencional. Es consistente con los controladores y vistas actuales.
- **Si:** confirmacion humana antes de crear orden. La IA no debe registrar ordenes automaticamente.
- **Si:** historial solo de resultados completos. Coincide con la decision de persistencia del SPEC 25.
- **Si:** no mostrar imagenes despues de validar. Las imagenes no se guardan.
- **No:** UI de chat. Fue omitida por decision del usuario.
- **No:** admin de proveedores IA. El cambio de proveedor se hace por configuracion.

---

## Riesgos identificados

| Riesgo | Mitigacion |
|--------|------------|
| El usuario puede creer que las imagenes quedan disponibles en historial | Mostrar resultado textual y no renderizar previsualizaciones posteriores |
| Faltan campos para crear orden desde la validacion | Dejar creacion de orden para SPEC 31 con captura de campos faltantes |
| Catalogos grandes pueden cargar lento | Reutilizar servicios existentes y no crear endpoints innecesarios en el MVP |
| Error del proveedor IA puede parecer error de formulario | Mostrar mensaje claro de proveedor no disponible o respuesta invalida |

---

## Lo que **no** esta en esta spec

- Creacion de ordenes.
- Chat de seguimiento.
- Metricas de aprendizaje.
- Persistencia de imagenes.
- Configuracion visual de proveedores IA.
- Nuevas reglas de estrategia.
