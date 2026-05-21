# SPEC 02 - Migracion de Employees a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** SPEC 01 · **Fecha:** 2026-05-20
> **Objetivo:** Migrar `Employees/Index`, `Employees/New` y `Employees/Edit` a componentes visuales de Metronic Tailwind conservando el comportamiento actual de DataTables, modales globales, acciones y validaciones.

## Alcance

**Incluye:**

- Migrar `Web/Views/Employees/Index.cshtml` a markup y clases visuales de Metronic Tailwind.
- Migrar el contenido modal de `Web/Views/Employees/New.cshtml` a markup y clases visuales de Metronic Tailwind.
- Migrar el contenido modal de `Web/Views/Employees/Edit.cshtml` a markup y clases visuales de Metronic Tailwind.
- Mantener DataTables con server-side AJAX hacia `~/Employees/JsonDataTable`.
- Mantener los filtros actuales por columna en DataTables.
- Migrar la columna de acciones de fila a un dropdown `kt-menu` de Metronic Tailwind.
- Mantener todas las acciones actuales de fila dentro del `kt-menu`: generar usuario, reenviar correo, editar empleado y habilitar/deshabilitar empleado.
- Mantener el uso del modal global existente para alta, edicion, generacion de usuario y confirmaciones.
- Mantener las reglas de validacion cliente actuales con `FormValidation`.
- Mantener los endpoints remotos actuales de validacion de numero de empleado y correo duplicado.
- Validar manualmente la vista Employees en desktop y mobile.
- Confirmar que la aplicacion sigue compilando con `dotnet build "TradingBookApp.sln"`.

**Fuera de alcance (para specs futuras):**

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `EmployeesController`, servicios, repositorios, DTOs, entidades o persistencia.
- Cambiar contratos JSON de `~/Employees/JsonDataTable`.
- Reemplazar el modal global Bootstrap existente por un modal Tailwind propio.
- Cambiar las reglas, mensajes o endpoints de validacion de formularios.
- Agregar, quitar o renombrar acciones de fila.
- Rediseñar completamente la experiencia visual de Employees.
- Eliminar Bootstrap, jQuery, DataTables, FormValidation u otras dependencias cliente existentes.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos backend.

Se reutiliza `Web.Models.EmployeesViewModel` sin cambios:

```csharp
EmployeesViewModel
```

Se mantienen los contratos cliente existentes de la tabla y formularios:

```text
POST ~/Employees/JsonDataTable
POST /Employees/CheckDuplicateKey
POST /Employees/CheckDuplicateEmail
POST ~/Employees/ForwardMail
POST ~/Employees/Delete
GET  ~/Employees/New
GET  ~/Employees/Edit/?id={id}
GET  ~/Account/SignUp/?id={id}
```

La unica estructura visual nueva definida por esta spec es el patron de acciones por fila con `kt-menu` de Metronic Tailwind:

```html
<div class="kt-menu" data-kt-menu="true">
  <div class="kt-menu-item kt-menu-item-dropdown" data-kt-menu-item-toggle="dropdown" data-kt-menu-item-trigger="click">
    <button class="kt-menu-toggle kt-btn kt-btn-sm kt-btn-icon kt-btn-ghost"></button>
    <div class="kt-menu-dropdown kt-menu-default w-full max-w-[175px]" data-kt-menu-dismiss="true">
      <!-- acciones actuales del empleado -->
    </div>
  </div>
</div>
```

Las acciones renderizadas dentro del menu deben seguir llamando a las funciones JavaScript existentes o sus equivalentes directos: `showModalForCreateUser`, `showModalForResendMail`, `showModalForUpdate` y `toDeleteEmployee`.

## Plan de implementacion

