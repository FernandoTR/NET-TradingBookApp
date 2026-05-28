# SPEC 20 — Rate Limiting + Validación (Phase 2)

> **Estado:** Implementado  · **Depende de:** SPEC 19 · **Fecha:** 2026-05-28
> **Objetivo:** Implementar rate limiting en endpoints de autenticación y corregir la validación de formularios.

---

## Alcance

**Incluye:**

- Agregar `AddRateLimiter` en `Web/DependencyInjection.cs` con política global para `/Account/*`
- Descomentar la validación de `ModelState` en `AccountController.Login` (líneas 77-80)
- Verificar que `ForgotPassword` ya protege contra enumeración de usuarios (ya redirige siempre a "confirmation")

**Fuera de alcance:**

- Rate limiting granular por endpoint específico (login vs forgot vs sendcode)
- Implementar IP blacklist o whitelist
- Agregar rate limiting fuera de los endpoints `/Account/*`
- Modificar el flujo 2FA (se mantiene como está)
- Auditoría documental de FromSqlRaw (ya verificado - los 9 usages usan SqlParameter correctamente)

---

## Data model

Esta funcionalidad no introduce nuevas estructuras de datos.

---

## Plan de implementación

**Paso 1 — Agregar Rate Limiting**

1. En `Web/DependencyInjection.cs`, agregar:
   ```csharp
   builder.Services.AddRateLimiter(options =>
   {
       options.RejectionStatusCode = 429;
       options.AddPolicy("AccountPolicy", context => 
           RateLimitPartition.GetFixedWindowLimiter(
               partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
               factory: _ => new FixedWindowRateLimiterOptions
               {
                   PermitLimit = 10,
                   Window = TimeSpan.FromSeconds(5)
               }));
       options.OnRejected = async (context, cancellationToken) =>
       {
           context.HttpContext.Response.StatusCode = 429;
           await context.HttpContext.Response.WriteAsync("Demasiadas solicitudes. Intente más tarde.", cancellationToken);
       };
   });
   ```
2. En `Web/Program.cs`, agregar después de `UseAuthentication`:
   ```csharp
   app.UseRateLimiter();
   ```
3. Aplicar la política a los endpoints de Account usando `[EnableRateLimiting("AccountPolicy")]` en los métodos correspondientes

**Paso 2 — Corregir validación de ModelState**

1. En `AccountController.Login`, descomentar las líneas 77-80:
   ```csharp
   if (!ModelState.IsValid)
   {
       return View("SignIn", model);
   }
   ```

**Paso 3 — Verificar enumeración en ForgotPassword**

1. Revisar `AccountController.ForgotPassword` para confirmar que siempre redirige a `ForgotPasswordConfirmation` sin importar si el usuario existe o no (ya verificado - el código hace redirect sin distinguir)

---

## Criterios de aceptación

- [ ] `Web/DependencyInjection.cs` tiene `AddRateLimiter` configurado con política "AccountPolicy"
- [ ] `Web/Program.cs` tiene `UseRateLimiter` después de `UseAuthentication`
- [ ] Los endpoints `/Account/SignIn`, `/Account/Login`, `/Account/ForgotPassword` tienen `[EnableRateLimiting("AccountPolicy")]`
- [ ] `AccountController.Login` descomenta la validación de `ModelState`
- [ ] `ForgotPassword` redirige a "confirmation" sin revelar si el email existe (ya verificado en código existente)
- [ ] `dotnet build "TradingBookApp.sln"` compila sin errores

---

## Decisiones tomadas y descartadas

- **Sí:** Rate limiting global simple por IP. 10 requests/5s es suficiente para prevenir brute force sin afectar usuarios legítimos.
- **Sí:** Usar `EnableRateLimiting` attribute en cada método. Explícito y fácil de auditar.
- **Sí:** Mantener el flujo 2FA actual sin cambios. El código ya protege contra enumeración en `ForgotPassword`.
- **No:** Rate limiting granular por endpoint. 10/min para todos los endpoints de Account es consistente y simple.
- **No:** IP blacklist/whitelist. Requiere persistencia y mantenimiento; fuera de alcance para esta phase.
- **No:** Auditoría documental de FromSqlRaw. Los 9 usages ya son seguros; no requiere cambios ni documentación adicional.

---

## Qué **no** está en esta spec

- Rate limiting para endpoints fuera de `/Account/*`
- Implementar account lockout adicional (ya existe en Identity con 5 intentos/5 min)
- Modificar el flujo de 2FA o la lógica de generación de códigos
- Agregar logging de intentos de rate limit
- Documentar la auditoría de FromSqlRaw