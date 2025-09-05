# IssueManager System - Shared Database Architecture

## Overview

The IssueManager system now uses a **unified SQLite database** (`BlazorDashboardDb.db`) shared across all components, ensuring data consistency and real-time synchronization between the Bot, API, and UI applications.

## Architecture Diagram

```
                              ┌─────────────────────────────────────────────────────────┐
                              │                 BlazorDashboardDb.db                    │
                              │                    (SQLite)                             │
                              │                                                         │
                              │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
                              │  │   Issues    │  │  Contacts   │  │    Users    │     │
                              │  └─────────────┘  └─────────────┘  └─────────────┘     │
                              │                                                         │
                              │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
                              │  │ Attachments │  │ AuditTrails │  │    Roles    │     │
                              │  └─────────────┘  └─────────────┘  └─────────────┘     │
                              └─────────────────────────────────────────────────────────┘
                                           ▲                    ▲                    ▲
                                           │                    │                    │
                                    EF Core │             EF Core │             HTTP API │
                                           │                    │                    │
┌─────────────────┐    HTTP API     ┌─────────────────┐  EF Core ┌─────────────────┐
│ IssueManager.Bot│───────────────▶ │ IssueManager.Api│─────────▶│ IssueManager.UI │
│                 │                 │                 │          │ (Blazor Server) │
│ - WhatsApp Bot  │                 │ - REST API      │          │ - Admin UI      │
│ - Issue Intake  │                 │ - CRUD Ops      │          │ - User Mgmt     │
│ - AI Assistant  │                 │ - Swagger UI    │          │ - Reports       │
└─────────────────┘                 └─────────────────┘          └─────────────────┘
```

## Component Configuration

### 1. IssueManager.UI (Blazor Server) - Primary Database Owner
**File**: `src/Server.UI/appsettings.json`
```json
{
  "DatabaseSettings": {
    "DBProvider": "sqlite",
    "ConnectionString": "Data Source=BlazorDashboardDb.db"
  }
}
```
**Role**: 
- Creates and manages the database schema
- Handles Entity Framework migrations
- Primary administrative interface

### 2. IssueManager.Api - Shared Database Consumer
**File**: `src/IssueManager.Api/appsettings.json`
```json
{
  "DatabaseSettings": {
    "DBProvider": "sqlite", 
    "ConnectionString": "Data Source=BlazorDashboardDb.db"
  }
}
```
**Role**:
- Provides REST API access to shared data
- Serves WhatsApp Bot and external integrations
- Read/write access via CQRS pattern

### 3. IssueManager.Bot - API Consumer (No Direct DB Access)
**File**: `src/Bot/appsettings.json`
```json
{
  "IssueManagerApi": {
    "BaseUrl": "https://localhost:7001"
  }
}
```
**Role**:
- Consumes API for all data operations
- No direct database access
- Independent deployment capability

## Data Flow Patterns

### 1. Issue Creation via Bot
```
User (WhatsApp) → Bot → API → Database ← UI (Real-time updates)
```

### 2. Issue Management via UI
```
Admin (Browser) → UI → Database → API (Data available immediately)
```

### 3. API Direct Access
```
External System → API → Database ← UI (Immediate synchronization)
```

## Benefits of Shared Database Architecture

### ✅ **Data Consistency**
- Single source of truth for all data
- No data synchronization issues
- Immediate consistency across all applications

### ✅ **Real-time Updates**
- Changes via API immediately visible in UI
- Bot-created issues appear instantly in admin interface
- No polling or message queues needed

### ✅ **Simplified Deployment**
- Single database file to manage
- Consistent backup and restore procedures
- No complex data replication setup

### ✅ **Development Efficiency**
- Shared data models and schema
- No data mapping between systems
- Unified testing against single database

### ✅ **Cost Effectiveness**
- Single database instance
- Reduced infrastructure complexity
- Lower maintenance overhead

## Database Schema

### Core Tables
```sql
-- Issues (Primary entity)
CREATE TABLE Issues (
    Id TEXT PRIMARY KEY,
    ReferenceNumber TEXT UNIQUE NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT NOT NULL,
    Category INTEGER NOT NULL,
    Priority INTEGER NOT NULL,
    Status INTEGER NOT NULL,
    ReporterContactId INTEGER,
    Channel TEXT,
    Product TEXT,
    Severity TEXT,
    Summary TEXT,
    SourceMessageIds TEXT,
    WhatsAppMetadata TEXT,
    ConsentFlag INTEGER NOT NULL,
    DuplicateOfId TEXT,
    TenantId TEXT NOT NULL,
    Created TEXT,
    CreatedBy TEXT,
    LastModified TEXT,
    LastModifiedBy TEXT
);

-- Contacts (Reporter information)
CREATE TABLE Contacts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT,
    Description TEXT,
    Email TEXT,
    PhoneNumber TEXT,
    Country TEXT,
    TenantId TEXT NOT NULL,
    Created TEXT,
    CreatedBy TEXT,
    LastModified TEXT,
    LastModifiedBy TEXT
);

-- Users (Identity and authentication)
CREATE TABLE AspNetUsers (
    Id TEXT PRIMARY KEY,
    UserName TEXT,
    Email TEXT,
    PhoneNumber TEXT,
    -- Additional identity fields...
);
```

