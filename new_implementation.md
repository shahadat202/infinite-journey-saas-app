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

---

## 7. UI/UX Professional Improvements (Latest Cycle)

### 7.1 Text Truncation with Ellipsis

- **Implementation**: Added CSS line-clamp utilities for campaign cards and detail views.
- **Behavior**: Long text content (titles, descriptions) now truncates with "..." after 2-3 lines instead of expanding indefinitely.
- **Files Modified**: `campaign-list.component.html`, `campaign-list.component.scss`

### 7.2 Rich Text Editor for Campaign Descriptions

- **Implementation**: Integrated Quill.js rich text editor (`ngx-quill`) for campaign creation/editing.
- **Features**: Bold, italic, underline, headings, lists, blockquotes, links, and more.
- **Display**: Campaign detail pages now render HTML content using `[innerHTML]` with proper styling for all rich text elements.
- **Files Modified**: `campaign-form.component.html`, `campaign-form.component.ts`, `campaign-detail.component.html`, `campaign-detail.component.scss`
- **Dependencies Added**: `ngx-quill`, `quill`

### 7.3 Full Responsive Design with Tailwind CSS

- **Implementation**: Added Tailwind CSS framework with custom configuration using CSS variables.
- **Responsive Breakpoints**:
  - Desktop (>1024px): Full sidebar, multi-column layouts
  - Tablet (768px-1024px): Collapsible sidebar, adjusted spacing
  - Mobile (<768px): Single column, hamburger menu, stacked layouts
- **Files Modified**: `tailwind.config.js`, `postcss.config.js`, `styles.scss`, `app.scss`, all component SCSS files
- **Dependencies Added**: `tailwindcss`, `postcss`, `autoprefixer`

### 7.4 Collapsible Left Sidebar Navigation

- **Implementation**: Replaced top navigation with professional left sidebar containing:
  - Main section: Campaigns
  - Manage section: Campaign Management, Theme Settings (staff only)
- **Features**:
  - Desktop: Always visible, sticky positioning
  - Mobile/Tablet: Collapsible with hamburger menu, overlay backdrop
  - Smooth animations and transitions
  - Active state highlighting
  - Auto-close on mobile after navigation
- **Files Modified**: `app.html`, `app.ts`, `app.scss`

### 7.5 Enhanced Color Picker with Preset Palette

- **Implementation**: Added hoverable color palette with 16 preset colors for theme customization.
- **Features**:
  - Click-to-select preset colors
  - Native color picker input
  - Manual hex code input
  - Hover effects with scale animation
- **Files Modified**: `theme-admin.component.html`, `theme-admin.component.ts`, `theme-admin.component.scss`

### 7.6 English Localization for NG-ZORRO Components

- **Implementation**: Configured NG-ZORRO to use English locale (`en_US`) globally.
- **Fixed Issues**:
  - Page size dropdown now shows English text
  - Date picker displays English month names
  - All component labels in English
- **Files Modified**: `app.config.ts`

### 7.7 Debounced Search Functionality

- **Implementation**: Added RxJS debounce (300ms) to search input in campaign admin.
- **Benefits**: Prevents API calls on every keystroke, improves performance, reduces server load.
- **Files Modified**: `campaign-admin.component.ts`

### 7.8 Date Picker Improvements

- **Implementation**: Fixed date picker language and positioning issues.
- **Features**:
  - English date format (`MMM dd, yyyy`)
  - Proper z-index for dropdown positioning
  - Full-width responsive date inputs
- **Files Modified**: `campaign-form.component.html`, `campaign-form.component.scss`

### 7.9 Font Awesome Icons Integration

- **Implementation**: Replaced emoji icons with professional Font Awesome icons throughout the application.
- **Features**:
  - Campaigns: `fa-bullhorn`
  - Campaign Management: `fa-cog`
  - Theme Settings: `fa-palette`
  - Action menu: `fa-ellipsis-v`, `fa-edit`, `fa-trash`, `fa-play-circle`, `fa-pause-circle`
- **Files Modified**: `app.html`, `campaign-admin.component.html`, `styles.scss`
- **Dependencies Added**: `@fortawesome/fontawesome-free`

### 7.10 Campaign Card Clickable Area

- **Implementation**: Made entire campaign card (image + title) clickable for better UX.
- **Features**:
  - Hover effect on title with color change
  - Image and title wrapped in single link
  - Description rendered as HTML using `[innerHTML]`
  - Bold titles for better visual hierarchy
- **Files Modified**: `campaign-list.component.html`, `campaign-list.component.scss`

### 7.11 Campaign Detail Progress Bar Positioning

- **Implementation**: Moved progress bar above description for immediate visibility.
- **Features**:
  - Progress section with subtle background
  - Better visual hierarchy
  - Container max-width to prevent text overflow
- **Files Modified**: `campaign-detail.component.html`, `campaign-detail.component.scss`

### 7.12 Desktop Sidebar Icon-Only Mode

- **Implementation**: Added toggle button for icon-only vs icon+label sidebar mode on desktop.
- **Features**:
  - Columns icon in header for toggle
  - 70px width in icon-only mode
  - Labels hidden in icon-only mode
  - Smooth width transitions
- **Files Modified**: `app.html`, `app.ts`, `app.scss`

### 7.13 Back Button Navigation

- **Implementation**: Added back buttons to campaign create/edit pages.
- **Features**:
  - "← Back to Campaigns" link at top of form
  - Consistent navigation pattern
  - Hover effects for better UX
- **Files Modified**: `campaign-form.component.html`, `campaign-form.component.scss`

### 7.14 Responsive Admin Table

- **Implementation**: Made admin table horizontally scrollable on smaller screens.
- **Features**:
  - Horizontal scroll wrapper
  - Touch scrolling on mobile
  - Visible scroll on desktop only when needed
- **Files Modified**: `campaign-admin.component.html`, `campaign-admin.component.scss`

### 7.15 Mobile Action Menu with Three-Dot

- **Implementation**: Added three-dot dropdown menu for table actions on mobile/tablet.
- **Features**:
  - Desktop: Full action buttons visible
  - Mobile/Tablet: Three-dot menu with icons
  - Icons for each action (edit, delete, activate/deactivate)
  - Professional dropdown positioning
- **Files Modified**: `campaign-admin.component.html`, `campaign-admin.component.ts`, `campaign-admin.component.scss`

---

## 8. Technical Stack Updates

### New Dependencies
- `tailwindcss` - Utility-first CSS framework
- `postcss` - CSS post-processing
- `autoprefixer` - CSS vendor prefixing
- `ngx-quill` - Angular Quill rich text editor wrapper
- `quill` - Rich text editor library
- `@fortawesome/fontawesome-free` - Professional icon library

### Configuration Files Added
- `tailwind.config.js` - Tailwind configuration with CSS variable integration
- `postcss.config.js` - PostCSS processing configuration

---

## 9. Design Principles Applied

- **Mobile-First**: All components designed for mobile, enhanced for larger screens
- **Accessibility**: Proper ARIA labels, keyboard navigation, semantic HTML
- **Performance**: Debounced inputs, optimized animations, efficient re-renders
- **Internationalization**: English locale configuration, proper text handling
- **User Experience**: Smooth transitions, clear feedback, intuitive navigation
- **Maintainability**: Component-based architecture, reusable utilities, consistent styling
