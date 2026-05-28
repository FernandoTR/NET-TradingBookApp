# SPEC 04 — Migración de CatFigure a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01, SPEC 02, SPEC 03 · **Fecha:** 2026-05-21
> **Objetivo:** Migrar `CatFigure/Index`, `CatFigure/New` y `CatFigure/Edit` a componentes visuales de Metronic Tailwind conservando el comportamiento actual de DataTables, modales, acciones y validaciones.

## Alcance

**Incluye:**

- Migrar `Web/Views/CatFigure/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/Index.cshtml` (header container con título y botón "Nuevo", sin breadcrumb, card con tabla, search con `kt-input` vía `initComplete` y `layout`).
- Migrar la columna `isActived` (Estado) de clases Bootstrap a badges `kt-badge` de Metronic Tailwind.
- Migrar el contenido modal de `Web/Views/CatFigure/New.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/New.cshtml` (`kt-card-content grid`, labels con `kt-form-label`, inputs con `kt-input`, footer con `kt-modal-footer`).
- Migrar el contenido modal de `Web/Views/CatFigure/Edit.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/Edit.cshtml`.
- Mantener DataTables con server-side AJAX hacia `~/CatFigure/JsonDataTable`.
- Migrar la columna de acciones de fila a un dropdown `kt-menu` de Metronic Tailwind (el HTML ya lo genera `ActionButtonHelper.GenerateActionMenu` en el controller, la vista debe renderizarlo correctamente).
- Mantener las acciones actuales de fila dentro del `kt-menu`: editar figura y eliminar figura.
- Migrar `showModal` y `showModaltoDelete` a `KTModal` (patrón de SPEC 03 — `KTModal.getInstance`).
- Mantener las reglas de validación cliente actuales con `FormValidation`, migrando sus plugins al mismo patrón de CatCategory (Trigger, SubmitButton, Message con clases Tailwind, feedback visual `core.element.validated`), sin agregar validaciones remotas.
- Mantener `id="frmdata"`, `id="btnSave"`, `Html.BeginForm`, `AntiForgeryToken`, campos ocultos y nombres de inputs sin cambios.
- Validar manualmente la vista CatFigure en desktop y mobile.
- Confirmar que la aplicación sigue compilando con `dotnet build "TradingBookApp.sln"`.

**Fuera de alcance (para specs futuras):**

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `CatFigureController`, servicios, repositorios, DTOs, entidades o `CatFigureViewModel`.
- Cambiar contratos JSON de `~/CatFigure/JsonDataTable`.
- Reemplazar el modal global existente por un modal Tailwind propio.
- Cambiar las reglas, mensajes o endpoints de validación de formularios.
- Agregar validación remota de código duplicado.
- Agregar, quitar o renombrar acciones de fila.
- Rediseñar completamente la experiencia visual de CatFigure.
- Eliminar Bootstrap, jQuery, DataTables, FormValidation u otras dependencias cliente existentes.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos backend.

Se reutiliza `Web.Models.CatFigureViewModel` sin cambios:

```csharp
CatFigureViewModel
```

El controlador `CatFigureController.JsonDataTable` ya genera la columna `Task` con el `kt-menu` de Metronic Tailwind mediante `ActionButtonHelper.GenerateActionMenu`, con las acciones de editar y eliminar. Esta spec no modifica ese código.

Se mantienen los contratos cliente existentes de la tabla y formularios:

```text
POST ~/CatFigure/JsonDataTable
POST ~/CatFigure/Delete
GET  ~/CatFigure/New
GET  ~/CatFigure/Edit/?id={id}
POST ~/CatFigure/Save
POST ~/CatFigure/Update
```

La columna `isActived` debe migrarse de su render Bootstrap actual a badges `kt-badge` de Metronic Tailwind, siguiendo el patrón:

```html
<span class="kt-badge kt-badge-success">Activo</span>
<span class="kt-badge kt-badge-danger">Inactivo</span>
```

## Plan de implementación

