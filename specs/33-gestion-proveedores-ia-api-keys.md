# SPEC 33 - Gestion de proveedores IA y referencias de API keys

> **Estado:** Implementado - **Depende de:** SPEC 27 - **Fecha:** 2026-06-30
> **Objetivo:** Crear un modulo administrativo para configurar proveedores IA activos y sus referencias de API keys sin almacenar secretos reales en SQL.

---

## Alcance

**Incluye:**

- Crear un modulo MVC administrativo para listar, crear, editar, activar y desactivar proveedores IA.
- Persistir la configuracion de proveedores en SQL con una fila por proveedor.
- Manejar un solo proveedor activo global para el asistente de validacion IA.
- Guardar modelo, endpoint, timeout, soporte de vision y nombre de variable/secret que contiene la API key.
- Mostrar estado de API key como configurada/no configurada usando presencia de la variable de entorno, sin mostrar su valor.
- Hacer que la BD sea la fuente de verdad y usar `appsettings.json` solo como fallback cuando no existan registros en BD.
- Limitar los proveedores creados a adaptadores soportados por codigo: OpenAI, MiniMax, DeepSeek, GLM y Kimi.
- Agregar permiso `AiProviders` y entrada de menu protegida por roles.
- Registrar auditoria de creacion, actualizacion, activacion y desactivacion sin incluir secretos.
- Crear script SQL versionado para tabla, datos iniciales y menu.

**Fuera de alcance:**

- Guardar API keys reales en SQL.
- Mostrar, copiar, revelar o rotar API keys desde la UI.
- Modificar variables de entorno, user secrets o archivos `appsettings.*.json` desde la aplicacion.
- Probar conexion contra proveedores IA en esta version.
- Crear adaptadores nuevos o soportar proveedores arbitrarios sin codigo.
- Cambiar prompts, schemas, reglas, score, metricas o flujo del asistente.
- Fine-tuning, entrenamiento o seleccion automatica de proveedor.

---

## Data model

Se agrega entidad en `Domain/Entities`:

```csharp
public partial class AiProviderConfiguration
{
    public int Id { get; set; }
    public string ProviderName { get; set; } = null!;
    public string ModelName { get; set; } = null!;
    public string? Endpoint { get; set; }
    public string ApiKeyEnvironmentVariable { get; set; } = null!;
    public bool SupportsVision { get; set; }
    public int TimeoutSeconds { get; set; } = 60;
    public bool IsActive { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
}
```

Convenciones de tabla:

- Tabla SQL: `AiProviderConfiguration`.
- `ProviderName` es unico y debe coincidir con un adaptador soportado: `OpenAI`, `MiniMax`, `DeepSeek`, `GLM`, `Kimi`.
- Solo puede existir una fila con `IsActive = 1`.
- Un proveedor con `IsEnabled = 0` no puede activarse ni usarse por el asistente.
- `ApiKeyEnvironmentVariable` guarda el nombre de la variable o secret, no el valor.

Se agregan DTOs en `Application/DTOs/AiProviders`:

```csharp
public sealed class AiProviderConfigurationDto
{
    public int Id { get; set; }
    public string ProviderName { get; set; } = null!;
    public string ModelName { get; set; } = null!;
    public string? Endpoint { get; set; }
    public string ApiKeyEnvironmentVariable { get; set; } = null!;
    public bool IsApiKeyConfigured { get; set; }
    public bool SupportsVision { get; set; }
    public int TimeoutSeconds { get; set; }
    public bool IsActive { get; set; }
    public bool IsEnabled { get; set; }
}

public sealed class AiProviderRuntimeConfiguration
{
    public string ProviderName { get; set; } = null!;
    public string ModelName { get; set; } = null!;
    public string Endpoint { get; set; } = null!;
    public string ApiKeyEnvironmentVariable { get; set; } = null!;
    public bool SupportsVision { get; set; }
    public int TimeoutSeconds { get; set; }
}
```

Se agregan interfaces en `Application/Interfaces`:

```csharp
public interface IAiProviderConfigurationService
{
    Task<IReadOnlyList<AiProviderConfigurationDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AiProviderConfigurationDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> CreateAsync(AiProviderConfigurationDto provider, string userId, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(AiProviderConfigurationDto provider, string userId, CancellationToken cancellationToken);
    Task<bool> ActivateAsync(int id, string userId, CancellationToken cancellationToken);
    Task<bool> DeactivateAsync(int id, string userId, CancellationToken cancellationToken);
}

public interface IAiProviderConfigurationResolver
{
    Task<AiProviderRuntimeConfiguration> GetActiveAsync(CancellationToken cancellationToken);
}
```

Se ajusta el contrato runtime de SPEC 27 para recibir configuracion activa desde BD/fallback:

