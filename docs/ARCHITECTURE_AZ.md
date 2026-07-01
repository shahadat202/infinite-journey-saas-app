# InfiniteJourney - Platform Overview & System Design (A-Z)

This document is a short, comprehensive brief that explains the main theme, purpose, and architecture of the **InfiniteJourney** multi-tenant SaaS DXP platform.

---

## 1. Product Vision & Theme

**InfiniteJourney** is an enterprise-grade multi-tenant Digital Experience Platform (DXP) designed specifically for:
- Non-Profit Organizations & Charities
- Islamic & Community Centers
- NGOs & Humanitarian Groups
- Educational Foundations

### Core Philosophy
Unlike standard site builders or simple donation platforms, InfiniteJourney acts as an **Operating System** for organizations. Every organization registers as a **Tenant** and receives:
1. A white-labeled public website with custom domain support.
2. Custom tenant-scoped branding (colors, logos, fonts).
3. Access to a modular, dynamic feature catalog (e.g. Campaigns, Donations, Memberships, Events).
4. Localized identity, users, and staff roles.

All tenants share **one codebase** and **one backend deployment**, providing zero-maintenance upgrades and high cost efficiency.

---

## 2. Platform Architecture (High-Level)

The platform is designed around a **three-project deployment model** to ensure separation of concerns and high scalability:

```
                  ┌────────────────────────────────────────┐
                  │          Identity Provider             │
                  │   InfiniteJourney.Keycloak (OIDC/JWT)  │
                  └──────────────────┬─────────────────────┘
                                     │
         1. Authenticate             │  3. Verify Token
         & Return JWT                │  via public JWKS
                                     ▼
┌─────────────────────────┐  2. Request  ┌─────────────────────────┐
│     Frontend (SPA)      ├─────────────►│      Backend (API)      │
│ InfiniteJourney.Frontend│  with Bearer │ InfiniteJourney.Backend │
│      (Angular 19)       │  Auth Token  │    (.NET Clean/CQRS)    │
└─────────────────────────┘              └───────────┬─────────────┘
                                                     │ 4. Read/Write
                                                     ▼
                                         ┌─────────────────────────┐
                                         │       PostgreSQL        │
                                         │   (Isolated Tenant DB)  │
                                         └─────────────────────────┘
```

1. **InfiniteJourney.Keycloak (OIDC)**: Manages authentication, SSO, and user security policies globally. It uses OAuth2 with PKCE to return a secure JWT token to the frontend.
2. **InfiniteJourney.Frontend (Angular 19)**: A standalone single-page application that renders the White Label public websites and administrative portals.
3. **InfiniteJourney.Backend (.NET Web API)**: Validates incoming JWT tokens, resolves the tenant via subdomain/host headers, enforces database row-level tenant isolation, and executes business actions.

---

## 3. Database Tenancy & Write Isolation

Data isolation is crucial in a shared-database multi-tenant environment.
- **Tenant Scope Resolution**: Incoming API calls pass the host header (e.g. `hope.localhost:5274`). The API extracts the subdomain to identify the tenant and sets the scoped `TenantId`.
- **Query Isolation**: EF Core applies a global query filter `e.TenantId == tenantContext.TenantId` to all database reads, preventing any tenant from reading another tenant's data.
- **Write Interceptor**: The database save engine stamps the resolved `TenantId` automatically on new inserts and raises a `TenantViolationException` if a cross-tenant update is attempted.

---

## 4. MediatR CQRS Pattern

We separate write operations (Commands) from read operations (Queries) to make the code highly maintainable and clean:
- **Clean Controllers**: Controllers contain no business logic. They accept the request parameters and send them directly to MediatR:
  ```csharp
  [HttpPost]
  public Task<IActionResult> Create(CreateCampaignCommand command, CancellationToken token)
      => SendCreatedAsync(command, token, result => (nameof(GetById), new { id = result.Id }, result));
  ```
- **Feature Folders**: All business rules, validations, mapping, and database transactions are stored inside a dedicated feature slice (e.g. `Campaigns/Commands` or `Campaigns/Queries`), preventing micro-architectural bloat as new modules are added.

---

## 5. Next Phase Focus Areas
- **Module Activation**: Fully implementing the feature toggling service so tenants can toggle features (Donations, Memberships, Blogs) dynamically.
- **Donation Processing Integration**: Integrating payment gateway clients (e.g. Stripe, PayPal) with tenant-scoped API keys.
- **User-Membership Synchronization**: Linking Keycloak registration events to local user profiles and memberships.
