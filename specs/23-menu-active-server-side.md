# SPEC 23 — Menú lateral con clase active desde servidor

> **Estado:** Implementado  · **Depende de:** Ninguno · **Fecha:** 2026-06-02
> **Objetivo:** Modificar `MenuHelper.RenderMenu()` para que el ítem del menú lateral correspondiente a la página actual reciba la clase `kt-menu-item-active` desde el servidor, los submenús usen `kt-menu-bullet` en vez de icono, y los padres accordion se expandan automáticamente cuando un hijo está activo.

---

## Alcance

**Incluye:**

- Pasar `ViewContext` como parámetro a `RenderMenu()` para acceder a la ruta actual desde el servidor
- Comparar la URL de cada `GetMenuByUserIdDto` con la ruta actual de la request (match exacto)
- Agregar clase `kt-menu-item-active` al `<div class="kt-menu-item">` del ítem que coincide
- Expandir automáticamente el padre accordion (`kt-menu-item-show`) cuando el ítem activo es un hijo suyo
- Modificar `RenderMenuItem` para que los ítems con `ParentMenuId != null` usen `kt-menu-bullet` en vez de `kt-menu-icon` + `<i>`
- Ajustar `RenderMenuItemParents` para que cuando tenga un hijo activo se marque como expandido

**Fuera de alcance:**

- Match por prefijo o por controller/action (solo match exacto de URL)
- Modificar la tabla `Menu` o el DTO `GetMenuByUserIdDto`
- Cambiar el CSS del tema Metronic/Tailwind (los estilos de `kt-menu-bullet` y `kt-menu-item-active:before:bg-primary` ya existen en `styles.css`)
- Eliminar o modificar el comportamiento JS existente de `data-kt-menu`
- Agregar clases active a páginas que no tengan entrada en el menú

---

## Data model

Esta funcionalidad no introduce ni modifica estructuras de datos. El DTO `GetMenuByUserIdDto` y la entidad `Menu` permanecen sin cambios. La lógica se implementa exclusivamente en el helper `MenuHelper.RenderMenu()` usando el `ViewContext` de la request actual.

---

## Plan de implementación

**Paso 1 — Modificar firma de `RenderMenu` y pasar `ViewContext`**
1. Cambiar `RenderMenu(this IHtmlHelper htmlHelper, List<GetMenuByUserIdDto> menuItems)` para que acepte un tercer parámetro `ViewContext viewContext`
2. En `_Layout.cshtml:78`, cambiar `@Html.RenderMenu(currentUser.MenuOptions)` por `@Html.RenderMenu(currentUser.MenuOptions, ViewContext)`
3. Compilar para validar

**Paso 2 — Detectar ruta activa en `RenderMenu`**
1. Extraer la ruta actual de `viewContext.HttpContext.Request.Path` dentro del método
2. Propagar ese valor como parámetro a los métodos privados que lo necesiten: `RenderMenuItem`, `RenderMenuItemParents`
3. Compilar para validar

**Paso 3 — Agregar `kt-menu-item-active` en `RenderMenuItem`**
1. Comparar `menuItem.URL` con la ruta actual (match exacto, ignorando ~/ y / inicial)
2. Si coinciden, agregar clase `kt-menu-item-active` al `<div class="kt-menu-item">`
3. Compilar para validar

**Paso 4 — Implementar `kt-menu-bullet` para sub-ítems**
1. En `RenderMenuItem`, si `menuItem.ParentMenuId != null`, reemplazar el `<span class="kt-menu-icon">` + `<i>` por un `<span class="kt-menu-bullet">`
   - El span de bullet debe contener un `<span class="kt-menu-bullet-dot">` interno según el markup de Metronic
2. Si `menuItem.ParentMenuId == null`, mantener el `kt-menu-icon` actual
3. Compilar para validar

**Paso 5 — Expandir padre accordion cuando hijo está activo**
1. En `RenderMenuItemParents`, determinar si alguno de sus hijos (`menuItems.Where(a => a.ParentMenuId == menuItem.MenuId)`) tiene URL que coincide con la ruta actual
2. Si hay un hijo activo, agregar clase `kt-menu-item-show` al `<div class="kt-menu-item">` del padre
3. Esto activa los estilos de accordion expandido (`kt-menu-item-show:`) del CSS de Metronic
4. Compilar para validar

**Paso 6 — Verificación final**
1. Ejecutar `dotnet build "TradingBookApp.sln"` — debe compilar sin errores
2. Ejecutar `dotnet run --project Web/Web.csproj` — la app inicia sin errores
3. Navegar a una página con entrada en el menú (ej: `~/AnalyticsConvergence`) y verificar que el ítem aparece resaltado sin intervención del JS
4. Verificar que un sub-ítem activo expande su padre accordion automáticamente
5. Verificar que los sub-ítems usan `kt-menu-bullet` en vez de icono

