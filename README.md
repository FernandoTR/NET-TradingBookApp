<div align="center">

# Trading Book

![TradingBookApp](./PortalTrading.png)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core%20MVC-10.0-512BD4?style=flat-square&logo=dotnet)](https://learn.microsoft.com/aspnet/core/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?style=flat-square&logo=dotnet)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=flat-square&logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind%20CSS-3-06B6D4?style=flat-square&logo=tailwindcss)](https://tailwindcss.com/)
[![Metronic](https://img.shields.io/badge/Metronic-9-1B84FF?style=flat-square)](https://keenthemes.com/metronic/)
[![Clean Architecture](https://img.shields.io/badge/Clean%20Architecture-0078D4?style=flat-square)]()

*Plataforma web para análisis de performance en trading de criptomonedas*

</div>

[Características](#características) • [Arquitectura](#arquitectura) • [Stack tecnológico](#stack-tecnológico) • [Inicio rápido](#inicio-rápido) • [Estructura del proyecto](#estructura-del-proyecto) • [Base de datos](#base-de-datos)

---

TradingBook es una plataforma web que centraliza el registro y análisis de operaciones de trading en criptomonedas. Su objetivo es transformar la ejecución operativa en información accionable: evaluar setups, medir efectividad, detectar patrones y mejorar la toma de decisiones a partir de datos reales.

> [!NOTE]
> El proyecto está construido con **.NET 10**, **Clean Architecture** y **Metronic Tailwind CSS**, con foco en escalabilidad, mantenibilidad y evolución continua del producto.

## Características

- **Trading Score Engine** — evalúa operaciones usando ubicación, tendencia, confirmación y zonas pivote.
- **Asistente de validación IA** — permite validar propuestas de trade con imágenes temporales, extracción multimodal, normalización contra catálogos, reglas determinísticas, Trading Score y evidencia histórica.
- **Creación de órdenes desde validaciones IA** — convierte una validación confirmada por el usuario en una orden usando el flujo existente de `Orders`.
- **Métricas operativas del asistente IA** — compara clasificación IA, confirmación humana y resultado real de la orden por proveedor y modelo.
- **Gestión de proveedores IA** — módulo administrativo para configurar proveedor, modelo, endpoint y referencia de API key sin almacenar secretos reales.
- **Proveedores multimodales** — adaptadores explícitos para OpenAI, MiniMax, DeepSeek, GLM y Kimi.
- **Análisis multidimensional** — estadísticas por trigger, escenario, figura, dirección, marco temporal y día de la semana.
- **Dashboard interactivo** — visualizaciones con Chart.js para métricas clave y DataTables 2.x para exploración tabular.
- **Gestión de cuentas** — múltiples cuentas con tracking de balance, depósitos y retiros.
- **Autenticación robusta** — ASP.NET Core Identity con 2FA (código QR + authenticator), confirmación de email y recuperación de contraseña.
- **Protección contra enumeración** — ForgotPassword redirige siempre a la página de confirmación sin revelar si el usuario existe.
- **Rate limiting** — endpoints de cuenta protegidos con límite de 10 peticiones cada 5 segundos por IP.
- **Cookies seguras** — política `CookieSecurePolicy.Always`, requiere HTTPS para flujos de autenticación.
- **Credenciales externalizadas** — `appsettings.Development.json` y `appsettings.Production.json` excluidos de Git; `appsettings.json` contiene solo placeholders `__CHANGE_ME__`.
- **Registro de actividad** — logging completo de operaciones con trazabilidad por usuario.
- **Módulos administrativos** — gestión de empleados, roles, usuarios, categorías, figuras, tipos de cuenta, temporalidades e instrumentos.

## Arquitectura

El proyecto sigue los principios de **Clean Architecture** con cuatro capas y dependencias unidireccionales:

```
Web ──────────────▶ Application ──────────────▶ Domain
  │                       ▲
  └──▶ Infrastructure ────┘
```

| Capa | Responsabilidad | Proyecto |
|---|---|---|
| **Web** | Controladores MVC, vistas Razor, middlewares, wwwroot | `Web/` |
| **Application** | Servicios de negocio, DTOs, interfaces, modelos | `Application/` |
| **Domain** | Entidades, enumeraciones, constantes (núcleo del negocio) | `Domain/` |
| **Infrastructure** | Persistencia, repositorios, email, Identity, logging | `Infrastructure/` |

El arranque ocurre en `Web/Program.cs` mediante tres métodos extensores que registran los servicios de cada capa: `AddApplicationServices()`, `AddInfrastructureServices()` y `AddWebServices()`.

## Stack tecnológico

| Componente | Tecnología |
|---|---|
| Framework | .NET 10.0 |
| Web | ASP.NET Core MVC + Razor Pages |
| ORM | Entity Framework Core 10.0 |
| Base de datos | SQL Server |
| UI Framework | Metronic 9 (Tailwind CSS) |
| Tablas | DataTables 2.x |
| Gráficos | Chart.js |
| Fechas | Flatpickr |
| Autenticación | ASP.NET Core Identity |
| 2FA | QR Code (QRCoder) + Authenticator |
| Email | MailKit (SMTP/Office 365) |
| Arquitectura | Clean Architecture |
| Localización | es-MX |

## Inicio rápido

### Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (local o vía Docker)
- Visual Studio 2022 o VS Code

### Configuración

1. Clona el repositorio.
2. Crea `Web/appsettings.Development.json` copiando el `appsettings.json`:

   ```bash
   cp Web/appsettings.json Web/appsettings.Development.json
   ```

3. Rellena los valores reales en `Web/appsettings.Development.json`: cadena de conexión y credenciales SMTP.
   > [!IMPORTANT]
   > `appsettings.Development.json` y `appsettings.Production.json` están excluidos de Git. Nunca guardes credenciales reales en `appsettings.json` — ese archivo solo contiene placeholders `__CHANGE_ME__`.

4. Configura las API keys de proveedores IA como variables de entorno. La aplicación solo guarda la referencia al nombre de la variable, nunca el valor del secreto:

   ```bash
   setx OPENAI_API_KEY "<tu-api-key>"
   setx MINIMAX_API_KEY "<tu-api-key>"
   setx DEEPSEEK_API_KEY "<tu-api-key>"
   setx GLM_API_KEY "<tu-api-key>"
   setx KIMI_API_KEY "<tu-api-key>"
   ```

   > [!IMPORTANT]
   > Reinicia la terminal o el proceso de la aplicación después de crear o actualizar variables de entorno. El módulo `AiProviders` valida si la variable existe, pero no muestra ni almacena su valor.

5. Restaura las herramientas de EF Core:

   ```bash
   cd Web
   dotnet tool restore
   ```

### Ejecución

```bash
# Desde la raíz del repositorio
dotnet build "TradingBookApp.sln"
dotnet run --project Web/Web.csproj --launch-profile https
```

Accede a la aplicación en `https://localhost:7221` o `http://localhost:5080`.

> [!NOTE]
> Usa el perfil HTTPS para los flujos de autenticación — las cookies de sesión requieren conexión segura.

## Estructura del proyecto

```
TradingBookApp/
├── Application/          # Lógica de aplicación
│   ├── Common/           # Utilidades compartidas (QueryOptions)
│   ├── DTOs/             # Objetos de transferencia de datos
│   ├── Interfaces/       # Contratos de repositorios y servicios
│   ├── Models/           # Modelos de resultado (Result<T>)
│   ├── Resources/        # Cadenas localizadas (ErrorMessage, Message)
│   ├── Services/         # Implementaciones de servicios de negocio
│   ├── GlobalUsings.cs
│   └── DependencyInjection.cs
├── Domain/               # Núcleo del negocio
│   ├── Constants/        # Valores constantes
│   ├── Entities/         # Entidades del dominio
│   └── Enums/            # Enumeraciones
├── Infrastructure/       # Implementaciones concretas
│   ├── Email/            # Envío de correos (MailKit)
│   ├── Identity/         # Autenticación y autorización
│   ├── Logging/          # Registro de actividad
│   ├── Persistence/
│   │   ├── Data/         # DbContext (ApplicationDbContext, LoggingDbContext)
│   │   └── Repositories/ # Repositorios
│   ├── Services/         # Servicios de infraestructura
│   ├── GlobalUsings.cs
│   └── DependencyInjection.cs
├── Web/                  # Punto de entrada ASP.NET Core
│   ├── Controllers/      # Controladores MVC (21 controladores)
│   ├── Views/            # Vistas Razor organizadas por módulo
│   ├── wwwroot/          # Assets estáticos (CSS, JS, imágenes)
│   ├── Helpers/          # Tag Helpers y utilidades de vista
│   ├── Models/           # ViewModels
│   ├── Program.cs
│   └── DependencyInjection.cs
├── specs/                # Especificaciones de migración y hardening
├── TradingBookApp.sln
├── CHANGELOG.md
└── README.md
```

## Base de datos

El proyecto usa un enfoque **database-first**: la base de datos SQL Server ya existe y `ApplicationDbContext` se generó mediante scaffolding.

### Aspectos clave

- Se registran dos contextos: `ApplicationDbContext` (negocio) y `LoggingDbContext` (auditoría), ambos contra `ConnectionStrings:DefaultConnection`.
- No hay migraciones de EF Core en el repositorio. Las consultas analíticas usan stored procedures como `usp_GetOrdersDataTable` y vistas como `View_Orders`.
- Los DTOs sin clave usados en queries `FromSqlRaw` deben registrarse en `ApplicationDbContext.Custom.cs`, el archivo que sobrevive a regeneraciones de scaffolding.

### Comandos útiles

```bash
# Restaurar herramientas EF (desde Web/)
dotnet tool restore

# Generar scaffolding desde una base de datos existente
dotnet ef dbcontext scaffold "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer -o Models -f
```

> [!WARNING]
> Si regeneras el scaffolding, `ApplicationDbContext.cs` se sobrescribe. El contenido protegido está en `ApplicationDbContext.Custom.cs` — no lo pierdas.

## Seguridad

| Mecanismo | Detalle |
|---|---|
| Rate limiting | 10 req / 5 s por IP en endpoints `Account` |
| Cookies | `CookieSecurePolicy.Always` — solo sobre HTTPS |
| 2FA | QR code + authenticator app |
| Secretos | `appsettings.*.json` excluidos de Git |
| Anti-enumeración | ForgotPassword no revela existencia de usuarios |
| Conexiones | `Encrypt=true` en todas las cadenas de conexión |
| Validación | `ModelState.IsValid` habilitado en todos los controladores |

## Recursos

- [Documentación de .NET](https://learn.microsoft.com/dotnet/)
- [ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/mvc/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [Clean Architecture en .NET](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/)
- [Tailwind CSS](https://tailwindcss.com/docs)
- [Metronic Tailwind](https://keenthemes.com/metronic/tailwind/)
- [Chart.js](https://www.chartjs.org/)
- [DataTables](https://datatables.net/)
