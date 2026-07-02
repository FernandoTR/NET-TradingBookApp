# SPEC 34 - Modulo ErrorLogs para errores de aplicacion

> **Estado:** Implementado - **Depende de:** SPEC 10 - **Fecha:** 2026-07-01
> **Objetivo:** Crear un modulo MVC separado para consultar los errores de aplicacion registrados en `ErrorLogs`, reutilizando el permiso de `Logs` y el patron visual del modulo `Logs`.

---

## Alcance

**Incluye:**

- Crear un modulo MVC separado `ErrorLogs` para consultar errores de aplicacion.
- Reutilizar la tabla existente `ErrorLogs` registrada en `LoggingDbContext`.
- Reutilizar el permiso actual `Permissions.Logs` para controlar acceso al modulo.
- Agregar entrada de menu con texto `Errores de Aplicación`.
- Crear listado con DataTables server-side basado en el patron visual y funcional de `Logs`.
- Mostrar columnas `Id`, `LogDate`, `MethodName`, `ExceptionMessage` y `ApplicationId`.
- Agregar accion `Ver detalle` por fila.
- Mostrar el detalle en un drawer de solo lectura.
- Incluir en el drawer `MethodName`, `ExceptionMessage`, `ExceptionStackTrace`, `ExceptionString`, `LogDate` y `ApplicationId`.
- Filtrar el listado por rango de fechas.
- Usar como rango inicial los ultimos 7 dias.
- Mantener formato visual Metronic Tailwind consistente con `Logs`.

**Fuera de alcance:**

- Crear una tabla nueva para errores.
- Cambiar la forma en que `LogService.ErrorLog` registra errores.
- Cambiar `LoggingDbContext` salvo que sea estrictamente necesario para consulta.
- Crear un permiso nuevo `Permissions.ErrorLogs`.
- Agregar filtros por `MethodName`, `ApplicationId`, tipo de excepcion o usuario.
- Agregar acciones para borrar, editar, archivar o resolver errores.
- Crear dashboard, metricas, graficas o agrupaciones de errores.
- Exportar errores a Excel, CSV o PDF.
- Integrar proveedores externos de logging.

---

## Data model

No se crea una tabla nueva ni se modifica la estructura de `ErrorLogs`.

Se reutiliza la entidad existente:

```csharp
public partial class ErrorLog
{
    public long Id { get; set; }
    public DateTime LogDate { get; set; }
    public string MethodName { get; set; } = null!;
    public string? ExceptionMessage { get; set; }
    public string? ExceptionStackTrace { get; set; }
    public string? ExceptionString { get; set; }
    public int ApplicationId { get; set; }
}
```

Se agrega modelo de vista en `Web/Models`:

```csharp
public class ErrorLogsViewModel
{
    public long Id { get; set; }
    public DateTime LogDate { get; set; }
    public string MethodName { get; set; } = null!;
    public string? ExceptionMessage { get; set; }
    public string? ExceptionStackTrace { get; set; }
    public string? ExceptionString { get; set; }
    public int ApplicationId { get; set; }
}
```

Se agrega contrato de consulta en `Application/Interfaces`:

```csharp
public interface IErrorLogService
{
    Task<IEnumerable<ErrorLog>> GetAllByDateRangeAsync(DateTime dateStart, DateTime dateEnd);
    Task<ErrorLog?> GetByIdAsync(long id);
}
```

Se agrega repositorio en `Application/Interfaces`:

```csharp
public interface IErrorLogRepository
{
    Task<IEnumerable<ErrorLog>> GetAllByDateRangeAsync(DateTime dateStart, DateTime dateEnd);
    Task<ErrorLog?> GetByIdAsync(long id);
}
```

Convenciones:

- La consulta usa `LoggingDbContext.ErrorLogs`.
- `LogDate` se filtra con rango inclusivo desde `00:00:00.000` hasta `23:59:59.000`.
- `ExceptionStackTrace` y `ExceptionString` no se muestran como columnas del DataTable.
- `ExceptionStackTrace` y `ExceptionString` solo se muestran en el drawer de detalle.

---

## Plan de implementacion

**Paso 1 - Crear contratos de consulta**

1. Agregar `IErrorLogRepository` en `Application/Interfaces`.
2. Agregar `IErrorLogService` en `Application/Interfaces`.
3. Crear `ErrorLogsViewModel` en `Web/Models`.
4. Compilar para validar que los contratos no rompen referencias existentes.

