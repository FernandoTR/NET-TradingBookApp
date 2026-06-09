# SPEC 24 - Integracion de Serilog

> **Estado:** Implementado - **Depende de:** Ninguno - **Fecha:** 2026-06-08
> **Objetivo:** Agregar Serilog al host web para registrar eventos estructurados de arranque, pipeline HTTP y errores en consola y archivos diarios sin modificar el servicio actual de auditoria en SQL Server.

---

## Alcance

**Incluye:**

- Agregar paquetes Serilog en `Web/Web.csproj`.
- Configurar Serilog en `Web/Program.cs` como logger del host web.
- Leer la configuracion de Serilog desde `Web/appsettings.json`.
- Registrar eventos en consola y archivo.
- Escribir archivos diarios en `Web/logs/app-.log`.
- Mantener retencion de 14 archivos de log.
- Agregar `UseSerilogRequestLogging()` para registrar requests HTTP.
- Agregar `Web/logs/` a `.gitignore`.

**Fuera de alcance:**

- Sink de Serilog hacia SQL Server.
- Migraciones, tablas nuevas o cambios de base de datos.
- Reemplazar `ILogService`, `LogService` o `LoggingDbContext`.
- Cambios en controladores o servicios para agregar logs explicitos.
- Dashboards, alertas u observabilidad distribuida.

---

## Data model

Esta funcionalidad no introduce entidades, DTOs ni cambios de base de datos.

Se agregan referencias NuGet en `Web/Web.csproj`:

```xml
<PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="10.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.1.1" />
```

Se agrega una seccion `Serilog` en `Web/appsettings.json` con:

- Nivel default `Information`.
- Nivel `Warning` para `Microsoft` y `Microsoft.AspNetCore`.
- Sink de consola.
- Sink de archivo en `logs/app-.log`.
- Rolling diario.
- Retencion de 14 archivos.

---

## Plan de implementacion

**Paso 1 - Agregar paquetes Serilog**

1. En `Web/Web.csproj`, agregar las referencias NuGet de Serilog acordadas.
2. Compilar para validar restore y compatibilidad de paquetes.

**Paso 2 - Configurar Serilog en el host**

1. En `Web/Program.cs`, agregar `using Serilog;`.
2. Crear un bootstrap logger antes de construir el host.
3. Envolver el arranque de la aplicacion en `try/catch/finally`.
4. Configurar `builder.Host.UseSerilog(...)` para leer desde `builder.Configuration`, servicios registrados y `LogContext`.
5. Llamar `Log.CloseAndFlush()` en `finally`.
6. Compilar para validar.

**Paso 3 - Agregar request logging**

1. En `Web/Program.cs`, agregar `app.UseSerilogRequestLogging()` despues de `app.UseStaticFiles()` y antes de `app.UseRouting()`.
2. Compilar para validar.

**Paso 4 - Configurar sinks y niveles**

1. En `Web/appsettings.json`, reemplazar la seccion `Logging` por `Serilog`.
2. Configurar consola y archivo diario `logs/app-.log`.
3. Configurar `retainedFileCountLimit` en `14`.
4. Compilar para validar.

**Paso 5 - Ignorar logs generados**

1. En `.gitignore`, agregar `Web/logs/`.
2. Confirmar que los archivos generados de log no quedan como cambios pendientes.

**Paso 6 - Verificacion final**

1. Ejecutar `dotnet build "TradingBookApp.sln"` desde la raiz.
2. Ejecutar `dotnet run --project Web/Web.csproj --launch-profile https`.
3. Navegar a una ruta MVC.
4. Verificar que se registra una request con Serilog.
5. Confirmar que se crea un archivo bajo `Web/logs/`.
6. Confirmar que `ILogService`, `LogService` y `LoggingDbContext` no cambiaron funcionalmente.

---

## Criterios de aceptacion

- [ ] `Web.csproj` contiene las referencias NuGet de Serilog acordadas.
- [ ] `Program.cs` inicializa Serilog desde configuracion.
- [ ] `Program.cs` cierra el logger con `Log.CloseAndFlush()`.
- [ ] `UseSerilogRequestLogging()` registra requests HTTP.
- [ ] `Web/appsettings.json` contiene la seccion `Serilog`.
- [ ] La configuracion escribe a consola.
- [ ] La configuracion escribe archivos diarios en `logs/app-.log`.
- [ ] La retencion configurada es de 14 archivos.
- [ ] `Web/logs/` esta ignorado por git.
- [ ] `ILogService`, `LogService` y `LoggingDbContext` permanecen sin cambios funcionales.
- [ ] `dotnet build "TradingBookApp.sln"` termina sin errores.

---

## Decisiones tomadas y descartadas

- **Si:** consola y archivo para esta primera version. Da observabilidad local sin depender de base de datos.
- **Si:** configurar Serilog desde `appsettings.json`. Permite ajustes por ambiente sin cambiar codigo.
- **Si:** retencion diaria de 14 archivos. Evita crecimiento indefinido y mantiene historial suficiente.
- **Si:** request logging para ASP.NET Core. Registra el pipeline HTTP sin tocar controladores.
- **No:** sink SQL Server. La auditoria existente ya usa SQL mediante `ILogService`.
- **No:** reemplazar `ILogService`. Evita mezclar logs tecnicos con auditoria funcional.
- **No:** dashboards y alertas. Pertenecen a una futura spec de observabilidad.

---

## Riesgos identificados

| Riesgo | Mitigacion |
|--------|------------|
| Una configuracion invalida de Serilog puede afectar el arranque | Usar bootstrap logger, `try/catch/finally` y `Log.CloseAndFlush()` |
| Niveles demasiado verbosos pueden generar mucho volumen | Mantener `Default = Information` y framework en `Warning` |
| Los archivos de log pueden crecer si cambia la retencion | Mantener `retainedFileCountLimit = 14` |
| Confusion entre logs tecnicos y auditoria de negocio | Dejar `ILogService`, `LogService` y `LoggingDbContext` intactos |

---

## Lo que **no** esta en esta spec

- Sink de Serilog hacia SQL Server.
- Migraciones, tablas nuevas o cambios de base de datos.
- Reemplazar `ILogService`, `LogService` o `LoggingDbContext`.
- Cambios en controladores o servicios para agregar logs explicitos.
- Dashboards, alertas u observabilidad distribuida.
