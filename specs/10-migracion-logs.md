# SPEC 10 — Migración de Logs/Index a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01 · **Fecha:** 2026-05-22
> **Objetivo:** Migrar `Logs/Index` a componentes visuales de Metronic Tailwind conservando el comportamiento actual de DataTables, el filtro de fechas vía drawer (patrón Home/Index), y sin agregar acciones de fila.

## Alcance

**Incluye:**

- Migrar `Web/Views/Logs/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de header container de `Web/Views/Home/Index.cshtml` (título + subtítulo + botón "Filtro" que abre un drawer) y la estructura de card + tabla de `Web/Views/CatFigure/Index.cshtml`.
- Reemplazar el toolbar Bootstrap actual (`kt_app_toolbar`, `kt_app_content`, `app-container`, `card`, `card-body`) por header container con `kt-container-fixed`, título "Registro de Eventos", subtítulo y botón "Filtro" con `kt-btn kt-btn-outline` y `data-kt-drawer-toggle="#filter_drawer"`.
- Reemplazar el dropdown menu de filtro (`kt_menu_filter`) por un drawer (`kt-drawer kt-drawer-end`) que contenga el daterangepicker y botones "Limpiar" / "Aplicar", siguiendo el patrón de `#filter_drawer` de Home/Index.
- Migrar el interior del drawer a clases Tailwind: label con `kt-form-label`, input con `kt-input`, botones con `kt-btn kt-btn-outline` / `kt-btn kt-btn-primary`.
- Mantener la funcionalidad del daterangepicker sin cambios (inicialización, rangos predefinidos, eventos `apply`/`cancel`, locale en español, envío de `fecha1` y `fecha2` al callback de DataTables).
- Eliminar el breadcrumb existente.
- Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable` con clase `kt-table` y `data-kt-datatable-table="true"`.
- Agregar script tag para `dataTables.min.js`.
- Mantener DataTables con server-side AJAX hacia `~/Logs/JsonDataTable`, pasando `fecha1` y `fecha2` como parámetros extra.
- Mantener las columnas actuales: `eventId` (ID), `eventDate` (Fecha, render con `moment`), `eventType` (Tipo de Evento), `description` (Descripción), `userName` (Usuario).
- Actualizar DataTables: reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
- Agregar search con `kt-input` vía `initComplete` y `layout` (patrón CatFigure, placeholder "Buscar eventos...").
- Convertir clases `className` de `min-w-Xpx` a `min-w-[Xpx]`.
- Eliminar `filter: true` (reemplazar por `searching: true`).
- Actualizar lenguaje de DataTables 1.x a DataTables 2.x (`processing`, `lengthMenu`, `zeroRecords`, `emptyTable`, `info`, `infoEmpty`, `infoFiltered`, `search`, `searchPlaceholder`, `paginate`, `aria`).
- Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback`.
- Agregar `KTMenu.createInstances()` en `drawCallback` para consistencia.
- Mantener `order: [[0, "desc"]]`.
- Validar manualmente la vista Logs en desktop y mobile.
- Confirmar que la aplicación sigue compilando con `dotnet build "TradingBookApp.sln"`.

**Fuera de alcance (para specs futuras):**

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `LogsController`, servicios, repositorios, DTOs, entidades o `LogsViewModel`.
- Cambiar contratos JSON de `~/Logs/JsonDataTable`.
- Agregar vistas `New`, `Edit` o `Delete` (no existen actualmente).
- Agregar columna de acciones (`task`) a la tabla.
- Agregar `ActionButtonHelper.GenerateActionMenu` en el controller.
- Reemplazar el daterangepicker por otro componente de fecha.
- Agregar, quitar o renombrar columnas de la tabla.
- Cambiar el rango de fechas por defecto (últimos 30 días).
- Eliminar Bootstrap, jQuery, DataTables, moment o daterangepicker.
- Rediseñar completamente la experiencia visual de Logs.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos backend.

Se reutiliza sin cambios:

```csharp
Web.Models.LogsViewModel
```

El controlador `LogsController.JsonDataTable` se mantiene sin modificaciones. Retorna las columnas `eventId`, `eventDate`, `eventType`, `description` y `userName`. Logs no tiene columna `task` ni usa `ActionButtonHelper`.

Se mantienen los contratos cliente existentes de la tabla:

```text
POST ~/Logs/JsonDataTable  (parámetros extra: fecha1, fecha2)
```

La única vista involucrada es `Web/Views/Logs/Index.cshtml`. No existen vistas `New`, `Edit` ni `Delete`.

La columna `eventDate` usa `moment` para formateo de fecha (`DD/MM/YYYY HH:mm:ss`) vía función inline en DataTables, sin dependencia de `Utilities.js`. No se usan `renderTrueFalse`, `renderFlag`, `renderStatusEmployee` ni ningún otro render de `Utilities.js` en esta vista.

