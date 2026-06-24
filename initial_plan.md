InfiniteJourney - Enterprise Multi-Tenant SaaS Architecture Master Prompt
ROLE
Act as a Principal Software Architect, Enterprise SaaS Architect, Domain-Driven Design Specialist, Cloud Solution Architect, Identity & Security Architect, and Lead Full-Stack Engineer.
You are responsible for designing a production-ready, enterprise-grade, highly scalable SaaS platform named:
InfiniteJourney
Your responsibility is to think like an architect building a platform that may eventually serve:
Thousands of organizations
Millions of users
Multiple regions and countries
Multiple domains and custom domains
Multiple organization types
Future microservices
Enterprise customers
Do not think like a website developer.
Do not think like a CRUD application developer.
Think like the architect of an organization's operating system.

PRODUCT VISION
InfiniteJourney is a single shared SaaS platform where organizations can create and manage their own digital ecosystem without maintaining separate codebases.
The platform should support:
Dawah Organizations
Islamic Organizations
Charities
NGOs
Volunteer Organizations
Community Organizations
Educational Organizations
Religious Organizations
Humanitarian Organizations
Social Welfare Organizations
Foundations
Fundraising Organizations
Relief Organizations
Membership Organizations
Every organization becomes a Tenant.
Every tenant receives:
Their own website
Their own branding
Their own users
Their own permissions
Their own content
Their own configuration
Their own modules
Their own analytics
All tenants share:
One codebase
One platform
One deployment strategy
One feature catalog
No tenant should ever affect another tenant.

PLATFORM PHILOSOPHY
The platform is NOT a charity website.
The platform is NOT a donation website.
The platform is an Organization Management & Digital Experience Platform.
Charity, donations, events, volunteers, courses, memberships, fundraising, and campaigns are merely modules within the platform.
The architecture must remain flexible enough to support future domains without redesigning the foundation.

PRIMARY OBJECTIVES
Objective 1 - Multi-Tenancy First
Multi-tenancy is the highest priority.
Before designing any business feature:
Design:
Tenant architecture
Tenant isolation
Tenant provisioning
Tenant lifecycle
Tenant resolution
Tenant middleware
Tenant security
Tenant-aware authorization
Tenant-aware caching
Tenant-aware background jobs
All future modules must inherit this foundation.

Objective 2 - Enterprise Identity & Access Management
Identity must be built around Keycloak.
Design:
Authentication
Authorization
SSO
Claims
Roles
Permissions
Organization membership
Tenant-scoped access
Platform-scoped access
Role hierarchy should support:
Platform Level:
Super Admin
Platform Support
Platform Auditor
Tenant Level:
Organization Owner
Organization Admin
Staff
Manager
Volunteer Coordinator
Content Manager
Finance Manager
Public Level:
Member
Volunteer
Donor
Subscriber
Guest
Organization administrators must never access another organization's resources.

Objective 3 - Dynamic Shared Feature Catalog
The platform must provide a centralized feature catalog.
Examples:
Donations
Campaigns
Events
Volunteers
Members
Courses
Fundraising
Sponsorship
Cases
Projects
Programs
Blog
News
Media Library
Gallery
Newsletter
Reports
Analytics
Website Builder
Every module is built once.
Every tenant decides:
Enable
Disable
Configure
Disabling a module:
Hides functionality
Preserves data
Preserves configuration
Re-enabling restores the previous state.

Objective 4 - Dynamic White Label Experience
Every tenant can customize:
Logo
Favicon
Typography
Colors
Homepage layout
Navigation
Footer
Widgets
Landing pages
Changing branding for Tenant A must never impact Tenant B.

Objective 5 - Long-Term Maintainability
The architecture must remain clean after:
10 modules
50 modules
100 modules
Adding module #50 should feel as clean as adding module #1.

REFERENCE EXPERIENCE STANDARDS
Draw inspiration from platforms such as:
Charity-focused donation experiences
Campaign-based fundraising platforms
Storytelling-focused humanitarian platforms
Modern NGO and nonprofit websites
Focus on:
Transparency
Storytelling
Trust
Donation conversion
Community engagement
Volunteer engagement
Operational management

TECHNOLOGY STACK
Backend
.NET Latest LTS
ASP.NET Core Web API
PostgreSQL
Entity Framework Core
CQRS
MediatR
FluentValidation
Repository Pattern
Unit Of Work
Clean Architecture
Domain Driven Design
NSwag
Redis
Serilog
Frontend
Angular Latest LTS
Standalone Components Only
Angular Signals
RxJS
TailwindCSS
SCSS only when necessary
Dynamic Theme Engine
Identity
Keycloak
OpenID Connect
OAuth2
Role Management
Permission Management
Claims-Based Authorization
Tenant Aware Authorization
Infrastructure
Docker
Docker Compose
CI/CD Ready
Kubernetes Ready
Cloud Ready

ARCHITECTURAL RULES
Mandatory:
Clean Architecture
Domain Driven Design
CQRS
SOLID
Vertical Slice Features
Domain Events
Integration Events
Eventual Microservice Migration Support
Domain layer must contain:
Entities
Value Objects
Domain Events
Aggregate Roots
No infrastructure dependencies allowed in Domain.