```csharp
public interface IAiVisionClient
{
    string ProviderName { get; }
    string PromptVersion { get; }
    string SchemaVersion { get; }

    Task<AiVisionExtractionDto> ExtractSetupAsync(
        CreateAiValidationDto request,
        IReadOnlyList<AiValidationImageInputDto> images,
        AiProviderRuntimeConfiguration configuration,
        CancellationToken cancellationToken);
}

public interface IAiVisionClientFactory
{
    Task<AiVisionClientSelection> CreateActiveClientAsync(CancellationToken cancellationToken);
}

public sealed class AiVisionClientSelection
{
    public IAiVisionClient Client { get; set; } = null!;
    public AiProviderRuntimeConfiguration Configuration { get; set; } = null!;
}
```

`ProviderName` queda como clave del adaptador, y `ModelName`, endpoint, timeout y API key se leen desde `AiProviderRuntimeConfiguration`.

El modulo Web usa `AiProviderConfigurationViewModel` sin campo para el valor real de la API key.

---

## Plan de implementacion

**Paso 1 - Crear persistencia y permiso**

1. Agregar `AiProviderConfiguration` en `Domain/Entities`.
2. Agregar `DbSet<AiProviderConfiguration>` y mapeo en `ApplicationDbContext`.
3. Agregar `AiProviders = 21` en `Domain/Enums/Permissions.cs`.
4. Crear script SQL versionado para tabla `AiProviderConfiguration`, indices, seed de los cinco proveedores actuales y entrada de menu.
5. Compilar para validar.

**Paso 2 - Crear contratos de aplicacion**

1. Agregar DTOs `AiProviderConfigurationDto` y `AiProviderRuntimeConfiguration`.
2. Agregar `IAiProviderConfigurationRepository`, `IAiProviderConfigurationService` y `IAiProviderConfigurationResolver`.
3. Actualizar `IAiVisionClient` y `IAiVisionClientFactory` para usar configuracion runtime asincronica.
4. Compilar para validar.

**Paso 3 - Implementar repositorio y servicio administrativo**

1. Crear `AiProviderConfigurationRepository` en `Infrastructure/Persistence/Repositories`.
2. Crear `AiProviderConfigurationService` en `Application/Services`.
3. Validar que `ProviderName` pertenezca a los adaptadores soportados.
4. Bloquear activacion si el proveedor esta desactivado, no soporta vision, no tiene modelo o no tiene endpoint absoluto.
5. En `ActivateAsync`, apagar cualquier activo anterior y dejar solo uno activo.
6. Registrar `ActivityLog` en crear, actualizar, activar y desactivar sin incluir secretos.
7. Compilar para validar.

**Paso 4 - Implementar resolver runtime con fallback**

1. Crear `AiProviderConfigurationResolver`.
2. Resolver primero el proveedor activo desde SQL.
3. Si la tabla esta vacia o no hay activo, construir configuracion desde `AiProviderOptions` como fallback.
4. Validar endpoint, modelo, soporte de vision y nombre de variable de API key antes de devolver configuracion.
5. Calcular estado de API key con `Environment.GetEnvironmentVariable` solo como booleano para UI/validacion.
6. Compilar para validar.

**Paso 5 - Refactorizar adaptadores IA**

1. Cambiar `AiVisionClientFactory` a `CreateActiveClientAsync`.
2. Cambiar `AiVisionClientBase` para usar `AiProviderRuntimeConfiguration` en cada request.
3. Enviar el request al endpoint de la configuracion activa y aplicar timeout por request.
4. Leer la API key desde `configuration.ApiKeyEnvironmentVariable`.
5. Mantener logs con proveedor/modelo/error sin headers, payloads de imagen ni secretos.
6. Actualizar `TradeValidationOrchestrator` para usar `AiVisionClientSelection`.
7. Compilar para validar.

**Paso 6 - Crear modulo MVC**

1. Crear `AiProvidersController` con `Index`, `JsonDataTable`, `New`, `Edit`, `Save`, `Update`, `Activate` y `Deactivate`.
2. Proteger acciones con `Permissions.AiProviders` siguiendo el patron de catalogos existentes.
3. Crear `AiProviderConfigurationViewModel` sin campo para API key real.
4. Crear vistas `Index`, `New` y `Edit` con DataTables y acciones de activar/desactivar.
5. Mostrar estado de API key como `Configurada` o `No configurada` sin mostrar valor.
6. Compilar para validar.

**Paso 7 - Registrar dependencias y limpiar configuracion**

1. Registrar servicio, resolver y repositorio en `Application/DependencyInjection.cs` e `Infrastructure/DependencyInjection.cs` segun su capa.
2. Mantener `AiProviderOptions` y `Web/appsettings.json` como fallback documentado.
3. Ajustar registros de `HttpClient` para que no dependan del endpoint estatico de `appsettings`.
4. Compilar para validar.

**Paso 8 - Verificacion final**

