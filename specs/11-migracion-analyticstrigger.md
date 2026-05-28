# SPEC 11 — Migración de AnalyticsTrigger/Index a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01 · **Fecha:** 2026-05-22
> **Objetivo:** Migrar `AnalyticsTrigger/Index` a componentes visuales de Metronic Tailwind conservando el comportamiento actual de DataTables y el filtro de gatillos vía drawer con kt-select.

## Alcance

**Incluye:**

- Migrar `Web/Views/AnalyticsTrigger/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de header container de `Web/Views/Home/Index.cshtml` (título + subtítulo + botón "Filtro" que abre un drawer) y la estructura de card + tabla de `Web/Views/CatFigure/Index.cshtml`.
- Reemplazar el toolbar Bootstrap actual (`kt_app_toolbar`, `kt_app_content`, `app-container`, `card`, `card-body`) por header container con `kt-container-fixed`, título "Análisis de Gatillos", subtítulo y botón "Filtro" con `kt-btn kt-btn-outline` y `data-kt-drawer-toggle="#filter_drawer"`.
- Reemplazar el dropdown menu de filtro (`kt_menu_filter`) por un drawer (`kt-drawer kt-drawer-end`) que contenga los 5 filtros con `kt-select`, siguiendo la estructura exacta de `#filter_drawer` de Home/Index: `kt-card-header` con título "Filtros Disponibles" y botón dismiss, `kt-card-content kt-scrollable-y-auto` con los dropdowns, y `kt-card-footer` con botones "Limpiar" / "Aplicar".
- Migrar los 5 Select2 (`CategoryId`, `AccountTypeId`, `InstrumentId`, `FrameId`, `DirectionId`) a `kt-select` de KTUI con `@Html.DropDownList`, clase `kt-select` y data-attributes (`data_kt_select`, `data_kt_select_enable_search`, `data_kt_select_placeholder`, `data_kt_select_config`), replicando la configuración exacta de Home/Index.
- Reemplazar `ClearFilterData` con `KTSelectHelper.setValue('#CategoryId', 1)` y `KTSelectHelper.clear(...)` para los otros 4 selects, igual que en Home/Index.
- Eliminar el breadcrumb existente.
- Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable` con clase `kt-table` y `data-kt-datatable-table="true"`.
- Mantener DataTables con server-side AJAX hacia `~/AnalyticsTrigger/JsonDataTable`, pasando `categoryId`, `accountTypeId`, `instrumentId`, `frameId`, `directionId` como parámetros extra.
- Mantener las 12 columnas actuales: `code`, `description`, `quantity`, `sl`, `tP1`, `tP2`, `tP3`, `slp` (render `renderSLPChart`), `tP1P` (render `renderTP1PChart`), `tP2P` (render `renderTP2PChart`), `tP3P` (render `renderTP3PChart`), `valid` (render `renderStatusAnalytics`).
- Migrar `renderProgressBar` en `Utilities.js` de clases Bootstrap (`progress`, `progress-bar`, `bg-danger`, `bg-light-danger`, etc.) a clases Tailwind compatibles con Metronic.
- Actualizar DataTables: reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
- Agregar `layout` (topStart, topEnd, bottomStart, bottomEnd) e `initComplete` para search con `kt-input` (placeholder "Buscar gatillos...").
- Convertir clases `className` de `min-w-Xpx` a `min-w-[Xpx]`.
- Eliminar `filter: true` y `filter: false` (opciones inválidas/no estándar).
- Activar `searching: true` (reemplazando `searching: false`).
- Activar `info: true` (reemplazando `info: false`).
- Actualizar lenguaje de DataTables 1.x a 2.x (`processing`, `lengthMenu`, `zeroRecords`, `emptyTable`, `info`, `infoEmpty`, `infoFiltered`, `search`, `searchPlaceholder`, `paginate`, `aria`).
- Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback`.
- Mantener `KTMenu.createInstances()` en `drawCallback`.
- Mantener `serverSide: true`, `pageLength: 10`.
- No agregar script tag para `dataTables.min.js` (ya se carga globalmente en `_Layout.cshtml`).
- Validar manualmente la vista AnalyticsTrigger en desktop y mobile.
- Confirmar que la aplicación sigue compilando con `dotnet build "TradingBookApp.sln"`.

