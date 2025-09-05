# IssueManager API

This is a RESTful API for managing issues in the IssueManager system. The API provides full CRUD operations for issues and integrates with the Clean Architecture Blazor application.

## Database Configuration

The API uses the **same SQLite database** as the main Blazor UI application: `BlazorDashboardDb.db`. This ensures data consistency across all components of the system.
{
  "DatabaseSettings": {
    "DBProvider": "sqlite",
    "ConnectionString": "Data Source=BlazorDashboardDb.db"
  }
}
## API Endpoints

### Issues Controller

#### GET /api/issues
Retrieves a paginated list of issues with optional filtering.

**Query Parameters:**
- `pageNumber` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Page size (default: 10) 
- `keyword` (string, optional): Search keyword
- `orderBy` (string, optional): Order by field (default: "Created")
- `sortDirection` (string, optional): Sort direction - "Ascending" or "Descending" (default: "Descending")
- `status` (IssueStatus, optional): Filter by status (New, InProgress, Resolved, Closed, OnHold)
- `priority` (IssuePriority, optional): Filter by priority (Low, Medium, High, Critical)
- `category` (IssueCategory, optional): Filter by category (Technical, Billing, General, Feature)

**Response:** `PaginatedData<IssueListDto>`

#### GET /api/issues/{id}
Retrieves a specific issue by ID.

**Parameters:**
- `id` (Guid): Issue ID

**Response:** `Result<IssueDto>`

#### POST /api/issues
Creates a new issue.

**Request Body:** `CreateIssueCommand`{
  "title": "string",
  "description": "string", 
  "category": "Technical|Billing|General|Feature",
  "priority": "Low|Medium|High|Critical",
  "status": "New|InProgress|Resolved|Closed|OnHold",
  "reporterContactId": 0,
  "channel": "string",
  "product": "string",
  "severity": "string", 
  "summary": "string",
  "consentFlag": true
}
**Response:** `Result<Guid>` (Created issue ID)

#### PUT /api/issues/{id}
Updates an existing issue.

**Parameters:**
- `id` (Guid): Issue ID

**Request Body:** `UpdateIssueCommand`{
  "id": "00000000-0000-0000-0000-000000000000",
  "title": "string",
  "description": "string",
  "category": "Technical|Billing|General|Feature", 
  "priority": "Low|Medium|High|Critical",
  "status": "New|InProgress|Resolved|Closed|OnHold",
  "reporterContactId": 0,
  "channel": "string",
  "product": "string",
  "severity": "string",
  "summary": "string", 
  "consentFlag": true,
  "duplicateOfId": "00000000-0000-0000-0000-000000000000"
}
**Response:** `Result<Guid>`

#### DELETE /api/issues/{id}
Deletes an issue.

**Parameters:**
- `id` (Guid): Issue ID

**Response:** `Result<Guid>`

## Data Models

### IssueDto
Complete issue details including:
- Basic information (ID, reference number, title, description)
- Classification (category, priority, status)
- Reporter information (contact ID, name, phone)
- Metadata (channel, product, severity, summary)
- WhatsApp specific data (source message IDs, metadata)
- Audit information (created/modified dates and users)

### IssueListDto  
Simplified issue information for list views including:
- Basic information (ID, reference number, title, description)
- Classification (category, priority, status)
- Reporter information
- Timestamps

### CreateIssueCommand
Request model for creating new issues with required fields:
- Title (required, max 200 chars)
- Description (required, max 2000 chars)  
- Category (required enum)
- Priority (required enum)
- Optional fields for reporter contact, channel, product, etc.

### UpdateIssueCommand
Request model for updating existing issues with all modifiable fields.

## Shared Database Architecture

The IssueManager system uses a **shared SQLite database** (`BlazorDashboardDb.db`) across all components:
┌─────────────────┐    API Calls    ┌─────────────────┐    Direct EF    ┌─────────────────┐
│ IssueManager.Bot│────────────────►│ IssueManager.Api│────────────────►│ BlazorDashboardDb│
└─────────────────┘                 └─────────────────┘                 │    (SQLite)     │
                                                        ┌─────────────────┤                 │
                                    ┌─────────────────┐ │  Direct EF      │ - Issues        │
                                    │ IssueManager.UI │─┘                 │ - Contacts      │
                                    │ (Blazor Server) │                   │ - Users         │
                                    └─────────────────┘                   │ - Audit Trails  │
                                                                         └─────────────────┘
**Benefits:**
- **Data Consistency**: All components work with the same data
- **Real-time Updates**: Changes via API are immediately visible in UI
- **Simplified Deployment**: Single database file to manage
- **Development Efficiency**: No data synchronization issues

## Error Handling

The API returns appropriate HTTP status codes:
- `200 OK` - Successful requests
- `201 Created` - Successful creation
- `400 Bad Request` - Invalid request data or validation errors
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server errors

Error responses include detailed error messages in the `Result` object format.

## Authentication & Authorization

The API integrates with the existing Clean Architecture Blazor authentication system. Authentication is currently disabled by default for development but can be enabled via configuration:
{
  "Authentication": {
    "RequireAuthentication": true,
    "DefaultScheme": "Bearer"
  }
}
## CORS Configuration

The API is configured to allow cross-origin requests from the Bot and UI applications:
{
  "CorsSettings": {
    "AllowedOrigins": [
      "https://localhost:7000",
      "https://localhost:7002", 
      "http://localhost:3000"
    ]
  }
}
## Getting Started

1. Ensure the shared database exists (created by the UI application)
2. Configure the same connection string in both API and UI projects
3. Run database migrations if needed (handled by UI application)
4. Start the API project
5. Access Swagger UI at `/swagger` for interactive API documentation

## Health Monitoring

- **Health Check Endpoint**: `GET /health`
- **Swagger Documentation**: Available at the root URL in development
- **Structured Logging**: Comprehensive request/response logging
- **Performance Monitoring**: Built-in request timing and metrics