1. Ejecutar `dotnet build "TradingBookApp.sln"`.
2. Ejecutar la app con perfil HTTPS.
3. Confirmar que el menu aparece solo para roles con permiso `AiProviders`.
4. Crear o editar un proveedor sin capturar API key real.
5. Activar un proveedor y confirmar que los demas quedan inactivos.
6. Desactivar un proveedor y confirmar que no puede usarse si queda inhabilitado.
7. Confirmar que el asistente usa el proveedor/modelo activos desde BD.
8. Confirmar que, sin registros en BD, el asistente sigue usando `appsettings.json` como fallback.

---

## Criterios de aceptacion

- [ ] Existe tabla/entidad `AiProviderConfiguration` con una fila por proveedor.
- [ ] `ProviderName` solo acepta adaptadores soportados por codigo.
- [ ] Solo puede existir un proveedor activo global.
- [ ] Un proveedor desactivado no puede activarse ni usarse por el asistente.
- [ ] El modulo permite listar, crear, editar, activar y desactivar proveedores IA.
- [ ] El modulo nunca solicita ni guarda el valor real de una API key.
- [ ] La UI muestra unicamente el nombre de la variable/secret de API key y su estado configurada/no configurada.
- [ ] La configuracion activa se resuelve desde SQL cuando hay datos validos.
- [ ] Si SQL no tiene proveedores configurados, el runtime usa `AiProviderOptions` como fallback.
- [ ] `TradeValidationOrchestrator` usa el proveedor/modelo activo resuelto por el nuevo factory asincronico.
- [ ] Los logs de error y actividad no contienen API keys, headers ni payloads de imagen.
- [ ] Existe permiso `AiProviders = 21` y entrada de menu protegida por roles.
- [ ] Las operaciones administrativas generan `ActivityLog` sin secretos.
- [ ] Existe script SQL versionado para tabla, indices, datos iniciales y menu.
- [ ] `dotnet build "TradingBookApp.sln"` termina sin errores.

---

## Decisiones tomadas y descartadas

- **Si:** crear un SPEC nuevo dependiente de SPEC 27. SPEC 27 dejo explicitamente fuera la pantalla administrativa de proveedores.
- **Si:** guardar solo referencias de API key. Reduce riesgo porque SQL no contiene secretos reales.
- **Si:** usar SQL como fuente de verdad con fallback a `AiProviderOptions`. Permite administrar desde UI sin romper el arranque actual.
- **Si:** una fila por proveedor. Mantiene compatibilidad con el modelo actual de `AiProviderDefinition`.
- **Si:** desactivar en lugar de borrar. Conserva historico y evita romper trazabilidad de proveedor/modelo.
- **Si:** permiso nuevo `AiProviders`. Encaja con el esquema actual de menus y roles.
- **Si:** limitar proveedores a adaptadores existentes. Evita crear configuraciones que el runtime no puede ejecutar.
- **No:** guardar API keys cifradas en SQL. Agrega gestion de llaves de cifrado y rotacion que no se necesita en esta version.
- **No:** prueba de conexion en esta version. Fue descartada para mantener el alcance en CRUD administrativo.
- **No:** editar archivos `appsettings.*.json` desde la aplicacion. Esos archivos estan excluidos de git y no deben ser mutados por el modulo web.
- **No:** proveedores arbitrarios sin adaptador. El formato de request/respuesta cambia por proveedor y debe vivir en codigo.

---

## Riesgos identificados

| Riesgo | Mitigacion |
|--------|------------|
| La BD queda sin proveedor activo valido | Usar fallback a `AiProviderOptions` solo cuando no existan registros validos y mostrar error claro en UI. |
| Se activa un proveedor sin API key configurada en el entorno | Mostrar estado `No configurada` y bloquear uso o devolver error controlado antes de llamar al proveedor. |
| Un usuario espera pegar la API key en la UI | Explicar en la pantalla que el modulo guarda el nombre de la variable/secret, no el valor. |
| El refactor de `IAiVisionClient` puede romper el flujo de validacion IA | Compilar despues del refactor y validar manualmente una extraccion con proveedor activo. |
| El script de menu puede usar un `ApplicationId` distinto en otra base | Dejar `@ApplicationId` ajustable siguiendo el patron de `Insert_Menu_AnalyticsConvergence.sql`. |
| Un proveedor creado con adaptador incorrecto no puede ejecutarse | Validar `ProviderName` contra la lista cerrada de adaptadores soportados. |

---

## Lo que **no** esta en esta spec

- Guardar API keys reales en SQL.
- Cifrar y rotar secretos desde la aplicacion.
- Mostrar o revelar API keys en pantalla.
- Editar variables de entorno, user secrets o archivos `appsettings.*.json`.
- Probar conexion contra proveedores IA.
- Agregar adaptadores nuevos.
- Soportar proveedores arbitrarios sin codigo.
- Cambiar prompts, schemas, reglas, score o metricas.
- Fine-tuning o entrenamiento.
- Seleccion automatica del mejor proveedor.
