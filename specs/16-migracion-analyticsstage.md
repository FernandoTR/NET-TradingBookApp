# SPEC 16 — Migración de AnalyticsStage/Index a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 11 · **Fecha:** 2026-05-24
> **Objetivo:** Migrar `AnalyticsStage/Index` a componentes visuales de Metronic Tailwind, consolidando la información de porcentaje dentro de las columnas principales (SL, TP1, TP2, TP3) y replicando el patrón de filtrado vía drawer de `AnalyticsTrigger/Index`.

## Alcance

**Incluye:**

- Migrar `Web/Views/AnalyticsStage/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de header container y card + tabla de `AnalyticsTrigger/Index`.
- Reemplazar el toolbar Bootstrap actual por header container (`kt-container-fixed`) con título "Análisis de Etapa", subtítulo "Consulta el rendimiento de las etapas configuradas en el sistema." y botón "Filtro" que abre un drawer (`#filter_drawer`).
- Replicar el drawer `#filter_drawer` de `AnalyticsTrigger/Index` con los 5 filtros (`CategoryId`, `AccountTypeId`, `InstrumentId`, `FrameId`, `DirectionId`) usando `kt-select`.
- Eliminar las columnas explícitas de porcentaje (`SL %`, `TP1 %`, `TP2 %`, `TP3 %`).
- Consolidar las barras de progreso dentro de las columnas `SL`, `TP1`, `TP2`, `TP3` asignando las funciones de render `renderSLPChart`, `renderTP1PChart`, `renderTP2PChart`, `renderTP3PChart` directamente a cada columna respectiva.
- Actualizar la configuración de DataTables al patrón 2.x (`layout`, `initComplete` con `kt-input`, `searching: false`, `info: true`, lenguaje 2.x, placeholder "Buscar etapas...").
- Eliminar breadcrumbs, dropdown de filtro vía `kt-menu`, tooltips Bootstrap y referencias a Select2.
- Adoptar `$.fn.DataTable.isDataTable('#dtTable')` antes de destroy y `KTSelectHelper` para manipulación de selects.

**Fuera de alcance (para specs futuras):**

- Cambios en `AnalyticsStageController`, servicios, repositorios o DTOs.
- Cambios en los contratos JSON de `~/AnalyticsStage/JsonDataTable`.
- Agregar vistas `New`, `Edit`, `Delete` o modales.
- Migración de dependencias base (Bootstrap, jQuery, etc.) de todo el proyecto.

## Data model

Esta funcionalidad no introduce nuevas estructuras de datos en el backend.

Se reutilizan sin cambios las estructuras existentes:

```csharp
Web.Models.AnalyticsStageViewModel
```

El controlador `AnalyticsStageController.JsonDataTable` se mantiene sin modificaciones. La lógica de consolidación de porcentajes dentro de las columnas principales (`SL`, `TP1`, `TP2`, `TP3`) se realizará exclusivamente asignando las funciones de render `renderSLPChart`, `renderTP1PChart`, `renderTP2PChart`, `renderTP3PChart` directamente a cada columna.

Se mantienen los contratos cliente existentes de la tabla:

```text
POST ~/AnalyticsStage/JsonDataTable (parámetros extra: categoryId, accountTypeId, instrumentId, frameId, directionId)
```

## Plan de implementación

1. **Migrar el markup de `Web/Views/AnalyticsStage/Index.cshtml` a Metronic Tailwind:**
   - Reemplazar el toolbar Bootstrap y layout actual por header container con `kt-container-fixed`, título "Análisis de Etapa", subtítulo "Consulta el rendimiento de las etapas configuradas en el sistema." y botón "Filtro" con `kt-btn kt-btn-outline` y `data-kt-drawer-toggle="#filter_drawer"`.
   - Eliminar el breadcrumb existente.
   - Implementar el drawer `#filter_drawer` (`kt-drawer kt-drawer-end`) replicando la estructura de `AnalyticsTrigger/Index` con los 5 filtros (`kt-select`) y botones "Limpiar" / "Aplicar".
   - Configurar la card (`kt-card kt-card-grid`) y la tabla (`kt-table` con `data-kt-datatable-table="true"`).

2. **Actualizar DataTables en el bloque `<script>`:**
   - Reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')`.
   - Agregar configuración `layout` e `initComplete` con `kt-input` para búsqueda, placeholder "Buscar etapas...".
   - **Modificar columnas:**
       - Eliminar las columnas explícitas `slp`, `tP1P`, `tP2P`, `tP3P`.
       - Asignar `render: renderSLPChart` a la columna `sl`, `render: renderTP1PChart` a `tP1`, `render: renderTP2PChart` a `tP2`, `render: renderTP3PChart` a `tP3`.
       - Marcar las columnas `sl`, `tP1`, `tP2`, `tP3` como `orderable: false`.
   - Ajustar `className` a formato `min-w-[Xpx]`.
   - Activar `searching: false`, `info: true`.
   - Actualizar lenguaje a formato 2.x (`processing`, `lengthMenu`, `paginate`, etc.).

