# SPEC 18 — Migración de Orders a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01 · **Fecha:** 2026-05-25
> **Objetivo:** Migrar `Orders/Index`, `Orders/New`, `Orders/Edit` y `Orders/Close` a componentes visuales de Metronic Tailwind conservando DataTables, modales, validaciones y replicando el filtrado vía drawer de `Home/Index`.

## Alcance

**Incluye:**

**Orders/Index:**
- Migrar el markup a Metronic Tailwind: header container (`kt-container-fixed`) con título "Órdenes", subtítulo y botón "Nuevo" estilizado como el "Generar una orden" de Home/Index (`kt-btn kt-btn-primary` con ícono `ki-plus`).
- Reemplazar el dropdown de filtros `kt_menu_filter` con Select2 por un drawer `#filter_drawer` (`kt-drawer kt-drawer-end`) con los 5 filtros (`kt-select`: Categoría, Tipo de Cuenta, Instrumento, Frame, Dirección), réplica exacta del drawer de Home/Index.
- Reemplazar los 5 Select2 por `kt-select` con `@Html.DropDownList` y data-attributes de KTUI (`data_kt_select`, `data_kt_select_enable_search`, `data_kt_select_search_placeholder`, `data_kt_select_placeholder`, `data_kt_select_config`).
- Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable` con `kt-table` y `data-kt-datatable-table="true"`.
- Actualizar DataTables: `$.fn.DataTable.isDataTable('#dtTable')` antes de `destroy`, lenguaje 2.x, `layout` + `initComplete` con `kt-input`, placeholder "Buscar órdenes...", `min-w-[Xpx]`, `serverSide: true`, `searching: false`, `info: true`.
- Mantener las 20 columnas actuales sin cambios de nombre, orden o renderizado, incluyendo la columna "Acciones" con el patrón de CatFigure/Index (sin tooltips Bootstrap, `orderable: false`, `className: "text-center min-w-[40px] w-[60px]"`).
- Actualizar `ClearFilterData` para usar `KTSelectHelper.setValue('#CategoryId', 1)` y `KTSelectHelper.clear(...)` en los demás.
- Actualizar `showModal` y `showModalForNew` a la API de KTModal (`KTModal.getInstance(modalEl).show()`).
- Migrar `showModaltoDelete`: usar `KTModal.getInstance(modalEl).hide()/show()` en lugar de la API Bootstrap.
- Eliminar breadcrumbs, tooltips Bootstrap legacy, dropdown `kt_menu_filter`, referencias a Select2 en el drawer y columna de acciones.

**Orders/New:**
- Migrar el markup a Metronic Tailwind: `kt-card-content grid gap-5`, reemplazar `row`/`col-md-6` por `grid grid-cols-1 md:grid-cols-2 gap-5`.
- Reemplazar todos los `<select>` con Select2 (`data_control="select2"`) por `@Html.DropDownList` con `kt-select` y data-attributes de KTUI en todos los dropdowns: `CategoryId`, `AccountTypeId`, `InstrumentId`, `DayId`, `StageId`, `FigureId`, `FrameId`, `TriggerId`, `DirectionId`, `SceneryId`, `OrderType`, `TradeType`, `LocationType`, `IsTrendAligned`, `ConfirmationType`.
- Reemplazar `form-control` por `kt-input` en campos de texto (`Quantity`, `Price`, `CommissionRate`, `Total`).
- Reemplazar clases Bootstrap (`row`, `col-md-6`, `mb-5`, `mb-4`, `fv-row mb-5`, `form-label`) por utilidades Tailwind/Metronic (`w-full`, `fv-row`, `kt-form-label max-w-56`, `grid gap-5`, etc.).
- Conservar KTStepper con sus 2 pasos, ajustando solo clases CSS a Tailwind en wrappers y botones de navegación.
- Conservar flatpickr para `#CreationDate` y `#Time` sin cambios.
- Reemplazar Inputmask manual por `InputMaskHelper.decimal()` para `#Quantity`, `#Price`, `#CommissionRate`, `#Total`.
- Migrar FormValidation al patrón CatFigure: reemplazar plugin `Bootstrap5` por `Trigger` + `SubmitButton` + `Message` con `clazz: 'text-red-500 text-sm mt-1'` y `container: function(field, element) { return element.closest('.fv-row'); }`.
- Agregar handler `core.element.validated` con clases Tailwind (`border-green-500` / `border-destructive ring-1 ring-red-500`).
- Para campos `kt-select`, el handler de `core.element.validated` debe buscar el wrapper `.kt-select` más cercano y aplicarle las clases de borde.
- Reemplazar `validatorStep1` y `validatorStep2` por un único validador que cubra todos los campos de ambos pasos.
- Migrar botones del modal-footer a `kt-btn kt-btn-secondary` (Cancelar/Anterior) y `kt-btn kt-btn-primary` (Guardar/Siguiente).
- Conservar `AddOrder()` (POST JSON a `~/Orders/AddOrder`), `flatpickr.onChange` que selecciona `DayId`, y lógica del stepper.

