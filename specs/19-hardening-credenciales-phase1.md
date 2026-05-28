# SPEC 19 — Hardening de Credenciales (Phase 1)

> **Estado:** Implementado · **Depende de:** Ninguno · **Fecha:** 2026-05-28
> **Objetivo:** Eliminar credenciales hardcodeadas de los archivos fuente usando un archivo de configuración separado `config/secrets.json`.

---

## Alcance

**Incluye:**

- Crear `config/secrets.json` (excluido de git) con todas las credenciales sensibles
- Crear `config/secrets.template.json` como ejemplo con valores placeholder `__CHANGE_ME__`
- Modificar `Infrastructure/DependencyInjection.cs` para cargar `config/secrets.json` al iniciar
- Eliminar `Persist Security Info=True` de todas las connection strings
- Eliminar `TrustServerCertificate=True` de todas las connection strings
- Dejar placeholders `__CHANGE_ME__` en `appsettings.json` y `appsettings.Development.json` como documentación
- Actualizar `.gitignore` para excluir `config/secrets.json`

**Fuera de alcance:**

- Modificar los archivos `.csproj` para copiar el archivo en build
- Implementar Azure Key Vault, AWS Secrets Manager, o HashiCorp Vault
- Cambiar las credenciales reales (el usuario debe regenerarlas fuera de la app)

---

## Data model

Esta funcionalidad no introduce nuevas estructuras de datos.

**Archivo nuevo `config/secrets.json`:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=...;Initial Catalog=...;User ID=sa;Password=...;Encrypt=true;TrustServerCertificate=false;"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.office365.com",
    "SmtpPort": 587,
    "SmtpSsl": true,
    "SmtpUser": "correo@ejemplo.com",
    "SmtpPassword": "contraseña_real"
  },
  "AppSettings": {
    "ServerAddress": "https://produccion.com",
    "EmailFrom": "correo@ejemplo.com",
    "EmailSupport": "soporte@ejemplo.com",
    "EmailSupportProvider": "soporte@ejemplo.com",
    "Protocolo": "https",
    "IsQUA": false
  }
}
```

**Archivo nuevo `config/secrets.template.json`:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=__CHANGE_ME__;Initial Catalog=__CHANGE_ME__;User ID=__CHANGE_ME__;Password=__CHANGE_ME__;Encrypt=true;"
  },
  "EmailSettings": {
    "SmtpServer": "__CHANGE_ME__",
    "SmtpPort": 587,
    "SmtpSsl": true,
    "SmtpUser": "__CHANGE_ME__",
    "SmtpPassword": "__CHANGE_ME__"
  },
  "AppSettings": {
    "ServerAddress": "__CHANGE_ME__",
    "EmailFrom": "__CHANGE_ME__",
    "EmailSupport": "__CHANGE_ME__",
    "EmailSupportProvider": "__CHANGE_ME__",
    "Protocolo": "https",
    "IsQUA": false
  }
}
```

---

## Plan de implementación

**Paso 1 — Crear estructura de archivos**

1. Crear directorio `config/` en la raíz del proyecto
2. Crear `config/secrets.template.json` con todos los campos placeholder `__CHANGE_ME__`
3. Agregar `config/secrets.json` a `.gitignore`

**Paso 2 — Modificar DependencyInjection**

1. En `Infrastructure/DependencyInjection.cs`, agregar antes del registro de DbContext:
   ```csharp
   builder.Configuration.AddJsonFile("config/secrets.json", optional: true, reloadOnChange: true);
   ```
2. Cambiar la connection string para usar `Encrypt=true` en lugar de `TrustServerCertificate=True`
3. Eliminar `Persist Security Info=True` de la connection string

**Paso 3 — Actualizar archivos de configuración**

1. En `appsettings.json`: reemplazar valores reales por `__CHANGE_ME__` en connection string y EmailSettings
2. En `appsettings.Development.json`: reemplazar valores reales por `__CHANGE_ME__`
3. Mantener las keys/estructura para no romper la configuración existente

**Paso 4 — Documentación**

1. Agregar comentario en `appsettings.json` y `appsettings.Development.json` indicando que las credenciales van en `config/secrets.json`

---

## Criterios de aceptación

- [ ] `config/secrets.template.json` existe con todos los campos placeholder `__CHANGE_ME__`
- [ ] `config/secrets.json` está excluido de git (no aparece en `git status`)
- [ ] `Infrastructure/DependencyInjection.cs` carga `config/secrets.json` al iniciar
- [ ] `appsettings.json` y `appsettings.Development.json` no contienen passwords ni credenciales reales
- [ ] Connection strings usan `Encrypt=true` en lugar de `TrustServerCertificate=True`
- [ ] Connection strings no tienen `Persist Security Info=True`
- [ ] `dotnet build "TradingBookApp.sln"` compila sin errores

---

## Decisiones tomadas y descartadas

- **Sí:** Usar `config/secrets.json` separado. El proyecto ya tiene `appsettings.Development.json` como capa de configuración. Un archivo adicional es consistente con el patrón existente y no requiere cambios de infraestructura.
- **Sí:** `Encrypt=true` en lugar de `TrustServerCertificate=True`. Encrypt habilita TLS correctamente sin confiar en certificados auto-firmados.
- **No:** Azure Key Vault / AWS Secrets Manager. Requiere cuenta cloud y está fuera del alcance de esta phase. La solución de archivo local es práctica para este proyecto.
- **No:** Modificar `.csproj` para copiar `config/secrets.json` al output. No es necesario - el archivo se mantiene en la raíz y se lee directamente.

---

## Qué **no** está en esta spec

- Regenerar las credenciales reales (el usuario lo hace manualmente fuera de la app)
- Implementar encrypted secrets o vault de cualquier tipo
- Modificar la arquitectura de configuración del proyecto (solo se añade una capa)