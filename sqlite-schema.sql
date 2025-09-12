CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Contacts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Contacts" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Email" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "Country" TEXT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL
);

CREATE TABLE "DataProtectionKeys" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_DataProtectionKeys" PRIMARY KEY AUTOINCREMENT,
    "FriendlyName" TEXT NULL,
    "Xml" TEXT NULL
);

CREATE TABLE "PicklistSets" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PicklistSets" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Value" TEXT NULL,
    "Text" TEXT NULL,
    "Description" TEXT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL
);

CREATE TABLE "Products" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Products" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Brand" TEXT NULL,
    "Unit" TEXT NULL,
    "Price" TEXT NOT NULL,
    "Pictures" TEXT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL
);

CREATE TABLE "SystemLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SystemLogs" PRIMARY KEY AUTOINCREMENT,
    "Message" TEXT NULL,
    "MessageTemplate" TEXT NULL,
    "Level" TEXT NOT NULL,
    "TimeStamp" TEXT NOT NULL,
    "Exception" TEXT NULL,
    "UserName" TEXT NULL,
    "ClientIP" TEXT NULL,
    "ClientAgent" TEXT NULL,
    "Properties" TEXT NULL,
    "LogEvent" TEXT NULL
);

CREATE TABLE "Tenants" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Tenants" PRIMARY KEY,
    "Name" TEXT NULL,
    "Description" TEXT NULL
);

CREATE TABLE "AspNetRoles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
    "TenantId" TEXT NULL,
    "Description" TEXT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    "Name" TEXT NULL,
    "NormalizedName" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    CONSTRAINT "FK_AspNetRoles_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id")
);

CREATE TABLE "AspNetUsers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
    "DisplayName" TEXT NULL,
    "Provider" TEXT NULL,
    "TenantId" TEXT NULL,
    "ProfilePictureDataUrl" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "IsLive" INTEGER NOT NULL,
    "RefreshToken" TEXT NULL,
    "RefreshTokenExpiryTime" TEXT NOT NULL,
    "SuperiorId" TEXT NULL,
    "Created" TEXT NULL,
    "LastModified" TEXT NULL,
    "TimeZoneId" TEXT NULL,
    "LanguageCode" TEXT NULL,
    "UserName" TEXT NULL,
    "NormalizedUserName" TEXT NULL,
    "Email" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "EmailConfirmed" INTEGER NOT NULL,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" INTEGER NOT NULL,
    "TwoFactorEnabled" INTEGER NOT NULL,
    "LockoutEnd" TEXT NULL,
    "LockoutEnabled" INTEGER NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL,
    CONSTRAINT "FK_AspNetUsers_AspNetUsers_SuperiorId" FOREIGN KEY ("SuperiorId") REFERENCES "AspNetUsers" ("Id"),
    CONSTRAINT "FK_AspNetUsers_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id")
);

CREATE TABLE "AspNetRoleClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
    "Description" TEXT NULL,
    "Group" TEXT NULL,
    "RoleId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
    "Description" TEXT NULL,
    "UserId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AuditTrails" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AuditTrails" PRIMARY KEY AUTOINCREMENT,
    "UserId" TEXT NULL,
    "AuditType" TEXT NOT NULL,
    "TableName" TEXT NULL,
    "DateTime" TEXT NOT NULL,
    "OldValues" TEXT NULL,
    "NewValues" TEXT NULL,
    "AffectedColumns" TEXT NULL,
    "PrimaryKey" TEXT NOT NULL,
    "DebugView" TEXT NULL,
    "ErrorMessage" TEXT NULL,
    CONSTRAINT "FK_AuditTrails_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL
);

CREATE TABLE "Documents" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Documents" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NULL,
    "Description" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "Content" TEXT NULL,
    "IsPublic" INTEGER NOT NULL,
    "URL" TEXT NULL,
    "DocumentType" TEXT NOT NULL,
    "TenantId" TEXT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_Documents_AspNetUsers_CreatedBy" FOREIGN KEY ("CreatedBy") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Documents_AspNetUsers_LastModifiedBy" FOREIGN KEY ("LastModifiedBy") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Documents_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id")
);

CREATE TABLE "LoginAudits" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_LoginAudits" PRIMARY KEY AUTOINCREMENT,
    "LoginTimeUtc" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    "UserName" TEXT NOT NULL,
    "IpAddress" TEXT NULL,
    "BrowserInfo" TEXT NULL,
    "Region" TEXT NULL,
    "Provider" TEXT NULL,
    "Success" INTEGER NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_LoginAudits_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserLoginRiskSummaries" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_UserLoginRiskSummaries" PRIMARY KEY AUTOINCREMENT,
    "UserId" TEXT NOT NULL,
    "UserName" TEXT NOT NULL,
    "RiskLevel" TEXT NOT NULL,
    "RiskScore" INTEGER NOT NULL,
    "Description" TEXT NULL,
    "Advice" TEXT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_UserLoginRiskSummaries_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

CREATE UNIQUE INDEX "IX_AspNetRoles_TenantId_Name" ON "AspNetRoles" ("TenantId", "Name");

