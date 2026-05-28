# SPEC 01 - Migracion inicial a Metronic Tailwind

> **Estado:** Implementado · **Depende de:** Ninguna · **Fecha:** 2026-05-18
> **Objetivo:** Migrar las referencias globales del layout y validar `Home/Index` para iniciar la convivencia controlada entre Metronic Bootstrap y Metronic Tailwind.

## Alcance

**Incluye:**

- Actualizar las referencias globales de assets en `Web/Views/Shared/_Layout.cshtml` para cargar `~/Template/assets_Tailwind/css/styles.css`.
- Actualizar las referencias globales de assets en `Web/Views/Shared/_Layout.cshtml` para cargar `~/Template/assets_Tailwind/js/core.bundle.js`.
- Mantener cargados los assets existentes de Bootstrap/Metronic Bootstrap cuando sean necesarios para componentes actuales que todavia dependen de ellos.
- Validar `Web/Views/Home/Index.cshtml` como la primera pagina migrada.
- Incluir validacion basica de render de los parciales del dashboard cargados por `Home/Index`, sin convertir su markup interno en esta spec.
- Confirmar que la aplicacion sigue compilando con `dotnet build "TradingBookApp.sln"`.
- Confirmar que `Home/Index` carga en desktop y mobile sin errores de consola causados por la migracion de assets.

**Fuera de alcance (para specs futuras):**

- Conversion completa de todas las vistas `.cshtml` de clases Bootstrap a clases Tailwind.
- Rediseño visual completo de `Home/Index`.
- Migracion del markup interno de vistas parciales del dashboard como `_Balance` y parciales de analytics.
- Eliminacion de `Web/wwwroot/Template/assets`.
- Eliminacion de Bootstrap, jQuery, Select2, Flatpickr, DataTables u otras dependencias cliente existentes.
- Migracion de paginas admin, account, orders, manage, catalog, users, roles, logs o analytics index.

## Modelo de datos

Esta funcionalidad no introduce nuevas estructuras de datos backend.

El unico contrato de assets introducido por esta spec es el par inicial de Tailwind usado por el layout:

```text
~/Template/assets_Tailwind/css/styles.css
~/Template/assets_Tailwind/js/core.bundle.js
```

Las rutas existentes de Bootstrap/Metronic Bootstrap siguen siendo validas durante esta fase hibrida.

## Plan de implementacion

1. Inspeccionar las referencias globales actuales de CSS y JS en `Web/Views/Shared/_Layout.cshtml` e identificar que referencias existentes de Bootstrap/Metronic Bootstrap deben permanecer por compatibilidad hibrida.
2. Agregar `~/Template/assets_Tailwind/css/styles.css` a las referencias globales de estilos en `_Layout.cshtml` sin eliminar estilos existentes requeridos.
3. Agregar `~/Template/assets_Tailwind/js/core.bundle.js` a las referencias globales de scripts en `_Layout.cshtml` sin eliminar scripts existentes requeridos.
4. Cargar la aplicacion y verificar que el layout autenticado siga renderizando header, sidebar, menus, modales, notificaciones y controles de calculadora de riesgo existentes.
5. Navegar a `Home/Index` y verificar que sigan apareciendo el shell del dashboard, menu de filtros, contenedores de graficas, boton de accion y contenido basico de parciales del dashboard renderizados por AJAX.
6. Revisar `Home/Index` en anchos desktop y mobile para confirmar que los assets hibridos no introducen una regresion que bloquee el layout.
7. Ejecutar `dotnet build "TradingBookApp.sln"` desde la raiz del repositorio y registrar cualquier warning preexistente por separado de regresiones de la migracion.

## Criterios de aceptacion