**Orders/Edit:**
- Migrar el markup a Metronic Tailwind (mismo patrón que New: `kt-card-content grid gap-5`, `grid grid-cols-1 md:grid-cols-2 gap-5`).
- Reemplazar todos los Select2 por `kt-select` en los mismos dropdowns que New (excepto los de Trade que no están en Edit).
- Reemplazar `form-control` por `kt-input`, clases Bootstrap por Tailwind.
- Conservar flatpickr y `InputMaskHelper` para campos numéricos.
- Migrar FormValidation al patrón CatFigure (mismo que New: `Trigger` + `SubmitButton` + `Message` + `core.element.validated` + soporte `kt-select`).
- Migrar botones del modal-footer a `kt-btn`.
- Conservar `@Html.BeginForm` con POST tradicional a `~/Orders/Update` y `@Html.AntiForgeryToken()`.

**Orders/Close:**
- Migrar el markup a Metronic Tailwind (mismo patrón que New, incluyendo layout de 2 columnas y KTStepper de 2 pasos).
- Reemplazar todos los Select2 por `kt-select` (`OrderType`, `TradeType`).
- Reemplazar `form-control` por `kt-input` en campos numéricos.
- Reemplazar clases Bootstrap por Tailwind.
- Conservar Inputmask Helper para `#Quantity`, `#Price`, `#CommissionRate`, `#Total`.
- Conservar la lógica de checkboxes con exclusión mutua (SL/TP1/TP2/TP3) sin cambios funcionales, solo ajustando clases visuales.
- Migrar FormValidation al patrón CatFigure con soporte `kt-select`.
- Migrar botones del modal-footer a `kt-btn`.
- Conservar `CloseOrder()` (POST JSON a `~/Orders/CloseOrder`).

**Fuera de alcance:**

- Cambios en `OrdersController`, servicios, repositorios, entidades, DTOs o ViewModels del backend.
- Cambios en los contratos JSON de `~/Orders/JsonDataTable`, `~/Orders/AddOrder`, `~/Orders/CloseOrder`, `~/Orders/Update`, `~/Orders/Delete`.
- Agregar, quitar o renombrar columnas de la DataTable de Index.
- Cambiar la lógica de negocio de creación, edición, cierre o eliminación de órdenes.
- Modificar `Home/Index` o su botón "Generar una orden".
- Eliminar Bootstrap, jQuery, DataTables, KTUI o flatpickr del proyecto.

## Data model

Esta funcionalidad no introduce nuevas estructuras de datos en el backend ni modifica la base de datos.

Se reutilizan sin cambios las estructuras existentes:

```csharp
Web.Models.OrdersViewModel
Web.Models.OrdersSellViewModel
Application.DTOs.GetTBOrdersDto
```

Los controladores y endpoints se mantienen sin modificaciones:

```text
GET  ~/Orders/Index
POST ~/Orders/JsonDataTable  (parámetros: categoryId, accountTypeId, instrumentId, frameId, directionId)
GET  ~/Orders/New
POST ~/Orders/AddOrder
GET  ~/Orders/Edit?id=
POST ~/Orders/Update
GET  ~/Orders/Close?id=
POST ~/Orders/CloseOrder
POST ~/Orders/Delete
```

