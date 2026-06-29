# SPEC 31 - Crear orden desde validacion IA confirmada

> **Estado:** Implementado - **Depende de:** SPEC 30 - **Fecha:** 2026-06-25
> **Objetivo:** Permitir que el usuario cree una nueva orden desde una validacion IA confirmada reutilizando el endpoint y contrato actual de `OrdersController.AddOrder`.

---

## Alcance

**Incluye:**

- Agregar accion visible en el resultado del asistente para crear orden.
- Usar solamente valores confirmados por el usuario, no valores crudos de IA.
- Reutilizar `OrdersCreateViewModel` como contrato para crear la orden.
- Reutilizar el POST existente `OrdersController.AddOrder([FromBody] OrdersCreateViewModel model)`.
- Capturar campos faltantes requeridos por `OrdersCreateViewModel`.
- Vincular `AiTradeValidation.OrderId` con la orden creada.
- Mantener validaciones actuales de saldo, cuenta y trade dentro de `OrdersController.AddOrder`.

**Fuera de alcance:**

- Crear un flujo nuevo de repositorio para ordenes.
- Saltarse `OrdersController.AddOrder`.
- Crear orden automaticamente al terminar la validacion IA.
- Crear orden sin confirmacion humana.
- Modificar reglas de saldo o cuenta.
- Guardar imagenes.
- Chat de seguimiento.

---

## Data model

Se reutiliza `Web/Models/OrdersViewModel.cs`:

```csharp
public class OrdersCreateViewModel
{
    public int CategoryId { get; set; }
    public int AccountTypeId { get; set; }
    public int InstrumentsId { get; set; }
    public DateTime CreationDate { get; set; }
    public TimeSpan Time { get; set; }
    public int DayId { get; set; }
    public int StageId { get; set; }
    public int FigureId { get; set; }
    public int FrameId { get; set; }
    public int TriggerId { get; set; }
    public int DirectionId { get; set; }
    public int SceneryId { get; set; }
    public string? OrderTypeId { get; set; }
    public string? TradeTypeId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? CommissionRate { get; set; }
    public decimal? Total { get; set; }
    public bool? IsTrendAligned { get; set; }
    public byte? LocationType { get; set; }
    public byte? ConfirmationType { get; set; }
    public bool? IsPivotZone { get; set; }
}
```

Se agrega view model de preparacion en `Web/Models`:

```csharp
public sealed class CreateOrderFromValidationViewModel
{
    public int ValidationId { get; set; }
    public OrdersCreateViewModel Order { get; set; } = null!;
}
```

Campos que deben venir de la validacion confirmada:

- `InstrumentsId`.
- `StageId`.
- `FigureId`.
- `FrameId`.
- `TriggerId`.
- `DirectionId`.
- `SceneryId`.
- `IsTrendAligned`.
- `LocationType`.
- `ConfirmationType`.
- `IsPivotZone`.

Campos que debe capturar el usuario antes de enviar a `OrdersController.AddOrder`:

- `CategoryId`.
- `AccountTypeId`.
- `CreationDate`.
- `Time`.
- `DayId`.
- `OrderTypeId`.
- `TradeTypeId`.
- `Quantity`.
- `Price`.
- `CommissionRate`.
- `Total`.

---

## Plan de implementacion

**Paso 1 - Preparar boton de crear orden**

1. En `Result.cshtml`, mostrar boton `Crear orden` solo cuando la validacion tenga valores confirmados suficientes.
2. Ocultar el boton si la validacion ya tiene `OrderId`.
3. Compilar para validar.

**Paso 2 - Crear endpoint de preparacion**

1. Agregar accion `CreateOrder` en `TradeAssistantController`.
2. Cargar la validacion por `ValidationId` y usuario actual.
3. Construir `CreateOrderFromValidationViewModel` con valores confirmados.
4. Cargar listas requeridas para campos faltantes.
5. Compilar para validar.

**Paso 3 - Crear vista de revision de orden**

