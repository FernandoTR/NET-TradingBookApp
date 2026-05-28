# SPEC 05 — Migración de CatAccountType a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01, SPEC 02, SPEC 03, SPEC 04 · **Fecha:** 2026-05-21
> **Objetivo:** Migrar `CatAccountType/Index`, `CatAccountType/New` y `CatAccountType/Edit` a componentes visuales de Metronic Tailwind conservando el comportamiento actual de DataTables, modales, acciones y validaciones.

## Alcance

**Incluye:**

- Migrar `Web/Views/CatAccountType/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Index.cshtml` (header container con título y botón "Nuevo", sin breadcrumb, card con tabla, search con `kt-input` vía `initComplete` y `layout`).
- Migrar la columna `isActived` (Estado) de `render: renderStatus` (Bootstrap) a `kt-badge` de Metronic Tailwind.
- Migrar el contenido modal de `Web/Views/CatAccountType/New.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/New.cshtml` (`kt-card-content grid`, labels con `kt-form-label`, inputs con `kt-input`, footer con `kt-modal-footer`).
- Migrar el contenido modal de `Web/Views/CatAccountType/Edit.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Edit.cshtml`.
- Mantener DataTables con server-side AJAX hacia `~/CatAccountType/JsonDataTable`.
- Migrar `showModal` y `showModaltoDelete` a `KTModal` (patrón de SPEC 04 — `KTModal.getInstance`).
- Migrar las funciones de render de badges en `Web/wwwroot/Template/custom/js/Utilities.js` de clases Bootstrap a `kt-badge` de Metronic Tailwind. Esto incluye `renderStatus`, `renderStatusEmployee`, `renderAccountType` y `renderStatusAnalytics`. El cambio es global y beneficia a todas las vistas que referencian estas funciones compartidas.
- Mantener las reglas de validación cliente actuales con `FormValidation`, migrando sus plugins al mismo patrón de CatFigure (Trigger, SubmitButton, Message con clases Tailwind, feedback visual `core.element.validated`), sin agregar validaciones remotas.
- Mantener `id="frmdata"`, `id="btnSave"`, `Html.BeginForm`, `AntiForgeryToken`, campos ocultos y nombres de inputs sin cambios.
- Validar manualmente CatAccountType en desktop y mobile.
- Validar que las demás vistas que usan `renderStatus` y funciones relacionadas de `Utilities.js` no se rompen con el cambio de badges.
- Confirmar que la aplicación sigue compilando con `dotnet build "TradingBookApp.sln"`.

**Fuera de alcance (para specs futuras):**

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `CatAccountTypeController`, servicios, repositorios, DTOs, entidades o `CatAccountTypeViewModel`.
- Cambiar contratos JSON de `~/CatAccountType/JsonDataTable`.
- Reemplazar el modal global existente por un modal Tailwind propio.
- Cambiar las reglas, mensajes o endpoints de validación de formularios.
- Agregar validación remota de código duplicado.
- Agregar, quitar o renombrar acciones de fila.
- Rediseñar completamente la experiencia visual de CatAccountType.
- Eliminar Bootstrap, jQuery, DataTables, FormValidation u otras dependencias cliente existentes.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos backend.

Se reutiliza `Web.Models.CatAccountTypeViewModel` sin cambios:

```csharp
CatAccountTypeViewModel
```

El controlador `CatAccountTypeController.JsonDataTable` ya genera la columna `Task` con el `kt-menu` de Metronic Tailwind mediante `ActionButtonHelper.GenerateActionMenu`, con las acciones de editar y eliminar. Esta spec no modifica ese código.

Se mantienen los contratos cliente existentes de la tabla y formularios:

```text
POST ~/CatAccountType/JsonDataTable
POST ~/CatAccountType/Delete
GET  ~/CatAccountType/New
GET  ~/CatAccountType/Edit/?id={id}
POST ~/CatAccountType/Save
POST ~/CatAccountType/Update
```

La columna `isActived` debe migrarse de su render Bootstrap actual a badges `kt-badge` de Metronic Tailwind, siguiendo el patrón:

```html
<span class="kt-badge kt-badge-success">Activo</span>
<span class="kt-badge kt-badge-danger">Inactivo</span>
```

