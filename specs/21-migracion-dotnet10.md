# SPEC 21 — Migración a .NET 10

> **Estado:** Aprobado · **Depende de:** Ninguno · **Fecha:** 2026-05-28
> **Objetivo:** Actualizar la solución completa de .NET 8.0 a .NET 10.0.300, incluyendo todos los paquetes NuGet, resolviendo Breaking Changes y manteniendo funcionalidad existente.

---

## Alcance

**Incluye:**

- Actualizar `TargetFramework` de `net8.0` a `net10.0` en los 4 proyectos (`Domain`, `Application`, `Infrastructure`, `Web`)
- Actualizar todos los paquetes NuGet a versiones compatibles con .NET 10 usando `dotnet-outdated` como herramienta de referencia
- Resolver errores de compilación causados por APIs deprecated o Breaking Changes identificados manualmente desde los changelogs de cada paquete
- Verificar que `config/secrets.json` y `AddJsonFile("config/secrets.json")` sigan funcionando
- Mantener la estructura de proyectos, capas, y arquitectura existente

**Fuera de alcance:**

- Reescribir código por cambios de arquitectura en .NET 10
- Cambiar de SQL Server a otro motor de base de datos
- Agregar soporte para Blazor o Minimal API si no existe
- Migración de Bootstrap/jQuery a Tailwind (ya existe iniciativa separada en specs 01–20)
- Implementar tests (scope futuro)
- Modificar la lógica de negocio o características de la aplicación

---

## Data model

Esta funcionalidad no introduce ni modifica estructuras de datos. La base de datos y los esquemas permanecen sin cambios.

---

## Plan de implementación

**Paso 1 — Preparar entorno y crear branch**

1. Hacer commit/tag en el estado actual de Develop antes de empezar la migración
2. Crear branch `feature/dotnet10-migration`
3. Instalar `dotnet-outdated` globalmente si no está: `dotnet tool install --global dotnet-outdated`

**Paso 2 — Actualizar TargetFramework**

1. En `Domain/Domain.csproj`: cambiar `<TargetFramework>net8.0</TargetFramework>` a `<TargetFramework>net10.0</TargetFramework>`
2. En `Application/Application.csproj`: mismo cambio
3. En `Infrastructure/Infrastructure.csproj`: mismo cambio
4. En `Web/Web.csproj`: mismo cambio
5. Compilar para identificar errores inmediatos

**Paso 3 — Actualizar paquetes Microsoft**

1. Ejecutar `dotnet-outdated -u` para identificar actualizaciones disponibles de paquetes Microsoft
2. Actualizar paquetes en orden: `Microsoft.Extensions.*` → `Microsoft.EntityFrameworkCore.*` → `Microsoft.AspNetCore.*` → `Microsoft.Extensions.Identity.*`
3. Por cada actualización, compilar y resolver Breaking Changes
4. Documente errores encontrados y soluciones aplicadas

**Paso 4 — Actualizar paquetes de terceros**

1. Actualizar `MailKit`, `QRCoder`, `Newtonsoft.Json`, `Selenium.WebDriver`, `Selenium.WebDriver.ChromeDriver`
2. Por cada paquete, verificar compatibilidad con .NET 10 desde NuGet.org
3. Compilar y resolver errores

**Paso 5 — Verificación de infraestructura**

1. Verificar que `config/secrets.json` se carga correctamente
2. Verificar que `AddJsonFile("config/secrets.json", optional: true, reloadOnChange: true)` sigue funcionando
3. Compilar solución completa sin errores
4. Probar que la aplicación inicia correctamente con `dotnet run --project Web/Web.csproj`

---

## Criterios de aceptación

- [ ] Los 4 proyectos (`Domain`, `Application`, `Infrastructure`, `Web`) tienen `<TargetFramework>net10.0</TargetFramework>`
- [ ] `dotnet-outdated` está instalado y muestra las actualizaciones disponibles
- [ ] Todos los paquetes Microsoft están actualizados a versiones compatibles con .NET 10
- [ ] Todos los paquetes de terceros (`MailKit`, `QRCoder`, `Newtonsoft.Json`, `Selenium.WebDriver`) están actualizados
- [ ] `dotnet build "TradingBookApp.sln"` compila sin errores
- [ ] `config/secrets.json` se carga correctamente al iniciar la aplicación
- [ ] La aplicación inicia con `dotnet run --project Web/Web.csproj` sin errores
- [ ] Los endpoints de autenticación (`/Account/SignIn`, `/Account/Login`, `/Account/ForgotPassword`) funcionan correctamente con rate limiting
- [ ] `dotnet list package --vulnerable --include-transitive` no muestra vulnerabilidades críticas nuevas introducidas por la actualización

---

## Decisiones tomadas y descartadas

- **Sí:** Usar `dotnet-outdated` como herramienta para identificar versiones disponibles de paquetes. Facilita la actualización sin asumir versiones específicas.
- **Sí:** Actualizar paquetes Microsoft primero, luego terceros. Microsoft tiene dependencias más críticas y cambios más disruptivos.
- **Sí:** Compilar después de cada grupo de paquetes para identificar Breaking Changes incrementalmente.
- **Sí:** Crear branch `feature/dotnet10-migration` para mantener historial limpio en Develop.
- **Sí:** Hacer tag/commit en Develop antes de crear el branch para tener punto de rollback claro.
- **No:** No usar `dotnet migrate` porque no existe una herramienta oficial para migrar proyectos web de .NET 8 a .NET 10.
- **No:** No reescribir código por cambios de arquitectura — solo resolver errores de compilación de APIs deprecated.
- **No:** No agregar tests como parte de esta migración — está fuera del scope.

---

## Riesgos identificados

| Riesgo | Mitigación |
|--------|------------|
| **Paquete de terceros no compatible con .NET 10** | Investigar compatibilidad en NuGet.org antes de actualizar. Si no hay versión compatible, revertir ese paquete y buscar alternativa. |
| **Breaking Changes en Entity Framework Core** | Revisar changelog de EF Core 9 y 10 antes de actualizar. Mantener backwards compatibility de consultas SQL. |
| **Perdida de funcionalidad en tiempo de ejecución** | Probar endpoint de autenticación después de cada grupo de actualizaciones. |
| **Tiempo de migración largo** | Compilar incrementalmente. Si hay más de 20 errores, revertir y evaluar estrategia alternativa. |