---

## Criterios de aceptación

- [ ] `RenderMenu` recibe `ViewContext` como tercer parámetro
- [ ] `_Layout.cshtml` pasa `ViewContext` a `RenderMenu`
- [ ] El ítem de menú cuya `URL` coincide exactamente con la ruta actual recibe la clase `kt-menu-item-active` en su `<div class="kt-menu-item">`
- [ ] Los sub-ítems (`ParentMenuId != null`) renderizan `<span class="kt-menu-bullet">` en vez de `<span class="kt-menu-icon">` + `<i>`
- [ ] Los ítems de primer nivel (`ParentMenuId == null`) mantienen `kt-menu-icon` con su `<i>` de icono
- [ ] El padre accordion se expande automáticamente (`kt-menu-item-show`) cuando un hijo coincide con la ruta activa
- [ ] Los estilos de resaltado (bg accent, texto primary) se aplican correctamente sin depender del JS de `data-kt-menu`
- [ ] `dotnet build "TradingBookApp.sln"` compila sin errores
- [ ] `dotnet run --project Web/Web.csproj` inicia sin errores

---

## Decisiones tomadas y descartadas

- **Sí:** Pasar `ViewContext` como parámetro a `RenderMenu()`. Es explícito, no requiere modificar cada controller, y el helper obtiene la ruta directamente de `HttpContext.Request.Path`.
- **Sí:** Match exacto de URL (`~/AnalyticsConvergence` solo con esa ruta). Es la opción más simple y predecible; el usuario confirmó que no necesita match por prefijo ni por controller/action.
- **Sí:** La clase CSS es `kt-menu-item-active`, la misma que usa el JS de Metronic. Esto asegura que los estilos del tema (`kt-menu-item-active:bg-accent/60`, `kt-menu-item-active:text-primary`) se activen correctamente sin modificar CSS.
- **Sí:** `kt-menu-bullet` para sub-ítems (`ParentMenuId != null`). Los estilos ya existen en `styles.css` del tema; solo falta generarlos desde el helper.
- **Sí:** Expandir padre accordion (`kt-menu-item-show`) cuando un hijo está activo. Sin esto, el submenú activo quedaría oculto visualmente.
- **No:** Modificar `GetMenuByUserIdDto` o la tabla `Menu`. No se necesita nueva información en los DTOs; la lógica de matching es puramente del lado del helper.
- **No:** Eliminar el JS de `data-kt-menu`. La clase se agrega desde servidor, pero el JS de Metronic puede seguir coexistiendo sin conflicto (si ya existe la clase, el JS no la duplica ni la quita).
- **No:** Match por prefijo o controller/action. El usuario lo descartó explícitamente a favor del match exacto.

---

## Riesgos identificados

| Riesgo | Mitigación |
|--------|------------|
| **Conflicto entre clase server-side y JS de `data-kt-menu`** | El JS de Metronic puede estar agregando/quintando `kt-menu-item-active` también. Verificar durante el paso 6 que no haya doble-toggle o flickering. Si hay conflicto, evaluar deshabilitar la inicialización de `data-kt-menu` para el sidebar. |
| **`oListMenuId` estático no es thread-safe** | `MenuHelper` usa `static HashSet<int>? oListMenuId` como campo compartido. En un servidor multi-hilo puede causar condiciones de carrera. Como mitigación mínima, moverlo a variable local dentro de `RenderMenu()` en vez de campo estático. |
| **Formato de URL inconsistente** | Las URLs del menú vienen de la DB y pueden tener `~/` al inicio o no. La ruta de `Request.Path` nunca tiene `~/`. Normalizar ambas antes del match (remover `~/`, remover trailing `/`). |
| **El markup exacto de `kt-menu-bullet` no está confirmado** | Los estilos CSS de Metronic ya están en `styles.css` pero el markup esperado por el CSS debe verificarse inspeccionando el DOM de una página de demo de Metronic o el CSS mismo. Si el helper genera un markup incorrecto, el bullet no se renderiza. |

---

## Lo que **no** está en esta spec

- Match por prefijo o controller/action (solo match exacto de URL)
- Modificar tabla `Menu`, DTO `GetMenuByUserIdDto` o cualquier entidad de base de datos
- Cambiar o eliminar el CSS de Metronic (`styles.css`)
- Eliminar el JS de inicialización `data-kt-menu`
- Agregar `active` a páginas sin entrada de menú
