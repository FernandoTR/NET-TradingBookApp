# SPEC 08 — Migración de Roles a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01, SPEC 02 · **Fecha:** 2026-05-22
> **Objetivo:** Migrar `Roles/Index`, `Roles/New` y `Roles/Edit` a componentes visuales de Metronic Tailwind conservando el comportamiento actual de DataTables, modal, jsTree de permisos, acciones y validaciones.

## Alcance

**Incluye:**

- Migrar `Web/Views/Roles/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Index.cshtml` (header container con título y botón "Nuevo", sin breadcrumb, card con tabla, search con `kt-input` vía `initComplete` y `layout`).
- Migrar el contenido modal de `Web/Views/Roles/New.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/New.cshtml` (`kt-card-content grid`, labels con `kt-form-label`, inputs con `kt-input`, footer con `kt-modal-footer`).
- Migrar el contenido modal de `Web/Views/Roles/Edit.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Edit.cshtml`.
- Migrar el label "Permisos" y el contenedor del jsTree a clases visuales de Metronic Tailwind (`kt-form-label`, `w-full`), manteniendo el CSS y comportamiento de jsTree sin cambios.
- Mantener DataTables con server-side AJAX hacia `~/Roles/JsonDataTable`.
- Mantener las columnas actuales: ID (`rolId`), Nombre (`nameRoles`) y Acciones (`task`).
- Migrar la columna de acciones de fila a un dropdown `kt-menu` de Metronic Tailwind (el HTML ya lo genera `ActionButtonHelper.GenerateActionMenu` en el controller, la vista debe renderizarlo correctamente).
- Mantener la acción actual de fila dentro del `kt-menu`: editar perfil.
- Migrar `showModal` a `KTModal` (patrón de SPEC 02 — `KTModal.getInstance`), manteniendo `fetch()` para cargar contenido de partials.
- Renombrar funciones `ShowModalForNew` / `ShowModalForUpdate` a `showModalForNew` / `showModalForUpdate` (camelCase) para consistencia con catálogos migrados.
- Mantener las reglas de validación cliente actuales con `FormValidation`, migrando sus plugins al mismo patrón de CatFigure (Trigger, SubmitButton, Message con clases Tailwind, feedback visual `core.element.validated`), sin agregar validaciones remotas.
- Mantener `id="frmdata"`, `id="btnSave"`, `Html.BeginForm`, `AntiForgeryToken`, campos ocultos y nombres de inputs sin cambios.
- Mantener sin cambios los campos del formulario: `NameRoles`, `RolId`, `listAccessString` y el jsTree `CheckboxTree`.
- Mantener la inicialización de jsTree dentro del `$(document).ready()` de cada formulario sin cambios funcionales.
- Cambiar la referencia de jsTree en Index.cshtml de `~/Template/assets/plugins/custom/jstree/` a `~/Template/custom/lib/jstree/`.
- Eliminar `data-kt-indicator` del botón submit (consistencia con CatFigure).
- Unificar el botón Cancel de Edit.cshtml de `type="reset"` a `type="button"` (consistencia con CatFigure).
- Eliminar tooltips Bootstrap (`data-toggle="tooltip"`) del botón "Nuevo" y del `drawCallback`.
- Eliminar el breadcrumb existente.
- Validar manualmente la vista Roles en desktop y mobile.
- Confirmar que la aplicación sigue compilando con `dotnet build "TradingBookApp.sln"`.

**Fuera de alcance (para specs futuras):**

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `RolesController`, servicios, repositorios, DTOs, entidades o `RolesViewModel`.
- Cambiar contratos JSON de `~/Roles/JsonDataTable`.
- Reemplazar el modal global existente por un modal Tailwind propio.
- Cambiar las reglas, mensajes o endpoints de validación de formularios.
- Cambiar el comportamiento, configuración, plugins o CSS del jsTree de permisos.
- Agregar acción de eliminar perfil.
- Agregar, quitar o renombrar columnas de la tabla.
- Agregar, quitar o renombrar acciones de fila.
- Agregar columna de estado (`isActived`) a la tabla.
- Rediseñar completamente la experiencia visual de Roles.
- Eliminar Bootstrap, jQuery, DataTables, FormValidation o jsTree.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos backend.

