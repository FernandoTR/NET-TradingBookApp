# SPEC 07 — Migración de CatInstruments a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01, SPEC 02, SPEC 03, SPEC 04, SPEC 05 · **Fecha:** 2026-05-21
> **Objetivo:** Migrar `CatInstruments/Index`, `CatInstruments/New` y `CatInstruments/Edit` a componentes visuales de Metronic Tailwind conservando el comportamiento actual de DataTables, modales, acciones y validaciones.

## Alcance

**Incluye:**

- Migrar `Web/Views/CatInstruments/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Index.cshtml` (header container con título y botón "Nuevo", sin breadcrumb, card con tabla, search con `kt-input` vía `initComplete` y `layout`).
- Migrar la columna `isActived` (Estado) manteniendo `render: renderStatus` de `Utilities.js`, que ya usa `kt-badge` desde SPEC 05.
- Migrar la función `renderIconCoin` en `Web/wwwroot/Template/custom/js/Utilities.js` de clases Bootstrap (`symbol symbol-circle symbol-50px overflow-hidden me-3`) a clases Tailwind (`rounded-full size-9 shrink-0`).
- Migrar el contenido modal de `Web/Views/CatInstruments/New.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/New.cshtml` (`kt-card-content grid`, labels con `kt-form-label`, inputs con `kt-input`, footer con `kt-modal-footer`).
- Migrar el contenido modal de `Web/Views/CatInstruments/Edit.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Edit.cshtml`.
- Mantener DataTables con server-side AJAX hacia `~/CatInstruments/JsonDataTable`.
- Mantener las columnas actuales: Número (`id`), Nombre (`name`), Icono (`linkIcon`, render `renderIconCoin`), Ticker (`ticker`), Tipo (`instrumentType`), Moneda (`currency`), Mercado (`market`), Estado (`isActived`, render `renderStatus`) y Acciones (`task`).
- Migrar la columna de acciones de fila a un dropdown `kt-menu` de Metronic Tailwind (el HTML ya lo genera `ActionButtonHelper.GenerateActionMenu` en el controller, la vista debe renderizarlo correctamente).
- Mantener las acciones actuales de fila dentro del `kt-menu`: editar instrumento y eliminar instrumento.
- Migrar `showModal` y `showModaltoDelete` a `KTModal` (patrón de SPEC 04 — `KTModal.getInstance`).
- Mantener las reglas de validación cliente actuales con `FormValidation`, migrando sus plugins al mismo patrón de CatFigure (Trigger, SubmitButton, Message con clases Tailwind, feedback visual `core.element.validated`), sin agregar validaciones remotas.
- Mantener `id="frmdata"`, `id="btnSave"`, `Html.BeginForm`, `AntiForgeryToken`, campos ocultos y nombres de inputs sin cambios.
- Mantener sin cambios los campos del formulario: Name, Ticker, InstrumentType, Currency, Market, LinkIcon en ambos formularios.
- Mantener `Html.HiddenFor(d => d.IsActived)` dentro de `<div hidden>` en Edit.cshtml sin cambios.
- Eliminar tooltips Bootstrap (`data-toggle="tooltip"`) del botón "Nuevo" y del `drawCallback`.
- Eliminar el breadcrumb existente.
- Validar manualmente la vista CatInstruments en desktop y mobile.
- Confirmar que la aplicación sigue compilando con `dotnet build "TradingBookApp.sln"`.

**Fuera de alcance (para specs futuras):**

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `CatInstrumentsController`, servicios, repositorios, DTOs, entidades o `CatInstrumentsViewModels`.
- Cambiar contratos JSON de `~/CatInstruments/JsonDataTable`.
- Reemplazar el modal global existente por un modal Tailwind propio.
- Cambiar las reglas, mensajes o endpoints de validación de formularios.
- Agregar validación remota de código/ticker duplicado.
- Agregar validación al campo `LinkIcon`.
- Agregar, quitar o renombrar acciones de fila.
- Agregar la columna `updatedAt` a la tabla.
- Rediseñar completamente la experiencia visual de CatInstruments.
- Eliminar Bootstrap, jQuery, DataTables, FormValidation u otras dependencias cliente existentes.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos backend.

Se reutiliza `Web.Models.CatInstrumentsViewModels` sin cambios:

```csharp
CatInstrumentsViewModels
```

El controlador `CatInstrumentsController.JsonDataTable` ya genera la columna `Task` con el `kt-menu` de Metronic Tailwind mediante `ActionButtonHelper.GenerateActionMenu`, con las acciones de editar y eliminar. Esta spec no modifica ese código.

Se mantienen los contratos cliente existentes de la tabla y formularios:

