# Tech Stack

## Technology Stack Table

| Category | Technology | Version | Purpose | Rationale |
|----------|------------|---------|---------|-----------|
| Frontend Language | C# | 12.0 | Blazor Server components and logic | Type safety, shared language across stack, enterprise tooling |
| Frontend Framework | Blazor Server | .NET 9.0 | Server-side rendered web UI | Real-time SignalR integration, security, reduced client complexity |
| UI Component Library | MudBlazor | 7.0+ | Material Design components | Rich component ecosystem, theming, accessibility compliance |
| State Management | Blazor Server State | Built-in | Component state and SignalR updates | Native integration, real-time capabilities, simplified architecture |
| Backend Language | C# | 12.0 | API and business logic | Consistency with frontend, enterprise features, performance |
| Backend Framework | ASP.NET Core | 9.0 | Web API and hosting | Mature ecosystem, performance, built-in dependency injection |
| API Style | REST with MediatR | OpenAPI 3.0 | HTTP APIs with CQRS pattern | Clean separation, caching support, swagger documentation |
| Database | Multi-Provider | Latest | Primary data storage | MSSQL/PostgreSQL/SQLite support for deployment flexibility |
| Cache | FusionCache | 1.0+ | Application-level caching | Built-in integration with template, performance optimization |
| File Storage | Azure Blob Storage | Latest | Attachment and document storage | Scalable, secure, virus scanning integration |
| Authentication | ASP.NET Core Identity | 9.0 | User management and security | Multi-tenant support, role-based permissions, enterprise features |
| Frontend Testing | bUnit | 1.24+ | Blazor component testing | Blazor-specific testing framework, component isolation |
| Backend Testing | xUnit + FluentAssertions | Latest | Unit and integration testing | .NET standard, readable assertions, async support |
| E2E Testing | Playwright .NET | Latest | End-to-end browser testing | Cross-browser, reliable, .NET integration |
| Build Tool | .NET CLI | 9.0 | Compilation and packaging | Native .NET tooling, cross-platform support |
| Bundler | Built-in Blazor | .NET 9.0 | Asset bundling and optimization | Integrated with framework, no additional configuration |
| IaC Tool | Azure Resource Manager | Latest | Infrastructure provisioning | Native Azure integration, declarative templates |
| CI/CD | Azure DevOps | Latest | Build and deployment pipelines | Integrated with Azure, .NET optimized |
| Monitoring | Application Insights | Latest | Performance and error tracking | Azure native, .NET instrumentation, real-time dashboards |
| Logging | Serilog | 4.0+ | Structured logging | Rich formatting, multiple sinks, performance optimized |
| CSS Framework | MudBlazor + Custom | Latest | Styling and responsive design | Component-integrated styles, Material Design consistency |

## Bot Framework Additions

| Category | Technology | Version | Purpose | Rationale |
|----------|------------|---------|---------|-----------|
| Bot Framework | Microsoft Bot Framework | 4.21+ | WhatsApp integration | Native Azure integration, channel adapters |
| AI Services | Semantic Kernel | 1.0+ | LLM conversation processing | Microsoft AI stack, plugin architecture |
| WhatsApp Channel | Azure Communication Services | Latest | WhatsApp Business API | Enterprise-grade, compliant, scalable |