Se reutiliza `Web.Models.RolesViewModel` sin cambios:

```csharp
RolesViewModel
```

El controlador `RolesController.JsonDataTable` ya genera la columna `Task` con el `kt-menu` de Metronic Tailwind mediante `ActionButtonHelper.GenerateActionMenu`, con la acción de editar. Esta spec no modifica ese código.

Se mantienen los contratos cliente existentes de la tabla y formularios:

```text
POST ~/Roles/JsonDataTable
GET  ~/Roles/New
GET  ~/Roles/Edit/?id={id}
POST ~/Roles/Save
POST ~/Roles/Update
GET  ~/Roles/GetPermissions?rolId={rolId}
```

Roles no tiene columna `isActived`, por lo que no se migran badges de estado en la tabla ni en `Utilities.js`.

## Plan de implementación

1. Migrar `Web/Views/Roles/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Index.cshtml`:
   - Reemplazar todo el toolbar Bootstrap y layout (`kt_app_toolbar`, `kt_app_content`, `card`, `card-body`) por header container con `kt-container-fixed`, título "Perfiles de usuario", subtítulo y botón "Nuevo" con `kt-btn kt-btn-primary`.
   - Eliminar el breadcrumb completo.
   - Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable` con clase `kt-table` y `data-kt-datatable-table="true"`.
   - Cambiar referencia de jsTree CSS de `~/Template/assets/plugins/custom/jstree/jstree.bundle.css` a `~/Template/custom/lib/jstree/jstree.bundle.css`.
   - Agregar script tag para `dataTables.min.js`.
   - Cambiar referencia de jsTree JS de `~/Template/assets/plugins/custom/jstree/jstree.bundle.js` a `~/Template/custom/lib/jstree/jstree.bundle.js`.
   - Actualizar DataTables: reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
   - Actualizar DataTables con `layout` (topStart, topEnd, bottomStart, bottomEnd) y `initComplete` para el search con `kt-input` (mismo patrón de CatFigure, placeholder "Buscar perfiles...").
   - Mantener todas las columnas actuales con sus `data` y `name`: `rolId`, `nameRoles`, `task`.
   - Convertir clases `className` de `min-w-Xpx` a `min-w-[Xpx]`.
   - Actualizar lenguaje de DataTables 1.x (`sProcessing`, `oPaginate`) a DataTables 2.x (`processing`, `paginate`).
   - Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback`.
   - Migrar `showModal` a `KTModal.getInstance` en lugar de `.modal("show")`, manteniendo `fetch()` para cargar partials.
   - Renombrar `ShowModalForNew` a `showModalForNew` y `ShowModalForUpdate` a `showModalForUpdate` (camelCase).
   - Eliminar `$('[data-toggle="tooltip"]').tooltip()` de `showModalForNew`.
   - Eliminar `$("#NameRoles").focus()` de `showModal`.
   - Mantener `KTMenu.createInstances()` en el `drawCallback`.
   - No incluir funciones `showModaltoDelete` ni `processingDelete` (Roles no tiene acción de eliminar).

2. Migrar `Web/Views/Roles/New.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/New.cshtml`:
   - Agrupar campo NameRoles en `w-full fv-row` con `flex items-baseline flex-wrap lg:flex-nowrap gap-2.5`.
   - Label con clase `kt-form-label max-w-56`, input con clase `kt-input`.
   - Grupo de permisos: `w-full` (fuera de `fv-row`, sin validación), label "Permisos" con `kt-form-label`, `<hr>` y `<div id="CheckboxTree">` sin cambios funcionales.
   - Footer con `kt-modal-footer gap-2.5 justify-end`.
   - Botón cancelar: `kt-btn kt-btn-secondary`, `type="button"`, con `data-kt-modal-dismiss="#vModal"`.
   - Botón guardar: `kt-btn kt-btn-primary`, `type="button"`, sin `data-kt-indicator` ni indicadores de carga.
   - Mantener `id="frmdata"`, `id="btnSave"`, `Html.BeginForm("Save", "Roles")`, `AntiForgeryToken` y `Html.HiddenFor(d => d.RolId)`.
   - Actualizar FormValidation al patrón de CatFigure: plugins `SubmitButton`, `Trigger` y `Message` con `clazz: 'text-red-500 text-sm mt-1'`, remover plugin `Bootstrap5`, agregar evento `core.element.validated` para feedback visual con clases `border-green-500` / `border-destructive ring-1 ring-red-500`.
   - Mantener las mismas reglas de validación: `notEmpty` para `NameRoles`.
   - Mantener inicialización de jsTree sin cambios: `$('#CheckboxTree').jstree(...)` con misma configuración de `core`, `themes`, `plugins` y `checkbox`.
   - Mantener `$('#listAccessString').val($('#CheckboxTree').jstree('get_selected'))` en el submit.

