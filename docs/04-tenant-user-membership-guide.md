# Tenant, User & Membership — Explained

> Answers: *What is a tenant? How do I register OrganizationZ? How do users and memberships work?*

---

## 1. What is a Tenant?

**Yes — a Tenant is an organization on the platform.**

| Real world | Platform |
|------------|----------|
| Hope Foundation | Tenant `hope` |
| Community Relief | Tenant `relief` |
| Your OrganizationZ | Tenant `organizationz` (subdomain) |

Each tenant gets:
- Its own **subdomain** (`organizationz.infinitejourney.com` or `organizationz.localhost:4200`)
- Its own **data** (campaigns, donations, themes — isolated by `TenantId`)
- Its own **branding** (colors, logo)
- Its own **enabled modules** (Campaigns, Donations, etc.)
- Its own **staff and members**

Tenants **never see each other's data**. The backend enforces this with EF Core global query filters.

---

## 2. The Three Concepts

```
┌─────────────────────────────────────────────────────────────────┐
│                         PLATFORM                                 │
│  ┌──────────────┐   ┌──────────────┐   ┌──────────────┐         │
│  │  Tenant A    │   │  Tenant B    │   │  Tenant C    │         │
│  │ (Hope)       │   │ (Relief)     │   │ (OrgZ)       │         │
│  └──────┬───────┘   └──────┬───────┘   └──────┬───────┘         │
│         │                  │                  │                  │
│         └──────────────────┼──────────────────┘                  │
│                            │                                     │
│                    ┌───────▼────────┐                            │
│                    │  User (person) │  ← one human, one Keycloak login
│                    └───────┬────────┘                            │
│                            │                                     │
│              ┌─────────────┼─────────────┐                       │
│              │             │             │                       │
│        Membership     Membership    Membership                   │
│        at Tenant A    at Tenant B   at Tenant C                  │
│        Role: Admin    Role: Member  Role: Staff                  │
└─────────────────────────────────────────────────────────────────┘
```

| Concept | What it is | Scope |
|---------|------------|-------|
| **Tenant** | An organization (company, charity, mosque, NGO) | Platform |
| **User** | A real person with one login (Keycloak account) | Platform-wide |
| **Membership** | Link between a User and a Tenant with a **Role** | Per tenant |

**Example:** Sarah is one **User**. She is `OrganizationAdmin` at Hope Foundation (Membership 1) and `Volunteer` at Community Relief (Membership 2). One login, two organizations.

---

## 3. How OrganizationZ Gets Onboarded (Full Flow)

### Phase A — Tenant Provisioning *(Platform creates the organization)*

Today (dev): tenants are **seeded** (`hope`, `relief`).

Production flow (planned):

```
OrganizationZ owner → Signup / Stripe checkout / Super Admin
        │
        ▼
1. Create Tenant record (name: "Organization Z", subdomain: "organizationz")
2. Activate modules (Campaigns, Donations, …)
3. Create default Theme
4. Create Keycloak group for tenant
5. Mark Tenant Status = Active
```

### Phase B — Founder Becomes Organization Owner

```
Founder → Signs up via Keycloak (admin@organizationz.org)
        │
        ▼
1. Keycloak creates identity (JWT on login)
2. Backend lazy-creates User record (first API call)
3. Platform assigns Membership:
   - TenantId = OrganizationZ
   - UserId = founder
   - RoleName = OrganizationOwner
   - Status = Active
```

The founder is now **top admin** of OrganizationZ.

### Phase C — Adding Staff / Members

| Method | Who initiates | Flow |
|--------|---------------|------|
| **Invite** | Admin sends email invite | User accepts → Membership `Invited` → `Active` |
| **Self-register** | User signs up on tenant portal | Membership `Pending` → Admin approves → `Active` |
| **Admin assign** | Admin adds existing user | Membership created directly as `Active` |

Each membership stores:
- `TenantId` — which organization
- `UserId` — which person
- `RoleName` — what they can do (`Staff`, `OrganizationAdmin`, `Member`, …)
- `Status` — `Active`, `Suspended`, etc.

---

## 4. How Login + Tenant URL Work Together

| URL | Resolves to |
|-----|-------------|
| `hope.localhost:4200` | Tenant **Hope Foundation** |
| `organizationz.localhost:4200` | Tenant **OrganizationZ** |
| `localhost:4200` | No tenant — platform/marketing only |

When you open `organizationz.localhost:5274/api/campaigns`:
1. Middleware reads host → subdomain `organizationz`
2. Looks up Tenant in database
3. Sets `TenantContext.TenantId`
4. All queries return **only OrganizationZ data**

JWT tells the backend **who you are**. The host header tells the backend **which organization** you're acting in.

---

## 5. Current Database (Dev Seed)

| Tenant | Subdomain | Sample user |
|--------|-----------|-------------|
| Hope Foundation | `hope` | `admin@hope.org` (Keycloak) |
| Community Relief | `relief` | — |

Membership sync from Keycloak → DB is **next on the roadmap** (Phase 1 continuation).

---

## 6. Role Quick Reference

| Role | Can do |
|------|--------|
| `OrganizationOwner` | Everything including billing |
| `OrganizationAdmin` | Manage users, modules, all data |
| `Staff` | Day-to-day operations (campaigns, content) |
| `Member` / `Donor` | Public portal, own profile |
| `Guest` | Browse only, no login |

---

*Next implementation: Tenant self-registration API + User/Membership sync on first Keycloak login.*