Los contratos del lado cliente (DataTable en Index, fetch/JSON en New y Close, form POST en Edit) se mantienen funcionalmente idénticos.

Las vistas involucradas son exclusivamente:

```text
Web/Views/Orders/Index.cshtml
Web/Views/Orders/New.cshtml
Web/Views/Orders/Edit.cshtml
Web/Views/Orders/Close.cshtml
```

El helper `Web/wwwroot/Template/assets/js/custom/helpers/inputmask.helper.js` ya existe y no requiere modificación. Se consumirá mediante `InputMaskHelper.decimal()`.

No se crean ni modifican archivos JavaScript adicionales.

## Plan de implementación

### Paso 1 — Migrar `Orders/Index.cshtml`: header y drawer de filtros

1. Reemplazar todo el toolbar Bootstrap y layout actual por header container con `kt-container-fixed`, título "Órdenes", subtítulo "Consulta y administra las órdenes de trading registradas en el sistema.".
2. Agregar botón "Nuevo" con `kt-btn kt-btn-primary` + ícono `ki-filled ki-plus`, réplica del estilo del botón "Generar una orden" de Home/Index.
3. Eliminar breadcrumb y dropdown `kt_menu_filter`.
4. Implementar drawer `#filter_drawer` (`kt-drawer kt-drawer-end`) con estructura idéntica a Home/Index: `kt-card-header` con título "Filtros Disponibles" y botón dismiss, `kt-card-content kt-scrollable-y-auto` con 5 filas de filtro, `kt-card-footer` con botones "Limpiar" (`kt-btn kt-btn-outline`, `data-kt-drawer-dismiss="true"`) y "Aplicar" (`kt-btn kt-btn-primary grow`, `data-kt-drawer-dismiss="true"`).
5. Reemplazar los 5 `@Html.DropDownList` con Select2 (`data_control="select2"`) por `@Html.DropDownList` con `kt-select` y data-attributes de KTUI.
6. Botón "Filtro" en el header con `kt-btn kt-btn-outline` y `data-kt-drawer-toggle="#filter_drawer"`.
7. Card con `kt-card kt-card-grid` y `kt-card-table kt-scrollable-x-auto` conteniendo la tabla `dtTable` con `kt-table` y `data-kt-datatable-table="true"`.
8. Build y verificar que el drawer abre/cierra, los kt-select muestran opciones.

### Paso 2 — Migrar `Orders/Index.cshtml`: DataTable, acciones y modales

1. Actualizar DataTables: reemplazar `destroy: true` por `$.fn.DataTable.isDataTable('#dtTable')` + `destroy()`.
2. Agregar `layout` (topStart: [], topEnd: 'search', bottomStart: ['pageLength', 'info'], bottomEnd: 'paging').
3. Agregar `initComplete` con `kt-input` para búsqueda, placeholder "Buscar órdenes...".
4. Mantener las 20 columnas actuales: `id`, `orderType`, `instrument`, `date`, `time`, `day`, `stage`, `figure`, `frame`, `trigger`, `scenery`, `direction`, `sl`, `tp1`, `tp2`, `tp3`, `takeprofit`, `chart`, `status`, `task`.
5. Migrar columna `task` (Acciones) al patrón CatFigure: `orderable: false`, `className: "text-center min-w-[40px] w-[60px]"`, sin tooltips Bootstrap.
6. Convertir todas las clases `className` a `min-w-[Xpx]`.
7. Actualizar lenguaje a claves 2.x (`processing`, `lengthMenu`, `zeroRecords`, `emptyTable`, `info`, `infoEmpty`, `infoFiltered`, `search`, `searchPlaceholder`, `paginate`, `aria`).
8. Mantener `serverSide: true`, `pageLength: 10`, `searching: false`, activar `info: true`.
9. Actualizar `ClearFilterData`: usar `KTSelectHelper.setValue('#CategoryId', 1)` y `KTSelectHelper.clear(...)` para los otros 4 selects.
10. Actualizar `showModal`: usar `KTModal.getInstance(document.querySelector('#vModal')).show()` en lugar de `$("#vModal").modal("show")`.
11. Actualizar `showModaltoDelete`: usar `KTModal.getInstance(document.querySelector('#confirmDialog')).hide()/show()` en lugar de la API Bootstrap.
12. Eliminar `$('[data-toggle="tooltip"]').tooltip()` del `drawCallback` y de `showModalForNew`.
13. Build y verificar que la tabla carga datos, paginación, botones de acción, modal de nueva orden, modal de editar, modal de cerrar, y diálogo de confirmación para eliminar.