CREATE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE INDEX "IX_AspNetUsers_SuperiorId" ON "AspNetUsers" ("SuperiorId");

CREATE INDEX "IX_AspNetUsers_TenantId" ON "AspNetUsers" ("TenantId");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

CREATE INDEX "IX_AuditTrails_UserId" ON "AuditTrails" ("UserId");

CREATE INDEX "IX_Documents_CreatedBy" ON "Documents" ("CreatedBy");

CREATE INDEX "IX_Documents_LastModifiedBy" ON "Documents" ("LastModifiedBy");

CREATE INDEX "IX_Documents_TenantId" ON "Documents" ("TenantId");

CREATE INDEX "IX_LoginAudits_LoginTimeUtc" ON "LoginAudits" ("LoginTimeUtc");

CREATE INDEX "IX_LoginAudits_UserId" ON "LoginAudits" ("UserId");

CREATE INDEX "IX_LoginAudits_UserId_LoginTimeUtc" ON "LoginAudits" ("UserId", "LoginTimeUtc");

CREATE UNIQUE INDEX "IX_PicklistSets_Name_Value" ON "PicklistSets" ("Name", "Value");

CREATE UNIQUE INDEX "IX_Products_Name" ON "Products" ("Name");

CREATE INDEX "IX_SystemLogs_Level" ON "SystemLogs" ("Level");

CREATE INDEX "IX_SystemLogs_TimeStamp" ON "SystemLogs" ("TimeStamp");

CREATE INDEX "IX_UserLoginRiskSummaries_UserId" ON "UserLoginRiskSummaries" ("UserId");

CREATE INDEX "IX_UserLoginRiskSummaries_UserName" ON "UserLoginRiskSummaries" ("UserName");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250731120807_initialCreate', '9.0.8');

ALTER TABLE "Contacts" ADD "TenantId" TEXT NOT NULL DEFAULT '';

CREATE TABLE "Issues" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Issues" PRIMARY KEY,
    "ReferenceNumber" TEXT NOT NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Category" INTEGER NOT NULL,
    "Priority" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL,
    "ReporterContactId" INTEGER NULL,
    "SourceMessageIds" TEXT NULL,
    "WhatsAppMetadata" TEXT NULL,
    "ConsentFlag" INTEGER NOT NULL,
    "ReporterPhone" TEXT NULL,
    "ReporterName" TEXT NULL,
    "Channel" TEXT NULL,
    "Product" TEXT NULL,
    "Severity" TEXT NULL,
    "Summary" TEXT NULL,
    "DuplicateOfId" TEXT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_Issues_Contacts_ReporterContactId" FOREIGN KEY ("ReporterContactId") REFERENCES "Contacts" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Issues_Issues_DuplicateOfId" FOREIGN KEY ("DuplicateOfId") REFERENCES "Issues" ("Id") ON DELETE SET NULL
);

CREATE TABLE "Attachments" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Attachments" PRIMARY KEY,
    "IssueId" TEXT NOT NULL,
    "IssueId1" TEXT NOT NULL,
    "Uri" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    "SizeBytes" INTEGER NOT NULL,
    "ScanStatus" TEXT NOT NULL,
    "CreatedUtc" TEXT NOT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_Attachments_Issues_IssueId" FOREIGN KEY ("IssueId") REFERENCES "Issues" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Attachments_Issues_IssueId1" FOREIGN KEY ("IssueId1") REFERENCES "Issues" ("Id") ON DELETE CASCADE
);

CREATE TABLE "EventLogs" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_EventLogs" PRIMARY KEY,
    "IssueId" TEXT NOT NULL,
    "IssueId1" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    "Payload" TEXT NOT NULL,
    "CreatedUtc" TEXT NOT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_EventLogs_Issues_IssueId" FOREIGN KEY ("IssueId") REFERENCES "Issues" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_EventLogs_Issues_IssueId1" FOREIGN KEY ("IssueId1") REFERENCES "Issues" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Attachments_IssueId" ON "Attachments" ("IssueId");

CREATE INDEX "IX_Attachments_IssueId1" ON "Attachments" ("IssueId1");

CREATE INDEX "IX_EventLogs_IssueId" ON "EventLogs" ("IssueId");

CREATE INDEX "IX_EventLogs_IssueId1" ON "EventLogs" ("IssueId1");

CREATE INDEX "IX_Issues_DuplicateOfId" ON "Issues" ("DuplicateOfId");

CREATE UNIQUE INDEX "IX_Issues_ReferenceNumber" ON "Issues" ("ReferenceNumber");

CREATE INDEX "IX_Issues_ReporterContactId" ON "Issues" ("ReporterContactId");