PHASE 1 - BUSINESS ANALYSIS
Identify and document:
Business Goals
User Personas
Tenant Types
Stakeholders
SaaS Monetization Models
Core Business Processes
Functional Requirements
Non-Functional Requirements
Future Expansion Opportunities

PHASE 2 - MULTI-TENANCY ARCHITECTURE
Design and compare:
Option A
Shared Database + TenantId
Option B
Schema Per Tenant
Option C
Database Per Tenant
Option D
Hybrid Strategy
For each option, provide:
Advantages
Disadvantages
Cost
Operational Complexity
Scalability
Security
Performance
Recommend the best approach.
Design:
Tenant Resolution
Tenant Context
Tenant Middleware
Tenant Provisioning
Tenant Security
Tenant Caching
Tenant Background Processing
Support:
Subdomains
Custom Domains
JWT Claims
Request Headers

PHASE 3 - DOMAIN-DRIVEN DESIGN
Identify all bounded contexts.
For each context, provide:
Purpose
Responsibilities
Aggregate Roots
Entities
Value Objects
Domain Events
Repositories
Application Services
Generate a complete DDD map.

PHASE 4 - IDENTITY & ACCESS MANAGEMENT
Design complete Keycloak integration.
Decide and justify:
Single Realm Strategy
Realm Per Tenant Strategy
Design:
Users
Memberships
Claims
Roles
Permissions
Authentication Flow
Authorization Flow
SSO Flow
User Lifecycle
Support:
Multi-Organization Membership
Tenant Scoped Permissions
Platform Scoped Permissions
Generate:
Class Diagrams
Relationship Diagrams
Database Design

PHASE 5 - CORE DOMAIN MODEL
Design all possible entities.
Classify:
Essential
Important
Optional
Future
Include but do not limit to:
Tenant
Organization
User
Membership
Campaign
Donation
Donor
Volunteer
Event
Project
Program
Course
Sponsor
Beneficiary
Media
Article
Blog
Page
Contact
Newsletter
Report
AuditLog
Notification
Think beyond these and identify everything required for a world-class platform.

PHASE 6 - MODULAR FEATURE SYSTEM
Design:
Feature Catalog
Module Registry
Module Activation
Feature Toggles
Module Dependencies
Module Permissions
Support tenant-specific activation.

PHASE 7 - DYNAMIC WEBSITE BUILDER
Design:
Dynamic Pages
Dynamic Menus
Dynamic Hero Sections
Dynamic Blocks
Dynamic Widgets
Dynamic Footer
Dynamic SEO Settings
Dynamic Navigation
No coding required for tenant administrators.

PHASE 8 - THEME ENGINE
Design a runtime theming system.
Support:
Primary Color
Secondary Color
Accent Color
Typography
Light Mode
Dark Mode
Use:
CSS Variables
Tailwind Integration
Runtime Injection
Ensure automatic text contrast calculations.

PHASE 9 - APPLICATION ARCHITECTURE
Design a complete solution structure.
Backend:
Domain
Application
Infrastructure
API
Frontend:
Core
Shared
Features
Layout
Generated NSwag Clients
Explain every folder.

PHASE 10 - DATABASE DESIGN
Generate:
ER Diagram
Relationships
Table Structures
Index Strategy
Partition Strategy
Tenant Strategy

PHASE 11 - API STRATEGY
Design:
REST APIs
API Versioning
NSwag
OpenAPI
Generated Angular Clients
Security
Naming Standards
No manually written Angular API services.
All services generated automatically.

PHASE 12 - SECURITY
Design:
Authentication
Authorization
Tenant Isolation
Audit Logging
Encryption
Secret Management
OWASP Compliance
Rate Limiting
CSRF Protection
API Security

PHASE 13 - DEPLOYMENT
Design:
Docker Architecture
Docker Compose
Kubernetes Readiness
Monitoring
Logging
Alerting
Backup Strategy
Disaster Recovery

PHASE 14 - PHASE 1 IMPLEMENTATION TARGET
Before implementing any business module:
Fully complete:
Tenant Management Domain
Identity & Access Domain
Keycloak Integration
Theme Management Domain
Feature Catalog Domain
Then implement ONE complete reference business module.
Recommend which should be first:
Campaign
Donation
Event
Volunteer
Justify the recommendation.
Generate:
Full Backend Design
Full Frontend Design
CQRS Structure
Repository Design
DTO Design
NSwag Flow
Angular Structure
Generate complete class diagrams for:
Identity & Tenant Domain
First Reference Business Domain

OUTPUT REQUIREMENTS
Never provide simplistic answers.
Never skip architectural decisions.
Always explain:
Why
Alternatives
Trade-offs
Future implications
The final design must be:
Enterprise Grade
Production Ready
Multi-Tenant
Secure
Extensible
Maintainable
Cloud Ready
Kubernetes Ready
Future Microservice Ready
The architecture must be capable of becoming the foundational operating platform for organizations worldwide.
---
D:\infinite-journey-saas-app
---
https://lightuponlight.co.uk/
https://www.charitywater.org/
https://www.gofundme.com/en-gb
https://humanjourney.us/
https://www.charitywater.org/
https://theoceancleanup.com/
