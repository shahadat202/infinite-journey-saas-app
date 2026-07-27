# Domain Models & Entity Glossary

> **Living document.** Updated as each bounded context is implemented.
> Cross-reference: [ARCHITECTURE.md](../ARCHITECTURE.md) §5 (DDD Map) and §8 (Database Design).

---

## Table of Contents

1. [Base Types & Conventions](#1-base-types--conventions)
2. [Tenant Management Context](#2-tenant-management-context)
3. [Identity & Access Management Context](#3-identity--access-management-context)
4. [Campaign & Donation Context](#4-campaign--donation-context)
5. [Event & Volunteer Context](#5-event--volunteer-context) *(Phase 2)*
6. [Website Builder & Content Context](#6-website-builder--content-context) *(Phase 2)*
7. [Entity Classification Matrix](#7-entity-classification-matrix)
8. [Enum Reference](#8-enum-reference)

---

## 1. Base Types & Conventions

All domain entities share a common base type hierarchy:

```csharp
// Domain layer — InfiniteJourney.Domain/Common/BaseEntity.cs

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; protected set; }
}

public abstract class BaseTenantEntity : BaseEntity
{
    // EF Core Global Query Filter is registered against this property.
    // TenantSaveChangesInterceptor auto-stamps this on every insert.
    public Guid TenantId { get; set; }
}
```

### Conventions

| Convention | Rule |
|---|---|
| Primary keys | All `Guid` — never int/auto-increment |
| Timestamps | All `DateTimeOffset` (UTC) — never `DateTime` |
| Tenant scope | Every tenant-owned entity inherits `BaseTenantEntity` |
| Cross-tenant | Platform-level entities (e.g. `User`) inherit `BaseEntity` only |
| Enums | Stored as `VARCHAR` in PostgreSQL — never raw integer values |
| Currency | Stored as `decimal(18,2)` with an explicit `Currency` ISO code column |
| Soft delete | Not used by default — hard delete with audit log events |

---

## 2. Tenant Management Context

This context is the **root of the entire platform**. It owns the organizational boundary and everything that configures a tenant's identity on the system.

---

### Tenant *(Aggregate Root)*

**Purpose:** Represents a registered organization on the platform. It is the primary data isolation boundary — every piece of tenant-owned data traces back to a `TenantId`.

**Inherits:** `BaseEntity` (no TenantId on itself — it *is* the tenant)

| Property | Type | Description |
|---|---|---|
| `Id` | `Guid` | Platform-unique identifier |
| `Subdomain` | `string` | URL routing key (e.g. `hope`, `relief`) — globally unique |
| `CustomDomain` | `string?` | Optional external domain (e.g. `hopefoundation.org`) |
| `Name` | `string` | Display name of the organization |
| `Status` | `TenantStatus` | Lifecycle state of the tenant |
| `CreatedAt` | `DateTimeOffset` | UTC timestamp of registration |

**Lifecycle States (`TenantStatus`):**

```
Provisioning → Pending → Active → Suspended → Terminated
```

| State | Meaning |
|---|---|
| `Provisioning` | System is setting up DB, Keycloak, seed data |
| `Pending` | Setup complete; awaiting admin confirmation |
| `Active` | Fully operational |
| `Suspended` | Temporarily blocked (e.g. payment failure) |
| `Terminated` | Permanently deactivated; data retained for compliance |

**Domain Events:**
- `TenantProvisionedEvent` — fires when tenant transitions to `Pending`
- `TenantActivatedEvent` — fires on first `Active` transition
- `TenantSuspendedEvent` — fires on suspension
- `TenantPlanUpgradedEvent` — fires on subscription tier change

---

### Theme *(Entity — 1:1 with Tenant)*

**Purpose:** Holds the complete brand identity configuration for a tenant. Applied at runtime via CSS variable injection on the Angular frontend.

**Inherits:** `BaseTenantEntity`

| Property | Type | Description |
|---|---|---|
| `TenantId` | `Guid` | FK → Tenant (1:1) |
| `PrimaryColor` | `string` | Hex code (e.g. `#1E3A8A`) — main brand color |
| `SecondaryColor` | `string` | Hex code — supporting color |
| `AccentColor` | `string` | Hex code — highlight / CTA color |
| `FontFamily` | `string` | CSS font stack (e.g. `Inter, sans-serif`) |
| `LogoUrl` | `string?` | CDN URL for tenant logo |
| `FaviconUrl` | `string?` | CDN URL for browser favicon |
| `IsDarkMode` | `bool` | Default color scheme preference |

**Notes:**
- WCAG AA contrast is enforced at runtime on the frontend when computing text overlay colors.
- Changing Theme for Tenant A has zero impact on any other tenant.

---

### ModuleActivation *(Entity)*

**Purpose:** Tracks which modules (Campaigns, Donations, Events, etc.) are enabled for a specific tenant. Acts as the platform's feature toggle registry per tenant.

**Inherits:** `BaseTenantEntity`

| Property | Type | Description |
|---|---|---|
| `TenantId` | `Guid` | FK → Tenant |
| `ModuleKey` | `string` | Module identifier (e.g. `"Campaigns"`, `"Events"`) |
| `IsEnabled` | `bool` | Toggle state |
| `ConfigJson` | `string?` | Optional JSON config specific to this module activation |

**Behavior:**
- Disabling a module hides the UI and blocks the API endpoint (returns `403`).
- It **never** deletes underlying data — data is preserved for re-activation.
- A `UNIQUE (TenantId, ModuleKey)` constraint prevents duplicate activations.

---

### SubscriptionPlan *(Aggregate Root — Platform Level)* *(Phase 3)*

**Purpose:** Defines the SaaS pricing tier that governs which modules a tenant can access and at what resource limits.

| Property | Type | Description |
|---|---|---|
| `Id` | `Guid` | Plan identifier |
| `Name` | `string` | e.g. `Basic`, `Pro`, `Enterprise` |
| `AllowedModules` | `string[]` | Module keys included in this plan |
| `MaxAdminUsers` | `int` | Cap on tenant staff accounts |
| `HasCustomDomain` | `bool` | Custom domain mapping allowed |
| `HasDedicatedDb` | `bool` | Enterprise: dedicated PostgreSQL instance |
| `TransactionFeePercent` | `decimal` | Platform cut on donations (0.5%–2%) |

---

## 3. Identity & Access Management Context

This context manages who people are on the platform and what they are allowed to do — both at the platform level and within each tenant.

---

### User *(Aggregate Root — Platform Level)*

**Purpose:** Represents a registered individual on the platform. Keycloak owns the credential and authentication lifecycle. The local `User` entity stores profile metadata and acts as the FK target for all membership records.

**Inherits:** `BaseEntity` (not tenant-scoped — a user can belong to multiple tenants)

| Property | Type | Description |
|---|---|---|
| `Id` | `Guid` | Local platform identifier |
| `KeycloakUserId` | `string` | Keycloak `sub` claim — globally unique, indexed |
| `Email` | `string` | Primary contact email |
| `FirstName` | `string` | Given name |
| `LastName` | `string` | Family name |
| `CreatedAt` | `DateTimeOffset` | UTC registration timestamp |

**Key Design Decision:** The `User` row is created the first time a Keycloak-authenticated user makes a request to the backend (lazy provisioning via `UserRegisteredEvent`). No pre-registration step is required.

**Domain Events:**
- `UserRegisteredEvent` — fires on first login / profile creation
- `UserProfileUpdatedEvent` — fires on email or name change

---

### Membership *(Entity — Tenant-Scoped)*

**Purpose:** Maps a `User` to a `Tenant` with a specific role, tracking the full lifecycle of their relationship with that organization. A user can hold memberships across multiple tenants simultaneously.

**Inherits:** `BaseTenantEntity`

| Property | Type | Description |
|---|---|---|
| `TenantId` | `Guid` | FK → Tenant (scope) |
| `UserId` | `Guid` | FK → User |
| `RoleName` | `string` | Scoped authorization role (see Role Hierarchy below) |
| `Status` | `MembershipStatus` | Current lifecycle state |
| `JoinedAt` | `DateTimeOffset` | UTC timestamp when membership became active |
| `InvitedByUserId` | `Guid?` | Optional — who invited this member |

**Constraints:** `UNIQUE (TenantId, UserId)` — one membership record per user per tenant.

**Lifecycle States (`MembershipStatus`):**

| State | Meaning |
|---|---|
| `Invited` | Email invitation sent; not yet accepted |
| `Pending` | Self-registered; awaiting admin approval |
| `Active` | Full access per assigned role |
| `Suspended` | Temporarily blocked by admin |
| `Removed` | Permanently removed; record retained for audit |

**Role Name Values (Tenant Level):**

| Role | Description |
|---|---|
| `OrganizationOwner` | Full control including billing — typically the founding admin |
| `OrganizationAdmin` | User/role management, module config, full data access |
| `Staff` | Day-to-day operations access |
| `VolunteerCoordinator` | Volunteer and event management |
| `ContentManager` | Blog, pages, media management |
| `FinanceManager` | Donation reports, financial data access |
| `Member` | Registered member of the community (public-facing role) |
| `Volunteer` | Active volunteer with shift tracking |
| `Donor` | Identified donor with giving history |

**Domain Events:**
- `MembershipAssignedEvent` — fires when a membership becomes `Active`
- `MembershipSuspendedEvent` — fires on suspension
- `MembershipRoleChangedEvent` — fires on role update

---

## 4. Campaign & Donation Context

The **reference business module** for Phase 1. Validates the full multi-tenant isolation pipeline end-to-end.

---

### Campaign *(Aggregate Root)*

**Purpose:** A fundraising goal created by a tenant. It acts as the financial container that collects donations, tracks progress toward a target, and has a defined lifecycle.

**Inherits:** `BaseTenantEntity`

| Property | Type | Description |
|---|---|---|
| `TenantId` | `Guid` | Scope — which organization owns this campaign |
| `Title` | `string` | Public-facing name (e.g. `"Clean Water for Syria"`) |
| `Description` | `string` | Full narrative / story text |
| `CoverImageUrl` | `string?` | CDN URL for campaign hero image |
| `TargetAmount` | `decimal` | Monetary goal |
| `RaisedAmount` | `decimal` | Running total — updated by domain events from `Donation` |
| `Currency` | `string` | ISO 4217 code (e.g. `"GBP"`, `"USD"`) |
| `Status` | `CampaignStatus` | Current lifecycle state |
| `StartDate` | `DateTimeOffset?` | Optional scheduled start |
| `EndDate` | `DateTimeOffset?` | Optional deadline |
| `CreatedByUserId` | `Guid` | FK → User (staff who created it) |
| `CreatedAt` | `DateTimeOffset` | UTC creation timestamp |

**Lifecycle States (`CampaignStatus`):**

| State | Meaning |
|---|---|
| `Draft` | Created but not yet published — invisible to donors |
| `Active` | Live — accepting donations, visible on tenant portal |
| `Suspended` | Temporarily paused by admin |
| `Ended` | Completed — no longer accepting donations |
| `Archived` | Historical — accessible in reports but hidden from portal |

**Business Rules:**
- `RaisedAmount` is never directly set — it is recalculated by handling `DonationReceivedEvent`.
- A `Draft` campaign cannot receive donations.
- `TargetAmount` must be greater than zero.
- `EndDate` (if set) must be after `StartDate`.

**Domain Events:**
- `CampaignActivatedEvent` — fires when status transitions to `Active`
- `DonationReceivedEvent` — handled here to increment `RaisedAmount`
- `CampaignGoalReachedEvent` — fires when `RaisedAmount >= TargetAmount`
- `CampaignEndedEvent` — fires on status transition to `Ended`

---

### Donation *(Aggregate Root)*

**Purpose:** A monetary transaction contributed by a donor toward a specific campaign. Each donation is immutable once completed — the financial audit trail must never be altered.

**Inherits:** `BaseTenantEntity`

| Property | Type | Description |
|---|---|---|
| `TenantId` | `Guid` | Scope — which tenant this donation belongs to |
| `CampaignId` | `Guid` | FK → Campaign |
| `Amount` | `decimal` | Gross donation amount |
| `Currency` | `string` | ISO 4217 code (matches Campaign currency) |
| `DonorEmail` | `string` | Donor contact — used for receipts and tracking |
| `DonorName` | `string?` | Optional — may be anonymous |
| `IsAnonymous` | `bool` | When `true`, hide donor identity on public displays |
| `PaymentReference` | `string?` | External payment gateway transaction ID |
| `PaymentGateway` | `string?` | e.g. `"Stripe"`, `"PayPal"` |
| `Status` | `DonationStatus` | Current transaction state |
| `ProcessedAt` | `DateTimeOffset` | UTC timestamp of final status change |
| `Notes` | `string?` | Optional donor message |

**Lifecycle States (`DonationStatus`):**

| State | Meaning |
|---|---|
| `Pending` | Payment initiated, awaiting gateway confirmation |
| `Completed` | Payment confirmed — triggers `DonationReceivedEvent` |
| `Failed` | Payment declined or errored |
| `Refunded` | Amount returned to donor |

**Business Rules:**
- `Amount` must be greater than zero.
- Once `Completed`, status can only move to `Refunded` — never back to `Pending`.
- A `DonationReceivedEvent` is published only on transition to `Completed`.
- `RaisedAmount` on the parent `Campaign` is updated by handling this event.

**Domain Events:**
- `DonationReceivedEvent` — fires on `Completed` status; consumed by `Campaign`
- `DonationRefundedEvent` — fires on `Refunded` status; decrements `Campaign.RaisedAmount`

---

### RecurringPledge *(Entity — Phase 2)*

**Purpose:** A scheduled, repeating donation commitment from a donor (e.g. monthly £10 to a campaign).

| Property | Type | Description |
|---|---|---|
| `TenantId` | `Guid` | Scope |
| `CampaignId` | `Guid` | FK → Campaign |
| `DonorEmail` | `string` | Pledge holder |
| `Amount` | `decimal` | Per-cycle amount |
| `Currency` | `string` | ISO currency code |
| `FrequencyDays` | `int` | Cycle length (e.g. `30` = monthly) |
| `NextChargeDate` | `DateTimeOffset` | Scheduled next charge |
| `Status` | `PledgeStatus` | `Active`, `Paused`, `Cancelled` |
| `PaymentMethodToken` | `string` | Vault token from payment gateway |

---

## 5. Event & Volunteer Context *(Phase 2)*

---

### Event *(Aggregate Root)*

**Purpose:** A community gathering (physical or virtual) organized by a tenant — fundraisers, workshops, cleanups, prayer circles, etc.

**Inherits:** `BaseTenantEntity`

| Property | Type | Description |
|---|---|---|
| `TenantId` | `Guid` | Scope |
| `Title` | `string` | Event name |
| `Description` | `string` | Full event details |
| `Location` | `GeographicLocation` | Value object: address + lat/lng |
| `IsVirtual` | `bool` | Online-only event flag |
| `VirtualLink` | `string?` | Meeting URL for virtual events |
| `StartDateTime` | `DateTimeOffset` | Event start (UTC) |
| `EndDateTime` | `DateTimeOffset` | Event end (UTC) |
| `MaxAttendees` | `int?` | Capacity limit (null = unlimited) |
| `Status` | `EventStatus` | `Draft`, `Published`, `Cancelled`, `Completed` |
| `CoverImageUrl` | `string?` | Hero image CDN URL |

**Domain Events:**
- `EventPublishedEvent`
- `VolunteerShiftAssignedEvent`
- `EventCancelledEvent`

---

### VolunteerApplication *(Aggregate Root)*

**Purpose:** Tracks a volunteer's application and assignment to an event or shift.

**Inherits:** `BaseTenantEntity`

| Property | Type | Description |
|---|---|---|
| `TenantId` | `Guid` | Scope |
| `EventId` | `Guid` | FK → Event |
| `UserId` | `Guid` | FK → User |
| `ShiftId` | `Guid?` | FK → Shift (if assigned to specific shift) |
| `Status` | `VolunteerStatus` | `Applied`, `Approved`, `Rejected`, `Attended`, `NoShow` |
| `AppliedAt` | `DateTimeOffset` | UTC timestamp |
| `HoursLogged` | `decimal?` | Actual hours tracked on completion |

---

### Shift *(Entity)*

**Purpose:** A time-boxed segment of work within an event (e.g. "Setup crew 08:00–10:00").

| Property | Type | Description |
|---|---|---|
| `EventId` | `Guid` | FK → Event |
| `Name` | `string` | Shift label |
| `StartTime` | `DateTimeOffset` | |
| `EndTime` | `DateTimeOffset` | |
| `MaxVolunteers` | `int` | Capacity limit |

---

## 6. Website Builder & Content Context *(Phase 2)*

---

### Page *(Aggregate Root)*

**Purpose:** A dynamically configured web page for a tenant's public portal. Layout is driven by a JSON block array — no code changes needed to add, reorder, or remove sections.

**Inherits:** `BaseTenantEntity`

| Property | Type | Description |
|---|---|---|
| `TenantId` | `Guid` | Scope |
| `Slug` | `string` | URL path (e.g. `about-us`, `home`) — unique per tenant |
| `Title` | `string` | Page name (for admin UI) |
| `SeoSettings` | `SeoSettings` | Value object: meta title, description, OG image |
| `Layout` | `PageBlock[]` | Ordered list of block configurations |
| `IsPublished` | `bool` | Visibility on tenant portal |
| `PublishedAt` | `DateTimeOffset?` | UTC timestamp of last publish |

**`PageBlock` Value Object:**

```csharp
public record PageBlock(string Type, Dictionary<string, object> Params);

// Example instances
new PageBlock("HeroSection",     new { Title = "Hello!", BgUrl = "/media/bg.jpg" })
new PageBlock("DonationWidget",  new { CampaignId = "uuid", ShowProgress = true })
new PageBlock("StatsBanner",     new { Keys = new[] { "donors", "raised", "projects" } })
```

---

### Article *(Entity)*

**Purpose:** A blog post or news item authored by tenant staff.

**Inherits:** `BaseTenantEntity`

| Property | Type | Description |
|---|---|---|
| `TenantId` | `Guid` | Scope |
| `Slug` | `string` | URL-friendly unique identifier |
| `Title` | `string` | Article headline |
| `Body` | `string` | HTML / Markdown content |
| `AuthorUserId` | `Guid` | FK → User |
| `Category` | `string?` | Tag (e.g. `"news"`, `"updates"`) |
| `CoverImageUrl` | `string?` | CDN URL |
| `IsPublished` | `bool` | Visibility flag |
| `PublishedAt` | `DateTimeOffset?` | UTC publish timestamp |
| `SeoSettings` | `SeoSettings` | Meta tags value object |

---

### MediaFile *(Entity)*

**Purpose:** Tracks files uploaded to the tenant's media library (images, documents, videos).

**Inherits:** `BaseTenantEntity`

| Property | Type | Description |
|---|---|---|
| `TenantId` | `Guid` | Scope |
| `FileName` | `string` | Original file name |
| `StorageUrl` | `string` | CDN / blob storage URL |
| `ContentType` | `string` | MIME type (e.g. `image/jpeg`) |
| `FileSizeBytes` | `long` | File size for quota tracking |
| `UploadedByUserId` | `Guid` | FK → User |
| `UploadedAt` | `DateTimeOffset` | UTC timestamp |

---

## 7. Entity Classification Matrix

| Entity | Context | Classification | Phase | Status |
|---|---|---|---|---|
| `Tenant` | Tenant Management | Essential | 1 | ✅ Built |
| `Theme` | Tenant Management | Essential | 1 | ✅ Built |
| `ModuleActivation` | Tenant Management | Essential | 1 | ✅ Built |
| `SubscriptionPlan` | Tenant Management | Important | 3 | 🔜 Planned |
| `User` | IAM | Essential | 1 | ✅ Built |
| `Membership` | IAM | Essential | 1 | ✅ Built |
| `Campaign` | Campaign & Donation | Essential | 1 | ✅ Built |
| `Donation` | Campaign & Donation | Essential | 1 | 🔜 Next |
| `RecurringPledge` | Campaign & Donation | Important | 2 | 🔜 Planned |
| `DonorProfile` | Campaign & Donation | Important | 2 | 🔜 Planned |
| `Event` | Event & Volunteer | Important | 2 | 🔜 Planned |
| `VolunteerApplication` | Event & Volunteer | Important | 2 | 🔜 Planned |
| `Shift` | Event & Volunteer | Important | 2 | 🔜 Planned |
| `Page` | Website Builder | Important | 2 | 🔜 Planned |
| `Article` | Website Builder | Important | 2 | 🔜 Planned |
| `MediaFile` | Website Builder | Important | 2 | 🔜 Planned |
| `NavigationMenu` | Website Builder | Important | 2 | 🔜 Planned |
| `Course` | Education | Optional | 3 | 🔜 Future |
| `Sponsorship` | Fundraising | Optional | 3 | 🔜 Future |
| `BeneficiaryCase` | Humanitarian | Optional | 3 | 🔜 Future |
| `NewsletterSubscriber` | Engagement | Optional | 3 | 🔜 Future |
| `AuditLog` | Platform | Important | 2 | 🔜 Planned |
| `SystemConfig` | Platform | Optional | 3 | 🔜 Future |
| `AnalyticsReport` | Platform | Future | 4 | 🔜 Future |

---

## 8. Enum Reference

All enums are stored as `VARCHAR` in PostgreSQL. Never use integer values in code.

```csharp
// Tenant Management
public enum TenantStatus     { Provisioning, Pending, Active, Suspended, Terminated }

// IAM
public enum MembershipStatus { Invited, Pending, Active, Suspended, Removed }

// Campaign & Donation
public enum CampaignStatus   { Draft, Active, Suspended, Ended, Archived }
public enum DonationStatus   { Pending, Completed, Failed, Refunded }
public enum PledgeStatus     { Active, Paused, Cancelled }

// Event & Volunteer
public enum EventStatus      { Draft, Published, Cancelled, Completed }
public enum VolunteerStatus  { Applied, Approved, Rejected, Attended, NoShow }
```

### Value Objects Reference

| Value Object | Used In | Properties |
|---|---|---|
| `Money` | Campaign, Donation | `Amount (decimal)`, `Currency (string)` |
| `SeoSettings` | Page, Article | `MetaTitle`, `MetaDescription`, `OpenGraphImageUrl` |
| `PageBlock` | Page | `Type (string)`, `Params (Dictionary)` |
| `GeographicLocation` | Event | `Address`, `City`, `Country`, `Latitude?`, `Longitude?` |
| `DateTimeRange` | Event, Shift | `Start (DateTimeOffset)`, `End (DateTimeOffset)` |
| `EmailAddress` | User, Donation | Validated email string wrapper |
| `Subdomain` | Tenant | Validated, lowercase, alphanumeric+hyphen string wrapper |

---

*Last updated: Phase 1 complete — Campaign domain built. Next: Donation module.*