3. Migrar `Web/Views/Roles/Edit.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Edit.cshtml`:
   - Mismos cambios visuales que New (`kt-card-content grid`, `kt-form-label`, `kt-input`, `kt-modal-footer`, grupo de permisos con `kt-form-label`).
   - Mantener `Html.BeginForm("Update", "Roles")`, `Html.HiddenFor(d => d.RolId)` y `Html.HiddenFor(x => x.listAccessString)`.
   - Botón cancelar: unificar `type="reset"` a `type="button"`.
   - Actualizar FormValidation al mismo patrón que New.
   - Mantener inicialización de jsTree sin cambios (misma configuración, usando `tree.jstree('get_selected')` en submit).

4. Validar manualmente que Roles carga la tabla con datos, muestra el `kt-menu` de acciones, abre el modal de nuevo perfil, abre el modal de edición con el jsTree de permisos cargado correctamente, valida el nombre del rol, persiste los permisos seleccionados al guardar y actualiza correctamente.

5. Revisar Roles en desktop y mobile para confirmar que la migración visual no bloquea el uso de tabla, filtros, modales ni jsTree.

6. Ejecutar `dotnet build "TradingBookApp.sln"` desde la raíz del repositorio y confirmar que compila sin errores nuevos.

## Criterios de aceptación

- [ ] `Web/Views/Roles/Index.cshtml` usa markup y clases visuales de Metronic Tailwind (header container, card, tabla), replicando la estructura de `Web/Views/CatFigure/Index.cshtml`.
- [ ] `Web/Views/Roles/New.cshtml` usa markup y clases visuales de Metronic Tailwind (`kt-card-content grid`, `kt-form-label`, `kt-input`, `kt-modal-footer`), replicando la estructura de `Web/Views/CatFigure/New.cshtml`.
- [ ] `Web/Views/Roles/Edit.cshtml` usa markup y clases visuales de Metronic Tailwind (`kt-card-content grid`, `kt-form-label`, `kt-input`, `kt-modal-footer`), replicando la estructura de `Web/Views/CatFigure/Edit.cshtml`.
- [ ] Las referencias a jsTree en Index.cshtml apuntan a `~/Template/custom/lib/jstree/jstree.bundle.css` y `jstree.bundle.js`.
- [ ] DataTables sigue cargando datos desde `~/Roles/JsonDataTable` con `serverSide: true`.
- [ ] El search de DataTables usa `kt-input` con el patrón `initComplete` + `layout` de CatFigure.
- [ ] Las columnas se mantienen sin cambios: `rolId` (ID), `nameRoles` (Nombre), `task` (Acciones).
- [ ] La columna de acciones renderiza el `kt-menu` generado por `ActionButtonHelper.GenerateActionMenu`.
- [ ] El `kt-menu` de acciones sigue disponible después de paginar, filtrar u ordenar la tabla.
- [ ] La única acción disponible es editar perfil (Roles no tiene eliminar).
- [ ] `showModal` usa `KTModal.getInstance` en lugar de `.modal("show")` y mantiene `fetch()` para cargar partials.
- [ ] Las funciones `showModalForNew` y `showModalForUpdate` están en camelCase.
- [ ] El jsTree de permisos se inicializa y carga correctamente al abrir el modal de nuevo y edición.
- [ ] El label "Permisos" usa `kt-form-label` y su contenedor usa `w-full` con consistencia visual respecto al resto del formulario.
- [ ] Las validaciones cliente de `New.cshtml` y `Edit.cshtml` usan los plugins `Trigger`, `SubmitButton` y `Message` con clases Tailwind y feedback visual `core.element.validated`.
- [ ] Las reglas de validación se mantienen sin cambios: `notEmpty` para `NameRoles`.
- [ ] El botón submit no tiene `data-kt-indicator` ni indicadores de carga.
- [ ] El botón cancelar de Edit.cshtml usa `type="button"` (no `type="reset"`).
- [ ] No se modifica `RolesController`, sus servicios, repositorios, entidades, DTOs ni `RolesViewModel`.
- [ ] No existen funciones `showModaltoDelete` ni `processingDelete` en Index.cshtml.
- [ ] No hay tooltips Bootstrap (`data-toggle="tooltip"`) en Index.cshtml.
- [ ] No hay breadcrumb en Index.cshtml.
- [ ] Roles es usable en desktop y mobile.
- [ ] `dotnet build "TradingBookApp.sln"` termina correctamente, permitiendo solo warnings preexistentes no relacionados con esta migración.

