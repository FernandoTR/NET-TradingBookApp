# SPEC 17 — Migración de AnalyticsTime/Index a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 11 · **Fecha:** 2026-05-25
> **Objetivo:** Migrar `AnalyticsTime/Index` a componentes visuales de Metronic Tailwind, consolidando la información de porcentaje dentro de las columnas principales (SL, TP1, TP2, TP3) y replicando el patrón de filtrado vía drawer de `AnalyticsTrigger/Index`.

## Alcance

**Incluye:**

- **Migración de Markup**: Migrar `Web/Views/AnalyticsTime/Index.cshtml` a la estructura visual de Metronic Tailwind, usando el contenedor de encabezado (`kt-container-fixed`), contenedores de tarjetas (`kt-card kt-card-grid`) y tablas estilizadas (`kt-table` con `data-kt-datatable-table="true"`).
- **Nuevo Encabezado**: Reemplazar el toolbar Bootstrap actual por el contenedor de encabezado con el título "Análisis de Tiempo", el subtítulo "Consulta el rendimiento según la hora de la operación configurada en el sistema." y un botón "Filtro" (`kt-btn kt-btn-outline`) que abra el cajón de filtros `#filter_drawer`.
- **Cajón de Filtros (Drawer)**: Implementar el drawer `#filter_drawer` (`kt-drawer kt-drawer-end`) con los 5 filtros (`CategoryId`, `AccountTypeId`, `InstrumentId`, `FrameId`, `DirectionId`) utilizando el componente `kt-select` y limpiando/asignando valores mediante `KTSelectHelper`.
- **Consolidación de Columnas**: Eliminar las columnas explícitas de porcentaje (`SL %`, `TP1 %`, `TP2 %`, `TP3 %`) y consolidar el renderizado de la barra de progreso directamente dentro de las columnas principales `SL`, `TP1`, `TP2` y `TP3` aplicando respectivamente `renderSLPChart`, `renderTP1PChart`, `renderTP2PChart` y `renderTP3PChart` y marcándolas como no ordenables (`orderable: false`).
- **DataTables 2.x**: Configurar DataTables usando el patrón de la versión 2.x:
  - Destrucción segura usando `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
  - Configuración de `layout` (ocultando controles por defecto para usar los personalizados).
  - Inyección de buscador personalizado en `initComplete` con placeholder `"Buscar tiempos..."`.
  - Parámetros `searching: false`, `info: true` habilitados.
  - Formato de lenguaje compatible con la versión 2.x.
  - Ajuste de clases de ancho usando `min-w-[Xpx]` de Tailwind.
- **Remoción de Legacy**: Eliminar breadcrumbs, tooltips de Bootstrap legacy (`$('[data-toggle="tooltip"]').tooltip()`), Select2, y menús dropdown de tipo `kt-menu` antiguos.
- **Animaciones**: Implementar la animación suave de barras de progreso dentro del callback de dibujado (`drawCallback`) usando `requestAnimationFrame` y `setTimeout`.

**Fuera de alcance:**

- Modificaciones en el controlador `AnalyticsTimeController`, servicios, repositorios o DTOs del backend.
- Modificaciones en los contratos o estructura JSON del endpoint `~/AnalyticsTime/JsonDataTable`.
- Creación o migración de vistas de CRUD (New, Edit, Delete) o diálogos modales para gestión de registros.
- Eliminación global de dependencias como Bootstrap o jQuery del layout principal.

## Data model

Esta funcionalidad no introduce nuevas estructuras de datos en el backend ni realiza modificaciones en la base de datos.

Se reutilizan sin cambios las estructuras de datos y modelos existentes:

```csharp
Application.DTOs.GetTBAnalyticsTimeDto
```

El controlador `AnalyticsTimeController.JsonDataTable` se mantiene sin modificaciones. La lógica de consolidación de porcentajes dentro de las columnas principales (`SL`, `TP1`, `TP2`, `TP3`) se realiza exclusivamente en el lado del cliente (Frontend) asignando las funciones de renderizado `renderSLPChart`, `renderTP1PChart`, `renderTP2PChart`, `renderTP3PChart` directamente a cada columna respectiva dentro de la definición de DataTables.

Se mantienen los contratos cliente existentes de la tabla:

```text
POST ~/AnalyticsTime/JsonDataTable (parámetros de filtro: categoryId, accountTypeId, instrumentId, frameId, directionId)
```

## Plan de implementación

1. **Migrar el markup de `Web/Views/AnalyticsTime/Index.cshtml` a Metronic Tailwind:**
   - Reemplazar el toolbar de Bootstrap y breadcrumbs legados por un encabezado de tipo `kt-container-fixed` con el título "Análisis de Tiempo", subtítulo correspondiente y botón de acción "Filtro" equipado con `data-kt-drawer-toggle="#filter_drawer"`.
   - Implementar el drawer lateral `#filter_drawer` (`kt-drawer kt-drawer-end`) replicando la interfaz de filtros con componentes `kt-select` en lugar de dropdowns legados.
   - Diseñar la sección de contenido principal mediante las clases de contenedor `kt-card`, `kt-card-grid` y la tabla utilizando las clases `kt-table` y el atributo `data-kt-datatable-table="true"`.