### Paso 3 — Migrar `Orders/New.cshtml`: markup y campos del formulario

1. Reemplazar el layout Bootstrap actual por `kt-card-content grid gap-5`.
2. Reestructurar el formulario: reemplazar `row`/`col-md-6` por `grid grid-cols-1 md:grid-cols-2 gap-5` en cada sección del stepper.
3. Reemplazar todos los `<select>` con `data_control="select2"` por `@Html.DropDownList` con clase `kt-select` y data-attributes de KTUI (`data_kt_select="true"`, `data_kt_select_enable_search="true"`, `data_kt_select_search_placeholder="Buscar..."`, `data_kt_select_placeholder="Selecciona uno..."`, `data_kt_select_config="{''optionsClass'': ''kt-scrollable overflow-auto max-h-[250px]''}"`).
4. Reemplazar `form-control` por `kt-input` en `#Quantity`, `#Price`, `#CommissionRate`, `#Total`.
5. Reemplazar clases Bootstrap (`mb-5`, `mb-4`, `fv-row mb-5`, `form-label`, `fw-bold`) por utilidades Tailwind (`gap-2.5`, `kt-form-label max-w-56`, `font-semibold`).
6. Cada grupo de input debe seguir el patrón: `<div class="w-full fv-row">` con hijo `<div class="flex items-baseline flex-wrap lg:flex-nowrap gap-2.5">` conteniendo `@Html.Label` con `kt-form-label max-w-56` y el campo.
7. Conservar la estructura del KTStepper con sus 2 pasos, wrappers y botones, ajustando solo las clases de los botones de navegación a `kt-btn kt-btn-secondary` (Anterior) y `kt-btn kt-btn-primary` (Siguiente).
8. Migrar botones del modal-footer: Cancelar → `kt-btn kt-btn-secondary`, Submit → `kt-btn kt-btn-primary`.
9. Reemplazar `Inputmask(...).mask(...)` manual por `InputMaskHelper.decimal('#Quantity')`, `InputMaskHelper.decimal('#Price')`, `InputMaskHelper.decimal('#CommissionRate')`, `InputMaskHelper.decimal('#Total')`.
10. Conservar flatpickr para `#CreationDate` y `#Time` con su `onChange` que setea `#DayId`.
11. Eliminar toda inicialización manual de Select2.
12. Build y verificar que el formulario carga en el modal, los kt-select muestran opciones, flatpickr funciona, los inputs numéricos tienen máscara.

### Paso 4 — Migrar `Orders/New.cshtml`: FormValidation

1. Reemplazar `validatorStep1` y `validatorStep2` por un único validador `FormValidation.formValidation(form, ...)`.
2. Mover los campos validados del Step 1 (`CategoryId`, `AccountTypeId`, `InstrumentId`, `DayId`, `StageId`, `FigureId`, `FrameId`, `TriggerId`, `DirectionId`, `SceneryId`) y del Step 2 (`Total`) a un solo objeto `fields`.
3. Cada campo dropdown usa validador `notEmpty` con mensaje en español. `Total` usa `notEmpty` + `callback` que verifica `value > 0`.
4. Reemplazar plugins `Bootstrap5` por: `Trigger` (new `FormValidation.plugins.Trigger()`), `SubmitButton` (new `FormValidation.plugins.SubmitButton()`), `Message` (new `FormValidation.plugins.Message({ clazz: 'text-red-500 text-sm mt-1', container: function(field, element) { return element.closest('.fv-row'); } })`).
5. Agregar handler `on('core.element.validated', ...)` que aplica `border-green-500` o `border-destructive ring-1 ring-red-500` al elemento. Para campos `kt-select`, buscar `.closest('.kt-select')` del elemento nativo y aplicar las clases al wrapper (porque `kt-select` oculta el `<select>` nativo y el usuario ve el widget).
6. Conservar la lógica del stepper: en `kt.stepper.next`, validar los campos del paso actual; en submit, validar todos y llamar `AddOrder()` si `status === 'Valid'`.
7. Conservar `AddOrder()` sin cambios funcionales (construye JSON y hace POST a `~/Orders/AddOrder`).
8. Build y probar visualmente: llenar el formulario paso a paso, verificar que las validaciones muestran errores en rojo y bordes verdes al corregir, que el stepper no avanza con campos inválidos, y que el POST crea la orden correctamente.

