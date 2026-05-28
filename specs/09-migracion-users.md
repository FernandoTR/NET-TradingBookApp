# SPEC 09 — Migración de Users/Index y Users/Edit a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01, SPEC 02 · **Fecha:** 2026-05-22
> **Objetivo:** Migrar `Users/Index` y `Users/Edit` a componentes visuales de Metronic Tailwind conservando el comportamiento actual de DataTables, modales, acciones y validaciones.

## Alcance

**Incluye:**

- Migrar `Web/Views/Users/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Index.cshtml` (header container con título y botón "Nuevo", sin breadcrumb, card con tabla, search con `kt-input` vía `initComplete` y `layout`).
- Migrar `Web/Views/Users/Edit.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Edit.cshtml` (`kt-card-content grid`, labels con `kt-form-label`, inputs con `kt-input`, footer con `kt-modal-footer`).
- Migrar la sección de asignación de perfiles en `Edit.cshtml` (tabla de checkboxes con `listAccess`) a markup Tailwind conservando la misma funcionalidad de selección múltiple.
- Mantener DataTables con server-side AJAX hacia `~/Users/JsonDataTable`.
- Mantener las columnas actuales: Usuario (`email`), Correo Confirmado (`emailConfirmed`, render `renderTrueFalse`), Bandera Reseteo (`resetFlag`, render `renderFlag`), Rol (`name`), Estatus (`status`, render `renderStatusEmployee`), Accesos Erróneos (`accesFailedCount`), Fecha Fin Contraseña (`passwordEndDate`, render `moment`) y Acciones (`task`).
- Mantener las funciones de renderizado existentes en `Utilities.js`: `renderTrueFalse`, `renderFlag` y `renderStatusEmployee` sin cambios (ya usan `kt-badge` e íconos `ki-duotone` compatibles con Metronic Tailwind).
- Migrar la columna de acciones de fila a un dropdown `kt-menu` de Metronic Tailwind (el HTML ya lo genera `ActionButtonHelper.GenerateActionMenu` en el controller, la vista debe renderizarlo correctamente).
- Mantener las acciones actuales de fila dentro del `kt-menu`: modificar perfil (`showModalForModifyRoles`, solo para usuarios tipo 1) y activar/desactivar usuario (`toDeleteUser`).
- Migrar `showModal` a `KTModal` (patrón de SPEC 02 — `KTModal.getInstance`), manteniendo `fetch()` para cargar contenido de partials.
- Migrar `toDeleteUser` a `KTModal.getInstance` para show/hide del diálogo de confirmación, manteniendo el patrón `.bind`/`.unbind` de CatFigure.
- Mantener `showModalForUpdate` (aunque no es referenciada por el controller actual, se conserva por consistencia).
- Mantener `showModalForModifyRoles` con su título "Modificar Perfil Para el Usuario".
- Mantener las reglas de validación cliente actuales con `FormValidation`, migrando sus plugins al mismo patrón de CatFigure (Trigger, SubmitButton, Message con clases Tailwind, feedback visual `core.element.validated`).
- Mantener `id="frmdata"`, `id="btnSave"`, `Html.BeginForm`, `AntiForgeryToken`, campos ocultos y nombres de inputs sin cambios.
- Mantener sin cambios el campo `Name` (solo lectura, `disabled`) y la lista de checkboxes `listAccess[i].IsSelected` con ids `permissions`.
- Eliminar `data-kt-indicator` del botón submit (consistencia con CatFigure).
- Unificar el botón Cancel de Edit.cshtml de `data-bs-dismiss="modal"` a `data-kt-modal-dismiss="#vModal"` (consistencia con CatFigure).
- Eliminar `$("#EmployeeNumber").focus()` de `showModal` (es un vestigio de Employees, CatFigure no fuerza foco).
- Eliminar tooltips Bootstrap (`data-toggle="tooltip"`) del `drawCallback`.
- Eliminar el breadcrumb existente.
- Agregar script tag para `dataTables.min.js` en Index.cshtml.
- Validar manualmente la vista Users en desktop y mobile.
- Confirmar que la aplicación sigue compilando con `dotnet build "TradingBookApp.sln"`.