1. Migrar `Web/Views/CatFigure/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/Index.cshtml`:
   - Header container con `kt-container-fixed`, título "Figuras", subtítulo y botón "Nuevo" con `kt-btn kt-btn-primary`.
   - Eliminar el breadcrumb existente.
   - Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable`.
   - Agregar script tag para `dataTables.min.js`.
   - Actualizar DataTables: reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
   - Actualizar DataTables con `layout` (topStart, topEnd, bottomStart, bottomEnd) y `initComplete` para el search con `kt-input` (mismo patrón de CatCategory).
   - Migrar la columna `isActived` de `render: renderStatus` con clases Bootstrap a badges `kt-badge` de Metronic Tailwind inline en el render de DataTables.
   - Migrar las funciones `showModal`, `showModalForNew`, `showModalForUpdate`, `showModaltoDelete` y `processingDelete` a usar `KTModal.getInstance` en lugar de `.modal("show")` / `.modal("hide")`.

2. Migrar `Web/Views/CatFigure/New.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/New.cshtml`:
   - Agrupar campos en `kt-card-content grid gap-5`.
   - Cada campo: `w-full fv-row` con `flex items-baseline flex-wrap lg:flex-nowrap gap-2.5`.
   - Label con clase `kt-form-label max-w-56`, input con clase `kt-input`.
   - Footer con `kt-modal-footer gap-2.5 justify-end`.
   - Botón cancelar: `kt-btn kt-btn-secondary` con `data-kt-modal-dismiss="#vModal"`.
   - Botón guardar: `kt-btn kt-btn-primary`.
   - Mantener `id="frmdata"`, `id="btnSave"`, `Html.BeginForm("Save", "CatFigure")`, `AntiForgeryToken` y `Html.HiddenFor(d => d.Id)`.
   - Actualizar FormValidation al patrón de CatCategory: reemplazar plugin `Bootstrap5` por plugins `SubmitButton` y `Message` con `clazz: 'text-red-500 text-sm mt-1'`, agregar evento `core.element.validated` para feedback visual con clases `border-green-500` / `border-destructive ring-1 ring-red-500`.

3. Migrar `Web/Views/CatFigure/Edit.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/Edit.cshtml`:
   - Mismos cambios visuales que New (`kt-card-content grid`, `kt-form-label`, `kt-input`, `kt-modal-footer`).
   - Mantener `Html.BeginForm("Update", "CatFigure")` y `Html.HiddenFor(d => d.Id)`, `Html.HiddenFor(d => d.IsActived)`.
   - Actualizar FormValidation al mismo patrón que New.

4. Validar manualmente que CatFigure carga la tabla con datos, muestra el `kt-menu` de acciones, muestra el badge de estado correctamente, abre el modal de nueva figura, abre el modal de edición, ejecuta el diálogo de confirmación para eliminar y guarda correctamente.

5. Revisar CatFigure en desktop y mobile para confirmar que la migración visual no bloquea el uso de tabla, filtros, modales ni acciones.

6. Ejecutar `dotnet build "TradingBookApp.sln"` desde la raíz del repositorio y confirmar que compila sin errores nuevos.

## Criterios de aceptación

- [ ] `Web/Views/CatFigure/Index.cshtml` usa markup y clases visuales de Metronic Tailwind (header container, card, tabla), replicando la estructura de `Web/Views/CatCategory/Index.cshtml`.
- [ ] `Web/Views/CatFigure/New.cshtml` usa markup y clases visuales de Metronic Tailwind (`kt-card-content grid`, `kt-form-label`, `kt-input`, `kt-modal-footer`), replicando la estructura de `Web/Views/CatCategory/New.cshtml`.
- [ ] `Web/Views/CatFigure/Edit.cshtml` usa markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatCategory/Edit.cshtml`.
- [ ] DataTables sigue cargando datos desde `~/CatFigure/JsonDataTable` con `serverSide: true`.
- [ ] El search de DataTables usa `kt-input` con el patrón `initComplete` + `layout` de CatCategory.
- [ ] La columna `isActived` renderiza badges `kt-badge` de Metronic Tailwind en lugar de clases Bootstrap.
- [ ] La columna de acciones renderiza el `kt-menu` generado por `ActionButtonHelper.GenerateActionMenu`.
- [ ] El `kt-menu` de acciones sigue disponible después de paginar, filtrar u ordenar la tabla.
- [ ] Las acciones existentes siguen disponibles: editar figura y eliminar figura.
- [ ] `showModal`, `showModalForNew` y `showModalForUpdate` usan `KTModal.getInstance` en lugar de `.modal("show")`.
- [ ] `showModaltoDelete` usa `KTModal.getInstance` para el diálogo de confirmación.
- [ ] Las validaciones cliente de `New.cshtml` y `Edit.cshtml` usan los plugins `Trigger`, `SubmitButton` y `Message` con clases Tailwind y feedback visual `core.element.validated`.
- [ ] No se modifica `CatFigureController`, sus servicios, repositorios, entidades, DTOs ni `CatFigureViewModel`.
- [ ] CatFigure es usable en desktop y mobile.
- [ ] `dotnet build "TradingBookApp.sln"` termina correctamente, permitiendo solo warnings preexistentes no relacionados con esta migración.