## Decisiones

- **Sí:** Incluir `Web/Views/Roles/Index.cshtml`, `Web/Views/Roles/New.cshtml` y `Web/Views/Roles/Edit.cshtml`. El flujo funcional de Roles depende de la tabla y de los formularios modales de alta y edición.
- **Sí:** Replicar la estructura visual de `Web/Views/CatFigure/Index.cshtml`, `CatFigure/New.cshtml` y `CatFigure/Edit.cshtml`. CatFigure es la base canónica más reciente y mantiene consistencia visual entre módulos migrados.
- **Sí:** Mantener jsTree sin cambios funcionales (JS, CSS, configuración, plugins, inicialización dentro de `$(document).ready()`). jsTree tiene su propio CSS y comportamiento; esta spec solo migra el contenedor visual que lo envuelve.
- **Sí:** Migrar el label "Permisos" y contenedor del jsTree a `kt-form-label` y `w-full`. Mantiene consistencia visual con el resto de labels del formulario bajo Metronic Tailwind.
- **Sí:** Cambiar referencias de jsTree de `~/Template/assets/plugins/custom/jstree/` a `~/Template/custom/lib/jstree/`. La carpeta assets ya no existe; custom/lib es donde reside la librería.
- **Sí:** Eliminar el breadcrumb de Roles. Todos los catálogos migrados (Employees, CatCategory, CatFigure, CatAccountType, CatFrame, CatInstruments) ya eliminaron el suyo.
- **Sí:** Migrar `showModal` a `KTModal.getInstance`. El modal global `#vModal` ya se inicializa como KTModal desde SPEC 02; usar `.modal("show")` de Bootstrap rompería la compatibilidad.
- **Sí:** Renombrar `ShowModalForNew` / `ShowModalForUpdate` a camelCase. Todos los catálogos migrados usan camelCase para estas funciones.
- **Sí:** Actualizar FormValidation al patrón de CatFigure: plugins `Trigger`, `SubmitButton` y `Message` con clases Tailwind, más feedback visual `core.element.validated`. El plugin `Bootstrap5` puede tener conflictos con el nuevo markup Tailwind.
- **Sí:** Eliminar `data-kt-indicator` del botón submit y unificar Cancel a `type="button"`. CatFigure no usa indicadores de carga y todos los botones Cancel son `type="button"`.
- **Sí:** Eliminar `$("#NameRoles").focus()` de `showModal`. CatFigure no fuerza foco tras abrir el modal; el comportamiento de foco lo maneja el navegador.
- **Sí:** Reemplazar `destroy: true` inline en DataTables por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')`. El patrón de CatFigure es más explícito y evita intentos de destrucción sobre instancias inexistentes.
- **Sí:** Usar layout de DataTables 2.x y lenguaje con claves modernas (`processing`, `paginate`). El markup y JS de la tabla deben ser consistentes con CatFigure.
- **Sí:** Mantener `ActionButtonHelper.GenerateActionMenu` en el controlador sin cambios. El `kt-menu` ya se genera correctamente desde el servidor y la vista solo necesita renderizar el HTML.
- **No:** Cambiar `RolesController`, servicios, repositorios, entidades, DTOs o `RolesViewModel`. Esta spec es solo UI/markup/JS cliente.
- **No:** Agregar acción de eliminar perfil. Roles no tiene Delete en el controlador ni en el menú de acciones. Si se necesita en el futuro, debe ir en su propia spec.
- **No:** Cambiar la configuración, plugins, temas o CSS del jsTree. jsTree es un componente independiente de jQuery con su propio sistema de estilos; alterarlo podría romper el árbol de permisos.
- **No:** Agregar validación al jsTree de permisos. Actualmente no tiene validación y agregarla requiere decisión de reglas que van más allá de una migración visual.
- **No:** Agregar, quitar o renombrar columnas de la tabla. Las tres columnas actuales (`rolId`, `nameRoles`, `task`) se mantienen.
- **No:** Agregar columna de estado (`isActived`). Roles no tiene campo de estado y no lo requiere para su funcionalidad actual.
- **No:** Hacer un rediseño completo de Roles. El objetivo es replicar el comportamiento existente con la mejora visual de Metronic Tailwind.

