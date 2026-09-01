# InfiniteJourney — UML Class Diagram (Phase 1 Current State)

> **How to use:** Copy any section below into [draw.io](https://app.diagrams.net/), choose  
> `Extras → Edit Diagram` and paste the XML — or use the Mermaid blocks directly.  
> Each layer is a separate diagram to keep them readable.

---

## 1. Domain Layer — Full Class Diagram

```mermaid
classDiagram

    %% ─── Base Types ──────────────────────────────────────────────
    class BaseEntity {
        <<abstract>>
        +Guid Id
        +DateTimeOffset CreatedAt
        +DateTimeOffset? UpdatedAt
        +DateTimeOffset? LastModifiedAt
        +AddDomainEvent(IDomainEvent)
        +ClearDomainEvents()
    }

    class BaseTenantEntity {
        <<abstract>>
        +Guid TenantId
    }

    BaseEntity <|-- BaseTenantEntity

    %% ─── Tenant Management Context ───────────────────────────────
    class Tenant {
        +string Subdomain
        +string? CustomDomain
        +string Name
        +TenantStatus Status
    }

    class Theme {
        +Guid TenantId
        +string PrimaryColor
        +string SecondaryColor
        +string AccentColor
        +string FontFamily
        +string? LogoUrl
        +string? FaviconUrl
        +bool IsDarkMode
    }

    class ModuleActivation {
        +Guid TenantId
        +string ModuleKey
        +bool IsEnabled
        +string? ConfigJson
    }

    BaseEntity <|-- Tenant
    BaseTenantEntity <|-- Theme
    BaseTenantEntity <|-- ModuleActivation
    Tenant "1" *-- "1" Theme : has
    Tenant "1" *-- "*" ModuleActivation : configures

    %% ─── IAM Context ─────────────────────────────────────────────
    class User {
        +string KeycloakUserId
        +string Email
        +string FirstName
        +string LastName
    }

    class Membership {
        +Guid TenantId
        +Guid UserId
        +string RoleName
        +MembershipStatus Status
        +DateTimeOffset JoinedAt
        +Guid? InvitedByUserId
    }

    BaseEntity <|-- User
    BaseTenantEntity <|-- Membership
    User "1" *-- "*" Membership : holds
    Tenant "1" *-- "*" Membership : scopes

    %% ─── Campaign & Donation Context ─────────────────────────────
    class Campaign {
        +Guid TenantId
        +string Title
        +string Description
        +decimal TargetAmount
        +decimal RaisedAmount
        +CampaignStatus Status
        +string? CoverImageUrl
        +DateTimeOffset? StartDate
        +DateTimeOffset? EndDate
        +Create(tenantId, title, ...) Campaign$
        +UpdateDetails(title, ...)
        +Activate()
        +Deactivate()
        +End()
        +EnsureCanDelete()
        +RecordDonation(id, amount, email)
    }

    class Donation {
        +Guid TenantId
        +Guid CampaignId
        +decimal Amount
        +string Currency
        +string DonorEmail
        +string? DonorName
        +bool IsAnonymous
        +string? PaymentReference
        +string? PaymentGateway
        +DonationStatus Status
        +DateTimeOffset ProcessedAt
        +string? Notes
    }

    BaseTenantEntity <|-- Campaign
    BaseTenantEntity <|-- Donation
    Campaign "1" *-- "*" Donation : collects
    Tenant "1" *-- "*" Campaign : owns
    Tenant "1" *-- "*" Donation : records

    %% ─── Domain Events ────────────────────────────────────────────
    class IDomainEvent {
        <<interface>>
        +DateTimeOffset OccurredOn
    }

    class CampaignCreatedEvent {
        +Guid CampaignId
        +Guid TenantId
        +string Title
    }

    class CampaignProgressUpdatedEvent {
        +Guid CampaignId
        +Guid TenantId
        +decimal RaisedAmount
        +decimal TargetAmount
    }

    class CampaignGoalReachedEvent {
        +Guid CampaignId
        +Guid TenantId
        +decimal RaisedAmount
        +decimal TargetAmount
    }

    IDomainEvent <|.. CampaignCreatedEvent
    IDomainEvent <|.. CampaignProgressUpdatedEvent
    IDomainEvent <|.. CampaignGoalReachedEvent
    Campaign ..> CampaignCreatedEvent : publishes
    Campaign ..> CampaignProgressUpdatedEvent : publishes
    Campaign ..> CampaignGoalReachedEvent : publishes
```

---

## 2. Application Layer — CQRS + Interfaces

```mermaid
classDiagram

    %% ─── CQRS Abstractions ────────────────────────────────────────
    class ICommand~TResponse~ {
        <<interface>>
    }
    class ICommandHandler~TCommand_TResponse~ {
        <<interface>>
        +Handle(command, ct) Task~TResponse~
    }
    class IQuery~TResponse~ {
        <<interface>>
    }
    class IQueryHandler~TQuery_TResponse~ {
        <<interface>>
        +Handle(query, ct) Task~TResponse~
    }

    %% ─── Application Interfaces ───────────────────────────────────
    class IApplicationDbContext {
        <<interface>>
        +DbSet~Tenant~ Tenants
        +DbSet~Theme~ Themes
        +DbSet~ModuleActivation~ ModuleActivations
        +DbSet~User~ Users
        +DbSet~Membership~ Memberships
        +DbSet~Campaign~ Campaigns
        +DbSet~Donation~ Donations
        +SaveChangesAsync(ct) Task~int~
    }

    class ITenantContext {
        <<interface>>
        +Guid TenantId
        +string TenantName
        +string Subdomain
        +string? ConnectionString
        +bool IsResolved
        +IsFeatureEnabled(key) bool
    }

    class ICurrentUserService {
        <<interface>>
        +string? UserId
        +string? Email
        +IReadOnlyList~string~ Roles
    }

    class IFileStorageService {
        <<interface>>
        +SaveAsync(fileName, contentType, data, category, ct) Task~string~
        +DeleteAsync(path, ct) Task
    }

    %% ─── Campaign Commands ────────────────────────────────────────
    class CreateCampaignCommand {
        +string Title
        +string Description
        +decimal TargetAmount
        +string? CoverImageUrl
        +DateTimeOffset? StartDate
        +DateTimeOffset? EndDate
    }
    class CreateCampaignCommandHandler {
        -IApplicationDbContext _context
        -ITenantContext _tenantContext
        +Handle(cmd, ct) Task~CreateCampaignResultDto~
    }
    class CreateCampaignCommandValidator {
        +Rules: Title NotEmpty MaxLength(255)
        +Rules: TargetAmount GreaterThan(0)
        +Rules: EndDate gt StartDate
    }

    class UpdateCampaignCommand {
        +Guid CampaignId
        +string Title
        +string Description
        +decimal TargetAmount
        +string? CoverImageUrl
        +DateTimeOffset? StartDate
        +DateTimeOffset? EndDate
    }
    class UpdateCampaignCommandHandler {
        -IApplicationDbContext _context
        +Handle(cmd, ct) Task~CampaignDetailDto~
    }

    class DeleteCampaignCommand {
        +Guid CampaignId
    }
    class DeleteCampaignCommandHandler {
        -IApplicationDbContext _context
        -IFileStorageService _fileStorage
        +Handle(cmd, ct) Task~bool~
    }

    class ActivateCampaignCommand {
        +Guid CampaignId
    }
    class ActivateCampaignCommandHandler {
        -IApplicationDbContext _context
        +Handle(cmd, ct) Task~CampaignDetailDto~
    }

    %% ─── Campaign Queries ─────────────────────────────────────────
    class GetCampaignsQuery {
        +CampaignStatus? Status
        +int PageIndex
        +int PageSize
        +string? Search
        +string? SortBy
        +string? SortDirection
    }
    class GetCampaignsQueryHandler {
        -IApplicationDbContext _context
        +Handle(qry, ct) Task~PagedResult~CampaignListItemDto~~
    }

    class GetCampaignByIdQuery {
        +Guid CampaignId
    }
    class GetCampaignByIdQueryHandler {
        -IApplicationDbContext _context
        +Handle(qry, ct) Task~CampaignDetailDto?~
    }

    %% ─── Pipeline Behaviors ───────────────────────────────────────
    class ValidationBehavior~TRequest_TResponse~ {
        -IEnumerable~IValidator~ _validators
        +Handle(req, next, ct) Task~TResponse~
    }

    %% ─── Exceptions ───────────────────────────────────────────────
    class AppException {
        <<abstract>>
        +int StatusCode
        +string ErrorCode
    }
    class NotFoundException { StatusCode=404 }
    class BusinessRuleException { StatusCode=409 }
    class ConflictException { StatusCode=409 }
    class ForbiddenAppException { StatusCode=403 }
    class TenantViolationException { StatusCode=403 }

    AppException <|-- NotFoundException
    AppException <|-- BusinessRuleException
    AppException <|-- ConflictException
    AppException <|-- ForbiddenAppException
    AppException <|-- TenantViolationException

    %% ─── DTOs ─────────────────────────────────────────────────────
    class CampaignListItemDto {
        +Guid Id
        +string Title
        +string Description
        +decimal TargetAmount
        +decimal RaisedAmount
        +CampaignStatus Status
        +string? CoverImageUrl
        +DateTimeOffset? StartDate
        +DateTimeOffset? EndDate
    }
    class CampaignDetailDto {
        +Guid Id
        +string Title
        +string Description
        +decimal TargetAmount
        +decimal RaisedAmount
        +CampaignStatus Status
        +string? CoverImageUrl
        +DateTimeOffset? StartDate
        +DateTimeOffset? EndDate
        +DateTimeOffset CreatedAt
        +decimal ProgressPercent
    }
    class CreateCampaignResultDto {
        +Guid Id
    }

    %% ─── Grid / Pagination ────────────────────────────────────────
    class GridQuery {
        +int PageIndex
        +int PageSize
        +string? Search
        +string? SortBy
        +string? SortDirection
        +bool IsDescending
    }
    class PagedResult~T~ {
        +IReadOnlyList~T~ Data
        +int PageIndex
        +int PageSize
        +int Total
        +int TotalPages
        +Create(data, total, grid)$
        +Map~TDto~(selector) PagedResult~TDto~
    }

    ICommand~TResponse~ <|.. CreateCampaignCommand
    ICommand~TResponse~ <|.. UpdateCampaignCommand
    ICommand~TResponse~ <|.. DeleteCampaignCommand
    ICommand~TResponse~ <|.. ActivateCampaignCommand
    IQuery~TResponse~ <|.. GetCampaignsQuery
    IQuery~TResponse~ <|.. GetCampaignByIdQuery
    GetCampaignsQuery --|> GridQuery

    ICommandHandler~TCommand_TResponse~ <|.. CreateCampaignCommandHandler
    ICommandHandler~TCommand_TResponse~ <|.. UpdateCampaignCommandHandler
    ICommandHandler~TCommand_TResponse~ <|.. DeleteCampaignCommandHandler
    ICommandHandler~TCommand_TResponse~ <|.. ActivateCampaignCommandHandler
    IQueryHandler~TQuery_TResponse~ <|.. GetCampaignsQueryHandler
    IQueryHandler~TQuery_TResponse~ <|.. GetCampaignByIdQueryHandler

    CreateCampaignCommandHandler ..> IApplicationDbContext : uses
    CreateCampaignCommandHandler ..> ITenantContext : uses
    UpdateCampaignCommandHandler ..> IApplicationDbContext : uses
    DeleteCampaignCommandHandler ..> IApplicationDbContext : uses
    DeleteCampaignCommandHandler ..> IFileStorageService : uses
    ActivateCampaignCommandHandler ..> IApplicationDbContext : uses
    GetCampaignsQueryHandler ..> IApplicationDbContext : uses
    GetCampaignByIdQueryHandler ..> IApplicationDbContext : uses

    GetCampaignsQueryHandler ..> PagedResult~T~ : returns
    GetCampaignsQueryHandler ..> CampaignListItemDto : projects
    GetCampaignByIdQueryHandler ..> CampaignDetailDto : returns
    CreateCampaignCommandHandler ..> CreateCampaignResultDto : returns
    UpdateCampaignCommandHandler ..> CampaignDetailDto : returns
    ActivateCampaignCommandHandler ..> CampaignDetailDto : returns
```

---

## 3. Infrastructure Layer

```mermaid
classDiagram

    class IApplicationDbContext {
        <<interface>>
    }

    class ApplicationDbContext {
        -ITenantContext _tenantContext
        +DbSet~Tenant~ Tenants
        +DbSet~Campaign~ Campaigns
        +DbSet~Donation~ Donations
        +DbSet~User~ Users
        +DbSet~Membership~ Memberships
        +OnModelCreating(builder)
        +SaveChangesAsync(ct) Task~int~
    }

    class TenantSaveChangesInterceptor {
        -ITenantContext _tenantContext
        +SavingChangesAsync(event, result, ct)
    }

    class TenantContext {
        -Guid _tenantId
        -string _tenantName
        +bool IsResolved
        +SetTenant(id, name, subdomain, ...)
    }

    class ITenantContext {
        <<interface>>
        +Guid TenantId
        +bool IsResolved
        +IsFeatureEnabled(key) bool
    }

    class TenantResolver {
        -IApplicationDbContext _context
        +ResolveAsync(host, ct) Task~TenantResolution?~
    }

    class ITenantResolver {
        <<interface>>
        +ResolveAsync(host, ct) Task~TenantResolution?~
    }

    class CurrentUserService {
        -IHttpContextAccessor _accessor
        +string? UserId
        +string? Email
        +IReadOnlyList~string~ Roles
    }

    class ICurrentUserService {
        <<interface>>
    }

    class LocalFileStorageService {
        +string RootPath
        +SaveAsync(fileName, type, data, category, ct) Task~string~
        +DeleteAsync(path, ct) Task
    }

    class IFileStorageService {
        <<interface>>
    }

    class DatabaseInitializer {
        +InitializeAsync(services, isDev)$
    }

    IApplicationDbContext <|.. ApplicationDbContext
    ITenantContext <|.. TenantContext
    ITenantResolver <|.. TenantResolver
    ICurrentUserService <|.. CurrentUserService
    IFileStorageService <|.. LocalFileStorageService

    ApplicationDbContext ..> TenantSaveChangesInterceptor : uses
    ApplicationDbContext ..> ITenantContext : uses
    TenantSaveChangesInterceptor ..> ITenantContext : uses
    TenantResolver ..> IApplicationDbContext : uses
```

---

## 4. Web Layer — Controllers + Middleware

```mermaid
classDiagram

    class ApiControllerBase {
        <<abstract>>
        #ISender Mediator
        +SendAsync~TResponse~(request, ct) Task~IActionResult~
        +SendOrNotFoundAsync~TResponse~(request, ct) Task~IActionResult~
        +SendCreatedAsync~TResponse~(request, ct, factory) Task~IActionResult~
    }

    class CampaignsController {
        +GetAll(query, ct) Task~IActionResult~
        +GetById(id, ct) Task~IActionResult~
        +Create(command, ct) Task~IActionResult~
        +Update(id, command, ct) Task~IActionResult~
        +Delete(id, ct) Task~IActionResult~
        +Activate(id, ct) Task~IActionResult~
    }

    class FilesController {
        +Upload(command, ct) Task~IActionResult~
    }

    class TenantResolutionMiddleware {
        -HashSet~string~ _excludedPaths
        -HashSet~string~ _bypassHosts
        +InvokeAsync(ctx, resolver, tenantCtx) Task
    }

    class GlobalExceptionHandler {
        +TryHandleAsync(ctx, ex, ct) Task~bool~
    }

    class RequireModuleAttribute {
        +string ModuleKey
    }

    ApiControllerBase <|-- CampaignsController
    ApiControllerBase <|-- FilesController
    CampaignsController ..> RequireModuleAttribute : decorated
    CampaignsController ..> GetCampaignsQuery : dispatches
    CampaignsController ..> GetCampaignByIdQuery : dispatches
    CampaignsController ..> CreateCampaignCommand : dispatches
    CampaignsController ..> UpdateCampaignCommand : dispatches
    CampaignsController ..> DeleteCampaignCommand : dispatches
    CampaignsController ..> ActivateCampaignCommand : dispatches
```

---

## 5. Full Layer Dependency Map (Simplified)

```mermaid
classDiagram
    class DomainLayer {
        <<layer>>
        BaseEntity · BaseTenantEntity
        Campaign · Donation
        Tenant · Theme · ModuleActivation
        User · Membership
        IDomainEvent · Domain Events
    }

    class ApplicationLayer {
        <<layer>>
        ICommand · IQuery
        ICommandHandler · IQueryHandler
        IApplicationDbContext · ITenantContext
        ICurrentUserService · IFileStorageService
        Commands · Queries · DTOs
        PagedResult · GridQuery
        AppException hierarchy
        ValidationBehavior
    }

    class InfrastructureLayer {
        <<layer>>
        ApplicationDbContext
        TenantContext · TenantResolver
        CurrentUserService
        LocalFileStorageService
        TenantSaveChangesInterceptor
        EF Migrations · Configurations
        Keycloak JWT setup
    }

    class WebLayer {
        <<layer>>
        ApiControllerBase
        CampaignsController · FilesController
        TenantResolutionMiddleware
        GlobalExceptionHandler
        Program.cs (DI wiring)
    }

    class AppShared {
        <<layer>>
        ApiRoutes
        Enums: CampaignStatus
        Enums: DonationStatus · TenantStatus
        Enums: MembershipStatus
    }

    DomainLayer <.. ApplicationLayer : depends on
    ApplicationLayer <.. InfrastructureLayer : implements
    ApplicationLayer <.. WebLayer : dispatches to
    InfrastructureLayer <.. WebLayer : registered in
    AppShared <.. DomainLayer : uses enums
    AppShared <.. WebLayer : uses routes
```

---

## 6. Enums Reference

```mermaid
classDiagram
    class TenantStatus {
        <<enumeration>>
        Provisioning
        Pending
        Active
        Suspended
        Terminated
    }
    class MembershipStatus {
        <<enumeration>>
        Invited
        Pending
        Active
        Suspended
        Removed
    }
    class CampaignStatus {
        <<enumeration>>
        Draft
        Active
        Suspended
        Ended
        Archived
    }
    class DonationStatus {
        <<enumeration>>
        Pending
        Completed
        Failed
        Refunded
    }
    class PledgeStatus {
        <<enumeration>>
        Active
        Paused
        Cancelled
    }
```

---

## Notes for draw.io Import

1. Go to [app.diagrams.net](https://app.diagrams.net/)
2. `Extras → Edit Diagram`
3. Paste any Mermaid block above
4. Select **Mermaid** as the diagram type
5. Each section (1–6) is a separate diagram — import them individually for best readability

> **Last updated:** Phase 1 complete — Campaign domain, full CQRS pipeline, Infrastructure, Web layer.  
> **Next update:** After Donation module (T9) is complete.
