# SPEC 35 - Integracion OpenCode Go como proveedor IA con catalogo de modelos

> **Estado:** Implementado - **Depende de:** SPEC 33 - **Fecha:** 2026-07-03
> **Objetivo:** Integrar `OpenCodeGo` en el modulo `AiProviders` como proveedor IA configurable con catalogo de modelos precargado y runtime preparado, bloqueando su activacion mientras el modelo seleccionado no soporte vision.

---

## Alcance

**Incluye:**

- Agregar `OpenCodeGo` como proveedor soportado en el modulo existente `AiProviders`.
- Crear catalogo precargado de modelos OpenCode Go mediante script SQL versionado.
- Agregar relacion nullable `ModelCatalogId` desde `AiProviderConfiguration` hacia el catalogo de modelos.
- Extender `AiProviders` para que, al seleccionar `OpenCodeGo`, el usuario elija el modelo desde un dropdown de catalogo.
- Hacer que `ModelName`, `Endpoint`, `ApiProtocol` y `SupportsVision` sean de solo lectura para `OpenCodeGo` y se deriven del catalogo.
- Mantener edicion manual de `ModelName` y `Endpoint` para `OpenAI`, `MiniMax`, `DeepSeek`, `GLM` y `Kimi`.
- Guardar solo el nombre de la variable/secret de API key para `OpenCodeGo`, usando `OPENCODE_GO_API_KEY` como valor recomendado.
- Agregar soporte runtime para `OpenCodeGo` con protocolos `OpenAiChatCompletions` y `AnthropicMessages`.
- Bloquear la activacion de `OpenCodeGo` cuando el modelo seleccionado tenga `SupportsVision = false`.
- Sembrar los modelos OpenCode Go iniciales con `SupportsVision = false` porque la documentacion revisada no confirma soporte de vision.
- Reutilizar el permiso existente `Permissions.AiProviders`.
- Mantener el modulo dentro de `AiProviders`; no se crea un modulo administrativo nuevo.

**Fuera de alcance:**

- Guardar precios, limites de uso, consumo, top-ups o estado de suscripcion de OpenCode Go.
- Sincronizar dinamicamente modelos desde `https://opencode.ai/zen/go/v1/models`.
- Permitir crear o editar modelos OpenCode Go desde la UI.
- Activar modelos OpenCode Go sin soporte de vision confirmado.
- Crear un flujo text-only separado para el asistente IA.
- Guardar API keys reales en SQL.
- Mostrar, revelar, copiar o rotar API keys desde la aplicacion.
- Cambiar prompts, schemas, reglas, score, metricas o flujo funcional del asistente.
- Crear permisos nuevos o entradas de menu nuevas.
- Reemplazar el comportamiento actual de proveedores existentes.

---

## Data model

Se agrega entidad en `Domain/Entities`:

```csharp
public partial class AiProviderModelCatalog
{
    public int Id { get; set; }

    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string ModelId { get; set; } = null!;

    public string Endpoint { get; set; } = null!;

    public string ApiProtocol { get; set; } = null!;

    public bool SupportsVision { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; set; }
}
```

Se ajusta entidad existente `AiProviderConfiguration`:

```csharp
public partial class AiProviderConfiguration
{
    public int Id { get; set; }

    public int? ModelCatalogId { get; set; }

    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string? Endpoint { get; set; }

    public string ApiProtocol { get; set; } = null!;

    public string ApiKeyEnvironmentVariable { get; set; } = null!;

    public bool SupportsVision { get; set; }

    public int TimeoutSeconds { get; set; } = 60;

    public bool IsActive { get; set; }

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeactivatedAt { get; set; }

    public virtual AiProviderModelCatalog? ModelCatalog { get; set; }
}
```

Se ajustan DTOs en `Application/DTOs/AiProviders`:

```csharp
public sealed class AiProviderConfigurationDto
{
    public int Id { get; set; }

    public int? ModelCatalogId { get; set; }

    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string? Endpoint { get; set; }

    public string ApiProtocol { get; set; } = null!;

    public string ApiKeyEnvironmentVariable { get; set; } = null!;

    public bool IsApiKeyConfigured { get; set; }

    public bool SupportsVision { get; set; }

    public int TimeoutSeconds { get; set; }

    public bool IsActive { get; set; }

    public bool IsEnabled { get; set; }
}

public sealed class AiProviderModelCatalogDto
{
    public int Id { get; set; }

    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string ModelId { get; set; } = null!;

    public string Endpoint { get; set; } = null!;

    public string ApiProtocol { get; set; } = null!;

    public bool SupportsVision { get; set; }

    public bool IsEnabled { get; set; }

    public int SortOrder { get; set; }
}

public sealed class AiProviderRuntimeConfiguration
{
    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string Endpoint { get; set; } = null!;

    public string ApiProtocol { get; set; } = null!;

    public string ApiKeyEnvironmentVariable { get; set; } = null!;

    public bool SupportsVision { get; set; }

    public int TimeoutSeconds { get; set; }
}
```

