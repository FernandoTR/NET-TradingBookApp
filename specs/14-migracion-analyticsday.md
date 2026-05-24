# SPEC 14 — Migración de AnalyticsDay/Index a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01 · **Fecha:** 2026-05-24
> **Objetivo:** Migrar `AnalyticsDay/Index` a componentes visuales de Metronic Tailwind, consolidando la información de porcentaje dentro de las columnas principales (SL, TP1, TP2, TP3) y replicando el patrón de filtrado vía drawer de `AnalyticsTrigger/Index`.

## Alcance

**Incluye:**

- Migrar `Web/Views/AnalyticsDay/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de header container y card + tabla de `AnalyticsTrigger/Index`.
- Reemplazar el toolbar Bootstrap actual por header container (`kt-container-fixed`) con título "Análisis Diario", subtítulo y botón "Filtro" que abre un drawer (`#filter_drawer`).
- Replicar el drawer `#filter_drawer` de `AnalyticsTrigger/Index` con los 5 filtros (`CategoryId`, `AccountTypeId`, `InstrumentId`, `FrameId`, `DirectionId`) usando `kt-select`.
- Eliminar las columnas explícitas de porcentaje (`SL %`, `TP1 %`, `TP2 %`, `TP3 %`).
- Modificar el renderizado de las columnas `SL`, `TP1`, `TP2`, `TP3` para incluir la barra de progreso (reutilizando la lógica de `renderProgressBar` de `Utilities.js`) dentro de la misma celda de la columna.
- Actualizar la configuración de DataTables siguiendo el patrón de `AnalyticsTrigger/Index` (DataTables 2.x, `searching: true`, `info: true`, `min-w-[Xpx]`, layout con `kt-input`).
- Eliminar breadcrumbs, dropdowns de filtro antiguos, tooltips Bootstrap y referencias a Select2.

**Fuera de alcance (para specs futuras):**

- Cambios en `AnalyticsDayController`, servicios, repositorios o DTOs.
- Cambios en los contratos JSON de `~/AnalyticsDay/JsonDataTable`.
- Agregar vistas `New`, `Edit`, `Delete` o modales.
- Migración de dependencias base (Bootstrap, jQuery, etc.) de todo el proyecto.
- Rediseño de la lógica de negocio subyacente.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos en el backend.

Se reutilizan sin cambios las estructuras existentes:

```csharp
Web.Models.AnalyticsDayViewModel
```

El controlador `AnalyticsDayController.JsonDataTable` se mantiene sin modificaciones. La lógica de consolidación de porcentajes dentro de las columnas principales (`SL`, `TP1`, `TP2`, `TP3`) se realizará exclusivamente en el `render` de DataTables en el cliente.

Se mantienen los contratos cliente existentes de la tabla:

```text
POST ~/AnalyticsDay/JsonDataTable (parámetros extra: categoryId, accountTypeId, instrumentId, frameId, directionId)
```

Las funciones de renderizado en `Web/wwwroot/Template/custom/js/Utilities.js` se mantienen, pero se ajustará el `render` de `SL`, `TP1`, `TP2`, `TP3` en `Index.cshtml` para combinar el valor principal con su barra de porcentaje correspondiente.

## Plan de implementación

1. **Migrar el markup de `Web/Views/AnalyticsDay/Index.cshtml` a Metronic Tailwind:**
    - Reemplazar el toolbar Bootstrap y layout actual por header container con `kt-container-fixed`, título "Análisis Diario", subtítulo y botón "Filtro" con `kt-btn kt-btn-outline` y `data-kt-drawer-toggle="#filter_drawer"`.
    - Eliminar el breadcrumb existente.
    - Implementar el drawer `#filter_drawer` (`kt-drawer kt-drawer-end`) replicando la estructura de `AnalyticsTrigger/Index` con los 5 filtros (`kt-select`) y botones "Limpiar" / "Aplicar".
    - Configurar la card (`kt-card kt-card-grid`) y la tabla (`kt-table` con `data-kt-datatable-table="true"`).

2. **Actualizar DataTables en el bloque `<script>`:**
    - Reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')`.
    - Agregar configuración `layout` e `initComplete` con `kt-input` para búsqueda.
    - **Modificar columnas:**
        - Eliminar las columnas explícitas `slp`, `tP1P`, `tP2P`, `tP3P`.
        - Actualizar los `render` de las columnas `sl`, `tP1`, `tP2`, `tP3` para que incluyan, además del valor, la barra de progreso (usando `renderProgressBar`) en la misma celda.
    - Ajustar `className` a `min-w-[Xpx]`.
    - Activar `searching: true` e `info: true`.
    - Actualizar lenguaje a 2.x.