1. Inspeccionar `Web/Views/Employees/Index.cshtml`, `Web/Views/Employees/New.cshtml` y `Web/Views/Employees/Edit.cshtml` para separar cambios visuales de comportamiento existente.
2. Migrar el contenedor, toolbar, titulo, breadcrumb, boton "Nuevo" y card principal de `Employees/Index.cshtml` a clases y markup de Metronic Tailwind, conservando los IDs actuales usados por JavaScript.
3. Ajustar la inicializacion de DataTables en `Employees/Index.cshtml` para que las columnas, filtros, server-side AJAX, textos en español y callbacks sigan funcionando con el nuevo markup visual.
4. Migrar la columna `task` para renderizar sus acciones dentro de un dropdown `kt-menu` de Metronic Tailwind, conservando las llamadas a `showModalForCreateUser`, `showModalForResendMail`, `showModalForUpdate` y `toDeleteEmployee`.
5. Mantener la reinicializacion de menus despues de cada draw de DataTables usando la inicializacion compatible con Metronic Tailwind para que los `kt-menu` funcionen tras paginar, filtrar u ordenar.
6. Migrar `Employees/New.cshtml` a campos, labels, mensajes y botones con clases visuales de Metronic Tailwind, manteniendo `id="frmdata"`, `id="btnSave"`, `Html.BeginForm("Save", "Employees")`, `AntiForgeryToken`, campos ocultos y nombres de inputs.
7. Migrar `Employees/Edit.cshtml` a campos, labels, mensajes y botones con clases visuales de Metronic Tailwind, manteniendo `id="frmdata"`, `id="btnSave"`, `Html.BeginForm("Update", "Employees")`, `AntiForgeryToken`, campos ocultos y nombres de inputs.
8. Verificar que las reglas de `FormValidation` en `New.cshtml` y `Edit.cshtml` siguen apuntando a los mismos campos, mensajes y endpoints remotos.
9. Validar manualmente en la aplicacion que Employees carga la tabla, permite filtrar, abre el modal de alta, abre el modal de edicion, muestra el `kt-menu` de acciones y ejecuta las confirmaciones existentes.
10. Revisar Employees en desktop y mobile para confirmar que la migracion visual no bloquea el uso de tabla, filtros, modales ni acciones.
11. Ejecutar `dotnet build "TradingBookApp.sln"` desde la raiz del repositorio y registrar cualquier warning preexistente por separado de regresiones causadas por esta migracion.

## Criterios de aceptacion

- [ ] `Web/Views/Employees/Index.cshtml` usa markup y clases visuales de Metronic Tailwind para toolbar, breadcrumb, boton "Nuevo", contenedor principal y card de la tabla.
- [ ] `Web/Views/Employees/New.cshtml` usa markup y clases visuales de Metronic Tailwind sin cambiar el formulario `Save`.
- [ ] `Web/Views/Employees/Edit.cshtml` usa markup y clases visuales de Metronic Tailwind sin cambiar el formulario `Update`.
- [ ] DataTables sigue cargando datos desde `~/Employees/JsonDataTable` con `serverSide: true`.
- [ ] Los filtros por columna siguen funcionando para numero, nombres, apellidos, estado, email confirmado y tiene usuario.
- [ ] La columna de acciones renderiza un dropdown `kt-menu` de Metronic Tailwind.
- [ ] El `kt-menu` de acciones sigue disponible despues de paginar, filtrar u ordenar la tabla.
- [ ] Las acciones existentes siguen disponibles: generar usuario, reenviar correo, editar empleado y habilitar/deshabilitar empleado.
- [ ] El modal global existente sigue abriendo los formularios de alta, edicion y generacion de usuario.
- [ ] Las confirmaciones existentes para reenvio de correo y habilitar/deshabilitar empleado siguen funcionando.
- [ ] Las validaciones cliente de `New.cshtml` y `Edit.cshtml` conservan los mismos mensajes, reglas y validaciones remotas.
- [ ] No se modifica ningun controlador, servicio, repositorio, entidad, DTO ni modelo de vista.
- [ ] Employees es usable en desktop y mobile.
- [ ] `dotnet build "TradingBookApp.sln"` termina correctamente, permitiendo solo warnings preexistentes no relacionados con esta migracion.

