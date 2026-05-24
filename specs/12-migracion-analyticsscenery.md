# SPEC 12 — Migración de AnalyticsScenery/Index a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01, SPEC 11 · **Fecha:** 2026-05-24
> **Objetivo:** Migrar `AnalyticsScenery/Index` a componentes visuales de Metronic Tailwind conservando el comportamiento actual de DataTables y el filtro de escenarios vía drawer con kt-select, replicando exactamente el diseño de `AnalyticsTrigger/Index`.

## Alcance

**Incluye:**

- Migrar `Web/Views/AnalyticsScenery/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando exactamente la estructura de `Web/Views/AnalyticsTrigger/Index.cshtml` (título + subtítulo + botón "Filtro" que abre un drawer) y la estructura de card + tabla de `Web/Views/CatFigure/Index.cshtml`.
- Reemplazar el toolbar Bootstrap actual (`kt_app_toolbar`, `kt_app_content`, `app-container`, `card`, `card-body`) por un header container con `kt-container-fixed`, título "Análisis de Escenarios", subtítulo y botón "Filtro" con `kt-btn kt-btn-outline` y `data-kt-drawer-toggle="#filter_drawer"`.
- Reemplazar el dropdown menu de filtro (`kt_menu_filter`) por un drawer (`kt-drawer kt-drawer-end`) que contenga los 5 filtros con `kt-select`, siguiendo la estructura exacta de `#filter_drawer` de `AnalyticsTrigger/Index`: `kt-card-header` con título "Filtros Disponibles" y botón dismiss, `kt-card-content kt-scrollable-y-auto` con los dropdowns, y `kt-card-footer` con botones "Limpiar" / "Aplicar".
- Migrar los 5 dropdowns (`CategoryId`, `AccountTypeId`, `InstrumentId`, `FrameId`, `DirectionId`) a `kt-select` de KTUI con `@Html.DropDownList`, clase `kt-select` y data-attributes (`data_kt_select`, `data_kt_select_enable_search`, `data_kt_select_placeholder`, `data_kt_select_config`), replicando la configuración de `AnalyticsTrigger/Index`.
- Reemplazar la lógica de `ClearFilterData` para usar `KTSelectHelper.setValue('#CategoryId', 1)` y `KTSelectHelper.clear(...)` para los otros 4 selects, igual que en `AnalyticsTrigger/Index`.
- Eliminar el breadcrumb existente.
- Contenedor de tabla con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable` con clase `kt-table` y `data-kt-datatable-table="true"`.
- Mantener DataTables con server-side AJAX hacia `~/AnalyticsScenery/JsonDataTable`, pasando `categoryId`, `accountTypeId`, `instrumentId`, `frameId`, `directionId` como parámetros extra.
- Mantener las 11 columnas activas actuales: `description`, `quantity`, `sl`, `tP1`, `tP2`, `tP3`, `slp` (render `renderSLPChart`), `tP1P` (render `renderTP1PChart`), `tP2P` (render `renderTP2PChart`), `tP3P` (render `renderTP3PChart`), `valid` (render `renderStatusAnalytics`).
- No incluir la columna `code`, la cual ya está desactivada/comentada en el código original.
- Actualizar DataTables: reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
- Agregar `layout` (topStart, topEnd, bottomStart, bottomEnd) e `initComplete` para search con `kt-input` (placeholder "Buscar escenarios...").
- Convertir clases `className` de las columnas de `min-w-Xpx` a `min-w-[Xpx]`.
- Eliminar `filter: true` y `filter: false` (opciones inválidas/no estándar de DataTables).
- Activar `searching: true` (reemplazando `searching: false`).
- Activar `info: true` (reemplazando `info: false`).
- Actualizar lenguaje de DataTables 1.x a 2.x (`processing`, `lengthMenu`, `zeroRecords`, `emptyTable`, `info`, `infoEmpty`, `infoFiltered`, `search`, `searchPlaceholder`, `paginate`, `aria`).
- Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback`.
- Mantener `KTMenu.createInstances()` en `drawCallback`.
- Mantener `serverSide: true`, `pageLength: 10`.
- No agregar script tag para `dataTables.min.js` (ya se carga globalmente).
- Validar manualmente la vista AnalyticsScenery en desktop y mobile.
- Confirmar que la aplicación sigue compilando con `dotnet build "TradingBookApp.sln"`.

