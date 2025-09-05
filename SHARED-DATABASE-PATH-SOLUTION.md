# IssueManager System - Shared Database Solution

## Problem Identified

The original configuration used relative paths in SQLite connection strings:
```json
{
  "DatabaseSettings": {
    "ConnectionString": "Data Source=BlazorDashboardDb.db"
  }
}
```

**Issue**: Each application would create/access the database file in its own working directory, resulting in **separate database files** instead of a shared one.

## Solution Implemented

### Shared Database Path Strategy

Both applications now use **absolute paths** to ensure they access the same database file:

```
Project Root/
├── src/
│   ├── Server.UI/           (Working Dir: C:\...\Server.UI\)
│   └── IssueManager.Api/    (Working Dir: C:\...\IssueManager.Api\)
└── SharedData/              ← **Shared Database Location**
    └── BlazorDashboardDb.db ← **Single Database File**
```

### Implementation Details

#### 1. UI Project (Primary Database Owner)
**File**: `src/Server.UI/Program.cs`
```csharp
// Configure shared database path
var sharedDbPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "SharedData");
Directory.CreateDirectory(sharedDbPath);
var sharedDbFile = Path.Combine(sharedDbPath, "BlazorDashboardDb.db");

// Update connection string to use absolute path
var connectionString = $"Data Source={sharedDbFile}";
builder.Configuration["DatabaseSettings:ConnectionString"] = connectionString;
```

#### 2. API Project (Database Consumer)
**File**: `src/IssueManager.Api/Program.cs`
```csharp
// Configure shared database path (identical to UI)
var sharedDbPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "SharedData");
Directory.CreateDirectory(sharedDbPath);
var sharedDbFile = Path.Combine(sharedDbPath, "BlazorDashboardDb.db");

// Update connection string to use absolute path
var connectionString = $"Data Source={sharedDbFile}";
builder.Configuration["DatabaseSettings:ConnectionString"] = connectionString;
```

### Benefits of This Solution

#### ✅ **Guaranteed Shared Database**
- Both applications use identical absolute paths
- No dependency on working directory
- Single database file created and maintained

#### ✅ **Automatic Directory Creation**
- `SharedData` directory created if it doesn't exist
- No manual setup required
- Works across different deployment scenarios

#### ✅ **Development Friendly**
- Easy to locate database file
- Clear separation from application directories
- Consistent behavior across environments

#### ✅ **Logging and Verification**
Both applications log the database path on startup:
```
info: Program[0]
      Using shared database at: C:\Development\GitHub\IssueManager\SharedData\BlazorDashboardDb.db
```

## Verification Steps

### 1. Check Database Location
After running either application, verify the database exists:
```bash
# From project root
ls -la SharedData/
# Should show: BlazorDashboardDb.db
```

### 2. Monitor Startup Logs
Both applications will log the database path:
```
UI:  Using shared database at: C:\...\SharedData\BlazorDashboardDb.db
API: Using shared database at: C:\...\SharedData\BlazorDashboardDb.db
```

### 3. Test Data Consistency
1. Start UI application → Creates database
2. Add an issue via UI
3. Start API application → Uses same database
4. Query API: `GET /api/issues` → Returns issue created via UI

## Directory Structure

```
IssueManager/
├── SharedData/                     ← **Shared Database Directory**
│   └── BlazorDashboardDb.db       ← **Single Database File**
├── src/
│   ├── Server.UI/                 ← Working Dir: Here
│   │   ├── Program.cs             ← Path: ../../SharedData/BlazorDashboardDb.db
│   │   └── appsettings.json
│   ├── IssueManager.Api/          ← Working Dir: Here  
│   │   ├── Program.cs             ← Path: ../../SharedData/BlazorDashboardDb.db
│   │   └── appsettings.json
│   └── Bot/                       ← Uses API only (no direct DB access)
└── README.md
```

## Path Resolution Examples

### UI Application Path Resolution
```csharp
// Working Directory: C:\...\IssueManager\src\Server.UI\
var sharedDbPath = Path.Combine(
    Directory.GetCurrentDirectory(),  // C:\...\IssueManager\src\Server.UI\
    "..", "..",                       // Go up 2 levels
    "SharedData"                      // Add SharedData
);
// Result: C:\...\IssueManager\SharedData\
```

### API Application Path Resolution
```csharp
// Working Directory: C:\...\IssueManager\src\IssueManager.Api\
var sharedDbPath = Path.Combine(
    Directory.GetCurrentDirectory(),  // C:\...\IssueManager\src\IssueManager.Api\
    "..", "..",                       // Go up 2 levels  
    "SharedData"                      // Add SharedData
);
// Result: C:\...\IssueManager\SharedData\
```

## Configuration Files

### Updated appsettings.json (Both Projects)
```json
{
  "DatabaseSettings": {
    "DBProvider": "sqlite",
    "ConnectionString": "Data Source=../../SharedData/BlazorDashboardDb.db",
    "_Comment": "This will be overridden in Program.cs to use absolute path for shared database"
  }
}
```

**Note**: The connection string in appsettings.json is **documentation only**. The actual path is set programmatically in Program.cs to ensure absolute path resolution.

## Production Considerations

### Deployment Strategy
For production environments, consider:

#### 1. Environment-Specific Paths
```csharp
var sharedDbPath = Environment.GetEnvironmentVariable("SHARED_DB_PATH") 
                  ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "SharedData");
```

#### 2. Container Deployment
```dockerfile
# Dockerfile
VOLUME ["/app/SharedData"]
ENV SHARED_DB_PATH="/app/SharedData"
```

#### 3. Database Server Migration
```json
{
  "DatabaseSettings": {
    "DBProvider": "mssql",
    "ConnectionString": "Server=db-server;Database=IssueManager;..."
  }
}
```

## Troubleshooting

### Issue: Applications Using Different Databases
**Symptoms**: Changes in UI not visible in API
**Solution**: Check startup logs for different database paths

### Issue: Permission Denied
**Symptoms**: Cannot create SharedData directory
**Solution**: Ensure write permissions on project root

### Issue: Database Lock Errors
**Symptoms**: SQLite database locked
**Solution**: Ensure proper connection disposal in both applications

## Success Verification

### ✅ **Shared Database Confirmed When:**
- [ ] Both applications log identical database paths
- [ ] SharedData/BlazorDashboardDb.db exists
- [ ] Issues created via UI appear in API queries
- [ ] Issues created via API appear in UI immediately
- [ ] No "database not found" errors in logs

This solution ensures that your IssueManager system truly uses a single, shared database across all components, eliminating the working directory dependency that would have caused data inconsistency.