**Fuera de alcance (para specs futuras):**

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `UsersController`, servicios, repositorios, DTOs, entidades o `UsersListViewModel`.
- Cambiar contratos JSON de `~/Users/JsonDataTable`.
- Reemplazar el modal global existente por un modal Tailwind propio.
- Cambiar las reglas, mensajes o endpoints de validación de formularios.
- Agregar vista `Users/New` (no existe actualmente).
- Agregar acción o vista `Users/Delete` (la baja lógica ya se maneja desde Index vía `toDeleteUser`/`processingDelete`).
- Agregar, quitar o renombrar columnas de la tabla.
- Agregar, quitar o renombrar acciones de fila.
- Rediseñar completamente la experiencia visual de Users.
- Eliminar Bootstrap, jQuery, DataTables o FormValidation.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos backend.

Se reutilizan sin cambios:

```csharp
Web.Models.UsersListViewModel
Web.Models.UsersRolesViewModel
```

El controlador `UsersController.JsonDataTable` ya genera la columna `Task` con el `kt-menu` de Metronic Tailwind mediante `ActionButtonHelper.GenerateActionMenu`, con las acciones:
- Modificar perfil (`showModalForModifyRoles`) — solo para usuarios con `UserTypeId == 1`.
- Activar / Desactivar (`toDeleteUser`) — para todos los usuarios.

Esta spec no modifica ese código.

Se mantienen los contratos cliente existentes de la tabla y formularios:

```text
POST ~/Users/JsonDataTable
POST ~/Users/Delete
GET  ~/Users/Edit/?id={id}
POST ~/Users/Update
```

Las funciones de renderizado en `Web/wwwroot/Template/custom/js/Utilities.js` ya usan clases compatibles con Metronic Tailwind y no requieren cambios en esta spec:
- `renderStatusEmployee` → `kt-badge kt-badge-success` / `kt-badge kt-badge-danger`
- `renderTrueFalse` → íconos `ki-duotone ki-verify` / `ki-duotone ki-minus-circle`
- `renderFlag` → íconos `ki-duotone ki-flag`

La columna `passwordEndDate` usa `moment` para formateo de fecha (`DD/MM/YYYY HH:mm:ss`) vía función inline en DataTables, sin dependencia de `Utilities.js`.

## Plan de implementación

1. Migrar `Web/Views/Users/Index.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Index.cshtml`:
   - Reemplazar todo el toolbar Bootstrap y layout (`kt_app_toolbar`, `kt_app_content`, `app-container`, `card`, `card-body`) por header container con `kt-container-fixed`, título "Usuarios", subtítulo "Gestión de usuarios del sistema" y botón con `kt-btn kt-btn-primary` (sin tooltips).
   - Nota: Users no tiene vista `New.cshtml`; el botón "Nuevo" no se incluye en Index porque no existe endpoint de creación en el controller.
   - Eliminar el breadcrumb completo.
   - Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable` con clase `kt-table` y `data-kt-datatable-table="true"`.
   - Agregar script tag para `dataTables.min.js`.
   - Actualizar DataTables: reemplazar `destroy: true` por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')` antes de destruir.
   - Actualizar DataTables con `layout` (topStart, topEnd, bottomStart, bottomEnd) y `initComplete` para el search con `kt-input` (mismo patrón de CatFigure, placeholder "Buscar usuarios...").
   - Mantener todas las columnas actuales con sus `data`, `name` y `render`: `email`, `emailConfirmed` (`renderTrueFalse`), `resetFlag` (`renderFlag`), `name`, `status` (`renderStatusEmployee`), `accesFailedCount`, `passwordEndDate` (`moment`), `task`.
   - Convertir clases `className` de `min-w-Xpx` a `min-w-[Xpx]`.
   - Eliminar `filter: true` (no es una opción válida de DataTables; CatFigure usa `searching: true`).
   - Actualizar lenguaje de DataTables 1.x (`sProcessing`, `sLengthMenu`, `sZeroRecords`, `sEmptyTable`, `sInfo`, `sInfoEmpty`, `sInfoFiltered`, `sSearch`, `sLoadingRecords`, `oPaginate`, `oAria`, `buttons`) a DataTables 2.x (`processing`, `lengthMenu`, `zeroRecords`, `emptyTable`, `info`, `infoEmpty`, `infoFiltered`, `search`, `searchPlaceholder`, `paginate`, `aria`).
   - Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback`.
   - Migrar `showModal` a `KTModal.getInstance` en lugar de `.modal("show")`, manteniendo `fetch()` para cargar partials.
   - Eliminar `$("#EmployeeNumber").focus()` de `showModal`.
   - Migrar `toDeleteUser` a `KTModal.getInstance` para `.show()`/`.hide()` del `#confirmDialog`, manteniendo `.bind`/`.unbind` sobre `#confirmButtonYes`.
   - Mantener `processingDelete` sin cambios funcionales (solo usa `fetch` + redirect, no depende del modal).
   - Mantener `showModalForUpdate` y `showModalForModifyRoles` con camelCase (ya están en camelCase, sin cambios de nombre).
   - Mantener `KTMenu.createInstances()` en el `drawCallback`.

