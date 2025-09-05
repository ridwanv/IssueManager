# IssueManager UI Project with API Support

This Blazor Server UI project now includes REST API endpoints for managing issues, with integrated Swagger documentation.

## Features Added

### API Controller
- **IssuesController**: Full CRUD operations for issues
  - `GET /api/issues` - Get paginated list of issues with filtering
  - `GET /api/issues/{id}` - Get specific issue by ID
  - `POST /api/issues` - Create new issue
  - `PUT /api/issues/{id}` - Update existing issue
  - `DELETE /api/issues/{id}` - Delete issue

### Swagger Documentation
- **Development Only**: Swagger UI is available at `/swagger` when running in development mode
- **Interactive Testing**: Test API endpoints directly from the browser
- **Auto-generated**: Documentation is generated from XML comments and controller attributes

## Accessing the API Documentation

1. **Start the application** in development mode:
   ```bash
   cd src/Server.UI
   dotnet run
   ```

2. **Open Swagger UI** in your browser:
   ```
   https://localhost:7002/swagger
   ```

3. **Explore and test** the API endpoints interactively

## API Features

### CORS Support
- Configured to allow cross-origin requests
- Supports all HTTP methods and headers
- Useful for integration with external applications

### Error Handling
- Comprehensive error responses
- Appropriate HTTP status codes
- Detailed error messages in development

### Documentation
- XML comments automatically included in Swagger
- Parameter descriptions and examples
- Response model documentation

## Integration

The API endpoints use the same:
- **Database**: Shared SQLite database with the Blazor UI
- **MediatR**: Same command/query handlers
- **Validation**: Same validation rules and business logic
- **Authentication**: Integrated with the existing identity system

## Development

- **Real-time Updates**: Changes via API are immediately visible in the Blazor UI
- **Shared Business Logic**: No duplication of validation or business rules
- **Consistent Data Models**: Same DTOs and commands used throughout

## Production Notes

- Swagger UI is disabled in production environments
- API endpoints remain available for external integrations
- Same security and authentication rules apply as the main UI