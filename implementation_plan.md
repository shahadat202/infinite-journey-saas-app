# InfiniteJourney — Implementation Plan

> **This file tracks what we build, in what order, and what is next.**
> It is updated continuously as development progresses.
>
> For architecture decisions → [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)
> For domain entity details → [`docs/compodoc-additional-docs/03-domain-models.md`](docs/compodoc-additional-docs/03-domain-models.md)
> For environment setup → [`docs/SETUP.md`](docs/SETUP.md)

---

## 🎯 Next Move

**Donation Module (Phase 1 — T9)**

Build the complete `Donation` vertical slice — backend CQRS handlers, domain event wiring to update `Campaign.RaisedAmount`, EF Core migration, NSwag client regeneration, and Angular donation form UI.

---

## Phase 1 — Foundation & Reference Module

### ✅ Completed

- [x] **T1** — `TenantResolutionMiddleware` — resolves tenant from subdomain host header per request
- [x] **T2** — EF Core tenant isolation — global query filters + `TenantSaveChangesInterceptor`
- [x] **T3** — Keycloak JWT auth — JWKS validation + `realm_access.roles` → `ClaimsPrincipal` mapping
- [x] **T4** — Core domain entities — `Tenant`, `Theme`, `ModuleActivation`, `User`, `Membership`, `Campaign`, `Donation`
- [x] **T5** — Campaign CQRS — `CreateCampaignCommand`, `ActivateCampaignCommand`, `GetCampaignsQuery`, `GetCampaignByIdQuery`
- [x] **T6** — NSwag OpenAPI — spec exposed; `npm run generate-api` produces `infinite-journey-apis.ts`
- [x] **T7** — Angular Campaigns UI — standalone components using generated client + Angular Signals
- [x] **T8** — Three-project Docker structure — `Keycloak`, `Backend`, `Frontend` independently deployable; root `docker-compose.dev.yml`

---

### 🔜 In Progress / Up Next

#### T9 — Donation Module

**Backend**
- [ ] `CreateDonationCommand` + handler + `CreateDonationCommandValidator`
- [ ] `UpdateDonationStatusCommand` — transitions `Pending → Completed | Failed | Refunded`
- [ ] `GetDonationsByCampaignQuery` + handler
- [ ] `GetDonationByIdQuery` + handler
- [ ] `DonationReceivedEvent` — increments `Campaign.RaisedAmount` on `Completed`
- [ ] `DonationRefundedEvent` — decrements `Campaign.RaisedAmount` on `Refunded`
- [ ] EF Core entity type configuration for `Donation`
- [ ] DB migration

**Frontend**
- [ ] Regenerate NSwag client after backend is ready
- [ ] `DonationFormComponent` — donate to a specific campaign
- [ ] `DonationListComponent` — donations list on campaign detail page
- [ ] Wire real-time raised amount display into `CampaignDetailComponent`

---

#### T10 — User / Membership Sync

- [ ] `EnsureUserExistsHandler` — creates local `User` record from JWT claims on first request (`sub`, `email`, `given_name`, `family_name`)
- [ ] `UserRegisteredEvent` — published on first-time user creation
- [ ] `POST /api/memberships` — invite a user to a tenant with an assigned role
- [ ] `MembershipAssignedEvent` — published on membership activation
- [ ] Angular membership management screen (basic admin view)

---

#### T11 — Module Feature Toggle Enforcement

- [ ] `RequireFeatureFilter` — ASP.NET Core endpoint filter; returns `403` when module disabled
- [ ] Apply `[RequireFeature("Campaigns")]` to `CampaignsController`
- [ ] Apply `[RequireFeature("Donations")]` to `DonationsController`
- [ ] Angular `featureGuard` — blocks route activation if module is disabled for tenant
- [ ] `TenantContextService` fetches active module list on app bootstrap

---

## Phase 2 — Community Modules

*Begins after T9–T11 are complete.*

- [ ] **Memberships** — member registration flow, lifecycle management, role assignment UI
- [ ] **Events** — creation, RSVP, shift management, capacity limits
- [ ] **Volunteers** — applications, shift assignments, hours logging, attendance
- [ ] **Blog / News** — article authoring, categories, publish/draft workflow, SEO
- [ ] **Website Builder** — block-based dynamic page editor, navigation menu config
- [ ] **Media Library** — file upload to blob storage, tenant-scoped CDN URLs

---

## Phase 3 — Advanced Platform

- [ ] **Payment Integration** — Stripe Connect (platform fees + tenant payouts); recurring pledges
- [ ] **Courses** — curriculum management, enrollment, progress tracking
- [ ] **Sponsorships** — sponsor profiles, beneficiary case linking, impact reporting
- [ ] **Newsletter** — subscriber lists, email dispatch, unsubscribe management
- [ ] **Subscription Billing** — `SubscriptionPlan` entity, Stripe webhooks, plan lifecycle
- [ ] **Enterprise Keycloak** — dedicated realm per enterprise tenant, SAML/OIDC federation

---

## Phase 4 — Scale & Enterprise

- [ ] **Dedicated DB provisioning** — runtime connection string resolution for Enterprise tenants
- [ ] **Analytics & Reporting** — donation trends, campaign performance, volunteer hours
- [ ] **Audit Logging** — `AuditLog` entity; all commands emit structured audit entries
- [ ] **Mobile API layer** — hardened REST layer for mobile clients; push notifications
- [ ] **Multi-region** — CDN + read replicas; geo-based tenant routing
- [ ] **Microservice extraction** — Notification, Payment, Analytics as independent services

---

## Open Decisions

Revisited at the start of each phase.

| Decision | Recommended | Review At |
|---|---|---|
| Payment gateway | Stripe Connect (platform-managed) | Phase 3 kickoff |
| Enterprise Keycloak realms | Realm-per-enterprise-tenant | Phase 3 kickoff |
| Background jobs | Hangfire (PostgreSQL-backed) | Phase 2 kickoff |
| Media storage | Azure Blob / AWS S3 | Phase 2 kickoff |
| API versioning | URL path `/v1` | Before first public release |