## Decisiones

- **Sí:** Incluir `Web/Views/CatFigure/Index.cshtml`, `Web/Views/CatFigure/New.cshtml` y `Web/Views/CatFigure/Edit.cshtml`. El flujo funcional de CatFigure depende de la tabla y de los formularios modales de alta y edición.
- **Sí:** Replicar la estructura visual de `Web/Views/CatCategory/Index.cshtml`, `CatCategory/New.cshtml` y `CatCategory/Edit.cshtml`. Esto mantiene consistencia visual entre módulos migrados y evita decisiones de diseño por pantalla.
- **Sí:** Eliminar el breadcrumb de CatFigure. CatCategory y Employees ya eliminaron el suyo; mantenerlo en CatFigure crearía inconsistencia visual entre módulos migrados.
- **Sí:** Migrar la columna `isActived` de `renderStatus` con clases Bootstrap a badges `kt-badge` de Metronic Tailwind. Mantener el render Bootstrap rompería la consistencia visual del resto de la tabla migrada.
- **Sí:** Migrar `showModal` y `showModaltoDelete` a `KTModal.getInstance`. El modal global `#vModal` ya se inicializa como KTModal desde SPEC 02; usar `.modal("show")` de Bootstrap rompería la compatibilidad.
- **Sí:** Actualizar FormValidation al patrón de CatCategory: plugins `Trigger`, `SubmitButton` y `Message` con clases Tailwind, más feedback visual `core.element.validated`. El plugin `Bootstrap5` puede tener conflictos con el nuevo markup Tailwind.
- **Sí:** Mantener `ActionButtonHelper.GenerateActionMenu` en el controlador sin cambios. El `kt-menu` ya se genera correctamente desde el servidor y la vista solo necesita renderizar el HTML.
- **Sí:** Reemplazar `destroy: true` inline en DataTables por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')`. El patrón de CatCategory es más explícito y evita intentos de destrucción sobre instancias inexistentes.
- **No:** Cambiar `CatFigureController`, servicios, repositorios, entidades, DTOs o `CatFigureViewModel`. Esta spec es solo UI/markup/JS cliente.
- **No:** Agregar validación remota de código duplicado. Se descartó durante la fase de preguntas para mantener el alcance acotado a UI. Si se necesita en el futuro, debe ir en su propia spec.
- **No:** Agregar, quitar o renombrar acciones de fila. Editar y eliminar son las únicas acciones requeridas en CatFigure.
- **No:** Hacer un rediseño completo de CatFigure. El objetivo es replicar el comportamiento existente con la mejora visual de Metronic Tailwind.

## Riesgos

| Riesgo | Mitigación |
| ------ | ---------- |
| DataTables renderiza filas nuevas después de paginar, filtrar u ordenar y los menús `kt-menu` dejan de responder. | Reinicializar los menús de Metronic Tailwind en el callback de dibujo de DataTables (`KTMenu.createInstances()`). |
| El modal global `#vModal` no responde a `KTModal.getInstance` porque se perdió la instancia o se reinicializó incorrectamente. | Validar apertura de modales de alta y edición manualmente; mantener consistencia con el patrón de CatCategory que ya está probado. |
| Las clases Tailwind cambian el layout y FormValidation deja de encontrar los campos o aplicar feedback visual correctamente. | Mantener IDs, nombres de inputs y estructura `.fv-row`; validar los tres estados del formulario manualmente: vacío, inválido y envío exitoso. |
| La migración de la columna `isActived` a `kt-badge` rompe el render condicional de estado activo/inactivo. | Validar visualmente que filas con `isActived=true` muestren badge verde y `isActived=false` muestren badge rojo. |
| Warnings preexistentes del build se confunden con regresiones de esta migración. | Ejecutar `dotnet build "TradingBookApp.sln"` antes y después de la migración, separando warnings preexistentes de errores nuevos. |

## Lo que no está en esta spec

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `CatFigureController`, servicios, repositorios, DTOs, entidades o persistencia.
- Cambiar contratos JSON de `~/CatFigure/JsonDataTable`.
- Reemplazar el modal global existente por un modal Tailwind propio.
- Cambiar reglas, mensajes o endpoints de validación.
- Agregar validación remota de código duplicado.
- Agregar, quitar o renombrar acciones de fila.
- Rediseñar completamente la experiencia visual de CatFigure.
- Eliminar Bootstrap, jQuery, DataTables, FormValidation u otras dependencias cliente existentes.

Cada uno de esos puntos debe ir en su propia spec si se decide abordarlo.