**Paso 2 - Implementar repositorio y servicio**

1. Crear `ErrorLogRepository` en `Infrastructure/Persistence/Repositories`.
2. Consultar `LoggingDbContext.ErrorLogs` por rango de fechas.
3. Agregar consulta por `Id` para el endpoint de detalle.
4. Crear `ErrorLogService` en `Application/Services`.
5. Registrar repositorio y servicio en los contenedores de DI correspondientes.
6. Compilar para validar.

**Paso 3 - Crear controller MVC**

1. Crear `ErrorLogsController` en `Web/Controllers`.
2. Proteger `Index`, `JsonDataTable` y `Detail` con autenticacion.
3. Reutilizar `Permissions.Logs` para validar acceso.
4. Implementar `Index` siguiendo el patron de validacion de usuario de `LogsController`.
5. Implementar `JsonDataTable` con DataTables server-side.
6. Filtrar por rango de fechas con valor default de ultimos 7 dias.
7. Aplicar busqueda textual sobre `Id`, `LogDate`, `MethodName`, `ExceptionMessage` y `ApplicationId`.
8. Implementar endpoint `Detail(long id)` para devolver `MethodName`, `ExceptionMessage`, `ExceptionStackTrace`, `ExceptionString`, `LogDate` y `ApplicationId`.
9. Compilar para validar.

**Paso 4 - Crear vista Index**

1. Crear `Web/Views/ErrorLogs/Index.cshtml`.
2. Reutilizar el patron visual de `Web/Views/Logs/Index.cshtml`.
3. Mostrar header con titulo `Errores de Aplicación` y boton `Filtro`.
4. Crear drawer de filtros con daterangepicker.
5. Inicializar el rango por defecto en ultimos 7 dias.
6. Crear DataTable con columnas `Id`, `LogDate`, `MethodName`, `ExceptionMessage`, `ApplicationId` y `task`.
7. Agregar accion `Ver detalle` en la columna `task`.
8. Crear drawer de detalle de solo lectura.
9. Cargar detalle mediante endpoint `ErrorLogs/Detail/{id}` al presionar `Ver detalle`.
10. Mostrar `ExceptionStackTrace` y `ExceptionString` en bloques monoespaciados.
11. Compilar para validar.

**Paso 5 - Agregar menu**

1. Crear script SQL versionado `Infrastructure/Persistence/StoredProcedures/Spec34_ErrorLogsMenu.sql`.
2. Insertar entrada de menu `Errores de Aplicación`.
3. Reutilizar el permiso existente asociado a `Permissions.Logs`.
4. No crear tabla nueva ni permiso nuevo.
5. Documentar en el script los valores ajustables como `ApplicationId`, orden y menu padre.
6. Compilar para validar.

**Paso 6 - Verificacion final**

1. Ejecutar `dotnet build "TradingBookApp.sln"`.
2. Ejecutar la aplicacion con perfil HTTPS.
3. Confirmar que el menu `Errores de Aplicación` aparece para usuarios con permiso `Logs`.
4. Confirmar que usuarios sin permiso `Logs` no pueden abrir `ErrorLogs/Index`.
5. Confirmar que el listado carga errores de los ultimos 7 dias.
6. Confirmar que el filtro de rango de fechas recarga la tabla.
7. Confirmar que la busqueda textual filtra las columnas visibles.
8. Confirmar que `Ver detalle` abre el drawer y carga stack trace y exception string desde `Detail`.
9. Confirmar que el drawer no expone acciones de edicion, borrado o resolucion.

---

## Criterios de aceptacion

