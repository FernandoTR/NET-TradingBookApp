# SPEC 26 - Carga temporal segura de imagenes para IA

> **Estado:** Implementado - **Depende de:** SPEC 25 - **Fecha:** 2026-06-25
> **Objetivo:** Permitir que el usuario cargue imagenes de graficos solo durante la request de validacion, validarlas de forma segura y descartarlas despues de enviarlas al proveedor IA.

---

## Alcance

**Incluye:**

- Aceptar entre 1 y 4 imagenes para el MVP.
- Validar extension, MIME declarado, firma real del archivo, tamaño por archivo y tamaño total.
- Rechazar SVG y formatos no permitidos.
- Capturar rol, temporalidad, orden y comentario opcional por imagen durante la request.
- Entregar las imagenes al cliente IA como streams o bytes temporales.
- Descartar las imagenes al terminar la request, exista exito o error.
- Registrar errores tecnicos sin incluir contenido de imagen, rutas fisicas ni base64.

**Fuera de alcance:**

- Guardar imagenes en disco, base de datos, cloud storage o `wwwroot`.
- Guardar rutas, hashes o metadatos persistentes de imagenes.
- Soportar mas de 4 imagenes.
- Redimensionamiento avanzado, OCR propio o procesamiento visual fuera del proveedor IA.
- Crear adaptadores IA.
- Crear pantallas finales de resultado.

---

## Data model

Esta spec no agrega entidades persistentes.

Se agregan DTOs temporales en `Application/DTOs/AiValidation`:

```csharp
public sealed class AiValidationImageInputDto
{
    public string OriginalFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public string FrameCode { get; set; } = null!;
    public TradingImageRole ImageRole { get; set; }
    public int SortOrder { get; set; }
    public string? Comment { get; set; }
    public Stream Content { get; set; } = null!;
}
```

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

Se agregan opciones de configuracion no sensibles en `Web/appsettings.json`:

```json
{
  "AiTradeValidation": {
    "MaxImagesPerValidation": 4,
    "MaxImageSizeMb": 8,
    "MaxTotalUploadMb": 32,
    "AllowedContentTypes": ["image/jpeg", "image/png", "image/webp"]
  }
}
```

No se agrega `ITradingImageStorage` porque no habra almacenamiento de imagenes.

---

## Plan de implementacion

**Paso 1 - Crear opciones y enum de imagen**

1. Agregar `TradingImageRole` en `Domain/Enums`.
2. Agregar clase de opciones `AiTradeValidationOptions` en `Application` o `Web` segun el patron existente.
3. Configurar limites iniciales en `Web/appsettings.json`.
4. Compilar para validar.

**Paso 2 - Crear DTO temporal de imagen**

1. Agregar `AiValidationImageInputDto` en `Application/DTOs/AiValidation`.
2. Usar `Stream` o arreglo de bytes temporal sin campos persistentes.
3. Compilar para validar.

**Paso 3 - Crear validador de archivos**

1. Agregar interfaz `IAiValidationImageValidator` en `Application/Interfaces`.
2. Implementar validacion concreta en `Web` o `Infrastructure` sin guardar archivos.
3. Validar cantidad, tamanio individual, tamanio total, MIME y firma real.
4. Rechazar SVG aunque el MIME declarado parezca valido.
5. Compilar para validar.

**Paso 4 - Integrar con el request MVC**

1. Crear view model de entrada con `IFormFile` solo en `Web`.
2. Convertir `IFormFile` a `AiValidationImageInputDto` temporal.
3. Asegurar `await using` o disposicion equivalente de streams temporales.
4. Evitar escribir en `wwwroot`, `Web/logs`, `Temp` propio o base de datos.
5. Compilar para validar.

**Paso 5 - Manejo de errores**

1. Devolver mensajes de validacion claros cuando un archivo no sea aceptado.
2. Registrar errores con Serilog sin contenido de imagen ni base64.
3. Confirmar que si falla la validacion de imagen no se llama al proveedor IA.
4. Compilar para validar.

**Paso 6 - Verificacion final**

1. Ejecutar `dotnet build "TradingBookApp.sln"`.
2. Probar manualmente una imagen JPEG valida.
3. Probar manualmente un SVG rechazado.
4. Probar manualmente mas de 4 imagenes rechazadas.
5. Confirmar que no aparecen archivos nuevos de imagen en el repo ni en rutas de la app.

---

## Criterios de aceptacion

- [ ] El formulario acepta minimo 1 imagen y maximo 4 imagenes.
- [ ] Cada imagen requiere rol, temporalidad y orden.
- [ ] JPEG, PNG y WebP son los unicos formatos permitidos.
- [ ] SVG es rechazado siempre.
- [ ] El tamanio individual maximo es configurable.
- [ ] El tamanio total maximo es configurable.
- [ ] La validacion revisa firma real del archivo, no solo extension.
- [ ] Las imagenes se descartan al terminar la request.
- [ ] No se crean archivos persistentes de imagen.
- [ ] No se guardan rutas, hashes ni metadatos persistentes de imagen.
- [ ] Los logs no contienen imagenes en base64 ni rutas fisicas.
- [ ] `dotnet build "TradingBookApp.sln"` termina sin errores.

---

## Decisiones tomadas y descartadas

- **Si:** procesar imagenes solo durante la request. Cumple la decision de no almacenar imagenes.
- **Si:** limitar el MVP a 4 imagenes. Reduce memoria, costo y complejidad del primer flujo.
- **Si:** validar firma real. Evita confiar solo en extension o MIME enviado por el navegador.
- **No:** crear `TradingImageStorage`. No hay almacenamiento que abstraer.
- **No:** guardar hash persistente. El usuario no quiere conservar rastros de los archivos cargados.
- **No:** aceptar SVG. Es innecesario para graficos y aumenta superficie de ataque.

---

## Riesgos identificados

| Riesgo | Mitigacion |
|--------|------------|
| Cargar varias imagenes grandes puede aumentar memoria | Limites estrictos por archivo y por request |
| ASP.NET puede usar buffering temporal interno para uploads grandes | Mantener tamanios bajos y no crear almacenamiento persistente propio |
| Un archivo malicioso puede declarar MIME falso | Validar firma real antes de enviarlo al proveedor IA |
| Logs accidentales pueden exponer datos de imagen | Prohibir logging de base64, rutas y contenido de archivo |

---

## Lo que **no** esta en esta spec

- Persistencia de imagenes.
- Almacenamiento en disco o cloud.
- Hashes persistentes.
- Analisis IA.
- Normalizacion de catalogos.
- Reglas de estrategia.
- Interfaz de resultados.
- Creacion de ordenes.
