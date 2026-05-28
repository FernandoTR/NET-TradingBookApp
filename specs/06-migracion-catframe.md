# SPEC 06 — Migración de CatFrame a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01, SPEC 02, SPEC 03 · **Fecha:** 2026-05-21
> **Objetivo:** Migrar `CatFrame/Index`, `CatFrame/New` y `CatFrame/Edit` a componentes visuales de Metronic Tailwind conservando el comportamiento actual de DataTables, modales, acciones y validaciones.

## Alcance

**Incluye:**

- Migrar `Web/Views/CatFrame/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/Index.cshtml` (header container con título y botón "Nuevo", sin breadcrumb, card con tabla, search con `kt-input` vía `initComplete` y `layout`).
- Migrar el contenido modal de `Web/Views/CatFrame/New.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/New.cshtml` (`kt-card-content grid`, labels con `kt-form-label`, inputs con `kt-input`, footer con `kt-modal-footer`).
- Migrar el contenido modal de `Web/Views/CatFrame/Edit.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/Edit.cshtml`.
- Mantener DataTables con server-side AJAX hacia `~/CatFrame/JsonDataTable`.
- Mantener las columnas actuales: Número (`id`), Código (`code`), Descripción (`description`) y Acciones (`task`).
- Migrar la columna de acciones de fila a un dropdown `kt-menu` de Metronic Tailwind (el HTML ya lo genera `ActionButtonHelper.GenerateActionMenu` en el controller, la vista debe renderizarlo correctamente).
- Mantener las acciones actuales de fila dentro del `kt-menu`: editar frame y eliminar frame.
- Migrar `showModal` y `showModaltoDelete` a `KTModal` (patrón de SPEC 03 — `KTModal.getInstance`).
- Mantener las reglas de validación cliente actuales con `FormValidation`, migrando sus plugins al mismo patrón de CatCategory (Trigger, SubmitButton, Message con clases Tailwind, feedback visual `core.element.validated`), sin agregar validaciones remotas.
- Mantener `id="frmdata"`, `id="btnSave"`, `Html.BeginForm`, `AntiForgeryToken`, campos ocultos y nombres de inputs sin cambios.
- Validar manualmente la vista CatFrame en desktop y mobile.
- Confirmar que la aplicación sigue compilando con `dotnet build "TradingBookApp.sln"`.

**Fuera de alcance (para specs futuras):**

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `CatFrameController`, servicios, repositorios, DTOs, entidades o `CatFrameViewModel`.
- Cambiar contratos JSON de `~/CatFrame/JsonDataTable`.
- Reemplazar el modal global existente por un modal Tailwind propio.
- Cambiar las reglas, mensajes o endpoints de validación de formularios.
- Agregar validación remota de código duplicado.
- Agregar, quitar o renombrar acciones de fila.
- Rediseñar completamente la experiencia visual de CatFrame.
- Eliminar Bootstrap, jQuery, DataTables, FormValidation u otras dependencias cliente existentes.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos backend.

Se reutiliza `Web.Models.CatFrameViewModel` sin cambios:

```csharp
CatFrameViewModel
```

El controlador `CatFrameController.JsonDataTable` ya genera la columna `Task` con el `kt-menu` de Metronic Tailwind mediante `ActionButtonHelper.GenerateActionMenu`, con las acciones de editar y eliminar. Esta spec no modifica ese código.

Se mantienen los contratos cliente existentes de la tabla y formularios:

```text
POST ~/CatFrame/JsonDataTable
POST ~/CatFrame/Delete
GET  ~/CatFrame/New
GET  ~/CatFrame/Edit/?id={id}
POST ~/CatFrame/Save
POST ~/CatFrame/Update
```

CatFrame no tiene columna `isActived`, por lo que no requiere migración de badges en la tabla ni en `Utilities.js`.

## Plan de implementación