3. **Actualizar lógica JavaScript:**
   - Ajustar `ClearFilterData` para usar `KTSelectHelper.setValue('#CategoryId', 1)` y `KTSelectHelper.clear(...)` en los demás selects.
   - Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback`.
   - Agregar animación de barras de progreso en `drawCallback` (mismo bloque de `requestAnimationFrame` que `AnalyticsTrigger`).
   - Eliminar cualquier referencia residual a Select2.

4. **Validar la migración:**
   - Confirmar visualmente que las barras de progreso se renderizan correctamente dentro de las celdas de `SL`/`TP1`/`TP2`/`TP3`.
   - Validar funcionalidad de filtros, drawer y DataTables en desktop.
   - Ejecutar `dotnet build "TradingBookApp.sln"` para asegurar compilación correcta.

## Criterios de aceptación

- [ ] `Web/Views/AnalyticsStage/Index.cshtml` usa markup y clases visuales de Metronic Tailwind (header container, card, tabla).
- [ ] El header muestra el título "Análisis de Etapa", subtítulo y botón "Filtro" que abre el drawer.
- [ ] El drawer `#filter_drawer` contiene los 5 filtros (`kt-select`: Categoría, Tipo de Cuenta, Instrumento, Frame, Dirección) y funciona idéntico a `AnalyticsTrigger`.
- [ ] Las columnas explícitas (`SL %`, `TP1 %`, `TP2 %`, `TP3 %`) han sido eliminadas.
- [ ] Las columnas `SL`, `TP1`, `TP2`, `TP3` renderizan la barra de progreso directamente (usando `renderSLPChart`, `renderTP1PChart`, `renderTP2PChart`, `renderTP3PChart`).
- [ ] La configuración de DataTables incluye `searching: false`, `info: true`, formato 2.x (`layout`, `initComplete`), placeholder "Buscar etapas..." y `min-w-[Xpx]`.
- [ ] El comportamiento de los filtros (Limpiar, Aplicar, carga vía AJAX) funciona correctamente con `KTSelectHelper`.
- [ ] `ClearFilterData` setea `CategoryId` a `1` y limpia los demás selects.
- [ ] Se eliminaron elementos Bootstrap legacy (breadcrumb, tooltips, Select2, dropdown `kt-menu`).
- [ ] `dotnet build "TradingBookApp.sln"` compila sin nuevos errores.

## Decisiones tomadas y descartadas

- **Sí:** Consolidar las barras de porcentaje dentro de las columnas `SL`, `TP1`, `TP2`, `TP3` mediante las funciones de render existentes (`renderSLPChart`, `renderTP1PChart`, etc.). Elimina columnas redundantes y mejora la legibilidad al asociar visualmente el porcentaje con su valor.
- **Sí:** Replicar el patrón de diseño de `AnalyticsTrigger/Index` para el drawer de filtros (`#filter_drawer`), el uso de `kt-select`, la configuración de DataTables 2.x y la animación de barras en `drawCallback`. Garantiza consistencia en toda la suite de reportes analíticos.
- **Sí:** Mantener `CategoryId` con valor por defecto `1` al limpiar filtros (`KTSelectHelper.setValue`), consistente con `AnalyticsTrigger`.
- **Sí:** Mantener `AnalyticsStageController` y DTOs sin cambios. La migración es puramente de interfaz de usuario.
- **No:** Crear nuevas columnas en la tabla ni modificar los contratos del backend.
- **No:** Mantener dependencias legacy (Bootstrap, Select2, tooltips, `kt-menu` dropdown) en la nueva vista.

## Riesgos identificados

| Riesgo | Mitigación |
| ------ | ---------- |
| La consolidación de barras en las celdas `SL`/`TP1`/`TP2`/`TP3` provoca desalineación o desbordamiento visual. | Las funciones de render existentes (`renderSLPChart`, etc.) ya están probadas en `AnalyticsTrigger` y `AnalyticsDirection`; usar las mismas clases Tailwind asegura consistencia. |
| La ordenación de las columnas `SL`, `TP1`, `TP2`, `TP3` falla por el nuevo `render` que combina HTML. | Las columnas se marcan como `orderable: false`, igual que en `AnalyticsTrigger` y `AnalyticsDirection`. |
| Conflictos de CSS por residuos de clases antiguas (Bootstrap/Metronic legacy) que interfieran con Tailwind. | Eliminar completamente el markup anterior; el nuevo markup usa exclusivamente clases `kt-*` y utilidades Tailwind. |