El cambio de badges Bootstrap → `kt-badge` en `Web/wwwroot/Template/custom/js/Utilities.js` afecta las siguientes funciones compartidas:

- `renderStatus` → `kt-badge-success` / `kt-badge-danger`
- `renderStatusEmployee` → `kt-badge-success` / `kt-badge-danger`
- `renderAccountType` → `kt-badge-danger` / `kt-badge-warning` / `kt-badge-success` / `kt-badge-warning`
- `renderStatusAnalytics` → `kt-badge-success` / `kt-badge-warning`

## Plan de implementación

1. Migrar badges Bootstrap a `kt-badge` en `Web/wwwroot/Template/custom/js/Utilities.js`:
   - `renderStatus`: reemplazar `badge py-3 px-4 fs-7 badge-light-success` → `kt-badge kt-badge-success` y `badge py-3 px-4 fs-7 badge-light-danger` → `kt-badge kt-badge-danger`.
   - `renderStatusEmployee`: mismo reemplazo que `renderStatus`.
   - `renderAccountType`: reemplazar `badge-light-danger` → `kt-badge-danger`, `badge-light-warning` → `kt-badge-warning`, `badge-light-success` → `kt-badge-success`.
   - `renderStatusAnalytics`: reemplazar `badge-light-success` → `kt-badge-success`, `badge-light-warning` → `kt-badge-warning`.

