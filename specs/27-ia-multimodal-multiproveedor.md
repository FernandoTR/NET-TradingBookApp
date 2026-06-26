# SPEC 27 - IA multimodal multi-proveedor

> **Estado:** Implementado - **Depende de:** SPEC 25, SPEC 26 - **Fecha:** 2026-06-25
> **Objetivo:** Implementar una abstraccion multimodal que permita cambiar el proveedor y modelo IA desde configuracion, con adaptadores explicitos para OpenAI, MiniMax, DeepSeek, GLM y Kimi.

---

## Alcance

**Incluye:**

- Crear la interfaz `IAiVisionClient` en `Application`.
- Crear un selector de proveedor que resuelva el adaptador activo desde configuracion.
- Crear adaptadores explicitos para OpenAI, MiniMax, DeepSeek, GLM y Kimi.
- Configurar proveedor activo y modelo activo desde `appsettings.{Environment}.json`.
- Leer API keys desde variables de entorno o user secrets, no desde `appsettings.json` versionado.
- Enviar imagenes temporales del SPEC 26 al proveedor activo.
- Exigir respuesta JSON estricta segun schema versionado.
- Manejar errores de proveedor, timeouts, JSON invalido y modelos sin capacidad multimodal.

**Fuera de alcance:**

- Guardar imagenes o requests con imagenes.
- Calcular reglas, RR, score o convergencias.
- Permitir SQL generado por IA.
- Crear pantalla administrativa para cambiar proveedor.
- Fine-tuning o entrenamiento de modelos.
- Chat de seguimiento.

---

## Data model

Se agrega interfaz en `Application/Interfaces`:

```csharp
public interface IAiVisionClient
{
    string ProviderName { get; }
    string ModelName { get; }

    Task<AiVisionExtractionDto> ExtractSetupAsync(
        CreateAiValidationDto request,
        IReadOnlyList<AiValidationImageInputDto> images,
        CancellationToken cancellationToken);
}
```

Se agrega selector en `Application/Interfaces`:

```csharp
public interface IAiVisionClientFactory
{
    IAiVisionClient CreateActiveClient();
}
```

Se agregan opciones no sensibles:

```csharp
public sealed class AiProviderOptions
{
    public string ActiveProvider { get; set; } = null!;
    public string ActiveModel { get; set; } = null!;
    public Dictionary<string, AiProviderDefinition> Providers { get; set; } = new();
}

public sealed class AiProviderDefinition
{
    public string Model { get; set; } = null!;
    public string? Endpoint { get; set; }
    public string ApiKeyEnvironmentVariable { get; set; } = null!;
    public bool SupportsVision { get; set; }
    public int TimeoutSeconds { get; set; } = 60;
}
```

Configuracion conceptual en `appsettings.{Environment}.json`:

```json
{
  "Ai": {
    "ActiveProvider": "OpenAI",
    "ActiveModel": "gpt-4.1-mini",
    "Providers": {
      "OpenAI": {
        "Model": "gpt-4.1-mini",
        "Endpoint": "https://api.openai.com/v1/responses",
        "ApiKeyEnvironmentVariable": "OPENAI_API_KEY",
        "SupportsVision": true
      },
      "MiniMax": {
        "Model": "minimax-2.7",
        "Endpoint": "__CHANGE_ME__",
        "ApiKeyEnvironmentVariable": "MINIMAX_API_KEY",
        "SupportsVision": true
      },
      "DeepSeek": {
        "Model": "deepseek-v4",
        "Endpoint": "__CHANGE_ME__",
        "ApiKeyEnvironmentVariable": "DEEPSEEK_API_KEY",
        "SupportsVision": true
      },
      "GLM": {
        "Model": "glm-5.2",
        "Endpoint": "__CHANGE_ME__",
        "ApiKeyEnvironmentVariable": "GLM_API_KEY",
        "SupportsVision": true
      },
      "Kimi": {
        "Model": "kimi-k2.7",
        "Endpoint": "__CHANGE_ME__",
        "ApiKeyEnvironmentVariable": "KIMI_API_KEY",
        "SupportsVision": true
      }
    }
  }
}
```

Se agregan clases en `Infrastructure/ArtificialIntelligence`:

- `OpenAiVisionClient`.
- `MiniMaxVisionClient`.
- `DeepSeekVisionClient`.
- `GlmVisionClient`.
- `KimiVisionClient`.
- `AiVisionClientFactory`.
- `PromptTemplateProvider`.
- `AiStructuredOutputSchemaProvider`.

---

## Plan de implementacion

**Paso 1 - Crear contratos neutrales**

1. Agregar `IAiVisionClient` y `IAiVisionClientFactory` en `Application/Interfaces`.
2. Asegurar que los contratos no dependan de OpenAI ni de librerias de un proveedor.
3. Compilar para validar.

**Paso 2 - Crear schema y prompt versionados**

1. Crear `PromptTemplateProvider` con version inicial `trade-validation-v1`.
2. Crear `AiStructuredOutputSchemaProvider` con version inicial `ai-trade-validation-schema-v1`.
3. Exigir JSON estricto sin texto fuera del JSON.
4. Compilar para validar.

