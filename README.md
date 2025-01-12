# Documentación de la Estructura del Proyecto - ASP.NET Core

Este proyecto utiliza el enfoque de arquitectura limpia para organizar el código, facilitando la separación de responsabilidades, la escalabilidad y el mantenimiento. A continuación, se describe la estructura de carpetas y archivos del proyecto.

## **Capas del Proyecto**

### **1. Application**
Contiene la lógica de aplicación y define las interfaces, DTOs y servicios necesarios para interactuar con otras capas.

- **DTOs**: Objetos de transferencia de datos utilizados para encapsular y transportar datos entre capas.
- **Interfaces**: Contratos que definen la lógica que deben implementar las clases concretas.
- **Models**: Modelos que representan estructuras utilizadas en el contexto de la lógica de aplicación.
- **Resources**: Archivos de recursos como cadenas localizadas o configuraciones específicas.
- **Services**: Servicios de aplicación que implementan la lógica específica del negocio.
- **DependencyInjection.cs**: Configuración para registrar los servicios de Application en el contenedor de dependencias.
- **GlobalUsings.cs**: Archivo para declarar los using globales que simplifican las referencias en esta capa.

---

### **2. Domain**
Representa el núcleo del negocio y contiene las entidades, valores constantes y enumeraciones.

- **Constants**: Valores constantes que son utilizados en toda la aplicación.
- **Entities**: Clases que representan las entidades del dominio con sus propiedades y comportamientos.
- **Enums**: Enumeraciones que representan conjuntos de valores predefinidos.
- **GlobalUsings.cs**: Archivo para declarar los using globales que simplifican las referencias en esta capa.

---

### **3. Infrastructure**
Proporciona implementaciones concretas para las interfaces definidas en `Application`. Incluye servicios para correo electrónico, identidad, logging, persistencia y más.

- **Email**: Lógica relacionada con el envío de correos electrónicos.
- **Identity**: Manejo de autenticación y autorización.
- **Logging**: Configuración y servicios relacionados con el registro de eventos.
- **Persistence**:
  - **Data**: Contiene el `DbContext` para interactuar con la base de datos.
  - **Repositories**: Implementaciones de repositorios para acceder a los datos.
- **DependencyInjection.cs**: Configuración para registrar los servicios de Infrastructure en el contenedor de dependencias.
- **GlobalUsings.cs**: Archivo para declarar los using globales que simplifican las referencias en esta capa.

---

### **4. Web**
Proyecto ASP.NET Core en .NET 8 que actúa como punto de entrada de la aplicación. Incluye controladores, middlewares, y configuraciones específicas para la interacción con los usuarios y servicios externos.

---

## **Consideraciones Generales**
- La separación en capas asegura una alta cohesión dentro de cada capa y un bajo acoplamiento entre ellas.
- `DependencyInjection.cs` en cada capa se utiliza para registrar sus servicios específicos en el contenedor de dependencias global de ASP.NET Core.
- `GlobalUsings.cs` simplifica la gestión de espacios de nombres en los archivos de cada capa.

Esta estructura permite que el código sea modular, testable y fácilmente extensible, facilitando la colaboración en equipos grandes y el mantenimiento a largo plazo.


# Documentación

## Uso de Scaffold-DbContext
El comando `Scaffold-DbContext` se utiliza en proyectos basados en Entity Framework Core para generar automáticamente las clases de entidad y el contexto de base de datos a partir de una base de datos existente. Este proceso es útil cuando se adopta un enfoque de desarrollo basado en la base de datos.

### **Comando Utilizado**
```bash
Scaffold-DbContext 'Name=DefaultConnection' Microsoft.EntityFrameworkCore.SqlServer -OutputDir ../Domain/Entities -ContextDir ../Infrastructure/Persistence/Data -Context ApplicationDbContext -DataAnnotations -Force -Tables AspNetRoles,AspNetUsers,AspNetUserRoles,AspNetRoleClaims,AspNetUserClaims,AspNetUserLogins,AspNetUserTokens
```

### **Descripción de los Parámetros**

- **Cadena de conexión**: Define los detalles para conectarse a la base de datos SQL Server.
- **Microsoft.EntityFrameworkCore.SqlServer**: Especifica el proveedor de base de datos.
- **-OutputDir ../Domain/Entities**: Indica la carpeta donde se generarán las clases de entidad (en este caso, dentro de Domain/Entities).
- **-ContextDir ../Infrastructure/Persistence/Data**: Ubicación del archivo del contexto, aquí dentro de Infrastructure/Persistence/Data.
- **-Context ApplicationDbContext**: Nombre que se asignará a la clase del contexto generado.
- **-DataAnnotations**: Utiliza anotaciones de datos en las clases de entidad en lugar de solo la API fluida.
- **-Force**: Sobrescribe cualquier archivo generado previamente.
- **-Tables**: Excluir tablas de ASP.NET Identity del Scaffolding.

### Acciones Posteriores

1. **Eliminar la Cadena de Conexión en el Contexto**.
Una vez generado el contexto, es importante asegurar que la cadena de conexión no quede expuesta en el código fuente por motivos de seguridad.


## Configuración del archivo `appsettings.json`
1. Copia el archivo `appsettings.json` del repositorio.
2. Rellena los valores necesarios, como la cadena de conexión y cuenta de correo.