**Fuera de alcance (para specs futuras):**

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `AnalyticsSceneryController`, servicios, repositorios, DTOs, entidades o `AnalyticsSceneryViewModel`.
- Cambiar contratos JSON de `~/AnalyticsScenery/JsonDataTable`.
- Agregar vistas `New`, `Edit` o `Delete` (no existen actualmente).
- Agregar columna de acciones (`task`) a la tabla.
- Agregar modales.
- Agregar, quitar o renombrar columnas de la tabla.
- Reemplazar `kt-select` por otro componente de selección.
- Cambiar la lógica de filtrado del controller o del stored procedure.
- Rediseñar completamente la experiencia visual de AnalyticsScenery.
- Eliminar Bootstrap, jQuery o DataTables de la aplicación general.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos backend.

Se reutilizan sin cambios:

```csharp
Web.Models.AnalyticsSceneryViewModel
Application.DTOs.GetTBAnalyticsSceneryDto
```

El controlador `AnalyticsSceneryController.JsonDataTable` se mantiene sin modificaciones. Retorna las columnas `description`, `quantity`, `sl`, `tP1`, `tP2`, `tP3`, `slp`, `tP1P`, `tP2P`, `tP3P`, `valid`. AnalyticsScenery no tiene columna `task` ni usa `ActionButtonHelper`.

Se mantienen los contratos cliente existentes de la tabla:

```text
POST ~/AnalyticsScenery/JsonDataTable  (parámetros extra: categoryId, accountTypeId, instrumentId, frameId, directionId)
```

La única vista involucrada es `Web/Views/AnalyticsScenery/Index.cshtml`. No existen vistas `New`, `Edit` ni `Delete`.

Las funciones de renderizado en `Web/wwwroot/Template/custom/js/Utilities.js` (`renderProgressBar`, `renderSLPChart`, `renderTP1PChart`, `renderTP2PChart`, `renderTP3PChart` y `renderStatusAnalytics`) se reutilizan tal como están (habiéndose migrado ya a Tailwind en la SPEC 11).

## Plan de implementación

