# Data Models

## Issue

**Purpose:** Primary entity for tracking customer issues with full lifecycle management, attachments, and categorization

**Key Attributes:**
- Id: Guid - Primary identifier for multi-tenant scenarios
- Title: string - Issue summary extracted from WhatsApp or manual entry
- Description: string - Detailed issue content with rich text support
- Category: IssueCategory enum - Product categorization for routing
- Priority: IssuePriority enum - Business priority level (Low, Medium, High, Critical)
- Status: IssueStatus enum - Workflow state (New, InProgress, Resolved, Closed)
- ReporterContactId: Guid - Link to contact who reported the issue
- AssignedUserId: Guid? - Optional assignment to system user
- ProductId: Guid? - Optional product association
- CreatedAt: DateTime - Issue creation timestamp
- UpdatedAt: DateTime - Last modification timestamp
- TenantId: Guid - Multi-tenant isolation

**TypeScript Interface:**
```typescript
interface Issue {
  id: string;
  title: string;
  description: string;
  category: IssueCategory;
  priority: IssuePriority;
  status: IssueStatus;
  reporterContactId: string;
  assignedUserId?: string;
  productId?: string;
  createdAt: string;
  updatedAt: string;
  tenantId: string;
  attachments?: Attachment[];
  eventLogs?: EventLog[];
}

enum IssueCategory {
  Technical = 'technical',
  Billing = 'billing',
  General = 'general',
  Feature = 'feature'
}

enum IssuePriority {
  Low = 'low',
  Medium = 'medium',
  High = 'high',
  Critical = 'critical'
}

enum IssueStatus {
  New = 'new',
  InProgress = 'in_progress',
  Resolved = 'resolved',
  Closed = 'closed'
}
```

**Relationships:**
- One-to-Many with Attachment (issue attachments)
- One-to-Many with EventLog (audit trail)
- Many-to-One with Contact (reporter)
- Many-to-One with ApplicationUser (assignee)
- Many-to-One with Product (categorization)

## Attachment

**Purpose:** File attachments with security scanning and metadata for issues and conversations

**Key Attributes:**
- Id: Guid - Primary identifier
- IssueId: Guid - Parent issue reference
- FileName: string - Original file name with extension
- ContentType: string - MIME type for proper handling
- FileSize: long - Size in bytes for validation
- StoragePath: string - Azure Blob Storage path
- VirusScanStatus: ScanStatus enum - Security scan result
- UploadedAt: DateTime - Upload timestamp
- UploadedByUserId: Guid - User who uploaded (for audit)
- TenantId: Guid - Multi-tenant isolation

**TypeScript Interface:**
```typescript
interface Attachment {
  id: string;
  issueId: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  storagePath: string;
  virusScanStatus: ScanStatus;
  uploadedAt: string;
  uploadedByUserId: string;
  tenantId: string;
}

enum ScanStatus {
  Pending = 'pending',
  Clean = 'clean',
  Infected = 'infected',
  Failed = 'failed'
}
```

**Relationships:**
- Many-to-One with Issue (parent issue)
- Many-to-One with ApplicationUser (uploader)

## Contact

**Purpose:** Contact management for issue reporters from WhatsApp and manual entry

**Key Attributes:**
- Id: Guid - Primary identifier
- Name: string - Contact display name
- PhoneNumber: string? - WhatsApp phone number (E.164 format)
- Email: string? - Email address for notifications
- PreferredLanguage: Language enum - Communication language preference
- IsActive: bool - Soft delete flag
- CreatedAt: DateTime - First contact timestamp
- LastContactAt: DateTime - Most recent interaction
- TenantId: Guid - Multi-tenant isolation

**TypeScript Interface:**
```typescript
interface Contact {
  id: string;
  name: string;
  phoneNumber?: string;
  email?: string;
  preferredLanguage: Language;
  isActive: boolean;
  createdAt: string;
  lastContactAt: string;
  tenantId: string;
  issues?: Issue[];
}

enum Language {
  English = 'en',
  Afrikaans = 'af'
}
```

**Relationships:**
- One-to-Many with Issue (reported issues)

## EventLog

**Purpose:** Comprehensive audit trail for issue lifecycle events and system actions

**Key Attributes:**
- Id: Guid - Primary identifier
- IssueId: Guid - Related issue
- EventType: EventType enum - Type of event occurred
- Description: string - Human-readable event description
- OldValue: string? - Previous state (for changes)
- NewValue: string? - New state (for changes)
- UserId: Guid? - User who triggered event (null for system events)
- Timestamp: DateTime - When event occurred
- TenantId: Guid - Multi-tenant isolation

**TypeScript Interface:**
```typescript
interface EventLog {
  id: string;
  issueId: string;
  eventType: EventType;
  description: string;
  oldValue?: string;
  newValue?: string;
  userId?: string;
  timestamp: string;
  tenantId: string;
}

enum EventType {
  Created = 'created',
  StatusChanged = 'status_changed',
  Assigned = 'assigned',
  CommentAdded = 'comment_added',
  AttachmentAdded = 'attachment_added',
  PriorityChanged = 'priority_changed'
}
```

**Relationships:**
- Many-to-One with Issue (parent issue)
- Many-to-One with ApplicationUser (event trigger)