```text
POST ~/CatInstruments/JsonDataTable
POST ~/CatInstruments/Delete
GET  ~/CatInstruments/New
GET  ~/CatInstruments/Edit/?id={id}
POST ~/CatInstruments/Save
POST ~/CatInstruments/Update
```

La columna `isActived` se mantiene con `render: renderStatus`, que ya usa `kt-badge` de Metronic Tailwind desde SPEC 05.

La función `renderIconCoin` en `Web/wwwroot/Template/custom/js/Utilities.js` se migra de:

```html
<div class="symbol symbol-circle symbol-50px overflow-hidden me-3">
  <div class="symbol-label">
    <img src="..." alt="IconCoin" class="w-100">
  </div>
</div>
```

A:

```html
<img alt="IconCoin" class="rounded-full size-9 shrink-0" src="...">
```

## Plan de implementación

1. Migrar `renderIconCoin` en `Web/wwwroot/Template/custom/js/Utilities.js`:
   - Reemplazar `'<div class="symbol symbol-circle symbol-50px overflow-hidden me-3"><div class="symbol-label"><img src="' + data + '" alt="IconCoin" class="w-100"></div></div > '` por `'<img alt="IconCoin" class="rounded-full size-9 shrink-0" src="' + data + '">'`.

2. Migrar `Web/Views/CatInstruments/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Index.cshtml`:
   - Reemplazar todo el toolbar Bootstrap (`kt_app_toolbar`, `kt_app_content`, `card`, `card-body`) por header container con `kt-container-fixed`, título "Instrumentos", subtítulo y botón "Nuevo" con `kt-btn kt-btn-primary`.
   - Eliminar el breadcrumb completo.
   - Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable` con clase `kt-table` y `data-kt-datatable-table="true"`.
   - Agregar script tag para `dataTables.min.js`.
   - Actualizar DataTables: reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
   - Actualizar DataTables con `layout` (topStart, topEnd, bottomStart, bottomEnd) y `initComplete` para el search con `kt-input` (mismo patrón de CatFigure, placeholder "Buscar instrumentos...").
   - Mantener todas las columnas actuales con sus `render`: `linkIcon` con `renderIconCoin`, `isActived` con `renderStatus`.
   - Convertir clases `className` de `min-w-Xpx` a `min-w-[Xpx]`.
   - Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback`.
   - Migrar `showModal` a `KTModal.getInstance` en lugar de `.modal("show")`.
   - Migrar `showModaltoDelete` a `KTModal.getInstance` manteniendo el patrón `.bind`/`.unbind` de CatFigure.
   - Eliminar `$('[data-toggle="tooltip"]').tooltip()` de `showModalForNew`.
   - Mantener `KTMenu.createInstances()` en el `drawCallback`.

3. Migrar `Web/Views/CatInstruments/New.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/New.cshtml`:
   - Agrupar campos en `kt-card-content grid gap-5`.
   - Cada campo: `w-full fv-row` con `flex items-baseline flex-wrap lg:flex-nowrap gap-2.5`.
   - Label con clase `kt-form-label max-w-56`, input con clase `kt-input`.
   - Footer con `kt-modal-footer gap-2.5 justify-end`.
   - Botón cancelar: `kt-btn kt-btn-secondary` con `data-kt-modal-dismiss="#vModal"`.
   - Botón guardar: `kt-btn kt-btn-primary`.
   - Mantener `id="frmdata"`, `id="btnSave"`, `Html.BeginForm("Save", "CatInstruments")`, `AntiForgeryToken` y `Html.HiddenFor(d => d.Id)`.
   - Actualizar FormValidation al patrón de CatFigure: reemplazar plugin `Bootstrap5` por plugins `Trigger`, `SubmitButton` y `Message` con `clazz: 'text-red-500 text-sm mt-1'`, agregar evento `core.element.validated` para feedback visual con clases `border-green-500` / `border-destructive ring-1 ring-red-500`.
   - Mantener las mismas reglas de validación: notEmpty y stringLength para Name, Ticker, InstrumentType, Currency y Market. Sin validación para LinkIcon.

4. Migrar `Web/Views/CatInstruments/Edit.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Edit.cshtml`:
   - Mismos cambios visuales que New (`kt-card-content grid`, `kt-form-label`, `kt-input`, `kt-modal-footer`).
   - Mantener `Html.BeginForm("Update", "CatInstruments")`, `Html.HiddenFor(d => d.Id)` y `Html.HiddenFor(d => d.IsActived)` en `<div hidden>`.
   - Actualizar FormValidation al mismo patrón que New.

5. Validar manualmente que CatInstruments carga la tabla con datos, muestra los iconos con `renderIconCoin` migrado, muestra el badge de estado correctamente, muestra el `kt-menu` de acciones, abre el modal de nuevo instrumento, abre el modal de edición, ejecuta el diálogo de confirmación para eliminar y guarda correctamente.

