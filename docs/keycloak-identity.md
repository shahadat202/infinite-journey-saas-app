# Keycloak Integration & Identity Access Management (IAM) Guide

This guide details the security setup, authentication flows, and configuration procedures for **Keycloak Identity Access Management** across the InfiniteJourney ecosystem.

---

## 1. Authentication Flow: OIDC PKCE

To secure the frontend Angular single-page application without exposing client credentials, we use **OAuth 2.0 Authorization Code Flow with PKCE** (Proof Key for Code Exchange).

```mermaid
sequenceDiagram
    autonumber
    participant Browser as User Browser
    participant Angular as Angular App (Port 4200)
    participant Keycloak as Keycloak Server (Port 8080)
    participant API as ASP.NET Core API (Port 5274)

    Angular->>Browser: Load app (checks login status)
    Angular->>Angular: Generate code_verifier & code_challenge
    Angular->>Keycloak: Redirect to auth login screen with code_challenge
    Browser->>Keycloak: User enters username + password
    Keycloak->>Browser: Authentication success; redirects to Angular with auth_code
    Browser->>Angular: Load redirect URL with auth_code
    Angular->>Keycloak: POST code + code_verifier (token exchange)
    Keycloak->>Angular: Returns Access Token (JWT) + ID Token
    Angular->>API: HTTP Request with Bearer Token in Authorization header
    API->>API: Validate JWT signature using JWKS public keys
    API->>API: Map roles to ClaimsPrincipal
    API-->>Angular: HTTP Response (200 OK)
```

---

## 2. Keycloak Admin Console Setup

To configure the environment locally or in staging:

### Step 1: Create the Realm
1. Log in to the Keycloak Admin Console (e.g., `http://localhost:8080` using admin credentials).
2. Click the Realm dropdown in the top-left and select **Create realm**.
3. Set the realm name to `InfiniteJourney`.

### Step 2: Configure the Frontend Client (`infinite-journey-web`)
1. Navigate to **Clients** → Click **Create client**.
2. **Client type**: `OpenID Connect`.
3. **Client ID**: `infinite-journey-web`.
4. Click **Next** and toggle **Standard flow** ON, **Direct access grants** ON, and **Client authentication** OFF (since this is a public frontend SPA).
5. Set **Root URL** to `http://localhost:4200` (or local hostnames like `http://hope.localhost:4200` / `http://relief.localhost:4200`).
6. Set **Valid redirect URIs** to `http://*.localhost:4200/*` and `http://localhost:4200/*`.
7. Set **Web origins** to `*` or `+` (to allow CORS from all tenant domains).
8. Toggle **Proof Key for Code Exchange (PKCE)** to `ON` (Required).

### Step 3: Configure Realm Roles
1. Navigate to **Realm roles** → Click **Create role**.
2. Create the following tenant staff roles:
   - `OrganizationAdmin`
   - `Staff`
   - `Volunteer Coordinator`
   - `Content Manager`
   - `Finance Manager`

---

## 3. Backend JWT Validation Setup

The ASP.NET Core API validates the Bearer token sent in request headers:
- **Authority Issuer**: `http://localhost:8080/realms/InfiniteJourney`
- **JWKS Endpoint**: Keycloak exposes a JSON Web Key Set (JWKS) at `http://localhost:8080/realms/InfiniteJourney/.well-known/openid-configuration/jwks` which the API fetches to validate token signatures locally.
- **Claims Mapping**: Custom middleware parses the `realm_access` and `resource_access` JSON parameters from the JWT claims to map Keycloak roles to .NET [ClaimTypes.Role](file:///d:/infinite-journey-saas-app/InfiniteJourney.Backend/Infrustructure/InfiniteJourney.Infrustructure/Identity/AuthenticationDependencyInjection.cs#L72-L96) claims.

---

## 4. Frontend Angular Security

### Keycloak JS Integration
The Angular app uses the official `keycloak-js` library to initialize identity flow:
- **Initialization Mode**: `check-sso` (allows silent background checks to see if the browser has an active session without interrupting the user).
- **Silent SSO Redirect**: The browser loads [silent-check-sso.html](file:///d:/infinite-journey-saas-app/InfiniteJourney.Frontend/Web/InfiniteJourney.Web/public/assets/silent-check-sso.html) in a hidden iframe to verify cookies without reloading the SPA.

### Token Interceptor
The [authInterceptor](file:///d:/infinite-journey-saas-app/InfiniteJourney.Frontend/Web/InfiniteJourney.Web/src/app/core/interceptors/auth.interceptor.ts) dynamically appends the JWT access token to every outgoing `/api` HTTP request:
```typescript
return next(
  req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  })
);
```