CREATE INDEX "IX_Issues_TenantId" ON "Issues" ("TenantId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250904084929_initial111', '9.0.8');

ALTER TABLE "Issues" ADD "AssignedUserId" TEXT NULL;

ALTER TABLE "Issues" ADD "ProductId" INTEGER NULL;

CREATE TABLE "InternalNotes" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_InternalNotes" PRIMARY KEY,
    "IssueId" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "CreatedByUserId" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_InternalNotes_Issues_IssueId" FOREIGN KEY ("IssueId") REFERENCES "Issues" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Issues_ProductId" ON "Issues" ("ProductId");

CREATE INDEX "IX_InternalNotes_IssueId" ON "InternalNotes" ("IssueId");

CREATE INDEX "IX_InternalNotes_IssueId_CreatedAt" ON "InternalNotes" ("IssueId", "CreatedAt");

CREATE INDEX "IX_InternalNotes_TenantId" ON "InternalNotes" ("TenantId");

CREATE TABLE "ef_temp_Issues" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Issues" PRIMARY KEY,
    "AssignedUserId" TEXT NULL,
    "Category" INTEGER NOT NULL,
    "Channel" TEXT NULL,
    "ConsentFlag" INTEGER NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "Description" TEXT NOT NULL,
    "DuplicateOfId" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    "Priority" INTEGER NOT NULL,
    "Product" TEXT NULL,
    "ProductId" INTEGER NULL,
    "ReferenceNumber" TEXT NOT NULL,
    "ReporterContactId" INTEGER NULL,
    "ReporterName" TEXT NULL,
    "ReporterPhone" TEXT NULL,
    "Severity" TEXT NULL,
    "SourceMessageIds" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "Summary" TEXT NULL,
    "TenantId" TEXT NOT NULL,
    "Title" TEXT NOT NULL,
    "WhatsAppMetadata" TEXT NULL,
    CONSTRAINT "FK_Issues_Contacts_ReporterContactId" FOREIGN KEY ("ReporterContactId") REFERENCES "Contacts" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Issues_Issues_DuplicateOfId" FOREIGN KEY ("DuplicateOfId") REFERENCES "Issues" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Issues_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE SET NULL
);

INSERT INTO "ef_temp_Issues" ("Id", "AssignedUserId", "Category", "Channel", "ConsentFlag", "Created", "CreatedBy", "Description", "DuplicateOfId", "LastModified", "LastModifiedBy", "Priority", "Product", "ProductId", "ReferenceNumber", "ReporterContactId", "ReporterName", "ReporterPhone", "Severity", "SourceMessageIds", "Status", "Summary", "TenantId", "Title", "WhatsAppMetadata")
SELECT "Id", "AssignedUserId", "Category", "Channel", "ConsentFlag", "Created", "CreatedBy", "Description", "DuplicateOfId", "LastModified", "LastModifiedBy", "Priority", "Product", "ProductId", "ReferenceNumber", "ReporterContactId", "ReporterName", "ReporterPhone", "Severity", "SourceMessageIds", "Status", "Summary", "TenantId", "Title", "WhatsAppMetadata"
FROM "Issues";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Issues";

ALTER TABLE "ef_temp_Issues" RENAME TO "Issues";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Issues_DuplicateOfId" ON "Issues" ("DuplicateOfId");

CREATE INDEX "IX_Issues_ProductId" ON "Issues" ("ProductId");

CREATE UNIQUE INDEX "IX_Issues_ReferenceNumber" ON "Issues" ("ReferenceNumber");

CREATE INDEX "IX_Issues_ReporterContactId" ON "Issues" ("ReporterContactId");

CREATE INDEX "IX_Issues_TenantId" ON "Issues" ("TenantId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250904142700_FreshMigration', '9.0.8');

BEGIN TRANSACTION;
CREATE TABLE "IssueLinks" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_IssueLinks" PRIMARY KEY,
    "ParentIssueId" TEXT NOT NULL,
    "ChildIssueId" TEXT NOT NULL,
    "LinkType" INTEGER NOT NULL,
    "ConfidenceScore" decimal(5,4) NULL,
    "CreatedBySystem" INTEGER NOT NULL DEFAULT 0,
    "Metadata" TEXT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_IssueLinks_Issues_ChildIssueId" FOREIGN KEY ("ChildIssueId") REFERENCES "Issues" ("Id"),
    CONSTRAINT "FK_IssueLinks_Issues_ParentIssueId" FOREIGN KEY ("ParentIssueId") REFERENCES "Issues" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_IssueLinks_ChildIssueId" ON "IssueLinks" ("ChildIssueId");

CREATE INDEX "IX_IssueLinks_ConfidenceScore_TenantId" ON "IssueLinks" ("ConfidenceScore", "TenantId");

CREATE INDEX "IX_IssueLinks_CreatedBySystem_TenantId" ON "IssueLinks" ("CreatedBySystem", "TenantId");

CREATE INDEX "IX_IssueLinks_LinkType_TenantId" ON "IssueLinks" ("LinkType", "TenantId");

CREATE INDEX "IX_IssueLinks_ParentIssueId" ON "IssueLinks" ("ParentIssueId");

CREATE UNIQUE INDEX "IX_IssueLinks_ParentIssueId_ChildIssueId_TenantId" ON "IssueLinks" ("ParentIssueId", "ChildIssueId", "TenantId");

CREATE INDEX "IX_IssueLinks_TenantId" ON "IssueLinks" ("TenantId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250904145339_AddIssueLinks', '9.0.8');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250904185551_2', '9.0.8');

CREATE INDEX "IX_EventLogs_IssueId_CreatedUtc" ON "EventLogs" ("IssueId", "CreatedUtc");

CREATE INDEX "IX_EventLogs_TenantId" ON "EventLogs" ("TenantId");

CREATE INDEX "IX_EventLogs_Type" ON "EventLogs" ("Type");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250904190059_AddEventLogConfiguration', '9.0.8');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250904190415_3', '9.0.8');

DROP INDEX "IX_Issues_ProductId";

CREATE TABLE "ef_temp_Issues" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Issues" PRIMARY KEY,
    "AssignedUserId" TEXT NULL,
    "Category" INTEGER NOT NULL,
    "Channel" TEXT NULL,
    "ConsentFlag" INTEGER NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "Description" TEXT NOT NULL,
    "DuplicateOfId" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    "Priority" INTEGER NOT NULL,
    "Product" TEXT NULL,
    "ReferenceNumber" TEXT NOT NULL,
    "ReporterContactId" INTEGER NULL,
    "ReporterName" TEXT NULL,
    "ReporterPhone" TEXT NULL,
    "Severity" TEXT NULL,
    "SourceMessageIds" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "Summary" TEXT NULL,
    "TenantId" TEXT NOT NULL,
    "Title" TEXT NOT NULL,
    "WhatsAppMetadata" TEXT NULL,
    CONSTRAINT "FK_Issues_Contacts_ReporterContactId" FOREIGN KEY ("ReporterContactId") REFERENCES "Contacts" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Issues_Issues_DuplicateOfId" FOREIGN KEY ("DuplicateOfId") REFERENCES "Issues" ("Id") ON DELETE SET NULL
);