2. **Actualizar la configuración de DataTables en el bloque `<script>`:**
   - Implementar la validación e inicialización segura de la tabla con `$.fn.DataTable.isDataTable('#dtTable')` antes de realizar el `.destroy()`.
   - Configurar la propiedad `layout` de DataTables 2.x para el control estilizado de la paginación e información.
   - Definir las columnas de la siguiente manera:
     - Mantener columna `time` (Tiempo) con clase `className: "text-center min-w-[100px] w-[150px]"`.
     - Mantener columna `quantity` (Cantidad) con clase `className: "text-center min-w-[50px] w-[100px]"`.
     - Configurar columnas `sl`, `tP1`, `tP2` y `tP3` para utilizar respectivamente las funciones de renderizado `renderSLPChart`, `renderTP1PChart`, `renderTP2PChart` y `renderTP3PChart`. Marcar estas columnas con `orderable: false` y clases de ancho uniforme `min-w-[30px]`.
     - Eliminar por completo las columnas explícitas antiguas `slp`, `tP1P`, `tP2P` y `tP3P`.
     - Conservar la columna `valid` (Estatus) con el renderizado `renderStatusAnalytics` y alineación a la derecha `className: "text-end min-w-[80px] w-[80px]"`.
   - Habilitar `searching: false` e `info: true`.
   - Implementar `initComplete` para inyectar el componente de búsqueda personalizado con placeholder `"Buscar tiempos..."` y conectar su evento de teclado al filtrado del DataTable.
   - Actualizar el objeto `language` al formato moderno de la versión 2.x.

3. **Adaptar la lógica JavaScript e inicializadores de Metronic:**
   - Modificar la función `ClearFilterData` para utilizar `KTSelectHelper.setValue('#CategoryId', 1)` y aplicar `KTSelectHelper.clear(...)` en los demás dropdowns (`AccountTypeId`, `InstrumentId`, `FrameId`, `DirectionId`).
   - Ajustar el callback `drawCallback` de la tabla para:
     - Re-inicializar menús de Metronic con `KTMenu.createInstances()`.
     - Eliminar las referencias a tooltips obsoletos de Bootstrap.
     - Añadir animación para el renderizado suave de las barras de progreso usando `requestAnimationFrame` que busque los elementos `.kt-progress-indicator` y aplique el porcentaje almacenado en su atributo `data-width`.

4. **Validación técnica y compilación:**
   - Confirmar que la aplicación compila correctamente mediante el comando de compilación:
     ```bash
     dotnet build "TradingBookApp.sln"
     ```
   - Verificar de forma visual e interactiva que los filtros se aplican, el drawer se oculta/muestra correctamente, y las barras de progreso se animan de forma fluida.

## Criterios de aceptación

