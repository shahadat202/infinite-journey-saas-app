# Domain Models & Entity Glossary

This guide outlines the entities, aggregate roots, and value objects that drive the **Campaigns** and **Multi-Tenancy** business logic of InfiniteJourney.

---

## 1. Multi-Tenant Core Aggregates

### Tenant
- **Purpose**: Represents an organization on the platform (e.g., Hope Foundation, Community Relief). It acts as the primary boundary for data isolation.
- **Key Attributes**:
  - `Id` (GUID): Unique primary key.
  - `Subdomain` (string): Unique identifier for routing (e.g. `hope`, `relief`).
  - `CustomDomain` (string, optional): External domain mapped to the tenant (e.g. `hopefoundation.org`).
  - `Status` (enum): Current activation state (`Provisioning`, `Pending`, `Active`, `Suspended`).

### Theme
- **Purpose**: Defines the brand identity and styling configuration for a specific tenant.
- **Key Attributes**:
  - `TenantId` (GUID): Links directly to the owner tenant (1-to-1 relationship).
  - `PrimaryColor` / `SecondaryColor` / `AccentColor` (hex strings): Dynamically injected CSS variables on the frontend.
  - `FontFamily` (string): Selected brand typography.

### ModuleActivation
- **Purpose**: Tracks which functional modules (e.g., Campaigns, Donations) are activated and configured for a specific organization.
- **Key Attributes**:
  - `TenantId` (GUID): Links to the owner tenant.
  - `ModuleKey` (string): Unique key identifier of the module (e.g. `"Campaigns"`).
  - `IsEnabled` (boolean): Toggle switch for feature availability.

---

## 2. Identity & Access Control

### User
- **Purpose**: Represents an individual user profile registered on the platform. Keycloak manages credentials, while the local User entity maps profile metadata.
- **Key Attributes**:
  - `KeycloakUserId` (string): Unique identifier matching the Keycloak user ID.
  - `Email` (string): Primary contact address.
  - `FirstName` / `LastName` (strings): Personal details.

### Membership
- **Purpose**: Maps users to tenants, assigning tenant-scoped roles (e.g., Owner, Staff, Admin) and tracking their membership lifecycle.
- **Key Attributes**:
  - `TenantId` (GUID): Link to the organization.
  - `UserId` (GUID): Link to the user profile.
  - `RoleName` (string): Scoped authorization role (e.g., `"OrganizationAdmin"`).
  - `Status` (enum): Verification status (`Pending`, `Active`, `Suspended`).

---

## 3. Campaigns & Fundraising

### Campaign
- **Purpose**: An aggregate root representing a fundraising goal for a tenant (e.g., "Clean Water Project").
- **Key Attributes**:
  - `TenantId` (GUID): Scope of ownership.
  - `Title` / `Description` (strings): Core details.
  - `TargetAmount` (decimal): The monetary goal of the campaign.
  - `RaisedAmount` (decimal): Accumulated sum of completed donations.
  - `Status` (enum): Scoped lifecycle (`Draft`, `Active`, `Suspended`, `Ended`).

### Donation
- **Purpose**: Represents a monetary transaction contributed by a donor toward a specific campaign.
- **Key Attributes**:
  - `TenantId` / `CampaignId` (GUIDs): Scopes the transaction.
  - `Amount` (decimal): Financial value.
  - `DonorEmail` (string): Donor identifier.
  - `Status` (enum): Transaction lifecycle state (`Pending`, `Completed`, `Failed`, `Refunded`).
