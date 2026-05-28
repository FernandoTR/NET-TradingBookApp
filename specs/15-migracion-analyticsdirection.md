# Spec: 15-migracion-analyticsdirection.md

* **Estado:** Implementado
* **Fecha:** 2026-05-24
* **Objetivo:** Migrar `AnalyticsDirection/Index` a componentes visuales de Metronic Tailwind, consolidando la información de porcentaje dentro de las columnas principales (SL, TP1, TP2, TP3) y replicando el patrón de filtrado vía drawer de `AnalyticsTrigger/Index`.

### Alcance

**Incluido:**
- Migración de la vista `Web/Views/AnalyticsDirection/Index.cshtml` a la estructura de componentes Metronic Tailwind.
- Consolidación de la información de porcentaje (SL, TP1, TP2, TP3) dentro de las columnas principales, utilizando la lógica visual establecida en `AnalyticsTrigger`.
- Implementación del drawer de filtrado replicando la funcionalidad y estructura de `AnalyticsTrigger`.

**No incluido:**
- Cambios en la lógica de negocio del controlador `AnalyticsDirectionController`.
- Modificaciones en la estructura de los datos que llegan desde el servicio de aplicación.

### Data model

N/A - Esta migración se centra exclusivamente en la capa de presentación (UI). Se reutilizan las estructuras de datos y modelos de vista existentes de `AnalyticsDirection` y los modelos de filtrado definidos en `AnalyticsTrigger`.

### Implementation plan

1.  **Scaffolding de UI:** Actualizar `Web/Views/AnalyticsDirection/Index.cshtml` para utilizar el layout base y componentes de Metronic Tailwind.
2.  **Drawer de filtrado:** Integrar el componente de drawer de filtrado replicando la implementación de `AnalyticsTrigger/Index` (sección de filtros y trigger button).
3.  **Refactorización de Tabla:** Modificar la tabla de datos principal para consolidar las columnas SL, TP1, TP2, TP3 integrando los porcentajes dentro de las celdas siguiendo el patrón visual de `AnalyticsTrigger`.
4.  **Verificación:** Validar el funcionamiento del filtrado (aplicación de parámetros y recarga de vista) y la integridad de la visualización consolidada.

### Acceptance criteria

- [ ] La vista `AnalyticsDirection/Index` renderiza correctamente con el layout y componentes de Metronic Tailwind.
- [ ] El componente de filtrado (drawer) está integrado, es funcional y replica exactamente la interfaz de `AnalyticsTrigger`.
- [ ] Las columnas (SL, TP1, TP2, TP3) muestran correctamente el valor principal y el porcentaje consolidado dentro de la misma celda, siguiendo el diseño visual establecido.
- [ ] Los filtros aplicados en el drawer actualizan correctamente los datos de la tabla sin errores.

### Decisions taken and discarded

- **Decisión:** Reutilizar directamente el patrón de filtrado de `AnalyticsTrigger`.
  - **Justificación:** Asegura consistencia en toda la aplicación y reduce el tiempo de desarrollo al mantener una UX uniforme.
- **Decisión:** Consolidación visual en front-end en lugar de cambios en el backend.
  - **Justificación:** La estructura de datos actual es suficiente, solo requiere un remapeo visual en Razor para cumplir con el nuevo diseño.

### Identified risks

- **Conflictos de CSS:** Posibles solapamientos de estilos entre la estructura anterior y la nueva implementación de Metronic Tailwind si quedan residuos de clases antiguas.
- **Responsividad:** La consolidación de datos (valor + porcentaje) en las columnas principales puede afectar la legibilidad en dispositivos móviles si no se ajustan correctamente los breakpoints.
