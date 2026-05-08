![TradingBookApp](./PortalTrading.png)

# TradingBook

*Plataforma web para análisis de performance en trading cripto / Web platform for crypto trading performance analysis*

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core%20MVC-8.0-512BD4?style=flat-square&logo=dotnet)](https://learn.microsoft.com/aspnet/core/)
[![Entity Framework Core](https://img.shields.io/badge/Entity%20Framework%20Core-8.0-512BD4?style=flat-square&logo=dotnet)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=flat-square&logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Clean Architecture](https://img.shields.io/badge/Clean%20Architecture-0078D4?style=flat-square)]()

TradingBook es una plataforma web creada para centralizar el registro y análisis de operaciones de trading en criptomonedas. Su objetivo es transformar la ejecución operativa en información accionable, permitiendo evaluar setups, medir efectividad, detectar patrones y mejorar la toma de decisiones a partir de datos reales.

> [!TIP]
> The project was developed with **.NET 8** and **Clean Architecture**, with a focus on scalability, maintainability, and product evolution.

## Features / Características

- **Trading Score Engine**: Evalúa operaciones usando ubicación, tendencia, confirmación y zonas pivote 
- **Análisis Multidimensional**: Estadísticas por trigger, escenario, figura, dirección, marco temporal y día 
- **Dashboard Interactivo**: Visualizaciones con Chart.js para métricas clave 
- **Gestión de Cuentas**: Múltiples cuentas con tracking de balance y transacciones 
- **Seguridad Robusta**: Autenticación con 2FA y hashing de contraseñas 
- **Registro de Actividad**: Logging completo de operaciones 

## Architecture / Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                          Web                                │
│              (ASP.NET Core MVC, Controllers,                │
│               Views, wwwroot, Middleware)                   │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                     Application                             │
│        (Services, DTOs, Interfaces, Models)                 │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                       Domain                                │
│           (Entities, Enums, Constants)                       │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                   Infrastructure                            │
│    (Persistence, Repositories, Email, Identity,            │
│                   Logging, External Services)              │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure / Estructura del Proyecto

### 1. Application

Contiene la lógica de aplicación y define las interfaces, DTOs y servicios necesarios para interactuar con otras capas.

- `DTOs/`
   Objetos de transferencia de datos utilizados para encapsular y transportar datos entre capas. 

- `Interfaces/`
   Contratos que definen la lógica que deben implementar las clases concretas. 

- `Models/`
   Modelos que representan estructuras utilizadas en el contexto de la lógica de aplicación. 

- `Resources/`
   Archivos de recursos como cadenas localizadas o configuraciones específicas. 

- `Services/`
   Servicios de aplicación que implementan la lógica específica del negocio. 

- `DependencyInjection.cs`
   Configuración para registrar los servicios de Application en el contenedor de dependencias. 

- `GlobalUsings.cs`
   Archivo para declarar los using globales que simplifican las referencias en esta capa. 

---

### 2. Domain

Representa el núcleo del negocio y contiene las entidades, valores constantes y enumeraciones. 

- `Constants/`
   Valores constantes que son utilizados en toda la aplicación.

- `Entities/`
   Clases que representan las entidades del dominio con sus propiedades y comportamientos. 

- `Enums/`
   Enumeraciones que representan conjuntos de valores predefinidos. 

- `GlobalUsings.cs`
   Archivo para declarar los using globales que simplifican las referencias en esta capa. 

---

### 3. Infrastructure

Proporciona implementaciones concretas para las interfaces definidas en `Application`. Incluye servicios para correo electrónico, identidad, logging, persistencia y más. Provides concrete implementations for the interfaces defined in `Application`. 

- `Email/`
   Lógica relacionada con el envío de correos electrónicos.

- `Identity/`
   Manejo de autenticación y autorización. 

- `Logging/`
   Configuración y servicios relacionados con el registro de eventos. 

- `Persistence:`
   - `Data/`
     Contiene el `DbContext` para interactuar con la base de datos. 

   - `Repositories/`
     Implementaciones de repositorios para acceder a los datos. 

- `DependencyInjection.cs`
   Configuración para registrar los servicios de Infrastructure en el contenedor de dependencias. 

- `GlobalUsings.cs`
   Archivo para declarar los using globales que simplifican las referencias en esta capa. 

---

### 4. Web

Proyecto ASP.NET Core en .NET 8 que actúa como punto de entrada de la aplicación. Incluye controladores, middlewares, y configuraciones específicas para la interacción con los usuarios y servicios externos. 

- `Pages/`
   Contiene las Razor Pages que definen la UI y la lógica de presentación. 

- `Controllers/`
   Incluye controladores para endpoints adicionales, como APIs o acciones especializadas. 

- `Views/`
   Vistas compartidas y parciales, así como layouts reutilizables. 

- `wwwroot/`
   Archivos estáticos (CSS, JS, imágenes, plantillas). 

- `Template/`
   Recursos de plantillas, scripts personalizados y plugins. 

- `Media/Logos/`
   Almacena logotipos y recursos gráficos. 

---

## General Considerations / Consideraciones Generales

- La separación en capas asegura una alta cohesión dentro de cada capa y un bajo acoplamiento entre ellas.

- `DependencyInjection.cs` en cada capa se utiliza para registrar sus servicios específicos en el contenedor de dependencias global de ASP.NET Core. 

- `GlobalUsings.cs` simplifica la gestión de espacios de nombres en los archivos de cada capa. 

- Esta estructura permite que el código sea modular, testeable y fácilmente extensible, facilitando la colaboración en equipos grandes y el mantenimiento a largo plazo. 

## Tech Stack / Stack Tecnológico

| Component | Technology |
|-----------|------------|
| Framework | .NET 8 |
| Web | ASP.NET Core MVC |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Frontend | Bootstrap, JavaScript |
| Charts | Chart.js |
| Auth | ASP.NET Core Identity |
| Architecture | Clean Architecture |

## Quick Start / Inicio Rápido

### Prerequisites / Requisitos

- .NET 8 SDK
- SQL Server (Local or Docker)
- Visual Studio 2022 / VS Code

### Configuration / Configuración

1. Copia el archivo `appsettings.json` del repositorio.
2. Rellena los valores necesarios, como la cadena de conexión y cuenta de correo. 
3. Ejecuta las migraciones de Entity Framework Core si es necesario. 

### Running / Ejecución

```bash
cd Web
dotnet restore
dotnet build
dotnet run
```

Accede a la aplicación en `https://localhost:5001` o `http://localhost:5000`. 

## Resources / Recursos

- [.NET Documentation](https://learn.microsoft.com/dotnet/)
- [ASP.NET Core](https://learn.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [Clean Architecture](https://learn.microsoft.com/azure/architecture/dotnet-apps/)
- [Bootstrap](https://getbootstrap.com/)
- [Chart.js](https://www.chartjs.org/)