### Paso 5 — Migrar `Orders/Edit.cshtml`

1. Aplicar la misma migración de markup que New (sin stepper ni paso 2 de compra): `kt-card-content grid gap-5`, `grid grid-cols-1 md:grid-cols-2 gap-5`, `kt-select` en todos los dropdowns, `kt-input` en campos de texto.
2. Conservar `@Html.BeginForm("Update", ...)` con `@Html.AntiForgeryToken()` y campos ocultos (`Id`, `AuthorId`, `AlterDate`).
3. Reemplazar Select2 por `kt-select` en: `CategoryId`, `AccountTypeId`, `InstrumentId`, `DayId`, `StageId`, `FigureId`, `FrameId`, `TriggerId`, `DirectionId`, `SceneryId`, `LocationType`, `IsTrendAligned`, `ConfirmationType`.
4. Reemplazar `form-control` por `kt-input` en campos de solo lectura (`Grade`, `Score`) y ocultos.
5. Migrar FormValidation al patrón CatFigure (mismo que New: `Trigger` + `SubmitButton` + `Message` + `core.element.validated`), con soporte para `kt-select` (buscar `.closest('.kt-select')` en el handler de bordes).
6. Validar los mismos campos que New step 1: `CategoryId`, `AccountTypeId`, `InstrumentId`, `DayId`, `StageId`, `FigureId`, `FrameId`, `TriggerId`, `DirectionId`, `SceneryId`.
7. Migrar botones del modal-footer a `kt-btn`.
8. Conservar `form.submit()` tradicional (no fetch, Edit usa POST de formulario).
9. Build y probar: cargar orden existente, editar campos, verificar validación, guardar.

### Paso 6 — Migrar `Orders/Close.cshtml`

1. Aplicar la misma migración de markup que New: `kt-card-content grid gap-5`, `grid grid-cols-1 md:grid-cols-2 gap-5` para el paso 1 (checkboxes y campos de texto).
2. Conservar KTStepper con 2 pasos, ajustando solo clases de botones a `kt-btn`.
3. Reemplazar Select2 por `kt-select` en `OrderType` y `TradeType`.
4. Reemplazar `form-control` por `kt-input` en `#Quantity`, `#Price`, `#CommissionRate`, `#Total`.
5. Reemplazar Inputmask manual por `InputMaskHelper.decimal(...)`.
6. Conservar la lógica de checkboxes con exclusión mutua (SL/TP1/TP2/TP3) sin cambios, solo ajustando clases visuales de los checkboxes y labels a Tailwind (`flex items-center gap-2`, `kt-form-label`).
7. Migrar FormValidation al patrón CatFigure: unificar validadores `validatorStep1` y `validatorStep2` en uno solo con `Trigger` + `SubmitButton` + `Message` + `core.element.validated`.
8. Step 1: validar `Chart` como `notEmpty`. Step 2: validar `Total` con `notEmpty` + `callback > 0`.
9. Conservar `CloseOrder()` sin cambios (POST JSON a `~/Orders/CloseOrder`).
10. Migrar botones del modal-footer a `kt-btn`.
11. Build y probar: abrir modal de cierre, verificar checkboxes y su lógica de exclusión, llenar paso 2, validar, cerrar orden.

### Paso 7 — Validación final