**Fuera de alcance (para specs futuras):**

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `AnalyticsTriggerController`, servicios, repositorios, DTOs, entidades o `AnalyticsTriggerViewModel`.
- Cambiar contratos JSON de `~/AnalyticsTrigger/JsonDataTable`.
- Agregar vistas `New`, `Edit` o `Delete` (no existen actualmente).
- Agregar columna de acciones (`task`) a la tabla.
- Agregar modales.
- Agregar, quitar o renombrar columnas de la tabla.
- Migrar `renderStatusAnalytics` (ya usa `kt-badge`, compatible con Metronic).
- Reemplazar `kt-select` por otro componente de selección.
- Cambiar la lógica de filtrado del controller o del stored procedure.
- Rediseñar completamente la experiencia visual de AnalyticsTrigger.
- Eliminar Bootstrap, jQuery, DataTables o KTUI.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos backend.

Se reutilizan sin cambios:

```csharp
Web.Models.AnalyticsTriggerViewModel
Application.DTOs.GetTBAnalyticsTriggerDto
```

El controlador `AnalyticsTriggerController.JsonDataTable` se mantiene sin modificaciones. Retorna las columnas `id`, `code`, `description`, `quantity`, `sl`, `tP1`, `tP2`, `tP3`, `slp`, `tP1P`, `tP2P`, `tP3P`, `valid`. AnalyticsTrigger no tiene columna `task` ni usa `ActionButtonHelper`.

Se mantienen los contratos cliente existentes de la tabla:

```text
POST ~/AnalyticsTrigger/JsonDataTable  (parámetros extra: categoryId, accountTypeId, instrumentId, frameId, directionId)
```

La única vista involucrada es `Web/Views/AnalyticsTrigger/Index.cshtml`. No existen vistas `New`, `Edit` ni `Delete`.

Las funciones de renderizado en `Web/wwwroot/Template/custom/js/Utilities.js` requieren migración parcial:
- `renderStatusAnalytics` → ya usa `kt-badge kt-badge-success` / `kt-badge kt-badge-warning`, no requiere cambios.
- `renderProgressBar` → usa clases Bootstrap (`progress`, `progress-bar`, `bg-*`, `bg-light-*`, `d-flex`). Se migra a clases Tailwind compatibles con Metronic dentro de esta spec.
- `renderSLPChart`, `renderTP1PChart`, `renderTP2PChart`, `renderTP3PChart` → delegan en `renderProgressBar`, heredan la migración automáticamente.

## Plan de implementación