Se ajusta modelo de vista en `Web/Models`:

```csharp
public class AiProviderConfigurationViewModel
{
    public int Id { get; set; }

    public int? ModelCatalogId { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;

    public string? Endpoint { get; set; }

    public string ApiProtocol { get; set; } = string.Empty;

    public string ApiKeyEnvironmentVariable { get; set; } = string.Empty;

    public bool IsApiKeyConfigured { get; set; }

    public bool SupportsVision { get; set; }

    public int TimeoutSeconds { get; set; } = 60;

    public bool IsActive { get; set; }

    public bool IsEnabled { get; set; } = true;
}
```

Se agregan contratos en `Application/Interfaces`:

```csharp
public interface IAiProviderModelCatalogRepository
{
    Task<IReadOnlyList<AiProviderModelCatalog>> GetEnabledByProviderAsync(string providerName, CancellationToken cancellationToken);

    Task<AiProviderModelCatalog?> GetByIdAsync(int id, CancellationToken cancellationToken);
}

public interface IAiProviderModelCatalogService
{
    Task<IReadOnlyList<AiProviderModelCatalogDto>> GetEnabledByProviderAsync(string providerName, CancellationToken cancellationToken);

    Task<AiProviderModelCatalogDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
```

Convenciones de tabla:

- Tabla SQL nueva: `AiProviderModelCatalog`.
- `AiProviderModelCatalog.ProviderName` usa `OpenCodeGo` para los modelos de OpenCode Go.
- `AiProviderModelCatalog.ModelName` es el nombre visible, por ejemplo `GLM-5.2`.
- `AiProviderModelCatalog.ModelId` es el valor enviado al endpoint, por ejemplo `glm-5.2`.
- `AiProviderConfiguration.ModelName` conserva compatibilidad con el runtime actual y guarda el `ModelId` seleccionado cuando `ProviderName = OpenCodeGo`.
- `AiProviderConfiguration.ModelCatalogId` es nullable para no obligar a migrar proveedores existentes.
- `AiProviderConfiguration.ApiProtocol` define como construir el request runtime.
- Valores permitidos para `ApiProtocol`: `OpenAiChatCompletions` y `AnthropicMessages`.
- Los modelos OpenCode Go iniciales se siembran con `SupportsVision = false`.
- `OpenCodeGo` usa como API key recomendada la variable `OPENCODE_GO_API_KEY`.
- No se guardan precios, limites, consumo, top-ups ni estado de suscripcion.

Catalogo inicial de OpenCode Go:

```text
ProviderName: OpenCodeGo
Endpoint OpenAiChatCompletions: https://opencode.ai/zen/go/v1/chat/completions
Endpoint AnthropicMessages: https://opencode.ai/zen/go/v1/messages
SupportsVision: false para todos los modelos iniciales
```

Modelos iniciales con `OpenAiChatCompletions`:

```text
GLM-5.2 -> glm-5.2
GLM-5.1 -> glm-5.1
Kimi K2.7 Code -> kimi-k2.7-code
Kimi K2.6 -> kimi-k2.6
DeepSeek V4 Pro -> deepseek-v4-pro
DeepSeek V4 Flash -> deepseek-v4-flash
MiMo-V2.5 -> mimo-v2.5
MiMo-V2.5-Pro -> mimo-v2.5-pro
```

Modelos iniciales con `AnthropicMessages`:

```text
MiniMax M3 -> minimax-m3
MiniMax M2.7 -> minimax-m2.7
MiniMax M2.5 -> minimax-m2.5
Qwen3.7 Max -> qwen3.7-max
Qwen3.7 Plus -> qwen3.7-plus
Qwen3.6 Plus -> qwen3.6-plus
```

---

## Plan de implementacion