INSERT INTO "ef_temp_Issues" ("Id", "AssignedUserId", "Category", "Channel", "ConsentFlag", "Created", "CreatedBy", "Description", "DuplicateOfId", "LastModified", "LastModifiedBy", "Priority", "Product", "ReferenceNumber", "ReporterContactId", "ReporterName", "ReporterPhone", "Severity", "SourceMessageIds", "Status", "Summary", "TenantId", "Title", "WhatsAppMetadata")
SELECT "Id", "AssignedUserId", "Category", "Channel", "ConsentFlag", "Created", "CreatedBy", "Description", "DuplicateOfId", "LastModified", "LastModifiedBy", "Priority", "Product", "ReferenceNumber", "ReporterContactId", "ReporterName", "ReporterPhone", "Severity", "SourceMessageIds", "Status", "Summary", "TenantId", "Title", "WhatsAppMetadata"
FROM "Issues";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Issues";

ALTER TABLE "ef_temp_Issues" RENAME TO "Issues";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Issues_DuplicateOfId" ON "Issues" ("DuplicateOfId");

CREATE UNIQUE INDEX "IX_Issues_ReferenceNumber" ON "Issues" ("ReferenceNumber");

CREATE INDEX "IX_Issues_ReporterContactId" ON "Issues" ("ReporterContactId");

CREATE INDEX "IX_Issues_TenantId" ON "Issues" ("TenantId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250904191141_RemoveProductIdForeignKey', '9.0.8');

BEGIN TRANSACTION;
DROP INDEX "IX_EventLogs_IssueId1";

CREATE TABLE "ef_temp_EventLogs" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_EventLogs" PRIMARY KEY,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "CreatedUtc" TEXT NOT NULL,
    "IssueId" TEXT NOT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    "Payload" TEXT NOT NULL,
    "TenantId" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    CONSTRAINT "FK_EventLogs_Issues_IssueId" FOREIGN KEY ("IssueId") REFERENCES "Issues" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_EventLogs" ("Id", "Created", "CreatedBy", "CreatedUtc", "IssueId", "LastModified", "LastModifiedBy", "Payload", "TenantId", "Type")
SELECT "Id", "Created", "CreatedBy", "CreatedUtc", "IssueId", "LastModified", "LastModifiedBy", "Payload", "TenantId", "Type"
FROM "EventLogs";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "EventLogs";

ALTER TABLE "ef_temp_EventLogs" RENAME TO "EventLogs";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_EventLogs_IssueId" ON "EventLogs" ("IssueId");

CREATE INDEX "IX_EventLogs_IssueId_CreatedUtc" ON "EventLogs" ("IssueId", "CreatedUtc");

CREATE INDEX "IX_EventLogs_TenantId" ON "EventLogs" ("TenantId");

CREATE INDEX "IX_EventLogs_Type" ON "EventLogs" ("Type");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250904192614_FixEventLogForeignKey', '9.0.8');

BEGIN TRANSACTION;
ALTER TABLE "Issues" ADD "JiraCreatedAt" TEXT NULL;

ALTER TABLE "Issues" ADD "JiraKey" TEXT NULL;

ALTER TABLE "Issues" ADD "JiraLastSyncAt" TEXT NULL;

ALTER TABLE "Issues" ADD "JiraUrl" TEXT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905091454_JiraIntegration', '9.0.8');

CREATE TABLE "Agents" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Agents" PRIMARY KEY AUTOINCREMENT,
    "ApplicationUserId" TEXT NOT NULL,
    "Status" INTEGER NOT NULL,
    "MaxConcurrentConversations" INTEGER NOT NULL,
    "ActiveConversationCount" INTEGER NOT NULL,
    "LastActiveAt" TEXT NULL,
    "Skills" TEXT NULL,
    "Priority" INTEGER NOT NULL,
    "Notes" TEXT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_Agents_AspNetUsers_ApplicationUserId" FOREIGN KEY ("ApplicationUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Conversations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Conversations" PRIMARY KEY AUTOINCREMENT,
    "ConversationId" TEXT NOT NULL,
    "WhatsAppPhoneNumber" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "Mode" INTEGER NOT NULL,
    "CurrentAgentId" TEXT NULL,
    "EscalatedAt" TEXT NULL,
    "CompletedAt" TEXT NULL,
    "EscalationReason" TEXT NULL,
    "ConversationSummary" TEXT NULL,
    "MessageCount" INTEGER NOT NULL,
    "LastActivityAt" TEXT NOT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL
);