2. Migrar `Web/Views/Users/Edit.cshtml` a markup y clases visuales de Metronic Tailwind, replicando la estructura de `Web/Views/CatFigure/Edit.cshtml`:
   - Envolver campos en `kt-card-content grid gap-5`.
   - Campo Usuario (Name, solo lectura): `w-full fv-row` con `flex items-baseline flex-wrap lg:flex-nowrap gap-2.5`. Label "Usuario" con clase `kt-form-label max-w-56`, input con clase `kt-input` y atributo `disabled`.
   - Sección de perfiles: `w-full fv-row`. Label "Seleccione los perfiles para el usuario" con `kt-form-label`, subtítulo con `text-sm text-secondary-foreground`, `<hr>`, y los checkboxes de `listAccess` renderizados en una estructura simplificada con `flex items-center gap-2.5` por cada fila, conservando `form-check-input` y los `HiddenFor` de `RolId` y `Name`.
   - Footer con `kt-modal-footer gap-2.5 justify-end`.
   - Botón cancelar: `kt-btn kt-btn-secondary`, `type="button"`, con `data-kt-modal-dismiss="#vModal"`.
   - Botón guardar: `kt-btn kt-btn-primary`, `type="button"`, sin `data-kt-indicator` ni indicadores de carga (`indicator-label`/`indicator-progress`).
   - Mantener `id="frmdata"`, `id="btnSave"`, `Html.BeginForm("Update", "Users")`, `AntiForgeryToken` y `Html.HiddenFor(d => d.Id)` en `<div hidden>`.
   - Eliminar `@Html.ValidationSummary` y el `<p id="txtValidationMessage">` (el plugin Message de FormValidation maneja los mensajes automáticamente).

3. Actualizar FormValidation en `Edit.cshtml` al patrón de CatFigure:
   - Reemplazar plugin `Bootstrap5` por plugins `Trigger`, `SubmitButton` y `Message` con `clazz: 'text-red-500 text-sm mt-1'`.
   - Agregar evento `core.element.validated` para feedback visual con clases `border-green-500` / `border-destructive ring-1 ring-red-500`.
   - Mantener la regla de validación actual: campo `permissions` con validador `callback` que verifica al menos un checkbox seleccionado, mensaje "Debe seleccionar al menos un Permiso."
   - Mantener el campo `Name` sin validación (es de solo lectura).
   - Eliminar `submitButton.setAttribute('data-kt-indicator', 'on')` del handler del botón.

