# SPEC 13 — Migración de AnalyticsFigure/Index a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01, SPEC 11, SPEC 12 · **Fecha:** 2026-05-24
> **Objetivo:** Migrar `AnalyticsFigure/Index` a componentes visuales de Metronic Tailwind conservando el comportamiento actual de DataTables y el filtro de escenarios vía drawer con `kt-select`, replicando el diseño de `AnalyticsTrigger/Index` pero eliminando las columnas de porcentaje (`SL %`, `TP1 %`, `TP2 %`, `TP3 %`).

## Alcance

**Incluye:**

- Migrar `Web/Views/AnalyticsFigure/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `AnalyticsTrigger/Index.cshtml` (título + subtítulo + botón "Filtro" que abre un drawer) y la estructura de card + tabla.
- Reemplazar el toolbar Bootstrap actual por un header container con `kt-container-fixed`, título "Análisis de Figuras", subtítulo y botón "Filtro" con `data-kt-drawer-toggle="#filter_drawer"`.
- Implementar el drawer `#filter_drawer` (`kt-drawer kt-drawer-end`) con los 5 filtros (`CategoryId`, `AccountTypeId`, `InstrumentId`, `FrameId`, `DirectionId`) usando `kt-select` de KTUI, siguiendo exactamente el patrón de `AnalyticsTrigger/Index`.
- Configurar la lógica JS de filtros (`ClearFilterData`, `SearchData`) igual que en `AnalyticsTrigger`.
- Actualizar DataTables a 2.x con `serverSide: true`, `layout`, `initComplete` (con `kt-input` para búsqueda) y lenguaje moderno.
- **Eliminación específica:** Remover las columnas `slp` (SL %), `tP1P` (TP1 %), `tP2P` (TP2 %), `tP3P` (TP3 %) de la definición de DataTables.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos backend.

Se reutilizan sin cambios:

```csharp
Web.Models.AnalyticsFigureViewModel
Application.DTOs.GetTBAnalyticsFigureDto
```

El controlador `AnalyticsFigureController.JsonDataTable` se mantiene sin modificaciones. Aunque la vista dejará de renderizar las columnas `slp`, `tP1P`, `tP2P` y `tP3P`, el contrato JSON subyacente y los parámetros de filtrado (`POST ~/AnalyticsFigure/JsonDataTable`) permanecen iguales.

## Plan de implementación

1. **Migrar el markup de `Web/Views/AnalyticsFigure/Index.cshtml` a Metronic Tailwind:**
   - Reemplazar el layout Bootstrap por el header container con `kt-container-fixed`.
   - Agregar el título "Análisis de Figuras", subtítulo y botón "Filtro" (`data-kt-drawer-toggle="#filter_drawer"`).
   - Eliminar el breadcrumb.
   - Implementar el drawer `#filter_drawer` (`kt-drawer kt-drawer-end`) replicando la estructura de `AnalyticsTrigger/Index` (header, content, footer con botones "Limpiar"/"Aplicar").
   - Configurar los 5 `kt-select` en el drawer usando el patrón de KTUI.
   - Reemplazar la tabla por `kt-card kt-card-grid` y `kt-card-table` con `dtTable` y clase `kt-table`.

2. **Actualizar DataTables en el bloque `<script>`:**
   - Aplicar el patrón de chequeo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
   - Configurar `layout` (topStart, topEnd: 'search', bottomStart: ['pageLength', 'info'], bottomEnd: 'paging').
   - Configurar `initComplete` con `kt-input`.
   - **Remover explícitamente las columnas:** `slp` (SL %), `tP1P` (TP1 %), `tP2P` (TP2 %), `tP3P` (TP3 %) de la definición de columnas.
   - Mantener las columnas restantes.
   - Actualizar clases `className` a `min-w-[Xpx]`.
   - Activar `searching: true`, `info: true`, y actualizar el lenguaje a 2.x.

3. **Actualizar la lógica JavaScript:**
   - Implementar `ClearFilterData` usando `KTSelectHelper` para resetear los 5 selects (con `CategoryId = 1` como valor por defecto).
   - Mantener `SearchData` y `LoadDataTable` ajustándolos a la nueva estructura.
   - Limpiar `drawCallback` eliminando referencias a tooltips si existen.

4. **Validación:**
   - Validar que la tabla carga correctamente sin las 4 columnas eliminadas.
   - Probar filtros, drawer, búsqueda y paginación en desktop y mobile.

5. **Verificación final:**
   - Ejecutar `dotnet build "TradingBookApp.sln"` para asegurar compilación correcta.

## Criterios de aceptación

- [ ] `Web/Views/AnalyticsFigure/Index.cshtml` utiliza el layout de Metronic Tailwind (header container, card, `kt-table`).
- [ ] El drawer de filtros `#filter_drawer` está implementado correctamente con los 5 `kt-select` (Categoría, Tipo de Cuenta, Instrumento, Frame, Dirección) siguiendo el patrón de `AnalyticsTrigger`.
- [ ] Al presionar "Aplicar" en el drawer, la tabla se recarga con los filtros correctos.
- [ ] Las columnas `SL %`, `TP1 %`, `TP2 %` y `TP3 %` han sido eliminadas de la tabla y no aparecen en la vista.
- [ ] DataTables está configurado a la versión 2.x con `searching: true` e `info: true` activados.
- [ ] El buscador utiliza `kt-input` dentro del `layout` de DataTables.
- [ ] No existen referencias a clases Bootstrap (`card-body`, `breadcrumb`, etc.) ni a componentes legacy (Select2) en la vista.
- [ ] La vista es responsiva y usable en escritorio y dispositivos móviles.
- [ ] `dotnet build "TradingBookApp.sln"` se ejecuta exitosamente.

## Decisiones tomadas y descartadas

- **Sí: Replicar el patrón de `AnalyticsTrigger/Index`.** Al igual que en las migraciones previas, mantener la consistencia visual y de comportamiento (drawer + `kt-select` + DataTables 2.x) asegura una experiencia de usuario uniforme en todo el módulo de analítica.
- **Sí: Eliminar las columnas `SL %`, `TP1 %`, `TP2 %` y `TP3 %`.** Se atiende explícitamente a la instrucción de omitir estas columnas en la nueva vista, limpiando la interfaz y simplificando la tabla.
- **Sí: Mantener el backend intacto.** No se modifica el controlador ni el contrato JSON para evitar riesgos innecesarios, ya que el filtrado sigue siendo funcional mediante los mismos parámetros.
- **No: Modificar el diseño de la tabla más allá de la remoción de columnas.** Se mantiene la estructura de `kt-card` y `kt-table` para alinearse con los estándares visuales ya establecidos.