1. Crear `AiProviderModelCatalog` en `Domain/Entities` y agregar `ModelCatalogId`, `ApiProtocol` y navegacion opcional en `AiProviderConfiguration`; el sistema debe seguir compilando con proveedores existentes porque `ModelCatalogId` sera nullable.
2. Actualizar `ApplicationDbContext` con `DbSet<AiProviderModelCatalog>`, mapeo de tabla, indices, longitudes, relacion opcional con `AiProviderConfiguration` y columna `ApiProtocol`; el modelo EF debe conservar la configuracion actual de `AiProviderConfiguration`.
3. Crear script SQL versionado `Infrastructure/Persistence/StoredProcedures/Spec35_OpenCodeGoModelCatalog.sql` para crear `AiProviderModelCatalog`, agregar columnas nuevas a `AiProviderConfiguration`, sembrar modelos OpenCode Go precargados y dejar todos con `SupportsVision = 0`.
4. Extender DTOs y contratos en `Application/DTOs/AiProviders` y `Application/Interfaces` para exponer `ModelCatalogId`, `ApiProtocol` y consulta de modelos habilitados por proveedor.
5. Implementar `AiProviderModelCatalogRepository` en `Infrastructure/Persistence/Repositories` y `AiProviderModelCatalogService` en `Application/Services`; registrar ambos en DI.
6. Ajustar `AiProviderConfigurationService` para validar `OpenCodeGo` contra `AiProviderModelCatalog`, copiar desde catalogo `ModelId`, `Endpoint`, `ApiProtocol` y `SupportsVision`, y mantener edicion manual para los demas proveedores.
7. Ajustar `AiProviderConfigurationResolver` para devolver `ApiProtocol` en `AiProviderRuntimeConfiguration` y resolver correctamente configuraciones existentes sin catalogo.
8. Extender `AiProvidersController` para incluir `OpenCodeGo` en proveedores soportados, cargar modelos del catalogo, mapear `ModelCatalogId` y entregar un endpoint JSON para obtener modelos habilitados por proveedor.
9. Ajustar vistas `Web/Views/AiProviders/New.cshtml` y `Web/Views/AiProviders/Edit.cshtml` para mostrar dropdown de modelos cuando `ProviderName = OpenCodeGo`, dejar `ModelName`, `Endpoint`, `ApiProtocol` y `SupportsVision` de solo lectura para ese proveedor, y mantener campos manuales para proveedores existentes.
10. Agregar adaptador runtime `OpenCodeGoVisionClient` en `Infrastructure/ArtificialIntelligence` con construccion de request para `OpenAiChatCompletions` y `AnthropicMessages`; el adaptador debe fallar de forma controlada si recibe un `ApiProtocol` no soportado.
11. Registrar `OpenCodeGoVisionClient` en DI junto con los clientes IA existentes; el factory debe poder seleccionarlo cuando `ProviderName = OpenCodeGo`.
12. Ajustar la activacion para que `OpenCodeGo` no pueda activarse mientras el modelo seleccionado tenga `SupportsVision = false`, devolviendo mensaje claro en UI sin romper el listado.
13. Actualizar busqueda, columnas o badges del DataTable de `AiProviders` para mostrar `ApiProtocol` y estado de vision del modelo seleccionado cuando aplique.
14. Agregar o ajustar pruebas existentes del area IA para cubrir validacion de catalogo, bloqueo por `SupportsVision = false`, resolucion de `ApiProtocol` y construccion basica de requests OpenCode Go sin exponer API keys.
15. Ejecutar `dotnet build "TradingBookApp.sln"` y validar manualmente que `AiProviders` permite seleccionar `OpenCodeGo`, elegir un modelo del catalogo, guardar la configuracion y rechazar su activacion por falta de soporte de vision.

---

## Criterios de aceptacion

- [ ] `OpenCodeGo` aparece como proveedor soportado en `AiProviders`.
- [ ] Existe tabla `AiProviderModelCatalog` con modelos OpenCode Go precargados por script SQL versionado.
- [ ] `AiProviderConfiguration` permite `ModelCatalogId` nullable sin romper proveedores existentes.
- [ ] `AiProviderConfiguration` guarda `ApiProtocol` para el runtime.
- [ ] Al seleccionar `OpenCodeGo`, la UI muestra modelos habilitados desde `AiProviderModelCatalog`.
- [ ] Para `OpenCodeGo`, `ModelName`, `Endpoint`, `ApiProtocol` y `SupportsVision` se derivan del modelo seleccionado y no se editan manualmente.
- [ ] Para `OpenAI`, `MiniMax`, `DeepSeek`, `GLM` y `Kimi`, `ModelName` y `Endpoint` siguen siendo editables manualmente.
- [ ] La API key real de OpenCode Go no se guarda ni se muestra en SQL/UI.
- [ ] `OPENCODE_GO_API_KEY` queda como variable/secret recomendada para `OpenCodeGo`.
- [ ] Los modelos OpenCode Go iniciales quedan con `SupportsVision = false`.
- [ ] No se puede activar una configuracion `OpenCodeGo` cuyo modelo seleccionado tenga `SupportsVision = false`.
- [ ] El intento de activar un modelo sin vision muestra un error claro en la UI.
- [ ] El runtime tiene adaptador `OpenCodeGoVisionClient` registrado.
- [ ] `OpenCodeGoVisionClient` soporta `OpenAiChatCompletions` y `AnthropicMessages`.
- [ ] `OpenCodeGoVisionClient` falla de forma controlada ante un `ApiProtocol` no soportado.
- [ ] La seleccion activa desde `IAiVisionClientFactory` funciona para `OpenCodeGo` cuando exista un modelo con vision habilitada.
- [ ] No se guardan precios, limites, consumo, top-ups ni estado de suscripcion.
- [ ] No se sincroniza dinamicamente el endpoint `/models`.
- [ ] `dotnet build "TradingBookApp.sln"` termina sin errores nuevos.