4. Validar manualmente:
   - Index: la tabla carga datos desde `~/Users/JsonDataTable`, renderiza correctamente todas las columnas con sus badges/íconos, el `kt-menu` de acciones responde (modificar perfil y activar/desactivar), el `kt-menu` sobrevive a paginación/filtrado/ordenamiento.
   - Modal de edición: se abre con `KTModal.getInstance`, carga el contenido vía `fetch()`, muestra los checkboxes de perfiles correctamente, valida que al menos un perfil esté seleccionado, persiste los cambios al guardar.
   - Diálogo de confirmación para activar/desactivar: se abre con `KTModal.getInstance`, confirma la acción y ejecuta el redirect.
   - Desktop y mobile: la tabla es scrolleable, los modales son usables, los checkboxes son cliqueables.

5. Ejecutar `dotnet build "TradingBookApp.sln"` desde la raíz del repositorio y confirmar que compila sin errores nuevos.

## Criterios de aceptación

- [ ] `Web/Views/Users/Index.cshtml` usa markup y clases visuales de Metronic Tailwind (header container, card, tabla), replicando la estructura de `Web/Views/CatFigure/Index.cshtml`.
- [ ] `Web/Views/Users/Edit.cshtml` usa markup y clases visuales de Metronic Tailwind (`kt-card-content grid`, `kt-form-label`, `kt-input`, `kt-modal-footer`), replicando la estructura de `Web/Views/CatFigure/Edit.cshtml`.
- [ ] DataTables sigue cargando datos desde `~/Users/JsonDataTable` con `serverSide: true`.
- [ ] El search de DataTables usa `kt-input` con el patrón `initComplete` + `layout` de CatFigure.
- [ ] Las columnas se mantienen sin cambios: `email` (Usuario), `emailConfirmed` (Correo Confirmado), `resetFlag` (Bandera Reseteo), `name` (Rol), `status` (Estatus), `accesFailedCount` (Accesos Erróneos), `passwordEndDate` (Fecha Fin Contraseña), `task` (Acciones).
- [ ] Las funciones `renderTrueFalse`, `renderFlag` y `renderStatusEmployee` renderizan correctamente (ya usan `kt-badge` y `ki-duotone`, no requieren cambios).
- [ ] La columna de acciones renderiza el `kt-menu` generado por `ActionButtonHelper.GenerateActionMenu`.
- [ ] El `kt-menu` de acciones sigue disponible después de paginar, filtrar u ordenar la tabla.
- [ ] Las acciones existentes siguen disponibles: modificar perfil (solo usuarios tipo 1) y activar/desactivar usuario (todos).
- [ ] `showModal` usa `KTModal.getInstance` en lugar de `.modal("show")` y mantiene `fetch()` para cargar partials.
- [ ] `toDeleteUser` usa `KTModal.getInstance` para show/hide del `#confirmDialog`, manteniendo `.bind`/`.unbind` sobre `#confirmButtonYes`.
- [ ] `showModalForUpdate` y `showModalForModifyRoles` se mantienen sin cambios funcionales.
- [ ] La tabla de checkboxes de perfiles en `Edit.cshtml` se renderiza y funciona correctamente (selección/deselección, envío de `listAccess`).
- [ ] Las validaciones cliente de `Edit.cshtml` usan los plugins `Trigger`, `SubmitButton` y `Message` con clases Tailwind y feedback visual `core.element.validated`.
- [ ] La regla de validación de permisos (callback: al menos un checkbox seleccionado) se mantiene sin cambios.
- [ ] El botón submit no tiene `data-kt-indicator` ni indicadores de carga (`indicator-label`/`indicator-progress`).
- [ ] El botón cancelar de Edit.cshtml usa `data-kt-modal-dismiss="#vModal"` (no `data-bs-dismiss="modal"`).
- [ ] No se modifica `UsersController`, sus servicios, repositorios, entidades, DTOs, `UsersListViewModel` ni `UsersRolesViewModel`.
- [ ] No hay tooltips Bootstrap (`data-toggle="tooltip"`) en Index.cshtml.
- [ ] No hay breadcrumb en Index.cshtml.
- [ ] No hay `$("#EmployeeNumber").focus()` en `showModal`.
- [ ] Users es usable en desktop y mobile.
- [ ] `dotnet build "TradingBookApp.sln"` termina correctamente, permitiendo solo warnings preexistentes no relacionados con esta migración.