- [ ] Existe un modulo MVC separado `ErrorLogs`.
- [ ] `ErrorLogsController.Index` valida autenticacion y reutiliza `Permissions.Logs`.
- [ ] Usuarios sin permiso `Logs` no pueden abrir `ErrorLogs/Index`.
- [ ] El listado usa la tabla existente `ErrorLogs` mediante `LoggingDbContext`.
- [ ] No se crea una tabla nueva para errores.
- [ ] No se crea un permiso nuevo para `ErrorLogs`.
- [ ] El DataTable carga datos desde `ErrorLogs/JsonDataTable` con server-side habilitado.
- [ ] El filtro por rango de fechas se aplica sobre `LogDate`.
- [ ] El rango inicial del listado es ultimos 7 dias.
- [ ] La tabla muestra `Id`, `LogDate`, `MethodName`, `ExceptionMessage` y `ApplicationId`.
- [ ] La tabla incluye accion `Ver detalle`.
- [ ] La accion `Ver detalle` abre un drawer de solo lectura.
- [ ] El drawer carga el detalle mediante `ErrorLogs/Detail/{id}`.
- [ ] El drawer muestra `MethodName`, `ExceptionMessage`, `ExceptionStackTrace`, `ExceptionString`, `LogDate` y `ApplicationId`.
- [ ] `ExceptionStackTrace` y `ExceptionString` no se envian completos en cada fila del DataTable.
- [ ] Existe entrada de menu `Errores de Aplicación`.
- [ ] El script de menu reutiliza el permiso existente de `Logs`.
- [ ] No existen acciones para editar, borrar, archivar o resolver errores.
- [ ] `dotnet build "TradingBookApp.sln"` termina sin errores nuevos.

---

## Decisiones tomadas y descartadas

- **Si:** crear `ErrorLogs` como modulo MVC separado. La consulta usa otra tabla y otro contexto, por lo que no debe mezclarse dentro de `Logs`.
- **Si:** basarse visual y funcionalmente en `Logs`. Mantiene consistencia con el modulo existente y reduce decisiones nuevas de UI.
- **Si:** reutilizar `Permissions.Logs`. El usuario confirmo que el acceso debe ser el mismo que el modulo de logs actual.
- **Si:** reutilizar la tabla existente `ErrorLogs`. La aplicacion ya registra errores ahi mediante `LogService.ErrorLog`.
- **Si:** usar rango inicial de ultimos 7 dias. Evita cargar demasiados errores por defecto.
- **Si:** usar endpoint `Detail(long id)` para el drawer. Evita enviar stack traces largos en cada fila del DataTable.
- **Si:** mostrar `ExceptionStackTrace` y `ExceptionString` solo en el drawer. Mantiene la tabla legible.
- **Si:** agregar script SQL solo para menu. No se necesita tabla ni permiso nuevo.
- **No:** crear `Permissions.ErrorLogs`. El usuario decidio reutilizar el permiso de `Logs`.
- **No:** crear una tabla nueva de errores. La fuente existente es `ErrorLogs`.
- **No:** enviar stack trace completo en el payload del DataTable. Puede inflar respuestas y degradar la tabla.
- **No:** agregar filtros por metodo, aplicacion o tipo de excepcion. Esta version solo incluye rango de fechas.
- **No:** agregar acciones de borrado, edicion, archivado o resolucion. El modulo es de consulta.
- **No:** crear dashboard o metricas de errores. Es otra capacidad y debe ir en spec separada.

---

## Riesgos identificados

| Riesgo | Mitigacion |
|--------|------------|
| `ExceptionStackTrace` y `ExceptionString` pueden ser muy largos | Cargarlos solo desde `Detail(long id)` y mostrarlos en bloques scrolleables dentro del drawer. |
| Los errores pueden contener datos sensibles en mensajes o stack traces | El modulo queda protegido con autenticacion y reutiliza `Permissions.Logs`; no se agrega acceso publico. |
| La tabla puede cargar demasiados registros si no hay filtro | Usar ultimos 7 dias como rango por defecto y mantener paginacion server-side. |
| El script de menu puede requerir `ApplicationId`, padre u orden distinto por base | Dejar esos valores como variables ajustables en `Spec34_ErrorLogsMenu.sql`. |
| Fallos dentro del modulo de errores pueden intentar registrar nuevos errores | Mantener manejo de excepciones controlado en controller y evitar acciones que muten `ErrorLogs`. |

---

## Lo que no esta en esta spec

- Crear tabla nueva para errores.
- Modificar `LogService.ErrorLog`.
- Cambiar la estrategia de captura de excepciones de la aplicacion.
- Crear permiso nuevo `Permissions.ErrorLogs`.
- Agregar filtros por `MethodName`, `ApplicationId`, tipo de excepcion o usuario.
- Agregar acciones para borrar, editar, archivar o resolver errores.
- Crear dashboard, metricas, graficas o agrupaciones.
- Exportar errores a Excel, CSV o PDF.
- Integrar Serilog, Application Insights u otro proveedor externo de logging.