## Decisiones

- **Si:** Incluir `Web/Views/Employees/Index.cshtml`, `Web/Views/Employees/New.cshtml` y `Web/Views/Employees/Edit.cshtml`. El flujo funcional de Employees depende de la tabla y de los formularios modales de alta y edicion.
- **Si:** Usar los componentes visuales de Metronic Tailwind ya disponibles en `Web/wwwroot/Template/assets_Tailwind`. Esto mantiene la migracion alineada con la SPEC 01 y evita introducir una nueva fuente externa de UI.
- **Si:** Mantener DataTables con server-side AJAX. Reemplazarlo aumentaria el alcance y obligaria a rediseñar interaccion, paginacion, filtros y contrato de datos.
- **Si:** Conservar los filtros actuales por columna. Son funcionalidad existente y deben sobrevivir a la migracion visual.
- **Si:** Migrar la columna de acciones a un dropdown `kt-menu` de Metronic Tailwind. Esta es la mejora visual principal para las acciones de fila.
- **Si:** Conservar todas las acciones actuales dentro del `kt-menu`. La spec cambia presentacion visual, no comportamiento funcional.
- **Si:** Mantener el modal global existente. Sustituirlo por un modal Tailwind propio debe tratarse en otra spec para no mezclar migracion visual de Employees con cambio de infraestructura modal.
- **Si:** Mantener `FormValidation`, reglas, mensajes y validaciones remotas actuales. La migracion no cambia validacion ni backend.
- **No:** Cambiar controladores, servicios, repositorios, entidades, DTOs o persistencia. Esta spec es solo UI/markup/JS cliente de Employees.
- **No:** Hacer un rediseño completo de Employees. El objetivo es replicar comportamiento y aplicar una mejora visual minima compatible con Metronic Tailwind.
- **No:** Eliminar Bootstrap, jQuery, DataTables o FormValidation. Todavia son dependencias necesarias para esta pantalla y para la convivencia hibrida iniciada en SPEC 01.

## Riesgos

| Riesgo | Mitigacion |
| ------ | ---------- |
| DataTables renderiza filas nuevas despues de paginar, filtrar u ordenar y los menus `kt-menu` dejan de responder. | Reinicializar los menus de Metronic Tailwind en el callback de dibujo de DataTables. |
| Las clases Tailwind cambian el layout pero los selectores usados por JavaScript dejan de encontrar elementos existentes. | Mantener IDs, nombres de inputs y funciones JavaScript usadas por DataTables, modales y validaciones. |
| El modal global existente depende de Bootstrap mientras el contenido interno migra a Metronic Tailwind. | Mantener el modal global sin reemplazarlo y validar alta, edicion, generacion de usuario y confirmaciones manualmente. |
| Los filtros de columna pierden legibilidad o espacio en mobile al convivir con DataTables. | Validar la pantalla en desktop y mobile, priorizando que la tabla siga siendo usable aunque conserve scroll o comportamiento responsive existente. |
| Warnings preexistentes del build se confunden con regresiones de esta migracion. | Ejecutar `dotnet build "TradingBookApp.sln"` y separar warnings preexistentes de errores nuevos relacionados con Employees. |

## Lo que no esta en esta spec

- Reemplazar DataTables por una tabla Tailwind propia.
- Cambiar `EmployeesController`, servicios, repositorios, DTOs, entidades o persistencia.
- Cambiar contratos JSON de `~/Employees/JsonDataTable`.
- Reemplazar el modal global Bootstrap existente por un modal Tailwind propio.
- Cambiar reglas, mensajes o endpoints de validacion.
- Agregar, quitar o renombrar acciones de fila.
- Rediseñar completamente la experiencia visual de Employees.
- Eliminar Bootstrap, jQuery, DataTables, FormValidation u otras dependencias cliente existentes.

Cada uno de esos puntos debe ir en su propia spec si se decide abordarlo.