3. **Actualizar lógica JavaScript:**
    - Ajustar `LoadDataTable`, `ClearFilterData` y `SearchData` siguiendo el patrón de `AnalyticsTrigger/Index`.
    - Asegurar que `KTSelectHelper` manipule correctamente los nuevos selects.
    - Eliminar `[data-toggle="tooltip"]`.
    - Agregar `KTMenu.createInstances()` en `drawCallback`.

4. **Validar la migración:**
    - Confirmar visualmente que las barras de progreso se renderizan correctamente dentro de las celdas de `SL`/`TP1`/`TP2`/`TP3`.
    - Validar funcionalidad de filtros, drawer y DataTables en desktop y mobile.
    - Ejecutar `dotnet build "TradingBookApp.sln"` para asegurar compilación correcta.

## Criterios de aceptación

- [ ] `Web/Views/AnalyticsDay/Index.cshtml` usa markup y clases visuales de Metronic Tailwind (header container, card, tabla).
- [ ] El header muestra el título "Análisis Diario", subtítulo y botón "Filtro" que abre el drawer.
- [ ] El drawer `#filter_drawer` contiene los 5 filtros (`kt-select`) y funciona idéntico a `AnalyticsTrigger`.
- [ ] Las columnas explícitas (`SL %`, `TP1 %`, `TP2 %`, `TP3 %`) han sido eliminadas.
- [ ] Las columnas `SL`, `TP1`, `TP2`, `TP3` ahora renderizan el valor numérico junto con la barra de progreso (usando `renderProgressBar`) en la misma celda.
- [ ] La configuración de DataTables incluye `searching: true`, `info: true`, el formato moderno (layout, initComplete) y `min-w-[Xpx]`.
- [ ] El comportamiento de los filtros (Limpiar, Aplicar, carga vía AJAX) funciona correctamente.
- [ ] Se eliminaron elementos Bootstrap legacy (breadcrumb, tooltips, Select2).
- [ ] `dotnet build "TradingBookApp.sln"` compila sin nuevos errores.

## Decisiones tomadas y descartadas

- **Sí:** Consolidar las barras de porcentaje dentro de las columnas `SL`, `TP1`, `TP2`, `TP3` mediante `render` personalizado en DataTables. Esta decisión elimina columnas redundantes (`SL %`, `TP1 %`, etc.) y mejora la legibilidad de la tabla al asociar visualmente el porcentaje con su valor absoluto.
- **Sí:** Reutilizar `renderProgressBar` de `Utilities.js` para los nuevos renders. Es el componente probado y consistente con el resto de la aplicación.
- **Sí:** Replicar el patrón de diseño de `AnalyticsTrigger/Index` para el drawer de filtros (`#filter_drawer`), el uso de `kt-select` y la configuración de DataTables. Garantiza consistencia en toda la suite de reportes analíticos.
- **Sí:** Mantener `AnalyticsDayController` y DTOs sin cambios. La migración es puramente de interfaz de usuario y capa de presentación cliente.
- **No:** Crear nuevas columnas en la tabla ni modificar los contratos del backend.
- **No:** Mantener dependencias legacy (Bootstrap, Select2, tooltips) en la nueva vista. Se eliminan conforme se migra cada componente a su equivalente en Metronic Tailwind / KTUI.

## Riesgos identificados

| Riesgo | Mitigación |
| ------ | ---------- |
| La consolidación de valores y barras en una sola celda provoca problemas de alineación o desbordamiento visual. | Asegurar que la función `render` utilice clases `flex` y `items-center` de Tailwind para alinear correctamente el número y la barra dentro de la celda. |
| La ordenación de las columnas `SL`, `TP1`, `TP2`, `TP3` deja de funcionar correctamente por el nuevo `render` que combina datos (texto y HTML). | Configurar DataTables para que la ordenación actúe sobre el valor numérico crudo (utilizando `sort` o configurando la columna para separar la lógica de renderizado de la lógica de ordenación). |
| Problemas de renderizado en `Utilities.js` al reutilizar `renderProgressBar` en un contexto diferente. | Validar que las clases Tailwind de `renderProgressBar` se aplican correctamente dentro del contenedor `flex` de la nueva celda consolidada. |