CREATE TABLE "ConversationHandoffs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ConversationHandoffs" PRIMARY KEY AUTOINCREMENT,
    "ConversationId" INTEGER NOT NULL,
    "HandoffType" INTEGER NOT NULL,
    "FromParticipantType" INTEGER NOT NULL,
    "ToParticipantType" INTEGER NOT NULL,
    "FromAgentId" TEXT NULL,
    "ToAgentId" TEXT NULL,
    "Reason" TEXT NOT NULL,
    "ConversationTranscript" TEXT NULL,
    "ContextData" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "InitiatedAt" TEXT NOT NULL,
    "AcceptedAt" TEXT NULL,
    "CompletedAt" TEXT NULL,
    "Notes" TEXT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_ConversationHandoffs_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ConversationParticipants" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ConversationParticipants" PRIMARY KEY AUTOINCREMENT,
    "ConversationId" INTEGER NOT NULL,
    "Type" INTEGER NOT NULL,
    "ParticipantId" TEXT NULL,
    "ParticipantName" TEXT NULL,
    "WhatsAppPhoneNumber" TEXT NULL,
    "JoinedAt" TEXT NOT NULL,
    "LeftAt" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_ConversationParticipants_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_Agents_ApplicationUserId" ON "Agents" ("ApplicationUserId");

CREATE INDEX "IX_Agents_Priority" ON "Agents" ("Priority");

CREATE INDEX "IX_Agents_Status" ON "Agents" ("Status");

CREATE INDEX "IX_ConversationHandoffs_ConversationId" ON "ConversationHandoffs" ("ConversationId");

CREATE INDEX "IX_ConversationHandoffs_FromAgentId" ON "ConversationHandoffs" ("FromAgentId");

CREATE INDEX "IX_ConversationHandoffs_HandoffType" ON "ConversationHandoffs" ("HandoffType");

CREATE INDEX "IX_ConversationHandoffs_InitiatedAt" ON "ConversationHandoffs" ("InitiatedAt");

CREATE INDEX "IX_ConversationHandoffs_Status" ON "ConversationHandoffs" ("Status");

CREATE INDEX "IX_ConversationHandoffs_ToAgentId" ON "ConversationHandoffs" ("ToAgentId");

CREATE INDEX "IX_ConversationParticipants_ConversationId" ON "ConversationParticipants" ("ConversationId");

CREATE INDEX "IX_ConversationParticipants_ParticipantId" ON "ConversationParticipants" ("ParticipantId");

CREATE INDEX "IX_ConversationParticipants_Type" ON "ConversationParticipants" ("Type");

CREATE INDEX "IX_ConversationParticipants_WhatsAppPhoneNumber" ON "ConversationParticipants" ("WhatsAppPhoneNumber");

CREATE UNIQUE INDEX "IX_Conversations_ConversationId" ON "Conversations" ("ConversationId");

CREATE INDEX "IX_Conversations_CurrentAgentId" ON "Conversations" ("CurrentAgentId");

CREATE INDEX "IX_Conversations_LastActivityAt" ON "Conversations" ("LastActivityAt");

CREATE INDEX "IX_Conversations_Mode" ON "Conversations" ("Mode");

CREATE INDEX "IX_Conversations_Status" ON "Conversations" ("Status");

CREATE INDEX "IX_Conversations_WhatsAppPhoneNumber" ON "Conversations" ("WhatsAppPhoneNumber");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905124505_HumanEscalationEntities', '9.0.8');

DROP INDEX "IX_Conversations_CurrentAgentId";

DROP INDEX "IX_Conversations_WhatsAppPhoneNumber";

ALTER TABLE "Conversations" ADD "MaxTurns" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Conversations" ADD "StartTime" TEXT NOT NULL DEFAULT '0001-01-01 00:00:00';

ALTER TABLE "Conversations" ADD "ThreadId" TEXT NULL;

ALTER TABLE "Conversations" ADD "UserId" TEXT NULL;

ALTER TABLE "Conversations" ADD "UserName" TEXT NULL;