## Plan de implementación

1. Migrar el markup de `Web/Views/Logs/Index.cshtml` a Metronic Tailwind:
   - Reemplazar todo el toolbar Bootstrap y layout (`kt_app_toolbar`, `kt_app_content`, `app-container`, `card`, `card-body`) por header container con `kt-container-fixed`, título "Registro de Eventos", subtítulo "Consulta los eventos y acciones registradas en el sistema." y botón "Filtro" con `kt-btn kt-btn-outline` y `data-kt-drawer-toggle="#filter_drawer"`.
   - Eliminar el breadcrumb completo.
   - Reemplazar el dropdown menu de filtro (`kt_menu_filter`) por un drawer `#filter_drawer` con clase `kt-drawer kt-drawer-end`, replicando la estructura de `Home/Index.cshtml`: `kt-card-header` con título "Filtros Disponibles" y botón dismiss, `kt-card-content kt-scrollable-y-auto` con el daterangepicker y su label, y `kt-card-footer` con botones "Limpiar" y "Aplicar".
   - El input del daterangepicker (`#kt_daterangepicker_4`) conserva su id pero cambia su clase a `kt-input`.
   - La label del daterangepicker usa `kt-form-label` con texto "Rango de fechas:".
   - Los botones del footer del drawer usan: "Limpiar" con `kt-btn kt-btn-outline` y "Aplicar" con `kt-btn kt-btn-primary grow`, ambos con `data-kt-drawer-dismiss="true"`.
   - Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable` con clase `kt-table` y `data-kt-datatable-table="true"`.
   - Agregar script tag para `dataTables.min.js`.

2. Actualizar DataTables en el bloque `<script>`:
   - Reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
   - Agregar `layout` (topStart: [], topEnd: 'search', bottomStart: ['pageLength', 'info'], bottomEnd: 'paging') e `initComplete` para el search con `kt-input` (placeholder "Buscar eventos...").
   - Mantener todas las columnas actuales con sus `data`, `name` y `render`: `eventId`, `eventDate` (con render `moment`), `eventType`, `description`, `userName`.
   - Convertir clases `className` de `min-w-Xpx` a `min-w-[Xpx]`.
   - Reemplazar `filter: true` por `searching: true`.
   - Actualizar lenguaje de DataTables 1.x a 2.x: `processing`, `lengthMenu`, `zeroRecords`, `emptyTable`, `info`, `infoEmpty`, `infoFiltered`, `search` (vacío), `searchPlaceholder`, `paginate`, `aria`.
   - Mantener `serverSide: true`, `pageLength: 10`, `order: [[0, "desc"]]`.

3. Actualizar la lógica JavaScript:
   - Mantener la inicialización del daterangepicker sin cambios funcionales (misma configuración de `startDate`, `endDate`, `ranges`, `locale`, eventos `apply`/`cancel`).
   - Mantener `updateDateRange` sin cambios (actualiza `fechaInicio`, `fechaFin` y el valor del input).
   - Mantener `loadTableDate` con la misma lógica de paso de `{ fecha1: fechaInicio, fecha2: fechaFin }` en el `data` del AJAX.
   - Eliminar el código de prevención de cierre del menú (`stopPropagation` sobre `kt_menu_filter` y `.daterangepicker`), ya que el drawer no requiere esta protección.
   - Reemplazar el evento `click` directo sobre `#btn_AplicarFiltro` por un handler que llame a `loadTableDate()` (misma lógica, ahora el botón está dentro del drawer).
   - Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback`.
   - Agregar `KTMenu.createInstances()` en `drawCallback` para consistencia (aunque Logs no tiene `kt-menu`, previene problemas futuros).
   - Mantener la llamada inicial: `updateDateRange(start, end)` seguida de `loadTableDate()`.

4. Validar manualmente en desktop y mobile:
   - La tabla carga datos desde `~/Logs/JsonDataTable` con el rango de fechas por defecto (últimos 30 días).
   - El botón "Filtro" abre el drawer correctamente.
   - El daterangepicker dentro del drawer funciona: selección de rango, rangos predefinidos, botones Aplicar/Cancelar.
   - Al presionar "Aplicar", el drawer se cierra y la tabla se recarga con el nuevo rango de fechas.
   - Al presionar "Limpiar", el drawer se cierra sin recargar.
   - El search con `kt-input` filtra correctamente dentro de los datos cargados.
   - Todas las columnas se renderizan correctamente: ID, Fecha (formato `DD/MM/YYYY HH:mm:ss`), Tipo de Evento, Descripción, Usuario.
   - Paginación, ordenamiento y filtrado funcionan sin errores.
   - La vista es usable en desktop y mobile (tabla scrolleable, drawer funcional).

5. Ejecutar `dotnet build "TradingBookApp.sln"` desde la raíz del repositorio y confirmar que compila sin errores nuevos.

## Criterios de aceptación

- [ ] `Web/Views/Logs/Index.cshtml` usa markup y clases visuales de Metronic Tailwind (header container con `kt-container-fixed`, card con `kt-card kt-card-grid`, tabla con `kt-table`).
- [ ] El header container muestra el título "Registro de Eventos", subtítulo y un botón "Filtro" con `kt-btn kt-btn-outline` y `data-kt-drawer-toggle="#filter_drawer"`.
- [ ] El drawer `#filter_drawer` se abre al presionar el botón "Filtro" y contiene el daterangepicker, label "Rango de fechas:", y botones "Limpiar" / "Aplicar" en el footer.
- [ ] El drawer usa clases de Metronic Tailwind: `kt-drawer kt-drawer-end`, `kt-card-header`, `kt-card-content`, `kt-card-footer`, `kt-form-label`, `kt-input`, `kt-btn kt-btn-outline`, `kt-btn kt-btn-primary`.
- [ ] El daterangepicker se inicializa correctamente dentro del drawer y muestra los rangos predefinidos (Hoy, Ayer, Últimos 7 Días, Últimos 30 Días, Este Mes, Último Mes).
- [ ] Al presionar "Aplicar" en el drawer: el drawer se cierra, se actualiza `fechaInicio`/`fechaFin` y la tabla se recarga con el nuevo rango de fechas.
- [ ] Al presionar "Limpiar" en el drawer: el drawer se cierra sin recargar la tabla.
- [ ] DataTables sigue cargando datos desde `~/Logs/JsonDataTable` con `serverSide: true` y los parámetros `fecha1`/`fecha2`.
- [ ] El search de DataTables usa `kt-input` con el patrón `initComplete` + `layout` de CatFigure.
- [ ] Las columnas se mantienen sin cambios: `eventId` (ID), `eventDate` (Fecha con `moment`), `eventType` (Tipo de Evento), `description` (Descripción), `userName` (Usuario).
- [ ] Las clases `className` de las columnas usan el formato `min-w-[Xpx]`.
- [ ] El lenguaje de DataTables usa claves modernas de 2.x (`processing`, `searchPlaceholder`, `paginate`).
- [ ] No existe la opción `filter: true` en la configuración de DataTables; se reemplaza por `searching: true`.
- [ ] No hay tooltips Bootstrap (`data-toggle="tooltip"`) en Index.cshtml.
- [ ] No hay breadcrumb en Index.cshtml.
- [ ] No hay dropdown menu `kt_menu_filter` en Index.cshtml.
- [ ] No existe columna `task` ni acciones de fila en la tabla.
- [ ] No se modifica `LogsController`, sus servicios, repositorios, entidades, DTOs ni `LogsViewModel`.
- [ ] `dotnet build "TradingBookApp.sln"` termina correctamente, permitiendo solo warnings preexistentes no relacionados con esta migración.