**Paso 3 - Crear opciones de configuracion**

1. Agregar `AiProviderOptions` y `AiProviderDefinition`.
2. Configurar `Ai` en `Web/appsettings.json` con placeholders no sensibles.
3. Documentar que las API keys van en variables de entorno o user secrets.
4. Compilar para validar.

**Paso 4 - Crear adaptadores explicitos**

1. Implementar `OpenAiVisionClient`.
2. Implementar `MiniMaxVisionClient`.
3. Implementar `DeepSeekVisionClient`.
4. Implementar `GlmVisionClient`.
5. Implementar `KimiVisionClient`.
6. Cada adaptador debe mapear request comun a request especifica del proveedor.
7. Cada adaptador debe deserializar a `AiVisionExtractionDto`.
8. Compilar despues de cada adaptador.

**Paso 5 - Crear factory de proveedor activo**

1. Implementar `AiVisionClientFactory`.
2. Resolver `ActiveProvider` desde configuracion.
3. Validar que `ActiveModel` coincide con la definicion activa o aplicar la prioridad configurada.
4. Rechazar proveedor inexistente con error claro.
5. Rechazar proveedor sin `SupportsVision`.
6. Compilar para validar.

**Paso 6 - Registrar dependencias**

1. Registrar opciones con `IOptions<AiProviderOptions>`.
2. Registrar `HttpClient` por proveedor usando `IHttpClientFactory`.
3. Registrar todos los adaptadores y la factory en `Infrastructure/DependencyInjection.cs`.
4. Compilar para validar.

**Paso 7 - Manejar errores de proveedor**

1. Normalizar errores de timeout, 401, 429, 5xx y JSON invalido.
2. Registrar el proveedor y modelo usados, sin registrar imagenes ni API keys.
3. No crear `AiTradeValidation` cuando la extraccion no sea completa.
4. Compilar para validar.

**Paso 8 - Verificacion final**

1. Ejecutar `dotnet build "TradingBookApp.sln"`.
2. Cambiar `Ai:ActiveProvider` entre los proveedores configurados.
3. Confirmar que la app usa el adaptador esperado sin cambios de codigo.
4. Confirmar que una API key faltante produce error controlado.

---

## Criterios de aceptacion

- [ ] Existe `IAiVisionClient` neutral en `Application`.
- [ ] Existe `IAiVisionClientFactory` para resolver el proveedor activo.
- [ ] Existe adaptador explicito para OpenAI.
- [ ] Existe adaptador explicito para MiniMax.
- [ ] Existe adaptador explicito para DeepSeek.
- [ ] Existe adaptador explicito para GLM.
- [ ] Existe adaptador explicito para Kimi.
- [ ] El proveedor activo se cambia desde `appsettings.{Environment}.json`.
- [ ] Las API keys no estan en `appsettings.json` versionado.
- [ ] Un proveedor sin vision configurada se rechaza antes de enviar imagenes.
- [ ] La respuesta del proveedor se valida como JSON estricto.
- [ ] Si el proveedor falla, no se guarda validacion de negocio.
- [ ] Los logs no contienen imagenes ni secretos.
- [ ] `dotnet build "TradingBookApp.sln"` termina sin errores.

---

## Decisiones tomadas y descartadas

- **Si:** proveedor y modelo por configuracion. Permite cambiar de OpenAI a MiniMax, DeepSeek, GLM o Kimi sin tocar codigo de negocio.
- **Si:** API keys en variables de entorno o user secrets. Es mas seguro que versionarlas en `appsettings.json`.
- **Si:** adaptadores explicitos desde el inicio. Evita acoplar el sistema a un formato unico de OpenAI.
- **Si:** `Application` solo conoce contratos neutrales. Respeta Clean Architecture.
- **No:** pantalla administrativa para proveedores. Aumenta riesgo de exponer secretos y no fue requerida.
- **No:** prompts libres por usuario. El prompt debe estar versionado y controlado.
- **No:** permitir texto fuera del JSON. Complica normalizacion y abre espacio a interpretaciones ambiguas.

---

## Riesgos identificados

| Riesgo | Mitigacion |
|--------|------------|
| Algunos modelos configurados pueden no soportar imagenes en la practica | Validar `SupportsVision` y documentar endpoint/modelo por proveedor |
| Cada proveedor usa formatos distintos para imagenes y schema | Encapsular diferencias dentro de cada adaptador |
| Las respuestas JSON pueden variar aunque se pida schema estricto | Validar schema y rechazar respuestas invalidas |
| Cambiar proveedor puede cambiar calidad visual | Guardar proveedor, modelo, prompt y schema en cada validacion completada |
| Logs pueden exponer secrets por error | Nunca registrar headers, API keys ni payloads con imagenes |

---

## Lo que **no** esta en esta spec

- Reglas deterministicas.
- Score.
- Convergencias.
- Persistencia de imagenes.
- UI de configuracion de proveedores.
- Fine-tuning.
- Chat de seguimiento.
- Creacion de ordenes.