CREATE TABLE "ConversationMessages" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ConversationMessages" PRIMARY KEY AUTOINCREMENT,
    "ConversationId" INTEGER NOT NULL,
    "BotFrameworkConversationId" TEXT NOT NULL,
    "Role" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "ToolCallId" TEXT NULL,
    "ToolCalls" TEXT NULL,
    "ImageType" TEXT NULL,
    "ImageData" TEXT NULL,
    "Attachments" TEXT NULL,
    "Timestamp" TEXT NOT NULL,
    "UserId" TEXT NULL,
    "UserName" TEXT NULL,
    "ChannelId" TEXT NULL,
    "IsEscalated" INTEGER NOT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_ConversationMessages_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ConversationAttachments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ConversationAttachments" PRIMARY KEY AUTOINCREMENT,
    "ConversationId" INTEGER NOT NULL,
    "MessageId" INTEGER NULL,
    "BotFrameworkConversationId" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "ContentType" TEXT NOT NULL,
    "Url" TEXT NULL,
    "FileData" TEXT NULL,
    "FileSize" INTEGER NULL,
    "Timestamp" TEXT NOT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_ConversationAttachments_ConversationMessages_MessageId" FOREIGN KEY ("MessageId") REFERENCES "ConversationMessages" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ConversationAttachments_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Conversations_Status_Mode" ON "Conversations" ("Status", "Mode");

CREATE INDEX "IX_ConversationAttachments_ContentType" ON "ConversationAttachments" ("ContentType");

CREATE INDEX "IX_ConversationAttachments_ConversationId" ON "ConversationAttachments" ("ConversationId");

CREATE INDEX "IX_ConversationAttachments_MessageId" ON "ConversationAttachments" ("MessageId");

CREATE INDEX "IX_ConversationMessages_BotFrameworkConversationId" ON "ConversationMessages" ("BotFrameworkConversationId");

CREATE INDEX "IX_ConversationMessages_BotFrameworkConversationId_Timestamp" ON "ConversationMessages" ("BotFrameworkConversationId", "Timestamp");

CREATE INDEX "IX_ConversationMessages_ConversationId" ON "ConversationMessages" ("ConversationId");

CREATE INDEX "IX_ConversationMessages_Timestamp" ON "ConversationMessages" ("Timestamp");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905140054_UpdateConversationEntitiesStructure', '9.0.8');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905182203_AddAgentEntity', '9.0.8');

ALTER TABLE "Conversations" ADD "Priority" INTEGER NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250905190559_AddConversationPriority', '9.0.8');

CREATE TABLE "AgentNotificationPreferences" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AgentNotificationPreferences" PRIMARY KEY AUTOINCREMENT,
    "ApplicationUserId" TEXT NOT NULL,
    "EnableBrowserNotifications" INTEGER NOT NULL,
    "EnableAudioAlerts" INTEGER NOT NULL,
    "EnableEmailNotifications" INTEGER NOT NULL,
    "NotifyOnStandardPriority" INTEGER NOT NULL,
    "NotifyOnHighPriority" INTEGER NOT NULL,
    "NotifyOnCriticalPriority" INTEGER NOT NULL,
    "NotifyDuringBreak" INTEGER NOT NULL,
    "NotifyWhenOffline" INTEGER NOT NULL,
    "AudioVolume" INTEGER NOT NULL,
    "CustomSoundUrl" TEXT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_AgentNotificationPreferences_AspNetUsers_ApplicationUserId" FOREIGN KEY ("ApplicationUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_AgentNotificationPreferences_ApplicationUserId_TenantId" ON "AgentNotificationPreferences" ("ApplicationUserId", "TenantId");

CREATE INDEX "IX_AgentNotificationPreferences_TenantId" ON "AgentNotificationPreferences" ("TenantId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250906084415_AddAgentNotificationPreferences', '9.0.8');

ALTER TABLE "Conversations" RENAME COLUMN "ConversationId" TO "ConversationReference";

DROP INDEX "IX_Conversations_ConversationId";

CREATE UNIQUE INDEX "IX_Conversations_ConversationReference" ON "Conversations" ("ConversationReference");

ALTER TABLE "ConversationHandoffs" ADD "ConversationReference" TEXT NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250906133539_agenthandoff', '9.0.8');

ALTER TABLE "Issues" ADD "ConversationId" INTEGER NULL;

CREATE INDEX "IX_Issues_ConversationId" ON "Issues" ("ConversationId");

CREATE TABLE "ef_temp_Issues" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Issues" PRIMARY KEY,
    "AssignedUserId" TEXT NULL,
    "Category" INTEGER NOT NULL,
    "Channel" TEXT NULL,
    "ConsentFlag" INTEGER NOT NULL,
    "ConversationId" INTEGER NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "Description" TEXT NOT NULL,
    "DuplicateOfId" TEXT NULL,
    "JiraCreatedAt" TEXT NULL,
    "JiraKey" TEXT NULL,
    "JiraLastSyncAt" TEXT NULL,
    "JiraUrl" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    "Priority" INTEGER NOT NULL,
    "Product" TEXT NULL,
    "ReferenceNumber" TEXT NOT NULL,
    "ReporterContactId" INTEGER NULL,
    "ReporterName" TEXT NULL,
    "ReporterPhone" TEXT NULL,
    "Severity" TEXT NULL,
    "SourceMessageIds" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "Summary" TEXT NULL,
    "TenantId" TEXT NOT NULL,
    "Title" TEXT NOT NULL,
    "WhatsAppMetadata" TEXT NULL,
    CONSTRAINT "FK_Issues_Contacts_ReporterContactId" FOREIGN KEY ("ReporterContactId") REFERENCES "Contacts" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Issues_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Issues_Issues_DuplicateOfId" FOREIGN KEY ("DuplicateOfId") REFERENCES "Issues" ("Id") ON DELETE SET NULL
);