1. Migrar `Web/Views/CatFrame/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/Index.cshtml`:
   - Header container con `kt-container-fixed`, título "Catálogo de Frames", subtítulo y botón "Nuevo" con `kt-btn kt-btn-primary`.
   - Eliminar el breadcrumb existente.
   - Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable`.
   - Agregar script tag para `dataTables.min.js`.
   - Actualizar DataTables: reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
   - Actualizar DataTables con `layout` (topStart, topEnd, bottomStart, bottomEnd) y `initComplete` para el search con `kt-input` (mismo patrón de CatCategory).
   - Migrar las funciones `showModal`, `showModalForNew`, `showModalForUpdate`, `showModaltoDelete` y `processingDelete` a usar `KTModal.getInstance` en lugar de `.modal("show")` / `.modal("hide")`.
   - Mantener `KTMenu.createInstances()` en el `drawCallback`.

2. Migrar `Web/Views/CatFrame/New.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/New.cshtml`:
   - Agrupar campos en `kt-card-content grid gap-5`.
   - Cada campo: `w-full fv-row` con `flex items-baseline flex-wrap lg:flex-nowrap gap-2.5`.
   - Label con clase `kt-form-label max-w-56`, input con clase `kt-input`.
   - Footer con `kt-modal-footer gap-2.5 justify-end`.
   - Botón cancelar: `kt-btn kt-btn-secondary` con `data-kt-modal-dismiss="#vModal"`.
   - Botón guardar: `kt-btn kt-btn-primary`.
   - Mantener `id="frmdata"`, `id="btnSave"`, `Html.BeginForm("Save", "CatFrame")`, `AntiForgeryToken` y `Html.HiddenFor(d => d.Id)`.
   - Actualizar FormValidation al patrón de CatCategory: reemplazar plugin `Bootstrap5` por plugins `SubmitButton` y `Message` con `clazz: 'text-red-500 text-sm mt-1'`, agregar evento `core.element.validated` para feedback visual con clases `border-green-500` / `border-destructive ring-1 ring-red-500`.

3. Migrar `Web/Views/CatFrame/Edit.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/Edit.cshtml`:
   - Mismos cambios visuales que New (`kt-card-content grid`, `kt-form-label`, `kt-input`, `kt-modal-footer`).
   - Mantener `Html.BeginForm("Update", "CatFrame")` y `Html.HiddenFor(d => d.Id)`.
   - Actualizar FormValidation al mismo patrón que New.

4. Validar manualmente que CatFrame carga la tabla con datos, muestra el `kt-menu` de acciones, abre el modal de nuevo frame, abre el modal de edición, ejecuta el diálogo de confirmación para eliminar y guarda correctamente.

5. Revisar CatFrame en desktop y mobile para confirmar que la migración visual no bloquea el uso de tabla, filtros, modales ni acciones.

6. Ejecutar `dotnet build "TradingBookApp.sln"` desde la raíz del repositorio y confirmar que compila sin errores nuevos.

## Criterios de aceptación

- [ ] `Web/Views/CatFrame/Index.cshtml` usa markup y clases visuales de Metronic Tailwind (header container, card, tabla), replicando la estructura de `Web/Views/CatCategory/Index.cshtml`.
- [ ] `Web/Views/CatFrame/New.cshtml` usa markup y clases visuales de Metronic Tailwind (`kt-card-content grid`, `kt-form-label`, `kt-input`, `kt-modal-footer`), replicando la estructura de `Web/Views/CatCategory/New.cshtml`.
- [ ] `Web/Views/CatFrame/Edit.cshtml` usa markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/Edit.cshtml`.
- [ ] DataTables sigue cargando datos desde `~/CatFrame/JsonDataTable` con `serverSide: true`.
- [ ] El search de DataTables usa `kt-input` con el patrón `initComplete` + `layout` de CatCategory.
- [ ] La columna de acciones renderiza el `kt-menu` generado por `ActionButtonHelper.GenerateActionMenu`.
- [ ] El `kt-menu` de acciones sigue disponible después de paginar, filtrar u ordenar la tabla.
- [ ] Las acciones existentes siguen disponibles: editar frame y eliminar frame.
- [ ] `showModal`, `showModalForNew` y `showModalForUpdate` usan `KTModal.getInstance` en lugar de `.modal("show")`.
- [ ] `showModaltoDelete` usa `KTModal.getInstance` para el diálogo de confirmación.
- [ ] Las validaciones cliente de `New.cshtml` y `Edit.cshtml` usan los plugins `Trigger`, `SubmitButton` y `Message` con clases Tailwind y feedback visual `core.element.validated`.
- [ ] No se modifica `CatFrameController`, sus servicios, repositorios, entidades, DTOs ni `CatFrameViewModel`.
- [ ] CatFrame es usable en desktop y mobile.
- [ ] `dotnet build "TradingBookApp.sln"` termina correctamente, permitiendo solo warnings preexistentes no relacionados con esta migración.

## Decisiones

