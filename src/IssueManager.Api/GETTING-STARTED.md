# IssueManager API - Getting Started

## Overview
The IssueManager API provides RESTful endpoints for managing issues in the IssueManager system. This API serves as the backend for both the WhatsApp Bot and integrates with the Blazor UI using a **shared SQLite database**.

## Architecture Overview
┌─────────────────┐    HTTP API     ┌─────────────────┐    EF Core      ┌─────────────────┐
│ IssueManager.Bot│────────────────►│ IssueManager.Api│────────────────►│ BlazorDashboardDb│
└─────────────────┘                 └─────────────────┘                 │    (SQLite)     │
                                                        ┌─────────────────┤                 │
                                    ┌─────────────────┐ │  EF Core        │ Shared Database │
                                    │ IssueManager.UI │─┘                 │ - Issues        │
                                    │ (Blazor Server) │                   │ - Contacts      │
                                    └─────────────────┘                   │ - Users         │
                                                                         │ - Attachments   │
                                                                         └─────────────────┘
## Configuration

### Shared Database Setup
The API uses the **same SQLite database** as the Blazor UI application. This ensures data consistency and real-time synchronization across all components.

**Both projects use this configuration:**{
  "DatabaseSettings": {
    "DBProvider": "sqlite",
    "ConnectionString": "Data Source=BlazorDashboardDb.db"
  }
}
### CORS Configuration
Configure allowed origins for cross-origin requests from Bot and UI:
{
  "CorsSettings": {
    "AllowedOrigins": [
      "https://localhost:7000",  // Bot
      "https://localhost:7002",  // UI
      "http://localhost:3000"    // Development
    ]
  }
}
## Running the API

### Prerequisites
1. **.NET 9 SDK** installed
2. **Blazor UI application** must be set up first (creates the shared database)
3. **Database migrations** applied via the UI project

### Development Setup

#### 1. Ensure Database Exists
First, run the Blazor UI application to create and initialize the shared database:cd src/Server.UI
dotnet ef database update
dotnet runThis creates `BlazorDashboardDb.db` with all necessary tables and seed data.

#### 2. Start the APIcd src/IssueManager.Api
dotnet run
#### 3. Verify Setup
- **API**: Open browser to `https://localhost:7001`
- **Swagger UI**: Available at the root for interactive testing
- **Health Check**: `GET /health` should return healthy status

### Production Deployment

#### 1. Build Applications# Build UI (creates database)
cd src/Server.UI
dotnet build --configuration Release

# Build API
cd ../IssueManager.Api
dotnet build --configuration Release
#### 2. Deploy Database
Copy the `BlazorDashboardDb.db` file to your production environment.

#### 3. Deploy Applications# Publish UI
cd src/Server.UI
dotnet publish --configuration Release --output ./publish-ui

# Publish API
cd ../IssueManager.Api
dotnet publish --configuration Release --output ./publish-api
#### 4. Run Applications# Start UI (manages database)
cd publish-ui
dotnet CleanArchitecture.Blazor.Server.UI.dll

# Start API (uses shared database)
cd ../publish-api
dotnet IssueManager.Api.dll
## API Endpoints

### Health & Documentation
- **GET** `/health` - API health status
- **GET** `/` - Swagger UI (development only)

### Issues Management
- **GET** `/api/issues` - Get paginated list with filtering
- **GET** `/api/issues/{id}` - Get specific issue
- **POST** `/api/issues` - Create new issue
- **PUT** `/api/issues/{id}` - Update existing issue
- **DELETE** `/api/issues/{id}` - Delete issue

### Sample Requests

#### Create Issuecurl -X POST "https://localhost:7001/api/issues" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "API Test Issue",
    "description": "Created via API",
    "category": 1,
    "priority": 2,
    "status": 1,
    "channel": "API",
    "consentFlag": true
  }'
#### Get Issuescurl "https://localhost:7001/api/issues?pageSize=10&pageNumber=1"
## Integration Testing