## Decisiones

- **Sí:** Incluir `Web/Views/Users/Index.cshtml` y `Web/Views/Users/Edit.cshtml`. Users no tiene vistas `New` ni `Delete`; la creación de usuarios no está implementada en el controller y la baja lógica se maneja desde Index vía `toDeleteUser`/`processingDelete`.
- **Sí:** Replicar la estructura visual de `Web/Views/CatFigure/Index.cshtml` y `CatFigure/Edit.cshtml`. CatFigure es la base canónica más reciente y mantiene consistencia visual entre módulos migrados.
- **Sí:** Migrar `showModal` a `KTModal.getInstance`. El modal global `#vModal` ya se inicializa como KTModal desde SPEC 02; usar `.modal("show")` de Bootstrap rompería la compatibilidad.
- **Sí:** Migrar `toDeleteUser` a `KTModal.getInstance` para show/hide del `#confirmDialog`. El diálogo de confirmación ya se usa como KTModal en CatFigure y demás catálogos migrados.
- **Sí:** Mantener `showModalForUpdate` aunque el controller actual no la referencia directamente. Es una función definida en la vista que podría ser referenciada desde otros scripts o usarse en el futuro; eliminarla no es parte de una migración visual.
- **Sí:** Actualizar FormValidation al patrón de CatFigure: plugins `Trigger`, `SubmitButton` y `Message` con clases Tailwind, más feedback visual `core.element.validated`. El plugin `Bootstrap5` puede tener conflictos con el nuevo markup Tailwind.
- **Sí:** Eliminar `data-kt-indicator` y los spans `indicator-label`/`indicator-progress` del botón submit. CatFigure no usa indicadores de carga en botones de formulario modal.
- **Sí:** Eliminar `$("#EmployeeNumber").focus()` de `showModal`. Es un vestigio copiado de Employees; CatFigure no fuerza foco tras abrir el modal.
- **Sí:** Eliminar el breadcrumb de Users. Todos los catálogos migrados (Employees, CatCategory, CatFigure, CatAccountType, CatFrame, CatInstruments, Roles) ya eliminaron el suyo.
- **Sí:** Mantener `@Html.ValidationSummary` eliminado del markup. El plugin `Message` de FormValidation renderiza los mensajes automáticamente dentro de `.fv-row`, haciendo redundante el summary.
- **Sí:** Reemplazar `destroy: true` inline en DataTables por chequeo previo `$.fn.DataTable.isDataTable('#dtTable')`. El patrón de CatFigure es más explícito y evita intentos de destrucción sobre instancias inexistentes.
- **Sí:** Usar layout de DataTables 2.x y lenguaje con claves modernas (`processing`, `paginate`, `searchPlaceholder`). El markup y JS de la tabla deben ser consistentes con CatFigure.
- **Sí:** Mantener `ActionButtonHelper.GenerateActionMenu` en el controlador sin cambios. El `kt-menu` ya se genera correctamente desde el servidor y la vista solo necesita renderizar el HTML.
- **Sí:** Eliminar `filter: true` de la configuración de DataTables. No es una opción válida; CatFigure usa `searching: true`.
- **No:** Incluir vista `Users/New`. No existe actualmente y el controller no tiene endpoints de creación de usuarios. Si se necesita en el futuro, debe ir en su propia spec.
- **No:** Cambiar `UsersController`, servicios, repositorios, entidades, DTOs, `UsersListViewModel` o `UsersRolesViewModel`. Esta spec es solo UI/markup/JS cliente.
- **No:** Cambiar `renderTrueFalse`, `renderFlag` ni `renderStatusEmployee` en `Utilities.js`. Ya usan clases compatibles con Metronic Tailwind (`kt-badge`, `ki-duotone`) y no requieren migración.
- **No:** Agregar, quitar o renombrar columnas de la tabla. Las ocho columnas actuales se mantienen.
- **No:** Agregar, quitar o renombrar acciones de fila. Modificar perfil y Activar/Desactivar son las únicas acciones actuales.
- **No:** Agregar vista o acción de eliminación física de usuarios. La baja lógica ya existe vía `toDeleteUser`/`processingDelete` y redirige al Index con mensaje.
- **No:** Hacer un rediseño completo de Users. El objetivo es replicar el comportamiento existente con la mejora visual de Metronic Tailwind.

