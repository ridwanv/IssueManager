# IssueManager System Integration Test Guide

## Overview
This guide covers testing the complete integration between the IssueManager.Api and IssueManager.Bot after the refactoring to use API-based communication.

## System Architecture

```
┌─────────────────┐    HTTP API    ┌─────────────────┐    CQRS/EF    ┌─────────────────┐
│ IssueManager.Bot│───────────────►│ IssueManager.Api│──────────────►│ Database        │
│                 │                │                 │               │                 │
│ - WhatsApp Bot  │                │ - REST API      │               │ - Issues        │
│ - Semantic AI   │                │ - Swagger UI    │               │ - Contacts      │
│ - Issue Intake  │                │ - CRUD Ops      │               │ - Attachments   │
└─────────────────┘                └─────────────────┘               └─────────────────┘
```

## Pre-Testing Setup

### 1. Database Preparation
Ensure the database is set up with sample data:
```sql
-- Example test issues
INSERT INTO Issues (Id, ReferenceNumber, Title, Description, Category, Priority, Status, Created)
VALUES 
  (NEWID(), 'ISS-2025-001', 'Test Issue 1', 'Sample issue for testing', 1, 2, 1, GETDATE()),
  (NEWID(), 'ISS-2025-002', 'Test Issue 2', 'Another sample issue', 2, 3, 2, GETDATE());
```

### 2. Configuration Check
Verify configuration files are properly set:

**IssueManager.Api/appsettings.json:**
```json
{
  "DatabaseSettings": {
    "DBProvider": "sqlite",
    "ConnectionString": "Data Source=IssueManagerApi.db"
  }
}
```

**IssueManager.Bot/appsettings.json:**
```json
{
  "IssueManagerApi": {
    "BaseUrl": "https://localhost:7001"
  }
}
```

## Testing Scenarios

### 1. API Standalone Testing

#### Start the API
```bash
cd src/IssueManager.Api
dotnet run
```

#### Test API Endpoints
Open browser to `https://localhost:7001` for Swagger UI

**Test Cases:**
1. **GET /health** - Should return healthy status
2. **GET /api/issues** - Should return paginated issues
3. **GET /api/issues/{id}** - Should return specific issue
4. **POST /api/issues** - Should create new issue
5. **PUT /api/issues/{id}** - Should update existing issue
6. **DELETE /api/issues/{id}** - Should delete issue

**Sample POST Request:**
```json
{
  "title": "API Test Issue",
  "description": "Created via API testing",
  "category": 1,
  "priority": 2,
  "status": 1,
  "channel": "API",
  "product": "Test Product",
  "severity": "Medium",
  "summary": "Test issue summary",
  "consentFlag": true
}
```

### 2. Bot Integration Testing

#### Start Both Services
1. Start API: `cd src/IssueManager.Api && dotnet run`
2. Start Bot: `cd src/Bot && dotnet run`

#### Test Bot API Communication

**Test Cases:**
1. **Issue Creation via Bot**
   - Trigger issue intake flow in Bot
   - Verify Bot calls API correctly
   - Check logs for API communication
   - Confirm issue appears in database

2. **Issue Status Checking**
   - Use Bot to check status of existing issue
   - Verify API call returns correct data
   - Check response formatting in Bot

3. **Duplicate Detection**
   - Create issue with similar details
   - Test duplicate detection via API search
   - Verify Bot handles API responses correctly

4. **Error Handling**
   - Stop API service
   - Test Bot behavior when API is unavailable
   - Verify graceful error messages

### 3. End-to-End Testing

#### Complete Issue Lifecycle
1. **Creation**: Create issue via Bot → API → Database
2. **Retrieval**: Get issue via Bot → API → Database  
3. **Update**: Update issue via API directly
4. **Status Check**: Check updated status via Bot
5. **Deletion**: Delete issue via API

#### Load Testing
1. Create multiple issues rapidly via Bot
2. Monitor API performance and logs
3. Verify database consistency
4. Check Bot timeout handling

## Verification Points

### 1. API Layer Verification
- [ ] All endpoints respond correctly
- [ ] Proper HTTP status codes returned
- [ ] Request/response validation working
- [ ] Error handling functioning
- [ ] Swagger documentation accessible

### 2. Bot Integration Verification
- [ ] Bot successfully calls API endpoints
- [ ] HTTP client properly configured
- [ ] API responses correctly parsed
- [ ] Error handling for API failures
- [ ] Timeouts handled gracefully

### 3. Data Consistency Verification
- [ ] Issues created via Bot appear in database
- [ ] All required fields populated correctly
- [ ] Enum values properly mapped
- [ ] Timestamps accurate
- [ ] Reference numbers unique

### 4. Performance Verification
- [ ] API response times acceptable (<500ms)
- [ ] Bot processes messages without timeout
- [ ] Database queries optimized
- [ ] Memory usage stable
- [ ] No connection leaks

## Troubleshooting Guide

### Common Issues

#### 1. "Unable to resolve service for type IStringLocalizer"
**Solution:** Ensure API Program.cs includes localization setup

#### 2. Bot Cannot Connect to API
**Check:**
- API is running on correct port (7001)
- CORS settings include Bot origin
- Firewall not blocking connections
- Bot BaseUrl configuration correct

#### 3. Database Connection Issues
**Check:**
- Connection string in both projects
- Database file permissions
- EF migrations applied
- SQLite file accessible

#### 4. Authentication Errors
**Check:**
- Authentication disabled in API for testing
- Bot not sending authentication headers
- CORS policy allows credentials if needed

### Debugging Tips

1. **Enable Detailed Logging**
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Debug",
         "Microsoft.AspNetCore": "Information",
         "IssueManager": "Debug"
       }
     }
   }
   ```

2. **Monitor HTTP Traffic**
   - Use Fiddler or similar tool
   - Check request/response details
   - Verify headers and body content

3. **Database Inspection**
   - Use SQLite browser for development
   - Check issue records after operations
   - Verify foreign key relationships

## Success Criteria

### ✅ Integration Successful When:
1. Bot can create issues via API without errors
2. All CRUD operations work through API
3. Error handling is graceful and informative  
4. Performance meets acceptable thresholds
5. Data consistency maintained across operations
6. Logs show proper API communication flow

### 📊 Metrics to Monitor:
- API response times
- Bot message processing times
- Database query performance
- Memory usage patterns
- Error rates and types

## Production Readiness Checklist

- [ ] All tests passing
- [ ] Error handling comprehensive
- [ ] Logging properly configured
- [ ] Performance benchmarks met
- [ ] Security considerations addressed
- [ ] Deployment scripts prepared
- [ ] Monitoring setup planned
- [ ] Documentation complete

## Next Steps

1. **Performance Optimization**
   - Implement caching in API
   - Optimize database queries
   - Add compression for API responses

2. **Security Hardening**
   - Add authentication/authorization
   - Implement rate limiting
   - Add input validation

3. **Monitoring**
   - Add health checks
   - Implement metrics collection
   - Set up alerting

4. **Deployment**
   - Containerize applications
   - Set up CI/CD pipeline
   - Plan production deployment