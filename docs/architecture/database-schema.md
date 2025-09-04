# Database Schema

## Core Entity Tables

```sql
-- Issues table with full-text search support
CREATE TABLE Issues (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Description NTEXT NOT NULL,
    Category NVARCHAR(50) NOT NULL CHECK (Category IN ('technical', 'billing', 'general', 'feature')),
    Priority NVARCHAR(20) NOT NULL CHECK (Priority IN ('low', 'medium', 'high', 'critical')),
    Status NVARCHAR(20) NOT NULL CHECK (Status IN ('new', 'in_progress', 'resolved', 'closed')),
    ReporterContactId UNIQUEIDENTIFIER NOT NULL,
    AssignedUserId UNIQUEIDENTIFIER NULL,
    ProductId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    
    CONSTRAINT FK_Issues_Contacts FOREIGN KEY (ReporterContactId) 
        REFERENCES Contacts(Id),
    CONSTRAINT FK_Issues_Users FOREIGN KEY (AssignedUserId) 
        REFERENCES AspNetUsers(Id),
    CONSTRAINT FK_Issues_Products FOREIGN KEY (ProductId) 
        REFERENCES Products(Id)
);

-- Contacts table with phone number validation
CREATE TABLE Contacts (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL, -- E.164 format: +[1-9]\d{1,14}
    Email NVARCHAR(256) NULL,
    PreferredLanguage NVARCHAR(10) NOT NULL DEFAULT 'en' 
        CHECK (PreferredLanguage IN ('en', 'af')),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastContactAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TenantId UNIQUEIDENTIFIER NOT NULL
);

-- Attachments table with virus scanning status
CREATE TABLE Attachments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    IssueId UNIQUEIDENTIFIER NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    FileSize BIGINT NOT NULL,
    StoragePath NVARCHAR(500) NOT NULL,
    VirusScanStatus NVARCHAR(20) NOT NULL DEFAULT 'pending'
        CHECK (VirusScanStatus IN ('pending', 'clean', 'infected', 'failed')),
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UploadedByUserId UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    
    CONSTRAINT FK_Attachments_Issues FOREIGN KEY (IssueId) 
        REFERENCES Issues(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Attachments_Users FOREIGN KEY (UploadedByUserId) 
        REFERENCES AspNetUsers(Id)
);

-- Event logs for comprehensive audit trail
CREATE TABLE EventLogs (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    IssueId UNIQUEIDENTIFIER NOT NULL,
    EventType NVARCHAR(50) NOT NULL 
        CHECK (EventType IN ('created', 'status_changed', 'assigned', 'comment_added', 'attachment_added', 'priority_changed')),
    Description NVARCHAR(500) NOT NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    UserId UNIQUEIDENTIFIER NULL, -- NULL for system events
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    
    CONSTRAINT FK_EventLogs_Issues FOREIGN KEY (IssueId) 
        REFERENCES Issues(Id) ON DELETE CASCADE,
    CONSTRAINT FK_EventLogs_Users FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id)
);
```

## Performance Optimization Indexes

```sql
-- Primary query performance indexes
CREATE INDEX IX_Issues_Status_Priority ON Issues (Status, Priority) 
    INCLUDE (Title, CreatedAt, AssignedUserId);

CREATE INDEX IX_Issues_TenantId_Status ON Issues (TenantId, Status) 
    INCLUDE (Priority, CreatedAt, ReporterContactId);

CREATE INDEX IX_Issues_AssignedUser ON Issues (AssignedUserId, Status) 
    WHERE AssignedUserId IS NOT NULL;

CREATE INDEX IX_Issues_CreatedAt ON Issues (CreatedAt DESC);

-- Contact lookup optimization
CREATE INDEX IX_Contacts_PhoneNumber ON Contacts (PhoneNumber) 
    WHERE PhoneNumber IS NOT NULL;

CREATE INDEX IX_Contacts_TenantId_Active ON Contacts (TenantId, IsActive) 
    INCLUDE (Name, Email, PreferredLanguage);

-- Event log performance for audit queries
CREATE INDEX IX_EventLogs_IssueId_Timestamp ON EventLogs (IssueId, Timestamp DESC);

CREATE INDEX IX_EventLogs_TenantId_Timestamp ON EventLogs (TenantId, Timestamp DESC);
```