### Test Shared Database Access

#### 1. Create Issue via APIcurl -X POST "https://localhost:7001/api/issues" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Issue",
    "description": "Testing shared database",
    "category": 1,
    "priority": 2
  }'
#### 2. Verify in Blazor UI
- Open `https://localhost:7002`
- Navigate to Issues page
- Confirm the issue appears immediately

#### 3. Test Bot Integration
- Configure Bot to use API: `"BaseUrl": "https://localhost:7001"`
- Create issue via WhatsApp Bot
- Verify issue appears in both API and UI

### Performance Testing# Load test with curl
for i in {1..10}; do
  curl -s "https://localhost:7001/api/issues" > /dev/null &
done
wait
## Troubleshooting

### Common Issues

#### 1. Database Lock Errors
**Symptoms**: SQLite database locked errors
**Causes**: Multiple applications accessing SQLite concurrently
**Solutions**:
- Ensure connection strings use `Pooling=false`
- Consider moving to SQL Server/PostgreSQL for production
- Implement retry logic for transient errors

#### 2. Database File Not Found
**Symptoms**: "Cannot open database file" errors
**Solutions**:
- Run UI application first to create database
- Check file permissions
- Verify working directory contains database file

#### 3. Migration Issues
**Symptoms**: "No such table" errors
**Solutions**:
- Run migrations via UI project: `dotnet ef database update`
- Check that both projects target same database file
- Verify Entity Framework configurations match

#### 4. API Cannot Connect to Database
**Check**:
- Database file exists in API working directory
- Connection string matches UI configuration
- No file locks or permission issues

### Database Management

#### View Database Contents
Use SQLite browser or command line:sqlite3 BlazorDashboardDb.db
.tables
SELECT * FROM Issues LIMIT 5;
#### Backup Databasecp BlazorDashboardDb.db BlazorDashboardDb.backup.db
#### Reset Database
1. Delete `BlazorDashboardDb.db`
2. Run UI application to recreate
3. Restart API

### Performance Considerations

#### SQLite Limitations
- **Concurrent Writes**: Limited to single writer
- **Scalability**: Suitable for small to medium applications
- **File Locking**: Can cause contention under high load

#### Optimization Tips
- Use connection pooling appropriately
- Implement read-only connections where possible
- Consider database upgrade path for production scaling

### Production Recommendations

#### Database Upgrade Path
For production environments with higher loads:

1. **SQL Server**:{
  "DatabaseSettings": {
    "DBProvider": "mssql",
    "ConnectionString": "Server=...;Database=IssueManager;..."
  }
   }
2. **PostgreSQL**:{
  "DatabaseSettings": {
    "DBProvider": "postgresql", 
    "ConnectionString": "Host=...;Database=IssueManager;..."
  }
}
#### Deployment Architecture
- **Development**: Shared SQLite file
- **Staging/Production**: Dedicated database server
- **High Availability**: Database clustering/replication

## Support and Documentation

### Additional Resources
- **Main Project Documentation**: See root README
- **Blazor UI Setup**: `src/Server.UI/README.md`
- **Bot Configuration**: `src/Bot/README.md`
- **Integration Testing**: `INTEGRATION-TEST-GUIDE.md`

### Getting Help
- **GitHub Issues**: Report bugs and feature requests
- **Documentation**: Check project wiki and docs
- **Contact**: support@issuemanager.com

## Success Metrics

### ✅ Successful Setup Indicators
- [ ] Shared database file exists and is accessible
- [ ] API health check returns 200 OK
- [ ] Swagger UI loads and displays endpoints
- [ ] Issues created via API appear in Blazor UI
- [ ] Bot can successfully call API endpoints
- [ ] No database lock or connection errors in logs

### 📊 Performance Benchmarks
- API response time: < 500ms for typical requests
- Database query time: < 100ms for single issue lookup
- Concurrent users: 10+ without significant degradation
- Memory usage: Stable under normal load