INSERT INTO "ef_temp_Issues" ("Id", "AssignedUserId", "Category", "Channel", "ConsentFlag", "ConversationId", "Created", "CreatedBy", "Description", "DuplicateOfId", "JiraCreatedAt", "JiraKey", "JiraLastSyncAt", "JiraUrl", "LastModified", "LastModifiedBy", "Priority", "Product", "ReferenceNumber", "ReporterContactId", "ReporterName", "ReporterPhone", "Severity", "SourceMessageIds", "Status", "Summary", "TenantId", "Title", "WhatsAppMetadata")
SELECT "Id", "AssignedUserId", "Category", "Channel", "ConsentFlag", "ConversationId", "Created", "CreatedBy", "Description", "DuplicateOfId", "JiraCreatedAt", "JiraKey", "JiraLastSyncAt", "JiraUrl", "LastModified", "LastModifiedBy", "Priority", "Product", "ReferenceNumber", "ReporterContactId", "ReporterName", "ReporterPhone", "Severity", "SourceMessageIds", "Status", "Summary", "TenantId", "Title", "WhatsAppMetadata"
FROM "Issues";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Issues";

ALTER TABLE "ef_temp_Issues" RENAME TO "Issues";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Issues_ConversationId" ON "Issues" ("ConversationId");

CREATE INDEX "IX_Issues_DuplicateOfId" ON "Issues" ("DuplicateOfId");

CREATE UNIQUE INDEX "IX_Issues_ReferenceNumber" ON "Issues" ("ReferenceNumber");

CREATE INDEX "IX_Issues_ReporterContactId" ON "Issues" ("ReporterContactId");

CREATE INDEX "IX_Issues_TenantId" ON "Issues" ("TenantId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250906144323_AddConversationIdToIssues', '9.0.8');

BEGIN TRANSACTION;
CREATE TABLE "ConversationInsights" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ConversationInsights" PRIMARY KEY AUTOINCREMENT,
    "ConversationId" INTEGER NOT NULL,
    "SentimentScore" TEXT NOT NULL,
    "SentimentLabel" TEXT NOT NULL,
    "KeyThemes" TEXT NOT NULL,
    "ResolutionSuccess" INTEGER NULL,
    "CustomerSatisfactionIndicators" TEXT NOT NULL,
    "Recommendations" TEXT NOT NULL,
    "ProcessingModel" TEXT NOT NULL,
    "ProcessedAt" TEXT NOT NULL,
    "ProcessingDuration" TEXT NOT NULL,
    "TenantId" TEXT NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    CONSTRAINT "FK_ConversationInsights_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_ConversationInsights_ConversationId" ON "ConversationInsights" ("ConversationId");

CREATE INDEX "IX_ConversationInsights_ProcessedAt" ON "ConversationInsights" ("ProcessedAt");

CREATE INDEX "IX_ConversationInsights_SentimentScore" ON "ConversationInsights" ("SentimentScore");

CREATE INDEX "IX_ConversationInsights_TenantId_ProcessedAt" ON "ConversationInsights" ("TenantId", "ProcessedAt");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250907075524_AddConversationInsights', '9.0.8');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250907132954_UpdateConversationInsightColumnTypes', '9.0.8');

ALTER TABLE "Conversations" ADD "ResolutionCategory" INTEGER NULL;

ALTER TABLE "Conversations" ADD "ResolutionNotes" TEXT NULL;

ALTER TABLE "Conversations" ADD "ResolvedByAgentId" TEXT NULL;

CREATE INDEX "IX_Conversations_ResolutionCategory" ON "Conversations" ("ResolutionCategory");

CREATE INDEX "IX_Conversations_Status_ResolutionCategory" ON "Conversations" ("Status", "ResolutionCategory");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250908060529_AddConversationResolutionFields', '9.0.8');

ALTER TABLE "Conversations" ADD "ConversationChannelData" TEXT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250908085912_MoveConversationChannelDataToConversation', '9.0.8');

ALTER TABLE "ConversationInsights" ADD "ConversationId1" INTEGER NULL;

CREATE UNIQUE INDEX "IX_ConversationInsights_ConversationId1" ON "ConversationInsights" ("ConversationId1");