1. Ejecutar `dotnet build "TradingBookApp.sln"` y confirmar compilación sin errores nuevos.
2. Verificar visualmente: Index carga con drawer funcional, filtros aplican, DataTable con 20 columnas, botones de acción (Editar, Cerrar, Eliminar) funcionan, modales abren/cierran con KTModal API, formularios validan con kt-select, stepper funciona en New y Close, flatpickr e Inputmask operativos.
3. Verificar en mobile que la tabla es scrolleable, el drawer se abre correctamente, los formularios son usables.

## Criterios de aceptación

**Orders/Index:**
- [ ] `Orders/Index.cshtml` usa markup y clases de Metronic Tailwind (`kt-container-fixed`, `kt-card`, `kt-card-grid`, `kt-table`).
- [ ] El header muestra título "Órdenes", subtítulo, y botón "Nuevo" estilizado con `kt-btn kt-btn-primary` e ícono `ki-plus`.
- [ ] El botón "Filtro" abre el drawer `#filter_drawer` (`kt-drawer kt-drawer-end`).
- [ ] El drawer contiene 5 filtros (`CategoryId`, `AccountTypeId`, `InstrumentId`, `FrameId`, `DirectionId`) con `kt-select` y botones "Limpiar" y "Aplicar".
- [ ] `ClearFilterData` resetea `CategoryId` a 1 y limpia los demás con `KTSelectHelper`.
- [ ] Al presionar "Aplicar", el drawer se cierra y la tabla se recarga con los filtros seleccionados.
- [ ] DataTable usa `$.fn.DataTable.isDataTable('#dtTable')` antes de `destroy`, lenguaje 2.x, `layout` + `initComplete` con `kt-input` y placeholder "Buscar órdenes...", `min-w-[Xpx]`.
- [ ] Las 20 columnas se mantienen sin cambios de nombre, orden o renderizado.
- [ ] La columna "Acciones" usa el patrón CatFigure (`orderable: false`, `className: "text-center min-w-[40px] w-[60px]"`) sin tooltips Bootstrap.
- [ ] `searching: false` e `info: true` están configurados.
- [ ] `showModal`, `showModalForNew` y `showModalForClose` usan `KTModal.getInstance(...)`.
- [ ] `showModaltoDelete` usa `KTModal.getInstance` para el diálogo de confirmación.
- [ ] Se eliminaron breadcrumbs, `kt_menu_filter`, referencias a Select2 en filtros, y `$('[data-toggle="tooltip"]').tooltip()`.

**Orders/New:**
- [ ] `Orders/New.cshtml` usa markup de Metronic Tailwind (`kt-card-content grid gap-5`, `grid grid-cols-1 md:grid-cols-2 gap-5`, `w-full fv-row`, `flex items-baseline flex-wrap lg:flex-nowrap gap-2.5`).
- [ ] Todos los dropdowns usan `kt-select` con data-attributes de KTUI (sin `data_control="select2"`).
- [ ] Los campos de texto numéricos usan `kt-input`.
- [ ] `InputMaskHelper.decimal()` aplica máscara a `#Quantity`, `#Price`, `#CommissionRate`, `#Total`.
- [ ] KTStepper se mantiene con 2 pasos y botones estilizados con `kt-btn`.
- [ ] flatpickr funciona para `#CreationDate` y `#Time`, con selección automática de `#DayId`.
- [ ] FormValidation usa plugins `Trigger`, `SubmitButton` y `Message` (sin `Bootstrap5`).
- [ ] El handler `core.element.validated` aplica clases de borde Tailwind (`border-green-500` / `border-destructive ring-1 ring-red-500`) en campos nativos y en wrappers `.kt-select` para dropdowns.
- [ ] Un solo validador cubre todos los campos de ambos pasos.
- [ ] El stepper no avanza con campos inválidos y `AddOrder()` solo se ejecuta con validación `Valid`.
- [ ] El POST a `~/Orders/AddOrder` crea la orden correctamente.
- [ ] No hay referencias a `Bootstrap5` plugin, Select2 o clases Bootstrap legacy en el formulario.

