# Technical Assumptions

## Repository Structure: Monorepo
Continue with existing monorepo structure, extending current Clean Architecture foundation with new WhatsApp integration components in the existing src/Bot/ project and enhancing current Domain entities (Issue, Attachment, EventLog).

## Service Architecture
**Monolithic Clean Architecture** - Build upon existing .NET 9 Clean Architecture foundation with clear separation of concerns:
- **Domain Layer:** Extend existing Issue, Contact, and EventLog entities with WhatsApp-specific properties
- **Application Layer:** Add new CQRS handlers for bot interactions and issue processing workflows
- **Infrastructure Layer:** Implement WhatsApp Business API integration and extend existing database contexts
- **Server.UI Layer:** Enhance existing Blazor Server dashboard with issue management capabilities
- **Bot Project:** Dedicated service for WhatsApp webhook handling and bot conversation logic

## Testing Requirements
**Unit + Integration Testing** - Leverage existing test infrastructure:
- Unit tests for new Application layer handlers and Domain entity modifications
- Integration tests for WhatsApp API integration using existing ApplicationDbContextFactory patterns
- End-to-end tests for critical user workflows (issue submission to resolution)
- Mock WhatsApp API for development and automated testing environments

## Additional Technical Assumptions and Requests

**Database & Persistence:**
- Continue current multi-provider approach (SQL Server/PostgreSQL/SQLite) using existing Entity Framework Core setup
- Extend existing migration projects (Migrators.MSSQL, Migrators.PostgreSQL, Migrators.SqLite) for new entities
- Utilize existing ApplicationDbContextFactory pattern for all new database operations

**Authentication & Security:**
- Integrate with existing ASP.NET Core Identity system and multi-tenant architecture
- Leverage current permission-based authorization system for support staff access control
- Extend existing audit logging capabilities for WhatsApp communication tracking

**Communication & Integration:**
- WhatsApp Business API via webhook integration (Azure Communication Services or approved third-party provider)
- SignalR for real-time dashboard updates using existing infrastructure
- Extend existing IMailService pattern to create IWhatsAppService for message handling

**Development & Deployment:**
- Continue with existing Docker containerization approach for consistent deployment
- Extend current CI/CD pipelines to include Bot project deployment
- Use existing Azure App Service or Container Apps hosting model
