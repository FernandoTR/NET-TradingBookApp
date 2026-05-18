---
name: dotnet-security-validator
description: Analiza aplicaciones .NET y ASP.NET Core para detectar vulnerabilidades de seguridad, configuraciones inseguras, malas prácticas OWASP, problemas de autenticación/autorización, secretos expuestos, dependencias vulnerables y errores de hardening. Use cuando el usuario quiera auditar seguridad de APIs, aplicaciones web, microservicios o proyectos .NET. No usar para pentesting ofensivo, malware, bypass de autenticación ni explotación activa.
---

# .NET Security Validator Skill

## Objetivo

Validar la seguridad de aplicaciones .NET y ASP.NET Core utilizando análisis estático, revisión de configuración y validaciones basadas en OWASP Top 10 y recomendaciones oficiales de Microsoft.

## Procedimiento

### Paso 1 — Identificar el tipo de proyecto

Determinar:

- ASP.NET Core API
- MVC
- Blazor
- Minimal API
- Worker Service
- Microservicio
- Monolito

Identificar:

- versión de .NET
- proveedor de autenticación
- ORM utilizado
- tipo de base de datos
- proveedor cloud
- CI/CD

### Paso 2 — Validar configuración sensible

Revisar:

- appsettings.json
- appsettings.*.json
- Program.cs
- Startup.cs
- variables de entorno
- secretos hardcodeados
- cadenas de conexión

Leer:

- references/secure-config-patterns.md

### Paso 3 — Validar autenticación y autorización

Leer:

- references/authentication-validation.md

### Paso 4 — Validar vulnerabilidades OWASP

Leer:

- references/owasp-top10-dotnet.md

### Paso 5 — Validar dependencias vulnerables

Ejecutar:

```bash
dotnet list package --vulnerable --include-transitive
```

Ejecutar:

```powershell
scripts/analyze-dependencies.ps1
```

### Paso 6 — Validar hardening del runtime

Verificar:

- logging seguro
- rate limiting
- límites request
- sanitización
- protección brute-force

### Paso 7 — Ejecutar escaneo automatizado

```powershell
scripts/run-security-scan.ps1
```

### Paso 8 — Generar reporte

Usar:

- assets/security-report-template.md

Exportar:

```bash
python scripts/export-report.py
```
