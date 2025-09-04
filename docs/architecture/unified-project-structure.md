# Unified Project Structure

```plaintext
IssueManager/
├── .github/                           # CI/CD workflows and templates
│   ├── workflows/
│   │   ├── ci.yml                    # Continuous integration pipeline
│   │   ├── deploy-staging.yml        # Staging deployment
│   │   └── deploy-production.yml     # Production deployment
│   ├── ISSUE_TEMPLATE/               # GitHub issue templates
│   └── pull_request_template.md     # PR template
├── .bmad-core/                       # AI agent configuration
│   ├── core-config.yaml             # Project configuration
│   ├── tasks/                       # Automated workflow tasks
│   ├── templates/                   # Document templates
│   └── checklists/                  # Quality assurance checklists
├── src/                             # Source code organized by Clean Architecture
│   ├── Domain/                      # Core business entities and rules
│   │   ├── Common/                  # Shared domain concepts
│   │   │   ├── BaseEntity.cs
│   │   │   ├── DomainEvent.cs
│   │   │   └── ValueObject.cs
│   │   ├── Entities/                # Business entities
│   │   │   ├── Issue.cs
│   │   │   ├── Contact.cs
│   │   │   ├── Attachment.cs
│   │   │   ├── EventLog.cs
│   │   │   └── Product.cs
│   │   ├── Events/                  # Domain events
│   │   │   ├── IssueCreatedDomainEvent.cs
│   │   │   ├── IssueAssignedDomainEvent.cs
│   │   │   └── AttachmentUploadedDomainEvent.cs
│   │   ├── Enums/                   # Domain enumerations
│   │   │   ├── IssueStatus.cs
│   │   │   ├── IssuePriority.cs
│   │   │   └── IssueCategory.cs
│   │   └── Exceptions/              # Domain-specific exceptions
│   │       └── IssueNotFoundException.cs
│   ├── Application/                 # Business logic and CQRS
│   │   ├── Common/                  # Shared application concerns
│   │   │   ├── Behaviours/          # MediatR pipeline behaviors
│   │   │   ├── Interfaces/          # Application interfaces
│   │   │   ├── Mappings/           # AutoMapper profiles
│   │   │   ├── Models/             # DTOs and view models
│   │   │   ├── Security/           # Security policies and permissions
│   │   │   └── Specifications/      # Query specifications
│   │   ├── Features/               # CQRS handlers by feature
│   │   │   ├── Issues/
│   │   │   │   ├── Commands/       # Issue commands (Create, Update, Delete)
│   │   │   │   │   ├── CreateIssue/
│   │   │   │   │   │   ├── CreateIssueCommand.cs
│   │   │   │   │   │   ├── CreateIssueCommandHandler.cs
│   │   │   │   │   │   └── CreateIssueCommandValidator.cs
│   │   │   │   │   └── UpdateIssue/
│   │   │   │   └── Queries/        # Issue queries (Get, Search, List)
│   │   │   │       ├── GetIssues/
│   │   │   │       └── GetIssueById/
│   │   │   ├── Contacts/
│   │   │   │   ├── Commands/
│   │   │   │   └── Queries/
│   │   │   ├── Attachments/
│   │   │   ├── Security/           # Security analysis features
│   │   │   └── Analytics/          # Dashboard and reporting
│   │   └── Resources/              # Localization resources
│   │       ├── Strings.resx        # English strings
│   │       └── Strings.af.resx     # Afrikaans strings
│   ├── Infrastructure/             # External integrations and data access
│   │   ├── Data/                   # Entity Framework configuration
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── ApplicationDbContextFactory.cs
│   │   │   ├── Configurations/     # Entity configurations
│   │   │   │   ├── IssueConfiguration.cs
│   │   │   │   ├── ContactConfiguration.cs
│   │   │   │   └── AttachmentConfiguration.cs
│   │   │   └── Interceptors/       # EF interceptors for auditing
│   │   ├── Services/               # External service implementations
│   │   │   ├── FileStorageService.cs
│   │   │   ├── EmailService.cs
│   │   │   ├── GeolocationService.cs
│   │   │   └── SecurityAnalysisService.cs
│   │   ├── Identity/               # Authentication and authorization
│   │   │   ├── ApplicationUser.cs
│   │   │   ├── IdentityService.cs
│   │   │   └── PermissionService.cs
│   │   └── ExternalServices/       # Third-party API integrations
│   │       ├── WhatsAppService.cs
│   │       ├── VirusScanService.cs
│   │       └── AIService.cs
│   ├── Server.UI/                  # Blazor Server web application
│   │   ├── Components/             # Reusable Blazor components
│   │   │   ├── Common/
│   │   │   │   ├── LoadingSpinner.razor
│   │   │   │   ├── ErrorBoundary.razor
│   │   │   │   └── ConfirmDialog.razor
│   │   │   ├── Issues/
│   │   │   │   ├── IssueCard.razor
│   │   │   │   ├── IssueList.razor
│   │   │   │   ├── IssueStatusBadge.razor
│   │   │   │   └── AttachmentUpload.razor
│   │   │   └── Layout/
│   │   │       ├── MainLayout.razor
│   │   │       ├── NavMenu.razor
│   │   │       └── LoginLayout.razor
│   │   ├── Pages/                  # Razor pages with routing
│   │   │   ├── Issues/
│   │   │   │   ├── Index.razor     # Issue list page
│   │   │   │   ├── Details.razor   # Issue details page
│   │   │   │   ├── Create.razor    # Create issue page
│   │   │   │   └── Edit.razor      # Edit issue page
│   │   │   ├── Contacts/
│   │   │   ├── Dashboard/
│   │   │   ├── Admin/
│   │   │   └── Account/
│   │   ├── Controllers/            # API controllers for external integrations
│   │   │   ├── ApiControllerBase.cs
│   │   │   ├── IssuesController.cs
│   │   │   ├── ContactsController.cs
│   │   │   └── WebhookController.cs
│   │   ├── Services/               # UI-specific services
│   │   │   ├── NotificationService.cs
│   │   │   └── StateService.cs
│   │   ├── Hubs/                   # SignalR hubs
│   │   │   └── NotificationHub.cs
│   │   ├── wwwroot/                # Static web assets
│   │   │   ├── css/
│   │   │   │   ├── app.css
│   │   │   │   └── mudblazor.css
│   │   │   ├── js/
│   │   │   │   └── app.js
│   │   │   ├── lib/                # Third-party libraries
│   │   │   └── favicon.ico
│   │   ├── Resources/              # UI localization resources
│   │   │   ├── Pages/
│   │   │   └── Components/
│   │   ├── appsettings.json        # Application configuration
│   │   ├── appsettings.Development.json
│   │   ├── appsettings.Production.json
│   │   ├── Program.cs              # Application entry point
│   │   └── IssueManager.UI.csproj  # Project file
│   ├── Bot/                        # WhatsApp Bot service
│   │   ├── Dialogs/               # Bot Framework dialog flows
│   │   │   ├── IssueCreationDialog.cs
│   │   │   └── MainDialog.cs
│   │   ├── Services/              # Bot-specific services
│   │   │   ├── SemanticKernelBot.cs
│   │   │   ├── IssueExtractionService.cs
│   │   │   └── ConversationStateService.cs
│   │   ├── Plugins/               # Semantic Kernel plugins
│   │   │   ├── IssuePlugin.cs
│   │   │   ├── WikipediaPlugin.cs
│   │   │   └── ImageProcessingPlugin.cs
│   │   ├── Controllers/           # Bot webhook endpoints
│   │   │   └── BotController.cs
│   │   ├── appsettings.json
│   │   ├── Program.cs
│   │   └── Bot.csproj
│   └── Migrators/                 # Database migration projects
│       ├── Migrators.MSSQL/
│       │   ├── Migrations/        # EF Core migrations for SQL Server
│       │   └── Migrators.MSSQL.csproj
│       ├── Migrators.PostgreSQL/
│       │   ├── Migrations/        # EF Core migrations for PostgreSQL
│       │   └── Migrators.PostgreSQL.csproj
│       └── Migrators.SqLite/
│           ├── Migrations/        # EF Core migrations for SQLite
│           └── Migrators.SqLite.csproj
├── tests/                         # Test projects organized by layer
│   ├── Domain.UnitTests/          # Domain entity and business rule tests
│   │   ├── Entities/
│   │   ├── Events/
│   │   └── ValueObjects/
│   ├── Application.UnitTests/     # Application service and handler tests
│   │   ├── Features/
│   │   │   ├── Issues/
│   │   │   └── Contacts/
│   │   └── Common/
│   ├── Application.IntegrationTests/ # Integration tests with database
│   │   ├── Features/
│   │   ├── TestBase.cs
│   │   └── DatabaseFixture.cs
│   ├── Infrastructure.UnitTests/   # Infrastructure service tests
│   │   └── Services/
│   └── Server.UI.IntegrationTests/ # End-to-end UI tests
│       ├── Pages/
│       └── Controllers/
├── docs/                          # Project documentation
│   ├── architecture.md           # This architecture document
│   ├── prd.md                    # Product requirements document
│   ├── api/                      # API documentation
│   │   └── openapi.json
│   ├── deployment/               # Deployment guides
│   │   ├── azure-setup.md
│   │   └── local-development.md
│   └── images/                   # Documentation images and diagrams
├── scripts/                      # Build and deployment scripts
│   ├── build.sh                  # Cross-platform build script
│   ├── deploy.sh                 # Deployment script
│   ├── seed-data.sql             # Database seed data
│   └── setup-dev.ps1             # Windows development setup
├── infrastructure/               # Infrastructure as Code
│   ├── azure/                    # Azure Resource Manager templates
│   │   ├── main.bicep
│   │   ├── app-service.bicep
│   │   ├── database.bicep
│   │   └── bot-service.bicep
│   └── docker/                   # Docker configuration
│       ├── Dockerfile.ui
│       ├── Dockerfile.bot
│       └── docker-compose.yml
├── .env.example                  # Environment variable template
├── .gitignore                    # Git ignore patterns
├── .editorconfig                 # Code formatting standards
├── Directory.Build.props         # MSBuild properties for all projects
├── IssueManager.slnx             # Visual Studio solution file
├── global.json                   # .NET SDK version specification
├── nuget.config                  # NuGet package source configuration
└── README.md                     # Project overview and setup instructions
```