- **Sí:** Incluir `Web/Views/CatFrame/Index.cshtml`, `Web/Views/CatFrame/New.cshtml` y `Web/Views/CatFrame/Edit.cshtml`. El flujo funcional de CatFrame depende de la tabla y de los formularios modales de alta y edición.
- **Sí:** Replicar la estructura visual de `Web/Views/CatCategory/Index.cshtml`, `CatCategory/New.cshtml` y `CatCategory/Edit.cshtml`. Esto mantiene consistencia visual entre módulos migrados y evita decisiones de diseño por pantalla.
- **Sí:** Eliminar el breadcrumb de CatFrame. Employees, CatCategory, CatFigure y CatAccountType ya eliminaron el suyo; mantenerlo en CatFrame crearía inconsistencia visual entre módulos migrados.
- **Sí:** Migrar `showModal` y `showModaltoDelete` a `KTModal.getInstance`. El modal global `#vModal` ya se inicializa como KTModal desde SPEC 02; usar `.modal("show")` de Bootstrap rompería la compatibilidad.
- **Sí:** Actualizar FormValidation al patrón de CatCategory: plugins `Trigger`, `SubmitButton` y `Message` con clases Tailwind, más feedback visual `core.element.validated`. El plugin `Bootstrap5` puede tener conflictos con el nuevo markup Tailwind.
- **Sí:** Mantener `ActionButtonHelper.GenerateActionMenu` en el controlador sin cambios. El `kt-menu` ya se genera correctamente desde el servidor y la vista solo necesita renderizar el HTML.
- **Sí:** Reemplazar `destroy: true` inline en DataTables por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')`. El patrón de CatCategory es más explícito y evita intentos de destrucción sobre instancias inexistentes.
- **No:** Cambiar `CatFrameController`, servicios, repositorios, entidades, DTOs o `CatFrameViewModel`. Esta spec es solo UI/markup/JS cliente.
- **No:** Agregar validación remota de código duplicado. Se descartó para mantener el alcance acotado a UI. Si se necesita en el futuro, debe ir en su propia spec.
- **No:** Agregar, quitar o renombrar acciones de fila. Editar y eliminar son las únicas acciones requeridas en CatFrame.
- **No:** Hacer un rediseño completo de CatFrame. El objetivo es replicar el comportamiento existente con la mejora visual de Metronic Tailwind.
- **No:** Agregar columna `isActived` o cualquier otra columna a la tabla. CatFrame no tiene campo de estado y no lo requiere para su funcionalidad actual.

## Riesgos

| Riesgo | Mitigación |
| ------ | ---------- |
| DataTables renderiza filas nuevas después de paginar, filtrar u ordenar y los menús `kt-menu` dejan de responder. | Reinicializar los menús de Metronic Tailwind en el callback de dibujo de DataTables (`KTMenu.createInstances()`). |
| El modal global `#vModal` no responde a `KTModal.getInstance` porque se perdió la instancia o se reinicializó incorrectamente. | Validar apertura de modales de alta y edición manualmente; mantener consistencia con el patrón de CatCategory que ya está probado. |
| Las clases Tailwind cambian el layout y FormValidation deja de encontrar los campos o aplicar feedback visual correctamente. | Mantener IDs, nombres de inputs y estructura `.fv-row`; validar los tres estados del formulario manualmente: vacío, inválido y envío exitoso. |
| El `showModaltoDelete` actual usa `#confirmDialog` como modal separado con `.bind`/`.unbind`. Migrar a `KTModal.getInstance` requiere reemplazar esa lógica de binding por un handler directo. | Revisar el patrón de confirmación de CatCategory; usar `addEventListener` en lugar de `.bind`/`.unbind` de jQuery para el botón de confirmación. |
| Warnings preexistentes del build se confunden con regresiones de esta migración. | Ejecutar `dotnet build "TradingBookApp.sln"` antes y después de la migración, separando warnings preexistentes de errores nuevos. |

## Lo que no está en esta spec

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `CatFrameController`, servicios, repositorios, DTOs, entidades o `CatFrameViewModel`.
- Cambiar contratos JSON de `~/CatFrame/JsonDataTable`.
- Reemplazar el modal global existente por un modal Tailwind propio.
- Cambiar reglas, mensajes o endpoints de validación.
- Agregar validación remota de código duplicado.
- Agregar, quitar o renombrar acciones de fila.
- Rediseñar completamente la experiencia visual de CatFrame.
- Eliminar Bootstrap, jQuery, DataTables, FormValidation u otras dependencias cliente existentes.

Cada uno de esos puntos debe ir en su propia spec si se decide abordarlo.