2. Migrar `Web/Views/CatAccountType/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Index.cshtml`:
   - Header container con `kt-container-fixed`, título "Tipo de Cuentas", subtítulo y botón "Nuevo" con `kt-btn kt-btn-primary`.
   - Eliminar el breadcrumb existente.
   - Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable`.
   - Agregar script tag para `dataTables.min.js`.
   - Actualizar DataTables: reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
   - Actualizar DataTables con `layout` (topStart, topEnd, bottomStart, bottomEnd) y `initComplete` para el search con `kt-input` (mismo patrón de CatFigure).
   - Mantener `render: renderStatus` en la columna `isActived`. La función ya usará `kt-badge` tras el paso 1.
   - Migrar las funciones `showModal`, `showModalForNew`, `showModalForUpdate`, `showModaltoDelete` y `processingDelete` a usar `KTModal.getInstance` en lugar de `.modal("show")` / `.modal("hide")`.

3. Migrar `Web/Views/CatAccountType/New.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/New.cshtml`:
   - Agrupar campos en `kt-card-content grid gap-5`.
   - Cada campo: `w-full fv-row` con `flex items-baseline flex-wrap lg:flex-nowrap gap-2.5`.
   - Label con clase `kt-form-label max-w-56`, input con clase `kt-input`.
   - Footer con `kt-modal-footer gap-2.5 justify-end`.
   - Botón cancelar: `kt-btn kt-btn-secondary` con `data-kt-modal-dismiss="#vModal"`.
   - Botón guardar: `kt-btn kt-btn-primary`.
   - Mantener `id="frmdata"`, `id="btnSave"`, `Html.BeginForm("Save", "CatAccountType")`, `AntiForgeryToken` y `Html.HiddenFor(d => d.Id)`.
   - Actualizar FormValidation al patrón de CatFigure: reemplazar plugin `Bootstrap5` por plugins `SubmitButton` y `Message` con `clazz: 'text-red-500 text-sm mt-1'`, agregar evento `core.element.validated` para feedback visual con clases `border-green-500` / `border-destructive ring-1 ring-red-500`.

4. Migrar `Web/Views/CatAccountType/Edit.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Edit.cshtml`:
   - Mismos cambios visuales que New (`kt-card-content grid`, `kt-form-label`, `kt-input`, `kt-modal-footer`).
   - Mantener `Html.BeginForm("Update", "CatAccountType")` y `Html.HiddenFor(d => d.Id)`, `Html.HiddenFor(d => d.IsActived)`.
   - Actualizar FormValidation al mismo patrón que New.

5. Validar manualmente que CatAccountType carga la tabla con datos, muestra el `kt-menu` de acciones, muestra el badge de estado correctamente con `kt-badge`, abre el modal de nuevo tipo de cuenta, abre el modal de edición, ejecuta el diálogo de confirmación para eliminar y guarda correctamente.

6. Validar que otras vistas que usan `renderStatus` desde `Utilities.js` (p.ej. las que tengan columna de estado booleano) siguen mostrando badges correctamente con `kt-badge`.

7. Revisar CatAccountType en desktop y mobile para confirmar que la migración visual no bloquea el uso de tabla, filtros, modales ni acciones.

8. Ejecutar `dotnet build "TradingBookApp.sln"` desde la raíz del repositorio y confirmar que compila sin errores nuevos.

## Criterios de aceptación

- [ ] Las funciones `renderStatus`, `renderStatusEmployee`, `renderAccountType` y `renderStatusAnalytics` en `Web/wwwroot/Template/custom/js/Utilities.js` usan `kt-badge` de Metronic Tailwind en lugar de clases Bootstrap.
- [ ] `Web/Views/CatAccountType/Index.cshtml` usa markup y clases visuales de Metronic Tailwind (header container, card, tabla), replicando la estructura de `Web/Views/CatFigure/Index.cshtml`.
- [ ] `Web/Views/CatAccountType/New.cshtml` usa markup y clases visuales de Metronic Tailwind (`kt-card-content grid`, `kt-form-label`, `kt-input`, `kt-modal-footer`), replicando la estructura de `Web/Views/CatFigure/New.cshtml`.
- [ ] `Web/Views/CatAccountType/Edit.cshtml` usa markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Edit.cshtml`.
- [ ] DataTables sigue cargando datos desde `~/CatAccountType/JsonDataTable` con `serverSide: true`.
- [ ] El search de DataTables usa `kt-input` con el patrón `initComplete` + `layout` de CatFigure.
- [ ] La columna `isActived` renderiza badges `kt-badge` de Metronic Tailwind (`kt-badge-success` para Activo, `kt-badge-danger` para Inactivo).
- [ ] La columna de acciones renderiza el `kt-menu` generado por `ActionButtonHelper.GenerateActionMenu`.
- [ ] El `kt-menu` de acciones sigue disponible después de paginar, filtrar u ordenar la tabla.
- [ ] Las acciones existentes siguen disponibles: editar tipo de cuenta y eliminar tipo de cuenta.
- [ ] `showModal`, `showModalForNew` y `showModalForUpdate` usan `KTModal.getInstance` en lugar de `.modal("show")`.
- [ ] `showModaltoDelete` usa `KTModal.getInstance` para el diálogo de confirmación.
- [ ] Las validaciones cliente de `New.cshtml` y `Edit.cshtml` usan los plugins `Trigger`, `SubmitButton` y `Message` con clases Tailwind y feedback visual `core.element.validated`.
- [ ] No se modifica `CatAccountTypeController`, sus servicios, repositorios, entidades, DTOs ni `CatAccountTypeViewModel`.
- [ ] Otras vistas que usan funciones de `Utilities.js` migradas (p.ej. `renderStatus`, `renderStatusEmployee`, `renderAccountType`, `renderStatusAnalytics`) no rompen su renderizado de badges.
- [ ] CatAccountType es usable en desktop y mobile.
- [ ] `dotnet build "TradingBookApp.sln"` termina correctamente, permitiendo solo warnings preexistentes no relacionados con esta migración.

## Decisiones