## Riesgos

| Riesgo | Mitigación |
| ------ | ---------- |
| El jsTree de permisos pierde funcionalidad al cambiar el contenedor visual a clases Tailwind. | Mantener el `<div id="CheckboxTree">` intacto dentro del nuevo wrapper `w-full`; jsTree renderiza su propio HTML dentro de ese div y no depende de las clases del contenedor exterior. Validar manualmente que los checkboxes se muestran, se seleccionan/deseleccionan y persisten al guardar. |
| DataTables renderiza filas nuevas después de paginar, filtrar u ordenar y los menús `kt-menu` dejan de responder. | Reinicializar los menús de Metronic Tailwind en el callback de dibujo de DataTables (`KTMenu.createInstances()`). |
| El modal global `#vModal` no responde a `KTModal.getInstance` porque se perdió la instancia o se reinicializó incorrectamente. | Validar apertura de modales de alta y edición manualmente; mantener consistencia con el patrón de CatFigure que ya está probado. |
| Las clases Tailwind cambian el layout y FormValidation deja de encontrar los campos o aplicar feedback visual correctamente. | Mantener IDs, nombres de inputs y estructura `.fv-row` para NameRoles; validar los tres estados del formulario manualmente: vacío, inválido y envío exitoso. |
| La inicialización de jsTree usa `$.fn.jstree` que requiere jQuery y el bundle cargado en Index.cshtml. Si la ruta nueva `~/Template/custom/lib/jstree/jstree.bundle.js` no carga, el árbol no se renderiza. | Confirmar que el archivo existe en la ruta indicada; validar que el jsTree carga correctamente al abrir los modales de nuevo y edición desde Index. |
| El `listAccessString` se captura con `$('#CheckboxTree').jstree('get_selected')` (New) y `tree.jstree('get_selected')` (Edit). La diferencia de referencia entre New y Edit podría causar que una de las dos vistas falle. | No unificar las referencias en esta spec; validar ambas vistas manualmente. La diferencia es preexistente y no es parte de la migración visual. |
| Warnings preexistentes del build se confunden con regresiones de esta migración. | Ejecutar `dotnet build "TradingBookApp.sln"` antes y después de la migración, separando warnings preexistentes de errores nuevos. |

## Lo que no está en esta spec

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `RolesController`, servicios, repositorios, DTOs, entidades o `RolesViewModel`.
- Cambiar contratos JSON de `~/Roles/JsonDataTable`.
- Reemplazar el modal global existente por un modal Tailwind propio.
- Cambiar las reglas, mensajes o endpoints de validación de formularios.
- Cambiar el comportamiento, configuración, plugins o CSS del jsTree de permisos.
- Agregar acción de eliminar perfil.
- Agregar, quitar o renombrar columnas de la tabla.
- Agregar, quitar o renombrar acciones de fila.
- Agregar columna de estado (`isActived`) a la tabla.
- Rediseñar completamente la experiencia visual de Roles.
- Eliminar Bootstrap, jQuery, DataTables, FormValidation o jsTree.

Cada uno de esos puntos debe ir en su propia spec si se decide abordarlo.