- [ ] `Web/Views/AnalyticsTime/Index.cshtml` utiliza el layout, markup y clases visuales de Metronic Tailwind (`kt-container-fixed`, `kt-card`, `kt-card-grid` y `kt-table`).
- [ ] El encabezado muestra el título "Análisis de Tiempo", su subtítulo explicativo y el botón "Filtro" que activa de manera interactiva el drawer lateral.
- [ ] El drawer lateral `#filter_drawer` contiene los 5 filtros (`CategoryId`, `AccountTypeId`, `InstrumentId`, `FrameId`, `DirectionId`) estilizados con la clase `kt-select` y sus respectivas configuraciones de búsqueda y scroll de Metronic Tailwind.
- [ ] Las columnas explícitas de porcentaje (`SL %`, `TP1 %`, `TP2 %`, `TP3 %`) han sido eliminadas por completo del DOM y de la configuración del DataTable.
- [ ] Las columnas de valores principales (`SL`, `TP1`, `TP2`, `TP3`) integran visualmente su barra de progreso correspondiente mediante las funciones de render (`renderSLPChart`, `renderTP1PChart`, `renderTP2PChart`, `renderTP3PChart`) y se han marcado como no ordenables (`orderable: false`).
- [ ] La tabla está configurada bajo el estándar de DataTables 2.x, con búsqueda interna desactivada, visualización de info activada (`info: true`) y controles de paginación e información posicionados de acuerdo con la propiedad `layout`.
- [ ] El callback `initComplete` inyecta correctamente el input de búsqueda interactivo con el placeholder `"Buscar tiempos..."` y filtra la tabla en tiempo real.
- [ ] La función `ClearFilterData` resetea el filtro de categoría al valor por defecto `1` y limpia el resto de los filtros usando `KTSelectHelper`.
- [ ] El callback `drawCallback` ejecuta la animación de barras de progreso mediante `requestAnimationFrame` aplicando la propiedad de ancho en CSS a los elementos `.kt-progress-indicator`.
- [ ] Se han eliminado por completo las referencias legadas de Bootstrap, tooltip antiguo y Select2.
- [ ] El proyecto compila limpiamente sin errores al ejecutar:
  ```bash
  dotnet build "TradingBookApp.sln"
  ```

## Decisiones tomadas y descartadas

- **Sí: Consolidar los porcentajes dentro de las columnas `SL`, `TP1`, `TP2`, `TP3`**: Al igual que en `AnalyticsStage` y `AnalyticsTrigger`, unificar el valor numérico con la representación visual de su barra de porcentaje correspondiente mejora drásticamente el uso del espacio y la legibilidad en pantallas compactas.
- **Sí: Replicar el patrón de filtrado con drawer lateral (`#filter_drawer`)**: Proporciona consistencia visual en todo el módulo de reportes y análisis, unificando el comportamiento de los filtros utilizando la clase nativa `kt-select` y el helper `KTSelectHelper`.
- **Sí: Preservar la consistencia visual del buscador**: A pesar de que la tabla tiene deshabilitado el filtrado de servidor por búsqueda (`searching: false`), inyectar el buscador estilizado de Metronic mediante el callback `initComplete` con el placeholder `"Buscar tiempos..."` garantiza la consistencia visual de la suite de analítica.
- **Sí: Comportamiento por defecto al limpiar filtros**: Mantener `CategoryId` con un valor por defecto de `1` (Categoría Principal) al presionar "Limpiar", asegurando congruencia exacta con el comportamiento observado en `AnalyticsTrigger`.
- **No: Modificar controladores o endpoints del Backend**: Mantener intacto el código de `AnalyticsTimeController` y los contratos de datos de la API para garantizar un bajo riesgo de regresión en la lógica de negocio.
- **No: Mantener código o scripts legados de Bootstrap**: Remover en su totalidad componentes Bootstrap antiguos, Select2 obsoletos y tooltips clásicos para evitar conflictos visuales o de librerías JS en la nueva interfaz Tailwind.

## Riesgos identificados

| Riesgo | Mitigación |
| ------ | ---------- |
| **Problemas de desalineación o desbordamiento visual** en las columnas principales (`SL`, `TP1`, `TP2`, `TP3`) tras integrar la barra de progreso. | Las funciones de renderizado ya están probadas y optimizadas con Tailwind CSS en otras vistas migradas (`AnalyticsStage` y `AnalyticsTrigger`); se emplearán las mismas clases de ancho responsivo y contenedores flex de Metronic. |
| **Errores de ordenación** en las columnas principales debido a la combinación de valores numéricos e interactivos (HTML de la barra de progreso). | Se configurará explícitamente la propiedad `orderable: false` en las columnas `sl`, `tP1`, `tP2` y `tP3` para evitar comportamientos inesperados, consistente con el resto de las tablas de analítica. |
| **Conflictos visuales o errores en la consola de JS** debido a residuos de scripts legados de Bootstrap o Select2. | Se eliminará en su totalidad la declaración de tooltips legados de Bootstrap (`$('[data-toggle="tooltip"]').tooltip()`) del `drawCallback` y se sustituirán los inicializadores obsoletos de Select2 por `KTSelectHelper`. |