## Decisiones

- **Sí:** Migrar únicamente `Web/Views/Logs/Index.cshtml`. Logs no tiene vistas `New`, `Edit` ni `Delete`; el controller solo expone `Index` y `JsonDataTable`.
- **Sí:** Usar el patrón de filtro de `Home/Index.cshtml` con drawer (`kt-drawer kt-drawer-end`). El dropdown menu actual (`kt_menu_filter`) se reemplaza por un drawer que sigue la misma estructura visual de `#filter_drawer` de Home: header con título y botón dismiss, content con el daterangepicker, footer con botones "Limpiar" y "Aplicar".
- **Sí:** Mantener el daterangepicker sin cambios funcionales. La librería se carga globalmente vía `plugins.bundle.js`, su configuración (rangos, locale en español, eventos `apply`/`cancel`) es correcta y no depende del markup contenedor.
- **Sí:** Replicar la estructura de card + tabla de `Web/Views/CatFigure/Index.cshtml`. La tabla, el search con `kt-input` y el layout de DataTables deben ser consistentes con todos los módulos ya migrados.
- **Sí:** Agregar `KTMenu.createInstances()` en el `drawCallback`. Aunque Logs no tiene columna `task` ni `kt-menu`, mantener esta llamada previene problemas si en el futuro se agregan acciones o menús contextuales.
- **Sí:** Actualizar DataTables al patrón de CatFigure: `$.fn.DataTable.isDataTable` antes de `destroy`, lenguaje 2.x, `searching: true` en vez de `filter: true`, `min-w-[Xpx]`, `layout` + `initComplete` con `kt-input`.
- **Sí:** Eliminar el breadcrumb. Todos los módulos migrados (Employees, CatCategory, CatFigure, CatAccountType, CatFrame, CatInstruments, Roles, Users) ya eliminaron el suyo.
- **Sí:** Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback`. Ningún catálogo migrado usa tooltips Bootstrap; Logs no tiene elementos con `data-toggle="tooltip"` en la tabla.
- **No:** Agregar columna de acciones (`task`). Logs es una vista de consulta, no de administración. El controller no genera HTML de acciones ni usa `ActionButtonHelper`. Si se necesita en el futuro, debe ir en su propia spec.
- **No:** Cambiar `LogsController`, servicios, repositorios, entidades, DTOs o `LogsViewModel`. Esta spec es solo UI/markup/JS cliente.
- **No:** Cambiar las columnas de la tabla. Las cinco columnas actuales (`eventId`, `eventDate`, `eventType`, `description`, `userName`) se mantienen sin cambios de nombre, orden o render.
- **No:** Reemplazar el daterangepicker por otro componente (flatpickr, datepicker nativo, etc.). La librería actual funciona, está integrada con moment y es consistente con el resto de la aplicación.
- **No:** Eliminar el daterangepicker y usar solo el search textual. El filtro por rango de fechas es la funcionalidad principal de Logs; el search textual es complementario.
- **No:** Hacer un rediseño completo de Logs. El objetivo es migrar el markup a Metronic Tailwind conservando el comportamiento existente.

## Riesgos

| Riesgo | Mitigación |
| ------ | ---------- |
| El daterangepicker abre un calendario desplegable (`.daterangepicker`) que podría ser tapado u ocultado por el drawer al hacer clic fuera. | El drawer de Metronic Tailwind (`kt-drawer`) no cierra automáticamente con clics internos; el daterangepicker se renderiza como elemento `position: absolute` y no debería interferir. Validar manualmente que el calendario se muestra completo y es interactivo. |
| El daterangepicker requiere que el input esté visible en el DOM para inicializarse correctamente. Si el drawer está oculto con `hidden` al cargar la página, la inicialización podría fallar. | El valor inicial (`updateDateRange`) se aplica antes de la inicialización del daterangepicker; el input recibe su valor y el calendario se abre solo bajo demanda del usuario. Validar que el daterangepicker se inicializa correctamente al cargar la página y al abrir el drawer por primera vez. |
| `loadTableDate` destruye y recrea la tabla en cada llamada. Si el usuario presiona "Aplicar" múltiples veces rápidamente, podría haber conflictos de reinicialización. | El chequeo `$.fn.DataTable.isDataTable('#dtTable')` antes de `destroy()` previene intentos de destrucción sobre instancias inexistentes, igual que en CatFigure. |
| Las clases Tailwind del drawer (`kt-drawer-end`, `rounded-xl`, `border`, `border-border`) asumen que el CSS de Metronic Tailwind tiene definidas esas utilidades. Si falta alguna clase, el drawer podría no renderizarse correctamente. | Home/Index ya usa exitosamente el mismo drawer con las mismas clases; el CSS necesario ya está disponible en el tema. |
| El botón "Limpiar" del drawer no resetea el daterangepicker a su valor por defecto; solo cierra el drawer. El usuario podría esperar que limpie el filtro. | El comportamiento actual del botón "Reiniciar" en el dropdown original tampoco reseteaba el daterangepicker (era `type="reset"` con `data-kt-menu-dismiss="true"`). Documentar en la spec que "Limpiar" solo cierra el drawer sin alterar el filtro activo; si se requiere reseteo real, debe ir en una spec futura. |
| Warnings preexistentes del build se confunden con regresiones de esta migración. | Ejecutar `dotnet build "TradingBookApp.sln"` antes y después de la migración, separando warnings preexistentes de errores nuevos. |

## Lo que no está en esta spec

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `LogsController`, servicios, repositorios, DTOs, entidades o `LogsViewModel`.
- Cambiar contratos JSON de `~/Logs/JsonDataTable`.
- Agregar vistas `New`, `Edit` o `Delete` (no existen actualmente).
- Agregar columna de acciones (`task`) a la tabla.
- Agregar `ActionButtonHelper.GenerateActionMenu` en el controller.
- Reemplazar el daterangepicker por otro componente de fecha.
- Agregar, quitar o renombrar columnas de la tabla.
- Cambiar el rango de fechas por defecto (últimos 30 días).
- Agregar filtros adicionales al drawer (tipo de evento, usuario, etc.).
- Hacer que el botón "Limpiar" resetee el daterangepicker a su valor por defecto.
- Eliminar Bootstrap, jQuery, DataTables, moment o daterangepicker.

Cada uno de esos puntos debe ir en su propia spec si se decide abordarlo.