- **Sí:** Incluir `Web/Views/CatAccountType/Index.cshtml`, `Web/Views/CatAccountType/New.cshtml` y `Web/Views/CatAccountType/Edit.cshtml`. El flujo funcional de CatAccountType depende de la tabla y de los formularios modales de alta y edición.
- **Sí:** Replicar la estructura visual de `Web/Views/CatFigure/Index.cshtml`, `CatFigure/New.cshtml` y `CatFigure/Edit.cshtml`. Esto mantiene consistencia visual entre módulos migrados y evita decisiones de diseño por pantalla.
- **Sí:** Eliminar el breadcrumb de CatAccountType. Employees, CatCategory y CatFigure ya eliminaron el suyo; mantenerlo en CatAccountType crearía inconsistencia visual entre módulos migrados.
- **Sí:** Migrar la columna `isActived` a badges `kt-badge` de Metronic Tailwind. Mantener el render Bootstrap rompería la consistencia visual del resto de la tabla migrada.
- **Sí:** Migrar los badges de `Utilities.js` a `kt-badge` en lugar de hacerlo inline solo en CatAccountType. El cambio en el archivo compartido beneficia a todas las vistas que usan `renderStatus`, `renderStatusEmployee`, `renderAccountType` y `renderStatusAnalytics`, y evita tener badges inconsistentes entre vistas.
- **Sí:** Migrar `showModal` y `showModaltoDelete` a `KTModal.getInstance`. El modal global `#vModal` ya se inicializa como KTModal desde SPEC 02; usar `.modal("show")` de Bootstrap rompería la compatibilidad.
- **Sí:** Actualizar FormValidation al patrón de CatFigure: plugins `Trigger`, `SubmitButton` y `Message` con clases Tailwind, más feedback visual `core.element.validated`. El plugin `Bootstrap5` puede tener conflictos con el nuevo markup Tailwind.
- **Sí:** Mantener `ActionButtonHelper.GenerateActionMenu` en el controlador sin cambios. El `kt-menu` ya se genera correctamente desde el servidor y la vista solo necesita renderizar el HTML.
- **Sí:** Reemplazar `destroy: true` inline en DataTables por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')`. El patrón de CatFigure es más explícito y evita intentos de destrucción sobre instancias inexistentes.
- **No:** Cambiar `CatAccountTypeController`, servicios, repositorios, entidades, DTOs o `CatAccountTypeViewModel`. Esta spec es solo UI/markup/JS cliente.
- **No:** Agregar validación remota de código duplicado. Se descartó durante la fase de preguntas para mantener el alcance acotado a UI. Si se necesita en el futuro, debe ir en su propia spec.
- **No:** Agregar, quitar o renombrar acciones de fila. Editar y eliminar son las únicas acciones requeridas en CatAccountType.
- **No:** Hacer un rediseño completo de CatAccountType. El objetivo es replicar el comportamiento existente con la mejora visual de Metronic Tailwind.

## Riesgos

| Riesgo | Mitigación |
| ------ | ---------- |
| DataTables renderiza filas nuevas después de paginar, filtrar u ordenar y los menús `kt-menu` dejan de responder. | Reinicializar los menús de Metronic Tailwind en el callback de dibujo de DataTables (`KTMenu.createInstances()`). |
| El modal global `#vModal` no responde a `KTModal.getInstance` porque se perdió la instancia o se reinicializó incorrectamente. | Validar apertura de modales de alta y edición manualmente; mantener consistencia con el patrón de CatFigure que ya está probado. |
| Las clases Tailwind cambian el layout y FormValidation deja de encontrar los campos o aplicar feedback visual correctamente. | Mantener IDs, nombres de inputs y estructura `.fv-row`; validar los tres estados del formulario manualmente: vacío, inválido y envío exitoso. |
| El cambio global de badges Bootstrap a `kt-badge` en `Utilities.js` rompe el renderizado de estado en otras vistas que usan `renderStatus`, `renderStatusEmployee`, `renderAccountType` o `renderStatusAnalytics`. | Validar manualmente al menos una vista representativa por cada función migrada; las clases `kt-badge` son soportadas por el tema Metronic Tailwind cargado globalmente. |
| Las vistas que usan `renderStatusAnalytics` esperan textos distintos ("Válido"/"Pausa" en lugar de "Activo"/"Inactivo"). El cambio de clases CSS no altera el texto, pero el badge podría verse diferente visualmente. | Revisar visualmente que los badges de Analytics sigan siendo legibles con las nuevas clases `kt-badge-success` y `kt-badge-warning`. |
| Warnings preexistentes del build se confunden con regresiones de esta migración. | Ejecutar `dotnet build "TradingBookApp.sln"` antes y después de la migración, separando warnings preexistentes de errores nuevos. |

## Lo que no está en esta spec

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `CatAccountTypeController`, servicios, repositorios, DTOs, entidades o `CatAccountTypeViewModel`.
- Cambiar contratos JSON de `~/CatAccountType/JsonDataTable`.
- Reemplazar el modal global existente por un modal Tailwind propio.
- Cambiar reglas, mensajes o endpoints de validación.
- Agregar validación remota de código duplicado.
- Agregar, quitar o renombrar acciones de fila.
- Rediseñar completamente la experiencia visual de CatAccountType.
- Eliminar Bootstrap, jQuery, DataTables, FormValidation u otras dependencias cliente existentes.

Cada uno de esos puntos debe ir en su propia spec si se decide abordarlo.