1. Migrar el markup de `Web/Views/AnalyticsTrigger/Index.cshtml` a Metronic Tailwind:
   - Reemplazar todo el toolbar Bootstrap y layout (`kt_app_toolbar`, `kt_app_content`, `app-container`, `card`, `card-body`, `d-flex flex-column flex-column-fluid`) por header container con `kt-container-fixed`, título "Análisis de Gatillos", subtítulo "Consulta el rendimiento de los gatillos configurados en el sistema." y botón "Filtro" con `kt-btn kt-btn-outline` y `data-kt-drawer-toggle="#filter_drawer"`.
   - Eliminar el breadcrumb completo.
   - Reemplazar el dropdown menu de filtro (`kt_menu_filter`) por un drawer `#filter_drawer` con clase `kt-drawer kt-drawer-end`, replicando la estructura de `Home/Index.cshtml`: `kt-card-header` con título "Filtros Disponibles" y botón dismiss, `kt-card-content kt-scrollable-y-auto` con 5 filas de filtro (Categoría, Tipo de Cuenta, Instrumento, Frame, Dirección), y `kt-card-footer` con botones "Limpiar" y "Aplicar".
   - Cada fila de filtro usa `flex items-baseline flex-wrap lg:flex-nowrap gap-2.5`, label con `kt-form-label max-w-56` y `@Html.DropDownList` con clase `kt-select`, data-attributes de KTUI (`data_kt_select="true"`, `data_kt_select_enable_search="true"`, `data_kt_select_search_placeholder="Buscar..."`, `data_kt_select_placeholder="Selecciona uno..."`, `data_kt_select_config="{''optionsClass'': ''kt-scrollable overflow-auto max-h-[250px]''}"`).
   - Botón "Limpiar": `kt-btn kt-btn-outline`, `data-kt-drawer-dismiss="true"`, llama a `ClearFilterData()`.
   - Botón "Aplicar": `kt-btn kt-btn-primary grow`, `data-kt-drawer-dismiss="true"`, `id="btn_AplicarFiltro"`, llama a `SearchData()`.
   - Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable` con clase `kt-table` y `data-kt-datatable-table="true"`.

2. Actualizar DataTables en el bloque `<script>`:
   - Reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
   - Agregar `layout` (topStart: [], topEnd: 'search', bottomStart: ['pageLength', 'info'], bottomEnd: 'paging').
   - Agregar `initComplete` para el search con `kt-input` (placeholder "Buscar gatillos...").
   - Mantener todas las columnas actuales con sus `data`, `name`, `autoWidth` y `render`: `code`, `description`, `quantity`, `sl`, `tP1`, `tP2`, `tP3`, `slp` (renderSLPChart), `tP1P` (renderTP1PChart), `tP2P` (renderTP2PChart), `tP3P` (renderTP3PChart), `valid` (renderStatusAnalytics).
   - Convertir clases `className` de `min-w-Xpx` a `min-w-[Xpx]`.
   - Eliminar `filter: true` y `filter: false`.
   - Reemplazar `searching: false` por `searching: true`.
   - Reemplazar `info: false` por `info: true`.
   - Actualizar lenguaje de DataTables 1.x a 2.x: `processing`, `lengthMenu`, `zeroRecords`, `emptyTable`, `info`, `infoEmpty`, `infoFiltered`, `search` (vacío), `searchPlaceholder`, `paginate`, `aria`.
   - Mantener `serverSide: true`, `pageLength: 10`.
   - Eliminar `sLoadingRecords`, `oPaginate`, `oAria`, `buttons` (claves 1.x no usadas en 2.x).

3. Actualizar la lógica JavaScript:
   - Reemplazar `ClearFilterData`: usar `KTSelectHelper.setValue('#CategoryId', 1)` y `KTSelectHelper.clear(...)` para `#AccountTypeId`, `#InstrumentId`, `#FrameId`, `#DirectionId`, igual que en Home/Index. Luego llamar a `LoadDataTable()`.
   - Mantener `SearchData`: llama a `LoadDataTable()`.
   - Mantener `LoadDataTable`: recoge los valores de los 5 selects, construye el objeto `data`, destruye/recrea la tabla con `$.fn.DataTable.isDataTable` + `destroy()`.
   - Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback`.
   - Mantener `KTMenu.createInstances()` en `drawCallback`.
   - Mantener la llamada inicial: `$(document).ready` llama a `LoadDataTable()`.

4. Migrar `renderProgressBar` en `Web/wwwroot/Template/custom/js/Utilities.js` de Bootstrap a Tailwind:
   - Reemplazar `d-flex align-items-center w-100` por `flex items-center w-full`.
   - Reemplazar `progress h-6px w-100 me-2` por `flex-1 h-1.5 rounded-full bg-gray-200 me-2` con clase dinámica de color de fondo.
   - Reemplazar `progress-bar` por `h-full rounded-full` con clase dinámica de color de barra.
   - Reemplazar `text-gray-500 fw-semibold` por `text-gray-500 font-semibold`.
   - Mapeo de colores Bootstrap → Tailwind: `bg-danger` → `bg-red-500`, `bg-light-danger` → `bg-red-100`, `bg-primary` → `bg-blue-500`, `bg-light-primary` → `bg-blue-100`, `bg-warning` → `bg-amber-500`, `bg-light-warning` → `bg-amber-100`, `bg-success` → `bg-green-500`, `bg-light-success` → `bg-green-100`.
   - `renderSLPChart`, `renderTP1PChart`, `renderTP2PChart`, `renderTP3PChart` heredan el cambio automáticamente por delegación.

5. Validar manualmente en desktop y mobile:
   - La tabla carga datos desde `~/AnalyticsTrigger/JsonDataTable` con los filtros por defecto (CategoryId=1, resto vacíos).
   - El botón "Filtro" abre el drawer correctamente.
   - Los 5 `kt-select` muestran las opciones, permiten búsqueda y selección.
   - Al presionar "Aplicar", el drawer se cierra y la tabla se recarga con los filtros seleccionados.
   - Al presionar "Limpiar", CategoryId vuelve a 1, los demás se vacían, el drawer se cierra y la tabla se recarga.
   - El search con `kt-input` filtra correctamente dentro de los datos cargados.
   - Las 12 columnas se renderizan correctamente: Código, Descripción, Cantidad, SL, TP1, TP2, TP3, y las barras de progreso (SL%, TP1%, TP2%, TP3%) con los nuevos estilos Tailwind.
   - La columna Estatus muestra los badges "Válido" / "Pausa" correctamente.
   - Las barras de progreso se ven correctamente con los colores y porcentajes apropiados.
   - Paginación, ordenamiento y filtrado funcionan sin errores.
   - La vista es usable en desktop y mobile (tabla scrolleable, drawer funcional, selects usables).

6. Ejecutar `dotnet build "TradingBookApp.sln"` desde la raíz del repositorio y confirmar que compila sin errores nuevos.

## Criterios de aceptación

- [ ] `Web/Views/AnalyticsTrigger/Index.cshtml` usa markup y clases visuales de Metronic Tailwind (header container con `kt-container-fixed`, card con `kt-card kt-card-grid`, tabla con `kt-table`).
- [ ] El header container muestra el título "Análisis de Gatillos", subtítulo y un botón "Filtro" con `kt-btn kt-btn-outline` y `data-kt-drawer-toggle="#filter_drawer"`.
- [ ] El drawer `#filter_drawer` se abre al presionar el botón "Filtro" y contiene los 5 filtros: Categoría, Tipo de Cuenta, Instrumento, Frame y Dirección, cada uno con `kt-select`.
- [ ] El drawer usa la misma estructura que Home/Index: `kt-drawer kt-drawer-end`, `kt-card-header`, `kt-card-content kt-scrollable-y-auto`, `kt-card-footer`, labels con `kt-form-label max-w-56`, selects con clase `kt-select` y data-attributes de KTUI.
- [ ] Los 5 `kt-select` se inicializan correctamente dentro del drawer, permiten búsqueda y muestran las opciones cargadas desde el controller.
- [ ] Al presionar "Aplicar" en el drawer: el drawer se cierra y la tabla se recarga con los filtros seleccionados.
- [ ] Al presionar "Limpiar" en el drawer: CategoryId se resetea a 1, los demás selects se vacían, el drawer se cierra y la tabla se recarga.
- [ ] `ClearFilterData` usa `KTSelectHelper.setValue('#CategoryId', 1)` y `KTSelectHelper.clear(...)` para los otros 4 selects.
- [ ] DataTables sigue cargando datos desde `~/AnalyticsTrigger/JsonDataTable` con `serverSide: true` y los parámetros `categoryId`, `accountTypeId`, `instrumentId`, `frameId`, `directionId`.
- [ ] El search de DataTables usa `kt-input` con el patrón `initComplete` + `layout`.
- [ ] Las 12 columnas se mantienen sin cambios: `code`, `description`, `quantity`, `sl`, `tP1`, `tP2`, `tP3`, `slp`, `tP1P`, `tP2P`, `tP3P`, `valid`.
- [ ] Las columnas de porcentaje (`slp`, `tP1P`, `tP2P`, `tP3P`) renderizan barras de progreso con clases Tailwind (`flex`, `h-1.5`, `rounded-full`, `bg-red-500`, `bg-red-100`, etc.).
- [ ] Las clases `className` de las columnas usan el formato `min-w-[Xpx]`.
- [ ] El lenguaje de DataTables usa claves modernas de 2.x (`processing`, `searchPlaceholder`, `paginate`).
- [ ] `searching: true` e `info: true` están activos en la configuración de DataTables.
- [ ] No existe `filter: true` ni `filter: false` en la configuración de DataTables.
- [ ] No hay tooltips Bootstrap (`data-toggle="tooltip"`) en Index.cshtml.
- [ ] No hay breadcrumb en Index.cshtml.
- [ ] No hay dropdown menu `kt_menu_filter` en Index.cshtml.
- [ ] No hay referencias a Select2 (`data_control="select2"`) en Index.cshtml.
- [ ] No existe columna `task` ni acciones de fila en la tabla.
- [ ] No hay modales (AnalyticsTrigger no los tenía y no se agregan).
- [ ] No se modifica `AnalyticsTriggerController`, sus servicios, repositorios, entidades, DTOs ni `AnalyticsTriggerViewModel`.
- [ ] `dotnet build "TradingBookApp.sln"` termina correctamente, permitiendo solo warnings preexistentes no relacionados con esta migración.