CREATE TABLE "ef_temp_ConversationInsights" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ConversationInsights" PRIMARY KEY AUTOINCREMENT,
    "ConversationId" INTEGER NOT NULL,
    "ConversationId1" INTEGER NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "CustomerSatisfactionIndicators" TEXT NOT NULL,
    "KeyThemes" TEXT NOT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    "ProcessedAt" TEXT NOT NULL,
    "ProcessingDuration" TEXT NOT NULL,
    "ProcessingModel" TEXT NOT NULL,
    "Recommendations" TEXT NOT NULL,
    "ResolutionSuccess" INTEGER NULL,
    "SentimentLabel" TEXT NOT NULL,
    "SentimentScore" TEXT NOT NULL,
    "TenantId" TEXT NOT NULL,
    CONSTRAINT "FK_ConversationInsights_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ConversationInsights_Conversations_ConversationId1" FOREIGN KEY ("ConversationId1") REFERENCES "Conversations" ("Id")
);

INSERT INTO "ef_temp_ConversationInsights" ("Id", "ConversationId", "ConversationId1", "Created", "CreatedBy", "CustomerSatisfactionIndicators", "KeyThemes", "LastModified", "LastModifiedBy", "ProcessedAt", "ProcessingDuration", "ProcessingModel", "Recommendations", "ResolutionSuccess", "SentimentLabel", "SentimentScore", "TenantId")
SELECT "Id", "ConversationId", "ConversationId1", "Created", "CreatedBy", "CustomerSatisfactionIndicators", "KeyThemes", "LastModified", "LastModifiedBy", "ProcessedAt", "ProcessingDuration", "ProcessingModel", "Recommendations", "ResolutionSuccess", "SentimentLabel", "SentimentScore", "TenantId"
FROM "ConversationInsights";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "ConversationInsights";

ALTER TABLE "ef_temp_ConversationInsights" RENAME TO "ConversationInsights";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE UNIQUE INDEX "IX_ConversationInsights_ConversationId" ON "ConversationInsights" ("ConversationId");

CREATE UNIQUE INDEX "IX_ConversationInsights_ConversationId1" ON "ConversationInsights" ("ConversationId1");

CREATE INDEX "IX_ConversationInsights_ProcessedAt" ON "ConversationInsights" ("ProcessedAt");

CREATE INDEX "IX_ConversationInsights_SentimentScore" ON "ConversationInsights" ("SentimentScore");

CREATE INDEX "IX_ConversationInsights_TenantId_ProcessedAt" ON "ConversationInsights" ("TenantId", "ProcessedAt");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250908211550_ConversationAnalyticsPendingChanges', '9.0.8');

BEGIN TRANSACTION;
DROP INDEX "IX_ConversationInsights_ConversationId1";

CREATE TABLE "ef_temp_ConversationInsights" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ConversationInsights" PRIMARY KEY AUTOINCREMENT,
    "ConversationId" INTEGER NOT NULL,
    "Created" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "CustomerSatisfactionIndicators" TEXT NOT NULL,
    "KeyThemes" TEXT NOT NULL,
    "LastModified" TEXT NULL,
    "LastModifiedBy" TEXT NULL,
    "ProcessedAt" TEXT NOT NULL,
    "ProcessingDuration" TEXT NOT NULL,
    "ProcessingModel" TEXT NOT NULL,
    "Recommendations" TEXT NOT NULL,
    "ResolutionSuccess" INTEGER NULL,
    "SentimentLabel" TEXT NOT NULL,
    "SentimentScore" TEXT NOT NULL,
    "TenantId" TEXT NOT NULL,
    CONSTRAINT "FK_ConversationInsights_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_ConversationInsights" ("Id", "ConversationId", "Created", "CreatedBy", "CustomerSatisfactionIndicators", "KeyThemes", "LastModified", "LastModifiedBy", "ProcessedAt", "ProcessingDuration", "ProcessingModel", "Recommendations", "ResolutionSuccess", "SentimentLabel", "SentimentScore", "TenantId")
SELECT "Id", "ConversationId", "Created", "CreatedBy", "CustomerSatisfactionIndicators", "KeyThemes", "LastModified", "LastModifiedBy", "ProcessedAt", "ProcessingDuration", "ProcessingModel", "Recommendations", "ResolutionSuccess", "SentimentLabel", "SentimentScore", "TenantId"
FROM "ConversationInsights";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "ConversationInsights";

ALTER TABLE "ef_temp_ConversationInsights" RENAME TO "ConversationInsights";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE UNIQUE INDEX "IX_ConversationInsights_ConversationId" ON "ConversationInsights" ("ConversationId");

CREATE INDEX "IX_ConversationInsights_ProcessedAt" ON "ConversationInsights" ("ProcessedAt");

CREATE INDEX "IX_ConversationInsights_SentimentScore" ON "ConversationInsights" ("SentimentScore");

CREATE INDEX "IX_ConversationInsights_TenantId_ProcessedAt" ON "ConversationInsights" ("TenantId", "ProcessedAt");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250908211723_FixConversationInsightRelationship', '9.0.8');

BEGIN TRANSACTION;
ALTER TABLE "AspNetUsers" ADD "UserType" INTEGER NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250910205154_userroles', '9.0.8');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250912064731_FixConversationAssignmentUpdates', '9.0.8');

COMMIT;

