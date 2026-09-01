# InfiniteJourney — Implementation Change Log

> **Purpose:** Tracks every structural, architectural, and code-quality change made  
> during active development sessions. Ordered newest-first.  
> For the full roadmap → `implementation_plan.md`  
> For architecture decisions → `docs/ARCHITECTURE.md`  
> For class diagrams → `docs/diagram.md`

---

## Session — Phase 1 Refinement (Current)

### 1. UML Class Diagram (`docs/diagram.md`)

**Added:** `docs/diagram.md` — full Mermaid class diagrams for all four layers.

Covers:
- Domain layer: `BaseEntity`, `BaseTenantEntity`, all aggregate roots, domain events
- Application layer: CQRS abstractions, all campaign commands/queries, DTOs, grid models, exception hierarchy
- Infrastructure layer: `ApplicationDbContext`, interceptor, tenant/user/file services
- Web layer: `ApiControllerBase`, controllers, middleware
- Enum reference
- Full layer dependency map

Import each section into [draw.io](https://app.diagrams.net/) via `Extras → Edit Diagram`.

---

### 2. Campaign Feature — Structural Cleanup

#### `CampaignModels.cs` — DTOs and Mappings merged into one file

**Before:** Separate `Dtos/CampaignDtos.cs` and `Mappings/CampaignMappings.cs` folders.  
**After:** Single `Campaigns/Dtos/CampaignModels.cs` — all DTOs + static mapping extensions together.

```
Campaigns/
├── CampaignModels.cs          ← DTOs + CampaignMappings (one file, one scroll)
├── Commands/
│   ├── Index.cs               ← Pure application contracts only
│   ├── CreateCampaignCommandHandler.cs   ← Handler + Validator together
│   ├── UpdateCampaignCommandHandler.cs   ← Handler + Validator together
│   ├── DeleteCampaignCommandHandler.cs
│   └── ActivateCampaignCommandHandler.cs
└── Queries/
    ├── Index.cs
    ├── GetCampaignsQueryHandler.cs
    └── GetCampaignByIdQueryHandler.cs
```

**Why:** When a developer opens a handler file they see both the handler and its validator — no folder jumping. When they open `CampaignModels.cs` they see both the DTO shape and exactly how it maps from the domain entity.

---

#### `UpdateCampaignCommand` — no separate body DTO

**Before:**
```csharp
// Separate record mirroring all fields
public sealed record UpdateCampaignBody(string Title, ...);

// Controller spreading every field manually
new UpdateCampaignCommand(id, body.Title, body.Description, body.TargetAmount, ...)
```

**After:**
```csharp
// Command IS the body — bound directly from JSON
// Route id attached with one C# record expression
public Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignCommand command, ...)
    => SendAsync(command with { CampaignId = id }, cancellationToken);
```

**Rule going forward:** If a command's data is entirely from the request body, bind it directly as the command. If one value comes from the route (like `id`), use `with { }`. Never spread fields manually.

---

#### `Commands/Index.cs` — pure application contracts

`UpdateCampaignBody` removed from here. Application layer has no knowledge of HTTP body shapes. The controller is the transport layer — web input models live there.

---

### 3. Exception Hierarchy — `ConflictException` added

**File:** `Application/Common/Exceptions/AppExceptions.cs`

```
AppException (abstract)
├── NotFoundException          → 404 NOT_FOUND
├── BusinessRuleException      → 409 BUSINESS_RULE_VIOLATION
├── ConflictException          → 409 CONFLICT          ← NEW
├── ForbiddenAppException      → 403 FORBIDDEN
└── TenantViolationException   → 403 TENANT_VIOLATION
```

`ConflictException` is for resource **state conflicts** (e.g. deleting a campaign with donations).  
`BusinessRuleException` remains for domain rule violations (e.g. editing a completed record).  
The client can distinguish them by `errorCode`.

`DeleteCampaignCommandHandler` now throws `ConflictException` directly — no `try/catch InvalidOperationException` wrapper.

---

### 4. `ApiControllerBase` — `Mediator` visibility fixed

**Before:** `private ISender Mediator` — subclasses couldn't call it directly.  
**After:** `protected ISender Mediator` — correct visibility for a shared base-class helper.

---

### 5. `TenantResolutionMiddleware` — dev bypass + precise host handling

**Added:** `MultiTenancy:BypassHosts` config key.  
**`appsettings.Development.json`:** `"BypassHosts": ["localhost", "127.0.0.1"]`

Plain `localhost` now bypasses tenant resolution in dev — Swagger and health checks work without a subdomain. Production is unaffected.

**Also fixed:** `Host.Host` (hostname only) vs `Host.Value` (hostname:port) — now separated correctly so subdomain matching works with and without explicit port numbers.

---

### 6. `Program.cs` — HTTPS redirect guard + enum-as-string

**HTTPS redirect:** `UseHttpsRedirection()` is now conditional — only activates when not in Development or when an HTTPS URL is actually bound. Eliminated the noisy `warn: Failed to determine the https port` log.

**Enum serialization:**
```csharp
.AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
```
All enums now serialize as `"Active"` not `1`. Frontend receives `status: "Active"` — no integer guessing.

---

### 7. `PagedResult<T>` — enhanced

Added:
- `TotalPages` computed property — frontend doesn't need to calculate it
- `static Create(data, total, grid)` factory — no scattered object initializers  
- `Map<TDto>(selector)` — project to a different DTO type without rebuilding

---

### 8. `QueryableGridExtensions` — single-pass projection overload

**Added:** `ToPagedResultAsync<TSource, TDto>(query, grid, selector, ct)`

**Before (double-pass):**
```csharp
var paged = await query.ToPagedResultAsync<Campaign>(grid, ct);
return paged.Map(c => c.ToListItemDto()); // second list allocation
```

**After (single-pass):**
```csharp
return await query.ToPagedResultAsync(grid, c => c.ToListItemDto(), ct);
```

One count query + one fetch + one in-memory projection. No intermediate list.

**Also added:** `ApplySearch<T>` — case-insensitive OR search across specified string fields using expression trees.  
**Also added:** `ApplySort<T>` — sort from a static `Dictionary<string, Expression>` map with a default fallback.

---

### 9. `GetCampaignsQueryHandler` — static sort map

```csharp
private static readonly Dictionary<string, Expression<Func<Campaign, object>>> SortMap = new(...) { ... };
```

Allocated once at app startup, not on every request.

---

## Architecture Principles Established (Reference for All Future Modules)

| Concern | Decision |
|---|---|
| DTOs + Mappings | Co-located in one `FeatureModels.cs` per feature |
| Validators | Inside the same `.cs` file as their handler |
| Command contracts | `Commands/Index.cs` — application contracts only, no HTTP shapes |
| Controller binding | Body binds directly as command; route id via `with { }` |
| No fat service | Each handler injects only what it needs (`IApplicationDbContext`, `ITenantContext`, etc.) |
| Grid pattern | `GridQuery` → `ApplySearch` → `ApplySort` → `ToPagedResultAsync<TSource, TDto>` |
| Exception HTTP mapping | `AppException` subclass → `GlobalExceptionHandler` → RFC 7807 `ProblemDetails` |
| Enum wire format | Always string (`JsonStringEnumConverter`) |
| Dev bypass | `MultiTenancy:BypassHosts` in `appsettings.Development.json` |

---

## Files Changed This Session

| File | Change |
|---|---|
| `docs/diagram.md` | **Created** — full UML class diagrams, all layers |
| `docs/ARCHITECTURE.md` | Updated solution structure tree + controller pattern |
| `Application/Campaigns/Dtos/CampaignModels.cs` | Merged DTOs + mappings into one file |
| `Application/Campaigns/Commands/Index.cs` | Removed `UpdateCampaignBody`; pure contracts only |
| `Application/Campaigns/Commands/CreateCampaignCommandHandler.cs` | Validator co-located |
| `Application/Campaigns/Commands/UpdateCampaignCommandHandler.cs` | Validator co-located |
| `Application/Campaigns/Commands/DeleteCampaignCommandHandler.cs` | Uses `ConflictException` directly |
| `Application/Campaigns/Queries/GetCampaignsQueryHandler.cs` | Static sort map; single-pass projection |
| `Application/Common/Exceptions/AppExceptions.cs` | `ConflictException` added |
| `Application/Common/Models/PagedResult.cs` | `TotalPages`, `Create`, `Map` added |
| `Application/Common/Extensions/QueryableGridExtensions.cs` | Single-pass overload, `ApplySearch`, `ApplySort` |
| `Web/Controllers/ApiControllerBase.cs` | `Mediator` private → protected |
| `Web/Controllers/CampaignsController.cs` | Direct body binding; `UpdateCampaignBody` here |
| `Web/Middleware/TenantResolutionMiddleware.cs` | `BypassHosts`; precise host/port handling |
| `Web/Program.cs` | Conditional HTTPS redirect; `JsonStringEnumConverter` |
| `appsettings.Development.json` | `MultiTenancy:BypassHosts` added |