**Orders/Edit:**
- [ ] `Orders/Edit.cshtml` usa markup de Metronic Tailwind (mismo patrón que New, sin stepper ni paso de compra).
- [ ] Todos los dropdowns usan `kt-select` (sin Select2).
- [ ] FormValidation usa patrón CatFigure (`Trigger` + `SubmitButton` + `Message` + `core.element.validated` con soporte `kt-select`).
- [ ] Los mismos campos que New step 1 tienen validación `notEmpty`.
- [ ] `@Html.BeginForm` con POST tradicional a `~/Orders/Update` funciona correctamente.
- [ ] Campos ocultos (`Id`, `AuthorId`, `AlterDate`) se preservan.

**Orders/Close:**
- [ ] `Orders/Close.cshtml` usa markup de Metronic Tailwind (mismo patrón que New).
- [ ] KTStepper con 2 pasos y botones `kt-btn`.
- [ ] `OrderType` y `TradeType` usan `kt-select`.
- [ ] `InputMaskHelper.decimal()` en campos numéricos.
- [ ] Checkboxes SL/TP1/TP2/TP3 mantienen lógica de exclusión mutua funcional.
- [ ] FormValidation con `Chart` (notEmpty en paso 1) y `Total` (notEmpty + callback > 0 en paso 2) usando patrón CatFigure.
- [ ] `CloseOrder()` (POST JSON a `~/Orders/CloseOrder`) funciona sin errores.

**Generales:**
- [ ] `dotnet build "TradingBookApp.sln"` compila sin errores nuevos.
- [ ] Los 4 modales (New, Edit, Close, ConfirmDialog) abren y cierran con KTModal API.
- [ ] La vista es usable en desktop y mobile (tabla scrolleable, drawer funcional, formularios usables).

## Decisiones tomadas y descartadas

- **Sí: Una sola spec para las 4 vistas de Orders.** Comparten controlador, modelos, y las vistas New/Edit/Close son variantes del mismo formulario. Separarlas generaría duplicación en el plan y en el código compartido de validación.
- **Sí: Replicar el patrón de drawer de Home/Index para los filtros de Index.** Reemplaza el dropdown `kt_menu_filter` por `#filter_drawer` con `kt-select`, consistente con toda la suite de analítica migrada (specs 11–17).
- **Sí: Migrar todos los Select2 a `kt-select` en formularios también.** Reduce la dependencia de jQuery/Select2 progresivamente. Las vistas de Orders serán las primeras en combinar `kt-select` con FormValidation en formularios modales.
- **Sí: Usar el patrón de validación de CatFigure** (`Trigger` + `SubmitButton` + `Message` + `core.element.validated` con clases Tailwind). Es el estándar establecido en todos los catálogos migrados (specs 02–10).
- **Sí: Unificar `validatorStep1` y `validatorStep2` en un solo validador.** FormValidation soporta validar campos independientemente del paso del stepper. Un solo validador simplifica el código y evita duplicación de configuración de plugins.
- **Sí: Buscar `.closest('.kt-select')` en el handler `core.element.validated` para dropdowns.** `kt-select` oculta el `<select>` nativo y renderiza un widget propio. Para que el feedback visual de borde sea visible, las clases deben aplicarse al wrapper de KTUI, no al elemento nativo oculto.
- **Sí: Usar `KTModal.getInstance()` en lugar de `$("#vModal").modal()`.** `_Layout.cshtml` ya migró `#vModal` a `kt-modal` (Metronic Tailwind). La API Bootstrap `.modal()` es legacy y debe reemplazarse por la API nativa de KTModal para evitar conflictos.
- **Sí: Mantener `searching: false` en la DataTable de Index.** La tabla tiene 20 columnas y `serverSide: true`. Activar búsqueda cliente-side no es útil cuando la paginación es server-side y el volumen de columnas puede degradar la experiencia.
- **Sí: Usar `InputMaskHelper.decimal()` del helper existente.** Ya está disponible en `~/Template/assets/js/custom/helpers/inputmask.helper.js` y centraliza la configuración de Inputmask para campos decimales.
- **Sí: Conservar flatpickr sin cambios.** No existe un reemplazo nativo en Metronic Tailwind para date/time pickers que ofrezca la misma funcionalidad (selección de fecha con callback que setea DayId, selector de hora).
- **Sí: Conservar KTStepper con ajustes solo de clases CSS.** La funcionalidad del stepper (navegación entre pasos, validación por paso) se mantiene intacta; solo se migran las clases visuales de los botones y wrappers.
- **Sí: Conservar la lógica de checkboxes de Close sin cambios funcionales.** La exclusión mutua SL/TP1/TP2/TP3 con asignación automática de comentarios es lógica de negocio; migrar solo las clases visuales.
- **Sí: Estilizar el botón "Nuevo" de Index como el "Generar una orden" de Home.** `kt-btn kt-btn-primary` con ícono `ki-filled ki-plus`, consistencia visual entre el dashboard y la lista de órdenes.
- **No: Modificar `OrdersController`, servicios, repositorios, entidades, DTOs o ViewModels.** Esta spec es exclusivamente de interfaz de usuario y capa de presentación.
- **No: Cambiar los contratos JSON de ningún endpoint.** `JsonDataTable`, `AddOrder`, `Update`, `CloseOrder` y `Delete` se mantienen sin cambios en request/response.
- **No: Agregar, quitar o renombrar columnas de la DataTable.** Las 20 columnas actuales se preservan exactamente.
- **No: Eliminar dependencias globales (Bootstrap, jQuery, DataTables, KTUI, flatpickr).** La eliminación de dependencias legacy es una iniciativa transversal que requiere su propia spec.
- **No: Migrar Home/Index ni su botón "Generar una orden".** Home ya está migrado (SPEC 01). Esta spec solo toma su estilo como referencia para el botón "Nuevo" de Index.

