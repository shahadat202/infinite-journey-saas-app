# New Implementation Details

This document records the design decisions and implementation details for the updates made during this cycle.

## 1. User & Role Clearance

### Campaign Manage Tab Visibility

- **Current Behavior**: The "Manage" tab appears on the nav bar simply if the user is authenticated.
- **Improved Behavior**: We restrict the "Manage" navigation tab in both the frontend layout and the router settings using a role check.
- **Roles**: Only users with staff/managerial roles (`OrganizationOwner`, `OrganizationAdmin`, `Staff`, `Volunteer Coordinator`, `Content Manager`, `Finance Manager`) can see and navigate to the management console.
- **RBAC**: Handled dynamically using Keycloak realm and resource roles client-side (`AuthService.isStaff()`) and server-side policy guards (`[Authorize(Policy = "TenantStaff")]`).

---

## 2. File Upload & Storage Strategy

- **Backend Storage**: The backend maintains clean, local file storage isolated by tenant GUID.
- **Standard Flow**: The frontend reads files as base64 strings and submits them to `/api/files/upload`. The backend saves them to a structured path under the directory specified by `Storage:RootPath`, returning a short path like `/uploads/{tenantGuid}/images/{guid}.ext`.
- **Local Dev vs. Docker Production**:
  - Locally, the file root defaults to `UPLOADED_DATA` in the repository root.
  - In Docker, we configure `Storage__RootPath` to save to `/app/UPLOADED_DATA` and link it to a Docker volume `api_uploads` to ensure images persist across restarts.

---

## 3. Premium Toast Notifications

- **Styling**: Distinct colors and matching outline icons:
  - **Success**: Soft green (#059669 border, light background), checkmark icon. Auto-dismisses in 5s.
  - **Warning**: Warm yellow (#d97706 border, light background), alert triangle icon. Auto-dismisses in 5s.
  - **Error**: Vivid red (#dc2626 border, light background), cross circle icon. **Does not auto-dismiss** (persists until closed manually by the user).
- **Control**: Every toast has a close button in the top-right corner.

---

## 4. NZ-Zorro Campaign Management

- **NZ-Table Integration**: The list view utilizes `nz-table` for standard styling, including headers, row dividers, page size dropdowns, and pagination controls.
- **New Campaign View / Form Page**: We replace the sidebar drawer with a standalone edit/create view `/campaigns/manage/new` and `/campaigns/manage/edit/:id`. This page uses standard inputs, date pickers, target limits, and cover image upload zones styled via NG-ZORRO.

---

## 5. Deactivation Toggles

- **Functionality**: We wire the campaign row action to a single toggle button.
  - If status is `Active`, the button displays "Deactivate". Clicking it calls `POST /api/campaigns/{id}/deactivate` and updates the state.
  - If status is `Draft` or `Suspended`, the button displays "Activate". Clicking it calls `POST /api/campaigns/{id}/activate`.

---

## 6. Dynamic Tenant Theme Setup

- **Backend Configuration**: Extends `Theme` entity containing variables for PrimaryColor, SecondaryColor, AccentColor, FontFamily, and IsDarkMode.
- **Frontend Integration**: We create a `/theme/manage` setting dashboard.
- **DOM Injection**: On app bootstrap (`APP_INITIALIZER`), the frontend calls `GET /api/theme` to load the tenant theme. The theme service translates the values to custom CSS properties (`--primary`, `--secondary`, etc.) injected into the document root, causing the entire UI colors and fonts to adjust instantly.