1. **Migrar el markup de `Web/Views/AnalyticsScenery/Index.cshtml` a Metronic Tailwind:**
   - Reemplazar todo el toolbar Bootstrap y layout (`kt_app_toolbar`, `kt_app_content`, `app-container`, `card`, `card-body`, `d-flex flex-column flex-column-fluid`) por un header container con `kt-container-fixed`.
   - Agregar el título "Análisis de Escenarios", subtítulo "Consulta el rendimiento de los escenarios de mercado configurados." y un botón "Filtro" con `kt-btn kt-btn-outline` y `data-kt-drawer-toggle="#filter_drawer"`.
   - Eliminar el breadcrumb completo.
   - Reemplazar el dropdown menu de filtro (`kt_menu_filter`) por un drawer `#filter_drawer` con clase `kt-drawer kt-drawer-end`, replicando la estructura de `AnalyticsTrigger/Index.cshtml`: `kt-card-header` con título "Filtros Disponibles" y botón dismiss, `kt-card-content kt-scrollable-y-auto` con 5 filas de filtro (Categoría, Tipo de Cuenta, Instrumento, Frame, Dirección), y `kt-card-footer` con botones "Limpiar" y "Aplicar".
   - Cada fila de filtro usa `flex items-baseline flex-wrap lg:flex-nowrap gap-2.5`, label con `kt-form-label max-w-56` y `@Html.DropDownList` con clase `kt-select`, data-attributes de KTUI (`data_kt_select="true"`, `data_kt_select_enable_search="true"`, `data_kt_select_search_placeholder="Buscar..."`, `data_kt_select_placeholder="Selecciona uno..."`, `data_kt_select_config="{''optionsClass'': ''kt-scrollable overflow-auto max-h-[250px]''}"`).
   - Botón "Limpiar": `kt-btn kt-btn-outline`, `data-kt-drawer-dismiss="true"`, llama a `ClearFilterData()`.
   - Botón "Aplicar": `kt-btn kt-btn-primary grow`, `data-kt-drawer-dismiss="true"`, `id="btn_AplicarFiltro"`, llama a `SearchData()`.
   - Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable` con clase `kt-table` y `data-kt-datatable-table="true"`.

2. **Actualizar DataTables en el bloque `<script>`:**
   - Reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
   - Agregar `layout` (topStart: [], topEnd: 'search', bottomStart: ['pageLength', 'info'], bottomEnd: 'paging').
   - Agregar `initComplete` para el search con `kt-input` (placeholder "Buscar escenarios...").
   - Mantener todas las columnas actuales con sus `data`, `name`, `autoWidth` y `render` (excluyendo `code` ya que está comentada/desactivada).
   - Convertir clases `className` de las columnas de `min-w-Xpx` a `min-w-[Xpx]`.
   - Eliminar `filter: true` y `filter: false`.
   - Reemplazar `searching: false` por `searching: true`.
   - Reemplazar `info: false` por `info: true`.
   - Actualizar lenguaje de DataTables 1.x a 2.x: `processing`, `lengthMenu`, `zeroRecords`, `emptyTable`, `info`, `infoEmpty`, `infoFiltered`, `search` (vacío), `searchPlaceholder`, `paginate`, `aria`.
   - Mantener `serverSide: true`, `pageLength: 10`.
   - Eliminar claves obsoletas de DataTables 1.x (`sLoadingRecords`, `oPaginate`, `oAria`, `buttons`).

3. **Actualizar la lógica JavaScript:**
   - Reemplazar `ClearFilterData`: usar `KTSelectHelper.setValue('#CategoryId', 1)` y `KTSelectHelper.clear(...)` para `#AccountTypeId`, `#InstrumentId`, `#FrameId`, `#DirectionId`, igual que en `AnalyticsTrigger/Index.cshtml`. Luego llamar a `LoadDataTable()`.
   - Mantener `SearchData`: llama a `LoadDataTable()`.
   - Mantener `LoadDataTable`: recoge los valores de los 5 selects, construye el objeto `data`, destruye/recrea la tabla con `$.fn.DataTable.isDataTable` + `destroy()`.
   - Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback`.
   - Mantener `KTMenu.createInstances()` en `drawCallback`.
   - Mantener la llamada inicial: `$(document).ready` llama a `LoadDataTable()`.

4. **Validar manualmente en desktop y mobile:**
   - La tabla carga datos desde `~/AnalyticsScenery/JsonDataTable` con los filtros por defecto (CategoryId=1, resto vacíos).
   - El botón "Filtro" abre el drawer correctamente.
   - Los 5 `kt-select` muestran las opciones, permiten búsqueda y selección.
   - Al presionar "Aplicar", el drawer se cierra y la tabla se recarga con los filtros seleccionados.
   - Al presionar "Limpiar", CategoryId vuelve a 1, los demás se vacían, el drawer se cierra y la tabla se recarga.
   - El search con `kt-input` filtra correctamente dentro de los datos cargados.
   - Las 11 columnas se renderizan correctamente con los nuevos estilos.
   - Paginación, ordenamiento y filtrado funcionan sin errores.
   - La vista es usable en desktop y mobile (tabla scrolleable, drawer funcional, selects usables).

5. **Ejecutar `dotnet build "TradingBookApp.sln"` desde la raíz del repositorio y confirmar que compila sin errores nuevos.**

## Criterios de aceptación

- [ ] `Web/Views/AnalyticsScenery/Index.cshtml` usa markup y clases visuales de Metronic Tailwind (header container con `kt-container-fixed`, card con `kt-card kt-card-grid`, tabla con `kt-table`).
- [ ] El header container muestra el título "Análisis de Escenarios", subtítulo y un botón "Filtro" con `kt-btn kt-btn-outline` y `data-kt-drawer-toggle="#filter_drawer"`.
- [ ] El drawer `#filter_drawer` se abre al presionar el botón "Filtro" y contiene los 5 filtros: Categoría, Tipo de Cuenta, Instrumento, Frame y Dirección, cada uno con `kt-select`.
- [ ] El drawer usa la misma estructura que `AnalyticsTrigger/Index`: `kt-drawer kt-drawer-end`, `kt-card-header`, `kt-card-content`, `kt-card-footer`, labels con `kt-form-label max-w-56`, selects con clase `kt-select` y data-attributes de KTUI.
- [ ] Los 5 `kt-select` se inicializan correctamente dentro del drawer, permiten búsqueda y muestran las opciones cargadas desde el controller.
- [ ] Al presionar "Aplicar" en el drawer: el drawer se cierra y la tabla se recarga con los filtros seleccionados.
- [ ] Al presionar "Limpiar" en el drawer: CategoryId se resetea a 1, los demás selects se vacían, el drawer se cierra y la tabla se recarga.
- [ ] `ClearFilterData` usa `KTSelectHelper.setValue('#CategoryId', 1)` y `KTSelectHelper.clear(...)` para los otros 4 selects.
- [ ] DataTables sigue cargando datos desde `~/AnalyticsScenery/JsonDataTable` con `serverSide: true` y los parámetros `categoryId`, `accountTypeId`, `instrumentId`, `frameId`, `directionId`.
- [ ] El search de DataTables usa `kt-input` con el patrón `initComplete` + `layout`.
- [ ] Las 11 columnas se mantienen sin cambios: `description`, `quantity`, `sl`, `tP1`, `tP2`, `tP3`, `slp`, `tP1P`, `tP2P`, `tP3P`, `valid`. No se incluye la columna `code`.
- [ ] Las columnas de porcentaje (`slp`, `tP1P`, `tP2P`, `tP3P`) renderizan barras de progreso delegando en las funciones CSS de Tailwind de `Utilities.js`.
- [ ] Las clases `className` de las columnas usan el formato `min-w-[Xpx]`.
- [ ] El lenguaje de DataTables usa claves modernas de 2.x (`processing`, `searchPlaceholder`, `paginate`).
- [ ] `searching: true` e `info: true` están activos en la configuración de DataTables.
- [ ] No existe `filter: true` ni `filter: false` en la configuración de DataTables.
- [ ] No hay tooltips Bootstrap (`data-toggle="tooltip"`) en `Index.cshtml`.
- [ ] No hay breadcrumb en `Index.cshtml`.
- [ ] No hay dropdown menu `kt_menu_filter` en `Index.cshtml`.
- [ ] No hay referencias a Select2 (`data_control="select2"`) en `Index.cshtml`.
- [ ] No existe columna `task` ni acciones de fila en la tabla.
- [ ] No se modifica `AnalyticsSceneryController`, sus servicios, repositorios, entidades, DTOs ni `AnalyticsSceneryViewModel`.
- [ ] `dotnet build "TradingBookApp.sln"` termina correctamente, sin nuevos errores de compilación.

