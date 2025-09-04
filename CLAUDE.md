# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an Issue Management System built with Clean Architecture principles using .NET 9 and Blazor Server. The project extends a Clean Architecture Blazor template and includes specialized features for WhatsApp issue intake, AI-powered bot integration, and comprehensive issue tracking capabilities.

## Architecture & Project Structure

### Clean Architecture Layers
```
src/
├── Domain/           # Core entities, value objects, events
├── Application/      # Business logic, CQRS, DTOs, interfaces
├── Infrastructure/   # Data access, external services, implementations
├── Server.UI/        # Blazor Server UI components and pages
├── Bot/             # Bot Framework integration for WhatsApp
└── Migrators/       # Database migration projects
```

### Key Features
- **Issue Management**: Core issue tracking with attachments, event logging
- **WhatsApp Integration**: Bot-driven issue intake via WhatsApp
- **Multi-tenancy**: Built-in tenant isolation
- **Authentication**: ASP.NET Core Identity with role-based permissions
- **Real-time Updates**: SignalR for live notifications
- **Security Analysis**: Risk assessment and security monitoring

## Development Commands

### Build & Run
```bash
# Build solution
dotnet build

# Run main UI application
dotnet run --project src/Server.UI

# Run Bot service
dotnet run --project src/Bot
```

### Bot Service Development Setup

#### Local Development Environment
1. **Prerequisites**:
   - .NET 9 SDK
   - Visual Studio 2022 or VS Code
   - Azure account for OpenAI and optional services

2. **Configuration**:
   - Copy `src/Bot/appsettings.json` and configure required values:
     - `WhatsApp:WebhookSecret` - For signature verification
     - `AZURE_OPENAI_API_KEY` and `AZURE_OPENAI_API_ENDPOINT` - Required for AI functionality
     - `ApplicationInsights:ConnectionString` - Optional for telemetry

3. **Environment Variables** (Alternative to appsettings.json):
   ```bash
   export WhatsApp__WebhookSecret="your-webhook-secret"
   export AZURE_OPENAI_API_KEY="your-openai-key"
   export AZURE_OPENAI_API_ENDPOINT="https://your-openai.openai.azure.com/"
   export ApplicationInsights__ConnectionString="your-app-insights-connection-string"
   ```

#### Testing Bot Endpoints
```bash
# Health check
curl http://localhost:5000/health

# Ping test
curl http://localhost:5000/ping

# Webhook verification (WhatsApp registration)
curl "http://localhost:5000/webhook?hub.mode=subscribe&hub.challenge=test123&hub.verify_token=your_verify_token_here"

# Test webhook payload (requires valid signature)
curl -X POST http://localhost:5000/webhook \
  -H "Content-Type: application/json" \
  -H "X-Hub-Signature-256: sha256=calculated-signature" \
  -d '{"object":"whatsapp_business_account","entry":[]}'
```

#### Bot Service Architecture
- **Controllers**: `src/Bot/Controllers/BotController.cs` - Webhook and health endpoints
- **Middleware**: `src/Bot/Middleware/WhatsAppSignatureVerificationMiddleware.cs` - Security
- **Bots**: `src/Bot/Bots/SemanticKernelBot.cs` - AI conversation handling
- **Configuration**: `src/Bot/appsettings.json` - Service configuration
- **Logging**: Serilog with console and file output to `logs/` directory

### Database Operations
```bash
# Add new migration (MSSQL)
dotnet ef migrations add <MigrationName> --project src/Migrators/Migrators.MSSQL --startup-project src/Server.UI

# Update database
dotnet ef database update --project src/Migrators/Migrators.MSSQL --startup-project src/Server.UI

# For PostgreSQL
dotnet ef database update --project src/Migrators/Migrators.PostgreSQL --startup-project src/Server.UI

# For SQLite
dotnet ef database update --project src/Migrators/Migrators.SqLite --startup-project src/Server.UI
```

### Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Application.UnitTests
dotnet test tests/Application.IntegrationTests
```

## Key Domain Entities

### Core Entities
- **Issue**: Main entity for tracking issues with categories, priorities, attachments
- **Attachment**: File attachments with virus scanning status
- **EventLog**: Audit trail for issue lifecycle events
- **Contact**: Contact management for issue reporters
- **Product**: Product catalog for categorizing issues

### Identity & Security
- **ApplicationUser**: Extended user entity with tenant support
- **LoginAudit**: Security monitoring and login tracking
- **UserLoginRiskSummary**: Risk assessment for user logins

## CQRS Implementation Patterns

### Database Access Pattern (CRITICAL)
**Always use `IApplicationDbContextFactory` for database access in handlers:**

```csharp
public class YourHandler : IRequestHandler<YourRequest, Result<YourResponse>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    
    public async Task<Result<YourResponse>> Handle(YourRequest request, CancellationToken ct)
    {
        await using var db = await _dbContextFactory.CreateAsync(ct);
        // Database operations here
        await db.SaveChangesAsync(ct);
        return Result<YourResponse>.Success(data);
    }
}
```

### Command Pattern
- Commands go in `Application/Features/{Feature}/Commands/`
- Use `ICacheInvalidatorRequest<Result<T>>` for commands that modify data
- Include FluentValidation validators

### Query Pattern
- Queries go in `Application/Features/{Feature}/Queries/`
- Use `ICacheableRequest<Result<T>>` for cacheable queries
- Use `ProjectTo<TDto>()` for efficient data mapping

## UI Development Guidelines

### Component Structure
```
Pages/{Feature}/
├── {Feature}.razor              # Main page
├── Create{Entity}.razor         # Create dialog/page
├── Edit{Entity}.razor          # Edit dialog/page
├── View{Entity}.razor          # View details
└── Components/                 # Reusable components
```

### Standard Injections
```csharp
@inject IMediator Mediator
@inject ISnackbar Snackbar
@inject IDialogService DialogService
@inject IPermissionService PermissionService
@inject IStringLocalizer<YourComponent> L
@inject NavigationManager Navigation
```

### Authorization
```csharp
@attribute [Authorize(Policy = Permissions.Feature.Action)]
```

## Configuration

### Database Providers
Support for multiple database providers via `DatabaseSettings`:
- `mssql` - SQL Server
- `postgresql` - PostgreSQL  
- `sqlite` - SQLite

### Key Configuration Sections
- `DatabaseSettings`: Database provider and connection
- `IdentitySettings`: Authentication configuration
- `AISettings`: AI service configuration for bot
- `SmtpClientOptions`: Email settings

## Bot Integration

### Bot Framework Setup
The Bot project uses Microsoft Bot Framework with:
- **Semantic Kernel**: For AI-powered conversations
- **WhatsApp Channel**: Via Azure Communication Services or CM.com
- **Issue Intake**: Automated issue creation from conversations

### Bot Components
- `SemanticKernelBot`: Main bot logic with AI capabilities
- Plugins for Wikipedia, Image processing, Issue intake
- State management for conversation context

## Security & Permissions

### Permission System
- Permissions defined in `Application/Common/Security/Permissions/`
- Feature-specific permissions (e.g., `Contacts.Create`, `Products.View`)
- Role-based access control with tenant isolation

### Security Features
- **Security Analysis Service**: Risk assessment and threat detection
- **Login Auditing**: Comprehensive login monitoring
- **Multi-factor Authentication**: Built-in 2FA support

## Key Interfaces & Services

### Application Layer Interfaces
- `IApplicationDbContext`: Database context interface
- `IApplicationDbContextFactory`: Context factory (preferred for handlers)
- `IPermissionService`: Permission checking
- `ISecurityAnalysisService`: Security risk assessment
- `IGeolocationService`: IP geolocation
- `IMailService`: Email notifications

### Infrastructure Services
- `SecurityAnalysisService`: Implements security risk analysis
- `GeolocationService`: IP-based location services
- `UserService`: User management operations
- `TenantService`: Multi-tenant operations

## Development Best Practices

### Dependency Rules (CRITICAL)
```
UI → Application → Domain
Infrastructure → Application → Domain
```

**Never inject Infrastructure services directly in UI layer**

### Result Pattern
Use `Result<T>` pattern for all operations that can fail:
```csharp
public async Task<Result<EntityDto>> GetEntityAsync(int id)
{
    try
    {
        // Logic here
        return Result<EntityDto>.Success(entity);
    }
    catch (Exception ex)
    {
        return Result<EntityDto>.Failure(ex.Message);
    }
}
```

### Caching Strategy
- Use `ICacheableRequest<T>` for queries
- Use `ICacheInvalidatorRequest<T>` for commands
- FusionCache integration for performance

## Testing Strategy

### Test Projects
- `Application.UnitTests`: Business logic unit tests
- `Application.IntegrationTests`: Integration tests with database
- `Domain.UnitTests`: Domain entity tests
- `Infrastructure.UnitTests`: Service implementation tests

### Test Patterns
- Use TestBase for integration tests with database setup
- Mock `IApplicationDbContextFactory` in unit tests
- Use FluentAssertions for readable assertions

## Docker & Deployment

### Docker Support
- Multi-stage Dockerfile optimized for .NET 9
- Docker Compose with SQL Server support
- Environment variable configuration

### Key Environment Variables
- `UseInMemoryDatabase`: Toggle in-memory database
- `DatabaseSettings__DBProvider`: Database provider selection
- `DatabaseSettings__ConnectionString`: Database connection

## PRD Context

This project implements a WhatsApp Issue Intake Service as described in the PRD:
- Phase 1: WhatsApp message intake, LLM extraction, persistence
- Supports English and Afrikaans
- Azure Bot Service integration
- POPIA compliance for South African data protection
- Security analysis and risk assessment capabilities

## Multi-Language Support

The application supports internationalization with:
- Resource files in `Application/Resources/` and `Server.UI/Resources/`
- String localization via `IStringLocalizer<T>`
- Support for English, Afrikaans, German, and Chinese