## Decisiones

- **Sí:** Migrar únicamente `Web/Views/AnalyticsTrigger/Index.cshtml` y la función `renderProgressBar` en `Utilities.js`. AnalyticsTrigger no tiene vistas `New`, `Edit` ni `Delete`; el controller solo expone `Index` y `JsonDataTable`.
- **Sí:** Usar el patrón de filtro de `Home/Index.cshtml` con drawer (`kt-drawer kt-drawer-end`) y `kt-select` de KTUI. El dropdown menu actual (`kt_menu_filter`) con Select2 se reemplaza por un drawer idéntico al de Home: header con título y botón dismiss, content con los 5 selects, footer con botones "Limpiar" y "Aplicar".
- **Sí:** Reemplazar los 5 Select2 por `kt-select` de KTUI con `@Html.DropDownList` y data-attributes. Home/Index ya usa esta configuración exitosamente con los mismos 5 campos de filtro. Select2 depende de jQuery y su migración a KTUI reduce esa dependencia progresivamente.
- **Sí:** Usar `KTSelectHelper.setValue()` y `KTSelectHelper.clear()` para manipular los selects, igual que Home/Index. El helper ya está disponible vía `select.helper.js` cargado globalmente.
- **Sí:** Mantener el valor por defecto de `CategoryId = 1` al limpiar filtros. Este comportamiento existe en el código actual y en Home/Index; cambiarlo alteraría la funcionalidad sin beneficio.
- **Sí:** Replicar la estructura de card + tabla de `Web/Views/CatFigure/Index.cshtml`. La tabla, el search con `kt-input` y el layout de DataTables deben ser consistentes con todos los módulos ya migrados.
- **Sí:** Migrar `renderProgressBar` en `Utilities.js` de Bootstrap a Tailwind. Las barras de progreso son el único elemento visual de AnalyticsTrigger que aún depende de clases Bootstrap. Migrarlas completa la transición visual del módulo y no afecta a otros consumidores (solo AnalyticsTrigger las usa).
- **Sí:** Mapear colores Bootstrap a Tailwind: `bg-danger` → `bg-red-500`, `bg-light-danger` → `bg-red-100`, `bg-primary` → `bg-blue-500`, `bg-light-primary` → `bg-blue-100`, `bg-warning` → `bg-amber-500`, `bg-light-warning` → `bg-amber-100`, `bg-success` → `bg-green-500`, `bg-light-success` → `bg-green-100`.
- **Sí:** Actualizar DataTables al patrón de CatFigure: `$.fn.DataTable.isDataTable` antes de `destroy`, lenguaje 2.x, `searching: true`, `info: true`, `min-w-[Xpx]`, `layout` + `initComplete` con `kt-input`.
- **Sí:** Activar `searching: true` e `info: true`. La vista actual los tiene desactivados (`searching: false`, `info: false`); activarlos mejora la usabilidad y es consistente con todos los módulos migrados.
- **Sí:** Eliminar el breadcrumb. Todos los módulos migrados (Employees, CatCategory, CatFigure, CatAccountType, CatFrame, CatInstruments, Roles, Users, Logs) ya eliminaron el suyo.
- **Sí:** Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback`. Ningún catálogo migrado usa tooltips Bootstrap; AnalyticsTrigger no tiene elementos con `data-toggle="tooltip"` en la tabla.
- **Sí:** Agregar `KTMenu.createInstances()` en el `drawCallback`. Aunque AnalyticsTrigger no tiene columna `task` ni `kt-menu`, mantener esta llamada previene problemas si en el futuro se agregan acciones o menús contextuales.
- **Sí:** No agregar script tag para `dataTables.min.js` en la vista. Ya se carga globalmente en `_Layout.cshtml`; duplicarlo podría causar conflictos de inicialización.
- **No:** Agregar columna de acciones (`task`). AnalyticsTrigger es una vista de consulta analítica, no de administración. Si se necesita en el futuro, debe ir en su propia spec.
- **No:** Agregar modales ni vistas de edición. AnalyticsTrigger no tiene operaciones CRUD; el controller solo expone `Index` y `JsonDataTable`.
- **No:** Cambiar `AnalyticsTriggerController`, servicios, repositorios, entidades, DTOs o `AnalyticsTriggerViewModel`. Esta spec es solo UI/markup/JS cliente.
- **No:** Cambiar las columnas de la tabla. Las 12 columnas actuales se mantienen sin cambios de nombre, orden o render.
- **No:** Cambiar `renderStatusAnalytics` en `Utilities.js`. Ya usa `kt-badge` (`kt-badge-success`, `kt-badge-warning`) compatible con Metronic Tailwind.
- **No:** Reemplazar `kt-select` por otro componente de selección. KTUI Select es el componente canónico del theme Metronic Tailwind y ya está probado en Home/Index.
- **No:** Eliminar Bootstrap, jQuery, DataTables o KTUI. La eliminación de dependencias legacy es una iniciativa transversal que requiere su propia spec.
- **No:** Hacer un rediseño completo de AnalyticsTrigger. El objetivo es migrar el markup a Metronic Tailwind conservando el comportamiento existente.

## Riesgos

| Riesgo | Mitigación |
| ------ | ---------- |
| Los `kt-select` dentro del drawer `#filter_drawer` no se inicializan correctamente porque el drawer está oculto con `hidden` al cargar la página. | Home/Index ya usa exitosamente la misma estructura: drawer con `hidden` y `kt-select` en su interior. KTUI inicializa los componentes con data-attributes al cargar el DOM, sin importar visibilidad. Validar que los selects responden al abrir el drawer por primera vez. |
| `renderProgressBar` se comparte desde `Utilities.js`. Si otro módulo no migrado también la usara, el cambio de Bootstrap a Tailwind les afectaría visualmente. | Solo AnalyticsTrigger usa `renderSLPChart`, `renderTP1PChart`, `renderTP2PChart` y `renderTP3PChart`, que son los únicos consumidores de `renderProgressBar`. Ningún otro módulo la referencia. |
| El mapeo de colores Bootstrap → Tailwind produce barras con apariencia diferente (diferente saturación o contraste). | Los colores mapeados (`bg-red-500`/`bg-red-100`, `bg-blue-500`/`bg-blue-100`, `bg-amber-500`/`bg-amber-100`, `bg-green-500`/`bg-green-100`) son los equivalentes funcionales definidos en el theme de Metronic Tailwind. Validar visualmente las 4 barras en la tabla. |
| `LoadDataTable` destruye y recrea la tabla en cada llamada. Si el usuario presiona "Aplicar" múltiples veces rápidamente, podría haber conflictos de reinicialización. | El chequeo `$.fn.DataTable.isDataTable('#dtTable')` antes de `destroy()` previene intentos de destrucción sobre instancias inexistentes, igual que en CatFigure y Logs. |
| `dataTables.min.js` se carga globalmente en `_Layout.cshtml` pero la vista no lo declara. Si en el futuro se elimina del layout, AnalyticsTrigger dejaría de funcionar. | Todos los módulos migrados (9 specs) dependen de la carga global; una eventual eliminación requeriría una spec transversal. Este riesgo es compartido y no específico de AnalyticsTrigger. |
| Los `kt-select` del drawer comparten IDs con los `kt-select` de Home/Index si ambas vistas se renderizan simultáneamente (ej. vía partials en el dashboard). | Home/Index y AnalyticsTrigger/Index son páginas distintas, no se renderizan juntas. Los IDs (`#CategoryId`, `#AccountTypeId`, etc.) no colisionan en una misma página. |
| Warnings preexistentes del build se confunden con regresiones de esta migración. | Ejecutar `dotnet build "TradingBookApp.sln"` antes y después de la migración, separando warnings preexistentes de errores nuevos. |