6. Revisar CatInstruments en desktop y mobile para confirmar que la migración visual no bloquea el uso de tabla, filtros, modales ni acciones.

7. Ejecutar `dotnet build "TradingBookApp.sln"` desde la raíz del repositorio y confirmar que compila sin errores nuevos.

## Criterios de aceptación

- [ ] `renderIconCoin` en `Web/wwwroot/Template/custom/js/Utilities.js` usa clases Tailwind (`rounded-full size-9 shrink-0`) en lugar de Bootstrap (`symbol symbol-circle symbol-50px`).
- [ ] `Web/Views/CatInstruments/Index.cshtml` usa markup y clases visuales de Metronic Tailwind (header container, card, tabla), replicando la estructura de `Web/Views/CatFigure/Index.cshtml`.
- [ ] `Web/Views/CatInstruments/New.cshtml` usa markup y clases visuales de Metronic Tailwind (`kt-card-content grid`, `kt-form-label`, `kt-input`, `kt-modal-footer`), replicando la estructura de `Web/Views/CatFigure/New.cshtml`.
- [ ] `Web/Views/CatInstruments/Edit.cshtml` usa markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Edit.cshtml`.
- [ ] DataTables sigue cargando datos desde `~/CatInstruments/JsonDataTable` con `serverSide: true`.
- [ ] El search de DataTables usa `kt-input` con el patrón `initComplete` + `layout` de CatFigure.
- [ ] La columna `linkIcon` renderiza con `renderIconCoin` usando las nuevas clases Tailwind.
- [ ] La columna `isActived` sigue usando `render: renderStatus` con `kt-badge` de Metronic Tailwind.
- [ ] La columna de acciones renderiza el `kt-menu` generado por `ActionButtonHelper.GenerateActionMenu`.
- [ ] El `kt-menu` de acciones sigue disponible después de paginar, filtrar u ordenar la tabla.
- [ ] Las acciones existentes siguen disponibles: editar instrumento y eliminar instrumento.
- [ ] `showModal` y `showModalForUpdate` usan `KTModal.getInstance` en lugar de `.modal("show")`.
- [ ] `showModaltoDelete` usa `KTModal.getInstance` para el diálogo de confirmación.
- [ ] Las validaciones cliente de `New.cshtml` y `Edit.cshtml` usan los plugins `Trigger`, `SubmitButton` y `Message` con clases Tailwind y feedback visual `core.element.validated`.
- [ ] Las reglas de validación se mantienen sin cambios: notEmpty y stringLength para Name, Ticker, InstrumentType, Currency y Market; sin validación para LinkIcon.
- [ ] No se modifica `CatInstrumentsController`, sus servicios, repositorios, entidades, DTOs ni `CatInstrumentsViewModels`.
- [ ] No hay tooltips Bootstrap (`data-toggle="tooltip"`) en Index.cshtml.
- [ ] CatInstruments es usable en desktop y mobile.
- [ ] `dotnet build "TradingBookApp.sln"` termina correctamente, permitiendo solo warnings preexistentes no relacionados con esta migración.

## Decisiones

- **Sí:** Incluir `Web/Views/CatInstruments/Index.cshtml`, `Web/Views/CatInstruments/New.cshtml` y `Web/Views/CatInstruments/Edit.cshtml`. El flujo funcional de CatInstruments depende de la tabla y de los formularios modales de alta y edición.
- **Sí:** Replicar la estructura visual de `Web/Views/CatFigure/Index.cshtml`, `CatFigure/New.cshtml` y `CatFigure/Edit.cshtml`. CatFigure es la base canónica para catálogos con columna `isActived` y mantiene consistencia visual entre módulos migrados.
- **Sí:** Migrar `renderIconCoin` en `Utilities.js` a clases Tailwind (`rounded-full size-9 shrink-0`). El cambio unifica el aspecto visual con el resto de la tabla migrada y evita íconos con estilo Bootstrap en una tabla con markup Tailwind.
- **Sí:** Mantener `render: renderStatus` para la columna `isActived`. La función compartida en `Utilities.js` ya fue migrada a `kt-badge` en SPEC 05; duplicarla inline en CatInstruments sería inconsistente con el resto de vistas que usan la misma función.
- **Sí:** Eliminar el breadcrumb de CatInstruments. Employees, CatCategory, CatFigure, CatAccountType y CatFrame ya eliminaron el suyo; mantenerlo en CatInstruments crearía inconsistencia visual entre módulos migrados.
- **Sí:** Migrar `showModal` y `showModaltoDelete` a `KTModal.getInstance`. El modal global `#vModal` ya se inicializa como KTModal desde SPEC 02; usar `.modal("show")` de Bootstrap rompería la compatibilidad.
- **Sí:** Actualizar FormValidation al patrón de CatFigure: plugins `Trigger`, `SubmitButton` y `Message` con clases Tailwind, más feedback visual `core.element.validated`. El plugin `Bootstrap5` puede tener conflictos con el nuevo markup Tailwind.
- **Sí:** Mantener `ActionButtonHelper.GenerateActionMenu` en el controlador sin cambios. El `kt-menu` ya se genera correctamente desde el servidor y la vista solo necesita renderizar el HTML.
- **Sí:** Reemplazar `destroy: true` inline en DataTables por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')`. El patrón de CatFigure es más explícito y evita intentos de destrucción sobre instancias inexistentes.
- **Sí:** Eliminar tooltips Bootstrap (`data-toggle="tooltip"`). No tienen equivalente en el tema Metronic Tailwind y su eliminación es consistente con las migraciones anteriores.
- **No:** Cambiar `CatInstrumentsController`, servicios, repositorios, entidades, DTOs o `CatInstrumentsViewModels`. Esta spec es solo UI/markup/JS cliente.
- **No:** Agregar validación remota de código/ticker duplicado. Se descartó para mantener el alcance acotado a UI. Si se necesita en el futuro, debe ir en su propia spec.
- **No:** Agregar validación al campo `LinkIcon`. El campo no tiene validación actualmente y agregarla requiere decisión de reglas (notEmpty, regex URL, etc.) que van más allá de una migración visual.
- **No:** Agregar, quitar o renombrar acciones de fila. Editar y eliminar son las únicas acciones requeridas en CatInstruments.
- **No:** Agregar la columna `updatedAt` a la tabla. CatInstruments no la muestra actualmente y agregarla requiere decisión de formato/posicionamiento que va más allá de una migración visual.
- **No:** Hacer un rediseño completo de CatInstruments. El objetivo es replicar el comportamiento existente con la mejora visual de Metronic Tailwind.

