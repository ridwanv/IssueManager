# Coding Standards

## Critical Fullstack Rules
- **Database Access Pattern:** Always use `IApplicationDbContextFactory` for database access in handlers - never inject `IApplicationDbContext` directly
- **API Error Handling:** All API endpoints must use Result<T> pattern and return consistent error responses via ApiControllerBase
- **Tenant Isolation:** Never write queries without tenant filtering - use `ITenantService` for automatic tenant context injection
- **Blazor Component State:** Use `[Parameter]` for parent-child communication, scoped services for cross-component state sharing
- **Bot Message Processing:** All WhatsApp messages must be processed through Semantic Kernel plugins for consistent AI interpretation
- **File Upload Security:** All attachments must complete virus scanning before user access - check `VirusScanStatus` in all download endpoints
- **Permission Checks:** Use `[MustHavePermission]` attributes on controllers and `IPermissionService.HasPermissionAsync` in components
- **Multi-Database Compatibility:** Use GUID primary keys and avoid database-specific SQL in LINQ queries

## Naming Conventions
| Element | Frontend | Backend | Example |
|---------|----------|---------|---------|
| Blazor Components | PascalCase | - | `IssueCard.razor` |
| Command Handlers | - | PascalCase + Handler | `CreateIssueCommandHandler.cs` |
| API Endpoints | - | RESTful nouns | `/api/v1/issues/{id}` |
| Database Tables | - | PascalCase | `Issues`, `ContactDetails` |
| Entity Properties | PascalCase | PascalCase | `CreatedAt`, `TenantId` |