## Decisiones tomadas y descartadas

- **Sí: Copiar exactamente el diseño de `AnalyticsTrigger/Index`.** Tanto `AnalyticsTrigger` como `AnalyticsScenery` son vistas analíticas sumamente similares en su estructura y lógica de negocio. Duplicar la estrategia de drawer con `kt-select` y el layout de DataTables garantiza total homogeneidad visual en la sección de analítica.
- **Sí: Omitir la columna de `code` (Código).** En la vista actual de `AnalyticsScenery/Index.cshtml`, la columna `code` se encuentra comentada. Mantenerla fuera del alcance respeta el estado actual de la vista y evita mostrar información que no está soportada por el stored procedure subyacente de escenarios.
- **Sí: Activar `searching: true` e `info: true`.** Aunque la vista original los tenía en `false`, los demás catálogos y la vista `AnalyticsTrigger` los tienen activos. Al unificar bajo el patrón de Metronic Tailwind, habilitar la búsqueda dinámica por texto a través de `kt-input` añade valor y usabilidad sin costo de rendimiento.
- **Sí: Mantener `CategoryId = 1` como valor por defecto al limpiar.** Replicar este comportamiento asegura consistencia con el flujo actual del sistema y la SPEC 11.
- **No: Reemplazar o reescribir `renderProgressBar` de `Utilities.js`.** Dado que la SPEC 11 ya migró con éxito esta función compartida a Tailwind, no es necesario realizar ningún cambio en `Utilities.js` dentro de esta especificación. Simplemente reutilizaremos las funciones de renderizado actuales.
- **No: Modificar el backend o el stored procedure.** La lógica y los contratos JSON de `AnalyticsSceneryController` están probados y no requieren cambios. Esta especificación se limita 100% a la interfaz de usuario (capa de presentación cliente).

## Riesgos identificados

| Riesgo | Mitigación |
| ------ | ---------- |
| Colisión de IDs de selectores si se renderizaran múltiples drawers en una sola página. | `AnalyticsScenery/Index` y `AnalyticsTrigger/Index` son vistas separadas controladas por controladores diferentes. No hay escenarios donde se rendericen juntas en el mismo DOM. |
| Incompatibilidades visuales menores al activar `searching: true` e `info: true` por primera vez en esta tabla. | Se usará el patrón probado de `CatFigure` y `AnalyticsTrigger` para el contenedor de búsqueda y la paginación, garantizando un ajuste perfecto y responsivo en pantallas grandes y móviles. |
| Posibles errores de compilación por variables o dependencias huérfanas al limpiar código legacy. | Se ejecutará `dotnet build` inmediatamente después de la edición para verificar que la solución continúe compilando correctamente. |