- [ ] `_Layout.cshtml` referencia `~/Template/assets_Tailwind/css/styles.css`.
- [ ] `_Layout.cshtml` referencia `~/Template/assets_Tailwind/js/core.bundle.js`.
- [ ] Las referencias existentes requeridas de Bootstrap/Metronic Bootstrap permanecen disponibles para componentes que todavia dependen de ellas.
- [ ] `Web/wwwroot/Template/assets` permanece en su lugar.
- [ ] `Home/Index` carga sin errores de migracion de assets en la consola del navegador.
- [ ] El shell del dashboard en `Home/Index` renderiza toolbar, menu de filtros, contenedores de graficas y la accion "Generar una orden".
- [ ] El contenido basico de parciales del dashboard renderizados por AJAX aparece cuando hay datos disponibles.
- [ ] `Home/Index` sigue siendo usable en anchos desktop y mobile.
- [ ] `dotnet build "TradingBookApp.sln"` termina correctamente, permitiendo solo warnings preexistentes no relacionados con esta migracion.

## Decisiones

- **Si:** Empezar solo con referencias globales en `Web/Views/Shared/_Layout.cshtml`. Esto limita el primer paso de migracion a una superficie controlada.
- **Si:** Incluir `Web/Views/Home/Index.cshtml` como primera pagina de validacion. Esta pagina ejercita el layout autenticado, shell del dashboard, filtros, graficas y render de parciales.
- **Si:** Cargar solo `~/Template/assets_Tailwind/css/styles.css` y `~/Template/assets_Tailwind/js/core.bundle.js` desde el set de assets Tailwind en esta spec. Estos son los archivos Tailwind iniciales confirmados.
- **Si:** Permitir convivencia temporal con assets Bootstrap/Metronic Bootstrap. Las vistas actuales todavia dependen de clases Bootstrap, `data-bs-*`, plugins jQuery, Select2, Flatpickr, DataTables, modales y tooltips.
- **Si:** Mantener intacto `Web/wwwroot/Template/assets`. Eliminarlo pertenece a una spec posterior de limpieza cuando la migracion de componentes este completa.
- **Si:** Validar render basico de parciales del dashboard cargados por `Home/Index`. Esto detecta regresiones runtime obvias sin convertir esta spec en una migracion de parciales.
- **No:** Convertir todas las vistas `.cshtml` a Tailwind en esta spec. El repositorio tiene muchas vistas dependientes de Bootstrap y esto haria que la primera migracion sea demasiado amplia.
- **No:** Eliminar Bootstrap o plugins relacionados en esta spec. Eso romperia componentes existentes antes de migrar su markup e inicializacion.

## Riesgos

| Riesgo | Mitigacion |
| ------ | ---------- |
| Los estilos de Tailwind y Bootstrap entran en conflicto durante la fase hibrida. | Mantener disponibles los assets Bootstrap/Metronic Bootstrap y validar `Home/Index` manualmente en anchos desktop y mobile. |
| Los componentes `data-bs-*` existentes dejan de funcionar si se eliminan dependencias Bootstrap demasiado pronto. | No eliminar scripts Bootstrap/Metronic Bootstrap existentes en esta spec. |
| `Home/Index` parece funcional pero los parciales AJAX del dashboard fallan despues del cambio de assets. | Incluir validacion basica del render de contenido parcial del dashboard cuando haya datos disponibles. |
| Errores de consola preexistentes se confunden con regresiones de migracion. | Registrar errores de consola observados durante la verificacion y separar problemas preexistentes de nuevos errores de migracion de assets. |

## Lo que no esta en esta spec

- Conversion completa de todas las vistas `.cshtml` a Tailwind.
- Rediseño visual completo de `Home/Index`.
- Migracion interna de los parciales del dashboard.
- Eliminacion de `Web/wwwroot/Template/assets`.
- Eliminacion de Bootstrap, jQuery, Select2, Flatpickr, DataTables u otras dependencias existentes.
- Migracion de paginas distintas a `Home/Index`.

Cada uno de esos puntos debe ir en su propia spec si se decide abordarlo.