---

## Decisiones tomadas y descartadas

- **Si:** integrar `OpenCodeGo` solo como proveedor/modelos para el asistente IA actual. No se modela la suscripcion completa.
- **Si:** usar catalogo precargado de modelos. Evita carga manual repetitiva y mantiene consistencia con la documentacion inicial.
- **Si:** extender el modulo existente `AiProviders`. Reduce alcance y reutiliza permisos, UI y flujo administrativo actual.
- **Si:** agregar `AiProviderModelCatalog`. Permite varios modelos por proveedor sin romper la unicidad actual de `AiProviderConfiguration.ProviderName`.
- **Si:** agregar `ModelCatalogId` nullable en `AiProviderConfiguration`. Mantiene trazabilidad del modelo seleccionado sin obligar a migrar proveedores existentes.
- **Si:** guardar solo referencia de API key con `OPENCODE_GO_API_KEY`. Mantiene el patron seguro definido en SPEC 33.
- **Si:** soportar `OpenAiChatCompletions` y `AnthropicMessages`. OpenCode Go publica modelos en ambos endpoints.
- **Si:** marcar modelos Go iniciales con `SupportsVision = false`. La documentacion revisada no confirma soporte de vision.
- **Si:** bloquear activacion de `OpenCodeGo` mientras el modelo no soporte vision. El asistente actual depende de imagenes.
- **No:** guardar precios, limites, consumo, top-ups o estado de suscripcion. El usuario confirmo que no le interesa persistir esa informacion.
- **No:** sincronizar modelos dinamicamente desde `/models`. Queda fuera para mantener esta version predecible y acotada.
- **No:** permitir crear o editar modelos Go desde UI. El catalogo inicial sera administrado por script SQL.
- **No:** crear flujo text-only. Seria otro comportamiento del asistente y requiere una spec separada.
- **No:** reemplazar el flujo actual de proveedores existentes. `OpenAI`, `MiniMax`, `DeepSeek`, `GLM` y `Kimi` conservan edicion manual.

---

## Riesgos identificados

| Riesgo | Mitigacion |
|--------|------------|
| OpenCode Go no documenta soporte de vision para los modelos iniciales | Sembrar todos los modelos con `SupportsVision = false` y bloquear activacion hasta confirmacion explicita. |
| El usuario puede esperar que `OpenCodeGo` funcione inmediatamente en el asistente | Mostrar mensaje claro al intentar activar un modelo sin vision. |
| Los protocolos `OpenAiChatCompletions` y `AnthropicMessages` tienen payloads distintos | Implementar ramas separadas en `OpenCodeGoVisionClient` y fallar controladamente si el protocolo no es soportado. |
| El catalogo precargado puede quedar desactualizado frente a OpenCode Go | Dejar sincronizacion dinamica fuera de esta spec y actualizar el catalogo por script versionado cuando sea necesario. |
| Agregar `ModelCatalogId` puede afectar proveedores existentes si se fuerza la relacion | Mantener `ModelCatalogId` nullable y preservar edicion manual para proveedores actuales. |
| El runtime puede registrar datos sensibles en errores de proveedor | Mantener el patron actual de logs sanitizados sin API keys, headers, prompts completos ni imagenes. |

---

## Lo que **no** esta en esta spec

- Guardar precios, limites de uso, consumo, top-ups o estado de suscripcion de OpenCode Go.
- Sincronizar dinamicamente modelos desde `https://opencode.ai/zen/go/v1/models`.
- Crear o editar modelos OpenCode Go desde la UI.
- Activar `OpenCodeGo` mientras el modelo seleccionado no soporte vision.
- Crear un flujo text-only separado para el asistente IA.
- Guardar, mostrar, revelar, copiar o rotar API keys reales desde la aplicacion.
- Cambiar prompts, schemas, reglas, score, metricas o flujo funcional del asistente.
- Crear permisos nuevos, menus nuevos o modulos administrativos nuevos.
- Reemplazar el comportamiento actual de `OpenAI`, `MiniMax`, `DeepSeek`, `GLM` y `Kimi`.