## Riesgos identificados

| Riesgo | Mitigación |
| ------ | ---------- |
| **`kt-select` + FormValidation**: el feedback visual de borde no se aplica porque `kt-select` oculta el `<select>` nativo. | En el handler `core.element.validated`, detectar si el elemento es un `<select>` con clase `kt-select` y aplicar las clases de borde al wrapper `.closest('.kt-select')`. Las vistas de Orders son las primeras en combinar ambos; validar visualmente cada dropdown al probar. |
| **Inicialización de `kt-select` dentro del stepper**: los selects del paso 2 pueden no estar visibles al cargar el DOM. | KTUI inicializa componentes por data-attributes al cargar la página sin importar visibilidad (mismo principio que los `kt-select` dentro del drawer `#filter_drawer` con `hidden`). Home/Index ya valida este comportamiento. |
| **FormValidation unificado con stepper**: un solo validador para ambos pasos puede mostrar errores de campos no visibles. | Usar `validator.validate()` sin parámetros valida todos los campos. La navegación del stepper (`kt.stepper.next`) solo llama a `validate()` sin bloquear el avance si hay errores en otros pasos (se validarán al intentar submit). Alternativa: usar `validator.fields` para validar solo los campos del paso actual con `validator.revalidateField(fieldName)`. |
| **DataTable con 20 columnas**: la tabla puede desbordar en pantallas pequeñas. | El contenedor `kt-card-table kt-scrollable-x-auto` ya proporciona scroll horizontal. La versión Bootstrap actual ya requiere scroll; la migración no empeora la situación. |
| **Checkboxes de Close con Tailwind**: la lógica de exclusión mutua manipula `checked` y `disabled`, no clases CSS. El riesgo es bajo. | Las clases visuales de los checkboxes (`form-check-input` → clases Tailwind de KTUI) son cosméticas. La lógica JS que lee/escribe `checked` no se ve afectada. |
| **flatpickr dentro de KTModal**: el z-index del datepicker puede quedar detrás del modal. | flatpickr ya funciona dentro del `#vModal` actual (Bootstrap). KTModal usa z-index similar. Si hay conflicto, ajustar `appendTo` de flatpickr al cuerpo del modal. |
| **Pérdida de funcionalidad al unificar validadores**: `validatorStep1` y `validatorStep2` tenían lógicas separadas acopladas al stepper. | La unificación reduce código pero requiere probar que el stepper avanza correctamente: en `kt.stepper.next` del paso 1, validar solo campos del paso 1; en submit, validar todos. |