### Indexes for Performance
```sql
-- Optimized for API queries
CREATE INDEX IX_Issues_Status ON Issues(Status);
CREATE INDEX IX_Issues_Priority ON Issues(Priority);
CREATE INDEX IX_Issues_Category ON Issues(Category);
CREATE INDEX IX_Issues_Created ON Issues(Created);
CREATE INDEX IX_Issues_ReferenceNumber ON Issues(ReferenceNumber);

-- Optimized for Bot queries
CREATE INDEX IX_Contacts_PhoneNumber ON Contacts(PhoneNumber);
CREATE INDEX IX_Issues_ReporterContactId ON Issues(ReporterContactId);
```

## Performance Considerations

### SQLite Strengths
- **Fast Reads**: Excellent for read-heavy workloads
- **Simple Deployment**: Single file database
- **ACID Compliance**: Reliable transactions
- **Cross-Platform**: Works on all deployment targets

### SQLite Limitations
- **Concurrent Writes**: Single writer limitation
- **Scalability**: Not suitable for high-concurrency scenarios
- **Remote Access**: File-based, not network database

### Optimization Strategies

#### 1. Connection Management
```csharp
// Optimized connection string
"Data Source=BlazorDashboardDb.db;Cache=Shared;Pooling=true;Journal Mode=WAL"
```

#### 2. Read-Only Connections
```csharp
// For read-heavy operations in API
services.AddDbContext<ReadOnlyDbContext>(options =>
    options.UseSqlite(connectionString, opt => opt.QueryTrackingBehavior(QueryTrackingBehavior.NoTracking)));
```

#### 3. Write Batching
```csharp
// Batch multiple operations
using var transaction = await context.Database.BeginTransactionAsync();
// Multiple operations...
await transaction.CommitAsync();
```

## Monitoring and Maintenance

### Health Checks
```csharp
// API health check includes database connectivity
services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");
```

### Logging
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Backup Strategy
```bash
# Automated backup script
#!/bin/bash
BACKUP_DIR="/backups"
DB_FILE="BlazorDashboardDb.db"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

# Create backup
cp "$DB_FILE" "$BACKUP_DIR/BlazorDashboardDb_$TIMESTAMP.db"

# Cleanup old backups (keep 7 days)
find "$BACKUP_DIR" -name "BlazorDashboardDb_*.db" -mtime +7 -delete
```

## Migration and Scaling Path

### Development to Production
1. **Start**: SQLite shared database
2. **Scale**: SQL Server/PostgreSQL
3. **Optimize**: Read replicas, caching
4. **Enterprise**: Microservices with event sourcing

### Database Migration Strategy
```json
{
  "DatabaseSettings": {
    "DBProvider": "mssql",
    "ConnectionString": "Server=prod-sql;Database=IssueManager;Integrated Security=true"
  }
}
```

## Troubleshooting Guide

### Common Issues

#### 1. Database Lock Errors
```
SQLite Error: Database is locked
```
**Solutions**:
- Check for long-running transactions
- Ensure proper connection disposal
- Consider connection string optimizations

#### 2. Performance Degradation
```
Slow query performance
```
**Solutions**:
- Add appropriate indexes
- Use read-only connections for queries
- Implement query optimization

#### 3. Concurrent Access Issues
```
SQLITE_BUSY errors
```
**Solutions**:
- Implement retry logic
- Use WAL mode journaling
- Consider database upgrade

### Diagnostic Queries
```sql
-- Check database size and statistics
SELECT 
    name,
    sql
FROM sqlite_master 
WHERE type='table';

-- Monitor table sizes
SELECT 
    name,
    COUNT(*) as row_count
FROM sqlite_master sm
CROSS JOIN pragma_table_info(sm.name) pti
GROUP BY sm.name;
```

## Security Considerations

### File System Security
- Ensure database file has appropriate permissions
- Implement regular automated backups
- Consider encryption at rest for sensitive data

### Application Security
- Use parameterized queries (Entity Framework handles this)
- Implement proper authentication/authorization
- Log all data access operations

## Success Metrics

### ✅ **System Health Indicators**
- [ ] Database file accessible by all applications
- [ ] No database lock errors in logs
- [ ] API health checks passing
- [ ] UI responsiveness maintained
- [ ] Bot operations completing successfully

### 📊 **Performance Benchmarks**
- Database queries: < 100ms average
- API response times: < 500ms
- UI page loads: < 2s
- Bot message processing: < 3s
- Concurrent users: 50+ without degradation

This shared database architecture provides a solid foundation for the IssueManager system while maintaining the flexibility to scale and evolve as requirements grow.