## Riesgos

| Riesgo | Mitigación |
| ------ | ---------- |
| DataTables renderiza filas nuevas después de paginar, filtrar u ordenar y los menús `kt-menu` dejan de responder. | Reinicializar los menús de Metronic Tailwind en el callback de dibujo de DataTables (`KTMenu.createInstances()`). |
| El modal global `#vModal` no responde a `KTModal.getInstance` porque se perdió la instancia o se reinicializó incorrectamente. | Validar apertura de modales de alta y edición manualmente; mantener consistencia con el patrón de CatFigure que ya está probado. |
| Las clases Tailwind cambian el layout y FormValidation deja de encontrar los campos o aplicar feedback visual correctamente. | Mantener IDs, nombres de inputs y estructura `.fv-row`; validar los tres estados del formulario manualmente: vacío, inválido y envío exitoso. |
| El cambio de `renderIconCoin` en `Utilities.js` de clases Bootstrap a Tailwind rompe el renderizado en otras vistas que usan la misma función. | Validar manualmente al menos una vista que use `renderIconCoin`; `rounded-full size-9 shrink-0` son clases Tailwind estándar soportadas por el tema Metronic cargado globalmente. |
| El `showModaltoDelete` actual usa `$("#confirmButtonYes").bind`/`.unbind`. Migrar a `KTModal.getInstance` requiere mantener ese binding existente. | Revisar el patrón de confirmación de CatFigure; mantener `.bind`/`.unbind` sobre `#confirmButtonYes` y usar `KTModal.getInstance` solo para show/hide del modal. |
| Warnings preexistentes del build se confunden con regresiones de esta migración. | Ejecutar `dotnet build "TradingBookApp.sln"` antes y después de la migración, separando warnings preexistentes de errores nuevos. |

## Lo que no está en esta spec

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `CatInstrumentsController`, servicios, repositorios, DTOs, entidades o `CatInstrumentsViewModels`.
- Cambiar contratos JSON de `~/CatInstruments/JsonDataTable`.
- Reemplazar el modal global existente por un modal Tailwind propio.
- Cambiar las reglas, mensajes o endpoints de validación de formularios.
- Agregar validación remota de código/ticker duplicado.
- Agregar validación al campo `LinkIcon`.
- Agregar, quitar o renombrar acciones de fila.
- Agregar la columna `updatedAt` a la tabla.
- Rediseñar completamente la experiencia visual de CatInstruments.
- Eliminar Bootstrap, jQuery, DataTables, FormValidation u otras dependencias cliente existentes.

Cada uno de esos puntos debe ir en su propia spec si se decide abordarlo.