## Riesgos

| Riesgo | Mitigación |
| ------ | ---------- |
| El feedback visual `core.element.validated` aplica clases `border-green-500` / `border-destructive ring-1 ring-red-500` a elementos checkbox, cuyo aspecto visual puede ser inconsistente. | El validador `callback` sobre el campo `permissions` aplica a un grupo de checkboxes; el elemento que recibe el evento puede ser el primer checkbox del grupo. Validar manualmente que el feedback no rompe la apariencia de los checkboxes; si es necesario, ajustar el handler para ignorar inputs de tipo checkbox. |
| DataTables renderiza filas nuevas después de paginar, filtrar u ordenar y los menús `kt-menu` dejan de responder. | Reinicializar los menús de Metronic Tailwind en el callback de dibujo de DataTables (`KTMenu.createInstances()`). |
| El modal global `#vModal` no responde a `KTModal.getInstance` porque se perdió la instancia o se reinicializó incorrectamente. | Validar apertura del modal de edición manualmente; mantener consistencia con el patrón de CatFigure que ya está probado. |
| El `#confirmDialog` se abre con `KTModal.getInstance` pero el `.bind`/`.unbind` sobre `#confirmButtonYes` ya no funciona porque el modal se reinicializó como KTModal. | Mantener el mismo patrón de CatFigure: `KTModal.getInstance` solo para `.show()`/`.hide()`, el binding de eventos sobre `#confirmButtonYes` es independiente del tipo de modal. |
| Las clases Tailwind cambian el layout de los checkboxes de perfiles y FormValidation deja de encontrar el campo `permissions` o aplicar feedback visual. | Mantener la estructura `.fv-row` como contenedor del grupo de checkboxes y el `id="permissions"` en los checkboxes; validar manualmente los tres estados: sin selección (inválido), con selección (válido) y envío exitoso. |
| `showModalForUpdate` es código muerto que nunca se llama desde el controller actual. Si se elimina accidentalmente durante la migración, no hay impacto funcional. | Documentar en la spec que se conserva intencionalmente; si una futura spec la elimina, debe hacerlo explícitamente. |
| El cambio de `data-bs-dismiss="modal"` a `data-kt-modal-dismiss="#vModal"` en el botón cancelar de Edit.cshtml no cierra el modal correctamente si el atributo no es reconocido por KTModal. | CatFigure y todos los catálogos migrados usan `data-kt-modal-dismiss="#vModal"` sin problemas; validar cierre del modal de edición manualmente. |
| Warnings preexistentes del build se confunden con regresiones de esta migración. | Ejecutar `dotnet build "TradingBookApp.sln"` antes y después de la migración, separando warnings preexistentes de errores nuevos. |

## Lo que no está en esta spec

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `UsersController`, servicios, repositorios, DTOs, entidades o `UsersListViewModel`.
- Cambiar contratos JSON de `~/Users/JsonDataTable`.
- Reemplazar el modal global existente por un modal Tailwind propio.
- Cambiar las reglas, mensajes o endpoints de validación de formularios.
- Agregar vista `Users/New` (no existe actualmente).
- Agregar acción o vista `Users/Delete` (la baja lógica ya existe vía `toDeleteUser`/`processingDelete`).
- Agregar, quitar o renombrar columnas de la tabla.
- Agregar, quitar o renombrar acciones de fila.
- Rediseñar completamente la experiencia visual de Users.
- Eliminar Bootstrap, jQuery, DataTables o FormValidation.

Cada uno de esos puntos debe ir en su propia spec si se decide abordarlo.