1. Crear vista `CreateOrder.cshtml` en `Views/TradeAssistant`.
2. Mostrar valores provenientes de la validacion confirmada.
3. Permitir editar campos operativos requeridos por `OrdersCreateViewModel`.
4. Advertir que la orden se creara usando el flujo existente de ordenes.
5. Compilar para validar.

**Paso 4 - Reutilizar `OrdersController.AddOrder`**

1. Enviar el payload final como JSON al endpoint existente `/Orders/AddOrder`.
2. No duplicar la logica de saldo, cuenta, `Order`, `Trade` ni `AccountBalance`.
3. Manejar la respuesta `ResultBackViewModel` actual.
4. Compilar para validar.

**Paso 5 - Vincular validacion con orden**

1. Obtener el `OrderId` devuelto por el mensaje exitoso o ajustar respuesta minima si hace falta sin romper clientes existentes.
2. Llamar `IAiTradeValidationRepository.LinkOrderAsync`.
3. Impedir vincular una validacion de otro usuario.
4. Impedir crear una segunda orden desde la misma validacion.
5. Compilar para validar.

**Paso 6 - Verificacion final**

1. Ejecutar `dotnet build "TradingBookApp.sln"`.
2. Ejecutar la app con perfil HTTPS.
3. Crear una validacion completa.
4. Confirmar valores.
5. Crear orden desde la validacion.
6. Confirmar que aparece en el modulo de ordenes.
7. Confirmar que `AiTradeValidation.OrderId` queda vinculado.

---

## Criterios de aceptacion

- [ ] El resultado del asistente muestra `Crear orden` solo con confirmacion suficiente.
- [ ] El boton no aparece si la validacion ya tiene orden vinculada.
- [ ] La pantalla de creacion usa `OrdersCreateViewModel`.
- [ ] La creacion final llama a `OrdersController.AddOrder`.
- [ ] No se duplica la logica de `OrdersRepository.AddOrderAsync`.
- [ ] No se crea orden con valores no confirmados por el usuario.
- [ ] Se capturan los campos faltantes de cuenta, categoria, cantidad, precio, comision y total.
- [ ] La validacion queda vinculada con `OrderId` despues de crear la orden.
- [ ] Un usuario no puede crear orden desde una validacion de otro usuario.
- [ ] No se puede crear mas de una orden desde la misma validacion.
- [ ] `dotnet build "TradingBookApp.sln"` termina sin errores.

---

## Decisiones tomadas y descartadas

- **Si:** reutilizar `OrdersController.AddOrder`. Fue solicitado explicitamente y mantiene una sola ruta funcional para crear ordenes.
- **Si:** usar `OrdersCreateViewModel`. Evita inventar otro contrato para la misma operacion.
- **Si:** capturar campos faltantes antes de enviar. La validacion IA no conoce todos los datos necesarios de cuenta y trade.
- **Si:** vincular la validacion despues de exito. Mantiene trazabilidad entre asistencia y orden real.
- **No:** crear orden automaticamente. La IA solo asiste y el usuario decide.
- **No:** llamar repositorios de orden desde el asistente. Duplicaria la logica existente.

---

## Riesgos identificados

| Riesgo | Mitigacion |
|--------|------------|
| `OrdersController.AddOrder` devuelve el `OrderId` dentro del mensaje y no como campo estructurado | Ajustar respuesta de forma compatible o parsear solo como solucion temporal documentada |
| El usuario modifica campos clave despues de confirmar | Mostrar claramente valores confirmados y validar antes de enviar |
| La validacion puede quedar sin vinculo si falla el paso posterior a crear orden | Reintentar `LinkOrderAsync` o mostrar accion manual de vincular si la orden se creo |
| Reutilizar endpoint via JS puede ocultar errores de autorizacion | Manejar `ResultBackViewModel` y errores HTTP de forma visible |

---

## Lo que **no** esta en esta spec

- Nuevo servicio de creacion de ordenes.
- Nuevo repositorio de ordenes.
- Orden automatica sin confirmacion.
- Persistencia de imagenes.
- Chat de seguimiento.
- Metricas de aprendizaje.
