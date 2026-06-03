# AGENTS.md

## Commands
- Build the repo with `dotnet build "TradingBookApp.sln"` from the repository root. Current build succeeds but emits many nullable warnings and one MVC1000 warning.
- Run the web app with `dotnet run --project Web/Web.csproj --launch-profile https` or `--launch-profile http`; launch profiles set `ASPNETCORE_ENVIRONMENT=Development` and ports `https://localhost:7221` / `http://localhost:5080`.
- There are no test projects or CI workflows in the repo; use a focused `dotnet build` plus manual app checks unless tests are added.
- The EF CLI tool manifest is under `Web/.config/dotnet-tools.json`; run `dotnet tool restore` from `Web` before `dotnet ef` commands.

## Architecture
- Solution projects are `Domain`, `Application`, `Infrastructure`, and `Web` targeting `net8.0` with nullable enabled.
- Dependency direction is `Web -> Application + Infrastructure`, `Infrastructure -> Application`, `Application -> Domain`; keep domain entities/enums/constants in `Domain`, DTOs/interfaces/services in `Application`, EF/Identity/email/repositories in `Infrastructure`, and MVC controllers/views/static assets in `Web`.
- Runtime wiring starts in `Web/Program.cs`: `AddApplicationServices()`, `AddInfrastructureServices()`, and `AddWebServices()` are extension methods registered in each layer.
- Controllers are conventional MVC under `Web/Controllers`; default route is `{controller=Home}/{action=Index}/{id?}` plus Razor Pages.

## Database And EF
- SQL Server is required: `Infrastructure/DependencyInjection.cs` registers both `ApplicationDbContext` and `LoggingDbContext` against `ConnectionStrings:DefaultConnection`.
- No migrations are present. `ApplicationDbContext` looks database-first/scaffolded; keep custom EF mappings for stored-procedure DTOs in `Infrastructure/Persistence/Data/ApplicationDbContext.Custom.cs` so scaffolding does not overwrite them.
- Keyless DTOs used by `FromSqlRaw` queries must be registered in `ApplicationDbContext.Custom.cs` before repositories can query them via `_context.Set<T>()`.
- Repositories call database objects such as `usp_GetOrdersDataTable` and view `View_Orders`; build can pass without those objects, but runtime analytics/order flows need a compatible database.

## App Settings And Runtime Gotchas
- Real credentials (connection strings, SMTP) go in `appsettings.Development.json` or `appsettings.Production.json` (excluded from git via `appsettings.*.json` in `.gitignore`). `appsettings.json` contains only `__CHANGE_ME__` placeholders. ASP.NET Core loads `appsettings.{Environment}.json` automatically — no custom code needed.
- Authentication cookies use `CookieSecurePolicy.Always`; use the HTTPS launch profile when checking sign-in/auth flows.
- Request localization is hard-coded to `es-MX` in `Web/Program.cs`; date/number UI behavior follows that culture.
- Static JS/CSS libraries are vendored under `Web/wwwroot/Template`; there is no npm/package manifest in this repo.

<!-- CODEGRAPH_START -->
## CodeGraph

This project has a CodeGraph MCP server (`codegraph_*` tools) configured. CodeGraph is a tree-sitter-parsed knowledge graph of every symbol, edge, and file. Reads are sub-millisecond and return structural information grep cannot.

### When to prefer codegraph over native search

Use codegraph for **structural** questions — what calls what, what would break, where is X defined, what is X's signature. Use native grep/read only for **literal text** queries (string contents, comments, log messages) or after you already have a specific file open.

| Question | Tool |
|---|---|
| "Where is X defined?" / "Find symbol named X" | `codegraph_search` |
| "What calls function Y?" | `codegraph_callers` |
| "What does Y call?" | `codegraph_callees` |
| "What would break if I changed Z?" | `codegraph_impact` |
| "Show me Y's signature / source / docstring" | `codegraph_node` |
| "Give me focused context for a task/area" | `codegraph_context` |
| "Survey an unfamiliar module/topic" | `codegraph_explore` |
| "What files exist under path/" | `codegraph_files` |
| "Is the index healthy?" | `codegraph_status` |

### Rules of thumb

- **Trust codegraph results.** They come from a full AST parse. Do NOT re-verify them with grep — that's slower, less accurate, and wastes context.
- **Don't grep first** when looking up a symbol by name. `codegraph_search` is faster and returns kind + location + signature in one call.
- **Don't chain `codegraph_search` + `codegraph_node`** when you just want context — `codegraph_context` is one call.
- **`codegraph_explore` is the heavy hitter** for unfamiliar areas — it returns full source from all relevant files in one call, but is token-heavy. If your harness supports parallel subagents (e.g., Claude Code's Task tool), spawn one for explore-class questions to keep main session context clean.
- **Index lag**: the file watcher debounces ~500ms behind writes; don't re-query immediately after editing a file in the same turn.

### If `.codegraph/` doesn't exist

The MCP server returns "not initialized." Ask the user: *"I notice this project doesn't have CodeGraph initialized. Want me to run `codegraph init -i` to build the index?"*
<!-- CODEGRAPH_END -->
