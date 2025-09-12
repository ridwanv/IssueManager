IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [DisplayName] nvarchar(max) NULL,
    [Site] nvarchar(max) NULL,
    [TenantId] nvarchar(max) NULL,
    [TenantName] nvarchar(max) NULL,
    [ProfilePictureDataUrl] text NULL,
    [IsActive] bit NOT NULL,
    [IsLive] bit NOT NULL,
    [RefreshToken] nvarchar(max) NULL,
    [RefreshTokenExpiryTime] datetime2 NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [AuditTrails] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NULL,
    [AuditType] nvarchar(max) NOT NULL,
    [TableName] nvarchar(max) NULL,
    [DateTime] datetime2 NOT NULL,
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [AffectedColumns] nvarchar(max) NULL,
    [PrimaryKey] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_AuditTrails] PRIMARY KEY ([Id])
);

CREATE TABLE [DocumentTypes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_DocumentTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [KeyValues] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Value] nvarchar(max) NULL,
    [Text] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_KeyValues] PRIMARY KEY ([Id])
);

CREATE TABLE [Loggers] (
    [Id] int NOT NULL IDENTITY,
    [Message] nvarchar(max) NULL,
    [MessageTemplate] nvarchar(max) NULL,
    [Level] nvarchar(max) NOT NULL,
    [TimeStamp] datetime2 NOT NULL,
    [Exception] nvarchar(max) NULL,
    [UserName] nvarchar(max) NULL,
    [ClientIP] nvarchar(max) NULL,
    [ClientAgent] nvarchar(max) NULL,
    [Properties] nvarchar(max) NULL,
    [LogEvent] nvarchar(max) NULL,
    CONSTRAINT [PK_Loggers] PRIMARY KEY ([Id])
);

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Brand] nvarchar(max) NULL,
    [Unit] nvarchar(max) NULL,
    [Price] decimal(18,2) NOT NULL,
    [Pictures] nvarchar(max) NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);

CREATE TABLE [Tenants] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(max) NULL,
    [Group] nvarchar(max) NULL,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Documents] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [IsPublic] bit NOT NULL,
    [URL] nvarchar(max) NULL,
    [DocumentTypeId] int NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Documents_DocumentTypes_DocumentTypeId] FOREIGN KEY ([DocumentTypeId]) REFERENCES [DocumentTypes] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_Documents_DocumentTypeId] ON [Documents] ([DocumentTypeId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220510111620_InitialCreate', N'9.0.8');

ALTER TABLE [Documents] DROP CONSTRAINT [FK_Documents_DocumentTypes_DocumentTypeId];

DROP TABLE [DocumentTypes];

DROP INDEX [IX_Documents_DocumentTypeId] ON [Documents];

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Documents]') AND [c].[name] = N'DocumentTypeId');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Documents] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Documents] DROP COLUMN [DocumentTypeId];

ALTER TABLE [Documents] ADD [DocumentType] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [Documents] ADD [TenantId] nvarchar(450) NULL;

CREATE INDEX [IX_Documents_TenantId] ON [Documents] ([TenantId]);

ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220511023824_documentwithtenant', N'9.0.8');

EXEC sp_rename N'[AspNetUsers].[Site]', N'Provider', 'COLUMN';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220511044100_rename', N'9.0.8');

CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220802103032_addCustomer', N'9.0.8');

ALTER TABLE [Documents] ADD [Content] nvarchar(max) NULL;

ALTER TABLE [Documents] ADD [Status] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220819033015_Document_Status_Content', N'9.0.8');

ALTER TABLE [AspNetUsers] ADD [SuperiorId] nvarchar(450) NULL;

CREATE INDEX [IX_AspNetUsers_SuperiorId] ON [AspNetUsers] ([SuperiorId]);

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_AspNetUsers_SuperiorId] FOREIGN KEY ([SuperiorId]) REFERENCES [AspNetUsers] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221008050734_Superior-ApplicationUser', N'9.0.8');

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KeyValues]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [KeyValues] DROP CONSTRAINT [' + @var1 + '];');
UPDATE [KeyValues] SET [Name] = N'' WHERE [Name] IS NULL;
ALTER TABLE [KeyValues] ALTER COLUMN [Name] nvarchar(max) NOT NULL;
ALTER TABLE [KeyValues] ADD DEFAULT N'' FOR [Name];

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Documents]') AND [c].[name] = N'LastModifiedBy');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Documents] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Documents] ALTER COLUMN [LastModifiedBy] nvarchar(450) NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Documents]') AND [c].[name] = N'CreatedBy');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Documents] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Documents] ALTER COLUMN [CreatedBy] nvarchar(450) NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Name');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var4 + '];');
UPDATE [Customers] SET [Name] = N'' WHERE [Name] IS NULL;
ALTER TABLE [Customers] ALTER COLUMN [Name] nvarchar(50) NOT NULL;
ALTER TABLE [Customers] ADD DEFAULT N'' FOR [Name];

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserTokens]') AND [c].[name] = N'Name');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [AspNetUserTokens] ALTER COLUMN [Name] nvarchar(450) NOT NULL;

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserTokens]') AND [c].[name] = N'LoginProvider');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [AspNetUserTokens] ALTER COLUMN [LoginProvider] nvarchar(450) NOT NULL;

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserLogins]') AND [c].[name] = N'ProviderKey');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [AspNetUserLogins] ALTER COLUMN [ProviderKey] nvarchar(450) NOT NULL;

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserLogins]') AND [c].[name] = N'LoginProvider');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [AspNetUserLogins] ALTER COLUMN [LoginProvider] nvarchar(450) NOT NULL;

CREATE INDEX [IX_Documents_CreatedBy] ON [Documents] ([CreatedBy]);

CREATE INDEX [IX_Documents_LastModifiedBy] ON [Documents] ([LastModifiedBy]);

ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_AspNetUsers_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_AspNetUsers_LastModifiedBy] FOREIGN KEY ([LastModifiedBy]) REFERENCES [AspNetUsers] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230224122527_owner', N'9.0.8');

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditTrails]') AND [c].[name] = N'UserId');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [AuditTrails] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [AuditTrails] ALTER COLUMN [UserId] nvarchar(450) NULL;

CREATE INDEX [IX_AuditTrails_UserId] ON [AuditTrails] ([UserId]);

ALTER TABLE [AuditTrails] ADD CONSTRAINT [FK_AuditTrails_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230303094047_Owner_AuditTrail', N'9.0.8');

ALTER TABLE [AuditTrails] DROP CONSTRAINT [FK_AuditTrails_AspNetUsers_UserId];

ALTER TABLE [AuditTrails] ADD CONSTRAINT [FK_AuditTrails_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230502131206_DeleteBehavior_SetNull', N'9.0.8');

ALTER TABLE [Documents] DROP CONSTRAINT [FK_Documents_AspNetUsers_CreatedBy];

ALTER TABLE [Documents] DROP CONSTRAINT [FK_Documents_AspNetUsers_LastModifiedBy];

ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_AspNetUsers_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_AspNetUsers_LastModifiedBy] FOREIGN KEY ([LastModifiedBy]) REFERENCES [AspNetUsers] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230502131612_DeleteBehavior_Cascade', N'9.0.8');

ALTER TABLE [Documents] DROP CONSTRAINT [FK_Documents_AspNetUsers_CreatedBy];

ALTER TABLE [Documents] DROP CONSTRAINT [FK_Documents_AspNetUsers_LastModifiedBy];

ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_AspNetUsers_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_AspNetUsers_LastModifiedBy] FOREIGN KEY ([LastModifiedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230502132810_DeleteBehavior_Restrict', N'9.0.8');

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Created');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [Tenants] DROP COLUMN [Created];

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'CreatedBy');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Tenants] DROP COLUMN [CreatedBy];

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'LastModified');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [Tenants] DROP COLUMN [LastModified];

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'LastModifiedBy');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [Tenants] DROP COLUMN [LastModifiedBy];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230826094051_refactor_base_entity', N'9.0.8');

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'TenantId');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [TenantId] nvarchar(450) NULL;

CREATE INDEX [IX_AspNetUsers_TenantId] ON [AspNetUsers] ([TenantId]);

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240209022029_tenant_identity', N'9.0.8');

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'Message');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [Message] nvarchar(1000) NULL;

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'Level');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var16 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [Level] nvarchar(450) NOT NULL;

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'Exception');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [Exception] nvarchar(1000) NULL;

CREATE INDEX [IX_Loggers_Level_Message_Exception] ON [Loggers] ([Level], [Message], [Exception]);

CREATE INDEX [IX_Loggers_TimeStamp] ON [Loggers] ([TimeStamp]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240301003916_create_index_logger', N'9.0.8');

CREATE TABLE [DataProtectionKeys] (
    [Id] int NOT NULL IDENTITY,
    [FriendlyName] nvarchar(max) NULL,
    [Xml] nvarchar(max) NULL,
    CONSTRAINT [PK_DataProtectionKeys] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240329000950_IDataProtection', N'9.0.8');

DROP INDEX [IX_Loggers_Level_Message_Exception] ON [Loggers];
DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'Message');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [Message] nvarchar(4000) NULL;
CREATE INDEX [IX_Loggers_Level_Message_Exception] ON [Loggers] ([Level], [Message], [Exception]);

DROP INDEX [IX_Loggers_Level_Message_Exception] ON [Loggers];
DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'Exception');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [Exception] nvarchar(4000) NULL;
CREATE INDEX [IX_Loggers_Level_Message_Exception] ON [Loggers] ([Level], [Message], [Exception]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240331094404_logger_maxlength', N'9.0.8');

DROP INDEX [IX_Loggers_Level_Message_Exception] ON [Loggers];

DECLARE @var20 sysname;
SELECT @var20 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'Message');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var20 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [Message] nvarchar(4000) NULL;

CREATE INDEX [IX_Loggers_Exception] ON [Loggers] ([Exception]);

CREATE INDEX [IX_Loggers_Level] ON [Loggers] ([Level]);

CREATE INDEX [IX_Loggers_Message] ON [Loggers] ([Message]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240401031948_logger_ix', N'9.0.8');

DECLARE @var21 sysname;
SELECT @var21 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Name');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var21 + '];');
ALTER TABLE [Tenants] ALTER COLUMN [Name] nvarchar(450) NULL;

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Description');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var22 + '];');
ALTER TABLE [Tenants] ALTER COLUMN [Description] nvarchar(450) NULL;

DECLARE @var23 sysname;
SELECT @var23 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Unit');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var23 + '];');
ALTER TABLE [Products] ALTER COLUMN [Unit] nvarchar(450) NULL;

DECLARE @var24 sysname;
SELECT @var24 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Name');
IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var24 + '];');
ALTER TABLE [Products] ALTER COLUMN [Name] nvarchar(450) NULL;

DECLARE @var25 sysname;
SELECT @var25 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'LastModifiedBy');
IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var25 + '];');
ALTER TABLE [Products] ALTER COLUMN [LastModifiedBy] nvarchar(450) NULL;

DECLARE @var26 sysname;
SELECT @var26 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Description');
IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var26 + '];');
ALTER TABLE [Products] ALTER COLUMN [Description] nvarchar(450) NULL;

DECLARE @var27 sysname;
SELECT @var27 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'CreatedBy');
IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var27 + '];');
ALTER TABLE [Products] ALTER COLUMN [CreatedBy] nvarchar(450) NULL;

DECLARE @var28 sysname;
SELECT @var28 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Brand');
IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var28 + '];');
ALTER TABLE [Products] ALTER COLUMN [Brand] nvarchar(450) NULL;

DECLARE @var29 sysname;
SELECT @var29 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'UserName');
IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var29 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [UserName] nvarchar(450) NULL;

DECLARE @var30 sysname;
SELECT @var30 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'Properties');
IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var30 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [Properties] nvarchar(2000) NULL;

DECLARE @var31 sysname;
SELECT @var31 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'MessageTemplate');
IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var31 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [MessageTemplate] nvarchar(2000) NULL;

DECLARE @var32 sysname;
SELECT @var32 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'LogEvent');
IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var32 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [LogEvent] nvarchar(2000) NULL;

DECLARE @var33 sysname;
SELECT @var33 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'ClientIP');
IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var33 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [ClientIP] nvarchar(450) NULL;

DECLARE @var34 sysname;
SELECT @var34 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'ClientAgent');
IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var34 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [ClientAgent] nvarchar(450) NULL;

DECLARE @var35 sysname;
SELECT @var35 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KeyValues]') AND [c].[name] = N'Value');
IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [KeyValues] DROP CONSTRAINT [' + @var35 + '];');
ALTER TABLE [KeyValues] ALTER COLUMN [Value] nvarchar(450) NULL;

DECLARE @var36 sysname;
SELECT @var36 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KeyValues]') AND [c].[name] = N'Text');
IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [KeyValues] DROP CONSTRAINT [' + @var36 + '];');
ALTER TABLE [KeyValues] ALTER COLUMN [Text] nvarchar(450) NULL;

DECLARE @var37 sysname;
SELECT @var37 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KeyValues]') AND [c].[name] = N'LastModifiedBy');
IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [KeyValues] DROP CONSTRAINT [' + @var37 + '];');
ALTER TABLE [KeyValues] ALTER COLUMN [LastModifiedBy] nvarchar(450) NULL;

DECLARE @var38 sysname;
SELECT @var38 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KeyValues]') AND [c].[name] = N'Description');
IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [KeyValues] DROP CONSTRAINT [' + @var38 + '];');
ALTER TABLE [KeyValues] ALTER COLUMN [Description] nvarchar(450) NULL;

DECLARE @var39 sysname;
SELECT @var39 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KeyValues]') AND [c].[name] = N'CreatedBy');
IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [KeyValues] DROP CONSTRAINT [' + @var39 + '];');
ALTER TABLE [KeyValues] ALTER COLUMN [CreatedBy] nvarchar(450) NULL;

DECLARE @var40 sysname;
SELECT @var40 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Documents]') AND [c].[name] = N'URL');
IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [Documents] DROP CONSTRAINT [' + @var40 + '];');
ALTER TABLE [Documents] ALTER COLUMN [URL] nvarchar(450) NULL;

DECLARE @var41 sysname;
SELECT @var41 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Documents]') AND [c].[name] = N'Title');
IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [Documents] DROP CONSTRAINT [' + @var41 + '];');
ALTER TABLE [Documents] ALTER COLUMN [Title] nvarchar(450) NULL;

DECLARE @var42 sysname;
SELECT @var42 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Documents]') AND [c].[name] = N'Description');
IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [Documents] DROP CONSTRAINT [' + @var42 + '];');
ALTER TABLE [Documents] ALTER COLUMN [Description] nvarchar(450) NULL;

DECLARE @var43 sysname;
SELECT @var43 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Documents]') AND [c].[name] = N'Content');
IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [Documents] DROP CONSTRAINT [' + @var43 + '];');
ALTER TABLE [Documents] ALTER COLUMN [Content] nvarchar(4000) NULL;

DECLARE @var44 sysname;
SELECT @var44 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DataProtectionKeys]') AND [c].[name] = N'Xml');
IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [DataProtectionKeys] DROP CONSTRAINT [' + @var44 + '];');
ALTER TABLE [DataProtectionKeys] ALTER COLUMN [Xml] nvarchar(4000) NULL;

DECLARE @var45 sysname;
SELECT @var45 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DataProtectionKeys]') AND [c].[name] = N'FriendlyName');
IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [DataProtectionKeys] DROP CONSTRAINT [' + @var45 + '];');
ALTER TABLE [DataProtectionKeys] ALTER COLUMN [FriendlyName] nvarchar(450) NULL;

DECLARE @var46 sysname;
SELECT @var46 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'LastModifiedBy');
IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var46 + '];');
ALTER TABLE [Customers] ALTER COLUMN [LastModifiedBy] nvarchar(450) NULL;

DECLARE @var47 sysname;
SELECT @var47 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Description');
IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var47 + '];');
ALTER TABLE [Customers] ALTER COLUMN [Description] nvarchar(450) NULL;

DECLARE @var48 sysname;
SELECT @var48 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'CreatedBy');
IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var48 + '];');
ALTER TABLE [Customers] ALTER COLUMN [CreatedBy] nvarchar(450) NULL;

DECLARE @var49 sysname;
SELECT @var49 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditTrails]') AND [c].[name] = N'TableName');
IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [AuditTrails] DROP CONSTRAINT [' + @var49 + '];');
ALTER TABLE [AuditTrails] ALTER COLUMN [TableName] nvarchar(450) NULL;

DECLARE @var50 sysname;
SELECT @var50 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserTokens]') AND [c].[name] = N'Value');
IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT [' + @var50 + '];');
ALTER TABLE [AspNetUserTokens] ALTER COLUMN [Value] nvarchar(450) NULL;

DECLARE @var51 sysname;
SELECT @var51 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'TenantName');
IF @var51 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var51 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [TenantName] nvarchar(450) NULL;

DECLARE @var52 sysname;
SELECT @var52 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'SecurityStamp');
IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var52 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [SecurityStamp] nvarchar(450) NULL;

DECLARE @var53 sysname;
SELECT @var53 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'RefreshToken');
IF @var53 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var53 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [RefreshToken] nvarchar(450) NULL;

DECLARE @var54 sysname;
SELECT @var54 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'Provider');
IF @var54 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var54 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [Provider] nvarchar(450) NULL;

DECLARE @var55 sysname;
SELECT @var55 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'PhoneNumber');
IF @var55 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var55 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [PhoneNumber] nvarchar(450) NULL;

DECLARE @var56 sysname;
SELECT @var56 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'PasswordHash');
IF @var56 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var56 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [PasswordHash] nvarchar(450) NULL;

DECLARE @var57 sysname;
SELECT @var57 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'DisplayName');
IF @var57 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var57 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [DisplayName] nvarchar(450) NULL;

DECLARE @var58 sysname;
SELECT @var58 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'ConcurrencyStamp');
IF @var58 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var58 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [ConcurrencyStamp] nvarchar(450) NULL;

DECLARE @var59 sysname;
SELECT @var59 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserLogins]') AND [c].[name] = N'ProviderDisplayName');
IF @var59 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT [' + @var59 + '];');
ALTER TABLE [AspNetUserLogins] ALTER COLUMN [ProviderDisplayName] nvarchar(450) NULL;

DECLARE @var60 sysname;
SELECT @var60 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserClaims]') AND [c].[name] = N'Description');
IF @var60 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserClaims] DROP CONSTRAINT [' + @var60 + '];');
ALTER TABLE [AspNetUserClaims] ALTER COLUMN [Description] nvarchar(450) NULL;

DECLARE @var61 sysname;
SELECT @var61 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserClaims]') AND [c].[name] = N'ClaimValue');
IF @var61 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserClaims] DROP CONSTRAINT [' + @var61 + '];');
ALTER TABLE [AspNetUserClaims] ALTER COLUMN [ClaimValue] nvarchar(450) NULL;

DECLARE @var62 sysname;
SELECT @var62 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserClaims]') AND [c].[name] = N'ClaimType');
IF @var62 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserClaims] DROP CONSTRAINT [' + @var62 + '];');
ALTER TABLE [AspNetUserClaims] ALTER COLUMN [ClaimType] nvarchar(450) NULL;

DECLARE @var63 sysname;
SELECT @var63 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetRoles]') AND [c].[name] = N'Description');
IF @var63 IS NOT NULL EXEC(N'ALTER TABLE [AspNetRoles] DROP CONSTRAINT [' + @var63 + '];');
ALTER TABLE [AspNetRoles] ALTER COLUMN [Description] nvarchar(450) NULL;

DECLARE @var64 sysname;
SELECT @var64 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetRoles]') AND [c].[name] = N'ConcurrencyStamp');
IF @var64 IS NOT NULL EXEC(N'ALTER TABLE [AspNetRoles] DROP CONSTRAINT [' + @var64 + '];');
ALTER TABLE [AspNetRoles] ALTER COLUMN [ConcurrencyStamp] nvarchar(450) NULL;

DECLARE @var65 sysname;
SELECT @var65 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetRoleClaims]') AND [c].[name] = N'Group');
IF @var65 IS NOT NULL EXEC(N'ALTER TABLE [AspNetRoleClaims] DROP CONSTRAINT [' + @var65 + '];');
ALTER TABLE [AspNetRoleClaims] ALTER COLUMN [Group] nvarchar(450) NULL;

DECLARE @var66 sysname;
SELECT @var66 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetRoleClaims]') AND [c].[name] = N'Description');
IF @var66 IS NOT NULL EXEC(N'ALTER TABLE [AspNetRoleClaims] DROP CONSTRAINT [' + @var66 + '];');
ALTER TABLE [AspNetRoleClaims] ALTER COLUMN [Description] nvarchar(450) NULL;

DECLARE @var67 sysname;
SELECT @var67 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetRoleClaims]') AND [c].[name] = N'ClaimValue');
IF @var67 IS NOT NULL EXEC(N'ALTER TABLE [AspNetRoleClaims] DROP CONSTRAINT [' + @var67 + '];');
ALTER TABLE [AspNetRoleClaims] ALTER COLUMN [ClaimValue] nvarchar(450) NULL;

DECLARE @var68 sysname;
SELECT @var68 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetRoleClaims]') AND [c].[name] = N'ClaimType');
IF @var68 IS NOT NULL EXEC(N'ALTER TABLE [AspNetRoleClaims] DROP CONSTRAINT [' + @var68 + '];');
ALTER TABLE [AspNetRoleClaims] ALTER COLUMN [ClaimType] nvarchar(450) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240413024038_setglobalstringmaxlength', N'9.0.8');

DECLARE @var69 sysname;
SELECT @var69 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'TenantName');
IF @var69 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var69 + '];');
ALTER TABLE [AspNetUsers] DROP COLUMN [TenantName];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240427024603_remove_tenantName_from_ApplicationUser', N'9.0.8');

ALTER TABLE [AspNetRoles] ADD [TenantId] nvarchar(450) NULL;

CREATE INDEX [IX_AspNetRoles_TenantId] ON [AspNetRoles] ([TenantId]);

ALTER TABLE [AspNetRoles] ADD CONSTRAINT [FK_AspNetRoles_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240430105106_TenantId_ApplicationRole', N'9.0.8');

DROP TABLE [Customers];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240727000208_remove_customer', N'9.0.8');

CREATE TABLE [Contacts] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(450) NULL,
    [Email] nvarchar(450) NULL,
    [PhoneNumber] nvarchar(450) NULL,
    [Country] nvarchar(450) NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_Contacts] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240727010327_contact', N'9.0.8');

DECLARE @var70 sysname;
SELECT @var70 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KeyValues]') AND [c].[name] = N'Value');
IF @var70 IS NOT NULL EXEC(N'ALTER TABLE [KeyValues] DROP CONSTRAINT [' + @var70 + '];');
ALTER TABLE [KeyValues] ALTER COLUMN [Value] nvarchar(50) NULL;

DECLARE @var71 sysname;
SELECT @var71 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KeyValues]') AND [c].[name] = N'Text');
IF @var71 IS NOT NULL EXEC(N'ALTER TABLE [KeyValues] DROP CONSTRAINT [' + @var71 + '];');
ALTER TABLE [KeyValues] ALTER COLUMN [Text] nvarchar(100) NULL;

DECLARE @var72 sysname;
SELECT @var72 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KeyValues]') AND [c].[name] = N'Name');
IF @var72 IS NOT NULL EXEC(N'ALTER TABLE [KeyValues] DROP CONSTRAINT [' + @var72 + '];');
ALTER TABLE [KeyValues] ALTER COLUMN [Name] nvarchar(30) NOT NULL;

DECLARE @var73 sysname;
SELECT @var73 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[KeyValues]') AND [c].[name] = N'Description');
IF @var73 IS NOT NULL EXEC(N'ALTER TABLE [KeyValues] DROP CONSTRAINT [' + @var73 + '];');
ALTER TABLE [KeyValues] ALTER COLUMN [Description] nvarchar(255) NULL;

CREATE UNIQUE INDEX [IX_KeyValues_Name_Value] ON [KeyValues] ([Name], [Value]) WHERE [Value] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240910051641_picklist_index', N'9.0.8');

CREATE UNIQUE INDEX [IX_Products_Name] ON [Products] ([Name]) WHERE [Name] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240924110315_Index_Product', N'9.0.8');

DROP INDEX [IX_Products_Name] ON [Products];

DECLARE @var74 sysname;
SELECT @var74 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Name');
IF @var74 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var74 + '];');
UPDATE [Products] SET [Name] = N'' WHERE [Name] IS NULL;
ALTER TABLE [Products] ALTER COLUMN [Name] nvarchar(80) NOT NULL;
ALTER TABLE [Products] ADD DEFAULT N'' FOR [Name];

CREATE UNIQUE INDEX [IX_Products_Name] ON [Products] ([Name]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240924234445_MaxLenght_Name', N'9.0.8');

DECLARE @var75 sysname;
SELECT @var75 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'Properties');
IF @var75 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var75 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [Properties] nvarchar(max) NULL;

DECLARE @var76 sysname;
SELECT @var76 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'LogEvent');
IF @var76 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var76 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [LogEvent] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240925025658_MaxLength_logger', N'9.0.8');

DROP INDEX [IX_Loggers_Exception] ON [Loggers];

DECLARE @var77 sysname;
SELECT @var77 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'MessageTemplate');
IF @var77 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var77 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [MessageTemplate] nvarchar(max) NULL;

DROP INDEX [IX_Loggers_Message] ON [Loggers];
DECLARE @var78 sysname;
SELECT @var78 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'Message');
IF @var78 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var78 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [Message] nvarchar(1700) NULL;
CREATE INDEX [IX_Loggers_Message] ON [Loggers] ([Message]);

DECLARE @var79 sysname;
SELECT @var79 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'Exception');
IF @var79 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var79 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [Exception] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240925030404_remove_Exception_index', N'9.0.8');

DROP INDEX [IX_Loggers_Message] ON [Loggers];

DECLARE @var80 sysname;
SELECT @var80 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Loggers]') AND [c].[name] = N'Message');
IF @var80 IS NOT NULL EXEC(N'ALTER TABLE [Loggers] DROP CONSTRAINT [' + @var80 + '];');
ALTER TABLE [Loggers] ALTER COLUMN [Message] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240925030800_remove_message_index', N'9.0.8');

DROP TABLE [KeyValues];

CREATE TABLE [PicklistSets] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(30) NOT NULL,
    [Value] nvarchar(50) NULL,
    [Text] nvarchar(100) NULL,
    [Description] nvarchar(255) NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_PicklistSets] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_PicklistSets_Name_Value] ON [PicklistSets] ([Name], [Value]) WHERE [Value] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240929025539_rename_picklistset', N'9.0.8');

ALTER TABLE [AspNetUsers] ADD [Created] datetime2 NULL;

ALTER TABLE [AspNetUsers] ADD [CreatedBy] nvarchar(450) NULL;

ALTER TABLE [AspNetUsers] ADD [LastModified] datetime2 NULL;

ALTER TABLE [AspNetUsers] ADD [LastModifiedBy] nvarchar(450) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241007110122_IAuditableEntity_ApplicationUser', N'9.0.8');

DECLARE @var81 sysname;
SELECT @var81 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'ProfilePictureDataUrl');
IF @var81 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var81 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [ProfilePictureDataUrl] nvarchar(450) NULL;

ALTER TABLE [AspNetUsers] ADD [LanguageCode] nvarchar(450) NULL;

ALTER TABLE [AspNetUsers] ADD [TimeZoneId] nvarchar(450) NULL;

ALTER TABLE [AspNetRoles] ADD [Created] datetime2 NULL;

ALTER TABLE [AspNetRoles] ADD [CreatedBy] nvarchar(450) NULL;

ALTER TABLE [AspNetRoles] ADD [LastModified] datetime2 NULL;

ALTER TABLE [AspNetRoles] ADD [LastModifiedBy] nvarchar(450) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241007112447_TimeZoneId_LanguageCode', N'9.0.8');

CREATE INDEX [IX_AspNetUsers_CreatedBy] ON [AspNetUsers] ([CreatedBy]);

CREATE INDEX [IX_AspNetUsers_LastModifiedBy] ON [AspNetUsers] ([LastModifiedBy]);

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_AspNetUsers_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_AspNetUsers_LastModifiedBy] FOREIGN KEY ([LastModifiedBy]) REFERENCES [AspNetUsers] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241013014457_CreatedByUser', N'9.0.8');

DROP INDEX [IX_AspNetRoles_TenantId] ON [AspNetRoles];

DROP INDEX [RoleNameIndex] ON [AspNetRoles];

CREATE UNIQUE INDEX [IX_AspNetRoles_TenantId_Name] ON [AspNetRoles] ([TenantId], [Name]) WHERE [TenantId] IS NOT NULL AND [Name] IS NOT NULL;

CREATE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241015015428_ApplicationRole_Name_Index', N'9.0.8');

ALTER TABLE [AuditTrails] ADD [DebugView] nvarchar(max) NULL;

ALTER TABLE [AuditTrails] ADD [ErrorMessage] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241108042225_DebugView_AuditTrail', N'9.0.8');

DROP TABLE [Loggers];

CREATE TABLE [SystemLogs] (
    [Id] int NOT NULL IDENTITY,
    [Message] nvarchar(max) NULL,
    [MessageTemplate] nvarchar(max) NULL,
    [Level] nvarchar(450) NOT NULL,
    [TimeStamp] datetime2 NOT NULL,
    [Exception] nvarchar(max) NULL,
    [UserName] nvarchar(450) NULL,
    [ClientIP] nvarchar(450) NULL,
    [ClientAgent] nvarchar(450) NULL,
    [Properties] nvarchar(max) NULL,
    [LogEvent] nvarchar(max) NULL,
    CONSTRAINT [PK_SystemLogs] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_SystemLogs_Level] ON [SystemLogs] ([Level]);

CREATE INDEX [IX_SystemLogs_TimeStamp] ON [SystemLogs] ([TimeStamp]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250312135300_SystemLogs', N'9.0.8');

CREATE TABLE [LoginAudits] (
    [Id] int NOT NULL IDENTITY,
    [LoginTimeUtc] datetime2 NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NOT NULL,
    [IpAddress] nvarchar(45) NULL,
    [BrowserInfo] nvarchar(1000) NULL,
    [Region] nvarchar(500) NULL,
    [Provider] nvarchar(100) NULL,
    [Success] bit NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_LoginAudits] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LoginAudits_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [UserLoginRiskSummaries] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NOT NULL,
    [RiskLevel] nvarchar(max) NOT NULL,
    [RiskScore] int NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Advice] nvarchar(1000) NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_UserLoginRiskSummaries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserLoginRiskSummaries_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_LoginAudits_LoginTimeUtc] ON [LoginAudits] ([LoginTimeUtc]);

CREATE INDEX [IX_LoginAudits_UserId] ON [LoginAudits] ([UserId]);

CREATE INDEX [IX_LoginAudits_UserId_LoginTimeUtc] ON [LoginAudits] ([UserId], [LoginTimeUtc]);

CREATE INDEX [IX_UserLoginRiskSummaries_UserId] ON [UserLoginRiskSummaries] ([UserId]);

CREATE INDEX [IX_UserLoginRiskSummaries_UserName] ON [UserLoginRiskSummaries] ([UserName]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250703104349_LoginAudits', N'9.0.8');

ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_AspNetUsers_CreatedBy];

ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_AspNetUsers_LastModifiedBy];

DROP INDEX [IX_AspNetUsers_CreatedBy] ON [AspNetUsers];

DROP INDEX [IX_AspNetUsers_LastModifiedBy] ON [AspNetUsers];

DECLARE @var82 sysname;
SELECT @var82 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'CreatedBy');
IF @var82 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var82 + '];');
ALTER TABLE [AspNetUsers] DROP COLUMN [CreatedBy];

DECLARE @var83 sysname;
SELECT @var83 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'LastModifiedBy');
IF @var83 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var83 + '];');
ALTER TABLE [AspNetUsers] DROP COLUMN [LastModifiedBy];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250712234642_remove_CreatedBy_ApplicationUser', N'9.0.8');

ALTER TABLE [Contacts] ADD [TenantId] nvarchar(450) NOT NULL DEFAULT N'';

CREATE TABLE [Issues] (
    [Id] uniqueidentifier NOT NULL,
    [ReferenceNumber] nvarchar(20) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [Category] int NOT NULL,
    [Priority] int NOT NULL,
    [Status] int NOT NULL,
    [ReporterContactId] int NULL,
    [AssignedUserId] nvarchar(450) NULL,
    [ProductId] int NULL,
    [SourceMessageIds] nvarchar(450) NULL,
    [WhatsAppMetadata] nvarchar(450) NULL,
    [ConsentFlag] bit NOT NULL,
    [ReporterPhone] nvarchar(50) NULL,
    [ReporterName] nvarchar(100) NULL,
    [Channel] nvarchar(50) NULL,
    [Product] nvarchar(100) NULL,
    [Severity] nvarchar(50) NULL,
    [Summary] nvarchar(500) NULL,
    [DuplicateOfId] uniqueidentifier NULL,
    [TenantId] nvarchar(450) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_Issues] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Issues_Contacts_ReporterContactId] FOREIGN KEY ([ReporterContactId]) REFERENCES [Contacts] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Issues_Issues_DuplicateOfId] FOREIGN KEY ([DuplicateOfId]) REFERENCES [Issues] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Issues_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [Attachments] (
    [Id] uniqueidentifier NOT NULL,
    [IssueId] uniqueidentifier NOT NULL,
    [IssueId1] uniqueidentifier NOT NULL,
    [Uri] nvarchar(450) NOT NULL,
    [Type] nvarchar(450) NOT NULL,
    [SizeBytes] bigint NOT NULL,
    [ScanStatus] nvarchar(450) NOT NULL,
    [CreatedUtc] datetime2 NOT NULL,
    [TenantId] nvarchar(450) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_Attachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Attachments_Issues_IssueId] FOREIGN KEY ([IssueId]) REFERENCES [Issues] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Attachments_Issues_IssueId1] FOREIGN KEY ([IssueId1]) REFERENCES [Issues] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [EventLogs] (
    [Id] uniqueidentifier NOT NULL,
    [IssueId] uniqueidentifier NOT NULL,
    [IssueId1] uniqueidentifier NOT NULL,
    [Type] nvarchar(450) NOT NULL,
    [Payload] nvarchar(450) NOT NULL,
    [CreatedUtc] datetime2 NOT NULL,
    [TenantId] nvarchar(450) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_EventLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EventLogs_Issues_IssueId] FOREIGN KEY ([IssueId]) REFERENCES [Issues] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_EventLogs_Issues_IssueId1] FOREIGN KEY ([IssueId1]) REFERENCES [Issues] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [InternalNotes] (
    [Id] uniqueidentifier NOT NULL,
    [IssueId] uniqueidentifier NOT NULL,
    [Content] nvarchar(2000) NOT NULL,
    [CreatedByUserId] nvarchar(450) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [TenantId] nvarchar(450) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_InternalNotes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InternalNotes_Issues_IssueId] FOREIGN KEY ([IssueId]) REFERENCES [Issues] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [IssueLinks] (
    [Id] uniqueidentifier NOT NULL,
    [ParentIssueId] uniqueidentifier NOT NULL,
    [ChildIssueId] uniqueidentifier NOT NULL,
    [LinkType] int NOT NULL,
    [ConfidenceScore] decimal(5,4) NULL,
    [CreatedBySystem] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Metadata] nvarchar(1000) NULL,
    [TenantId] nvarchar(450) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_IssueLinks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_IssueLinks_Issues_ChildIssueId] FOREIGN KEY ([ChildIssueId]) REFERENCES [Issues] ([Id]),
    CONSTRAINT [FK_IssueLinks_Issues_ParentIssueId] FOREIGN KEY ([ParentIssueId]) REFERENCES [Issues] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Attachments_IssueId] ON [Attachments] ([IssueId]);

CREATE INDEX [IX_Attachments_IssueId1] ON [Attachments] ([IssueId1]);

CREATE INDEX [IX_EventLogs_IssueId] ON [EventLogs] ([IssueId]);

CREATE INDEX [IX_EventLogs_IssueId1] ON [EventLogs] ([IssueId1]);

CREATE INDEX [IX_InternalNotes_IssueId] ON [InternalNotes] ([IssueId]);

CREATE INDEX [IX_InternalNotes_IssueId_CreatedAt] ON [InternalNotes] ([IssueId], [CreatedAt]);

CREATE INDEX [IX_InternalNotes_TenantId] ON [InternalNotes] ([TenantId]);

CREATE INDEX [IX_IssueLinks_ChildIssueId] ON [IssueLinks] ([ChildIssueId]);

CREATE INDEX [IX_IssueLinks_ConfidenceScore_TenantId] ON [IssueLinks] ([ConfidenceScore], [TenantId]);

CREATE INDEX [IX_IssueLinks_CreatedBySystem_TenantId] ON [IssueLinks] ([CreatedBySystem], [TenantId]);

CREATE INDEX [IX_IssueLinks_LinkType_TenantId] ON [IssueLinks] ([LinkType], [TenantId]);

CREATE INDEX [IX_IssueLinks_ParentIssueId] ON [IssueLinks] ([ParentIssueId]);

CREATE UNIQUE INDEX [IX_IssueLinks_ParentIssueId_ChildIssueId_TenantId] ON [IssueLinks] ([ParentIssueId], [ChildIssueId], [TenantId]);

CREATE INDEX [IX_IssueLinks_TenantId] ON [IssueLinks] ([TenantId]);

CREATE INDEX [IX_Issues_DuplicateOfId] ON [Issues] ([DuplicateOfId]);

CREATE INDEX [IX_Issues_ProductId] ON [Issues] ([ProductId]);

CREATE UNIQUE INDEX [IX_Issues_ReferenceNumber] ON [Issues] ([ReferenceNumber]);

CREATE INDEX [IX_Issues_ReporterContactId] ON [Issues] ([ReporterContactId]);

CREATE INDEX [IX_Issues_TenantId] ON [Issues] ([TenantId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250904145443_AddIssueLinks', N'9.0.8');

ALTER TABLE [EventLogs] DROP CONSTRAINT [FK_EventLogs_Issues_IssueId1];

ALTER TABLE [Issues] DROP CONSTRAINT [FK_Issues_Products_ProductId];

DROP INDEX [IX_EventLogs_IssueId1] ON [EventLogs];

DECLARE @var84 sysname;
SELECT @var84 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EventLogs]') AND [c].[name] = N'IssueId1');
IF @var84 IS NOT NULL EXEC(N'ALTER TABLE [EventLogs] DROP CONSTRAINT [' + @var84 + '];');
ALTER TABLE [EventLogs] DROP COLUMN [IssueId1];

EXEC sp_rename N'[Issues].[ProductId]', N'ConversationId', 'COLUMN';

EXEC sp_rename N'[Issues].[IX_Issues_ProductId]', N'IX_Issues_ConversationId', 'INDEX';

ALTER TABLE [Issues] ADD [JiraCreatedAt] datetime2 NULL;

ALTER TABLE [Issues] ADD [JiraKey] nvarchar(450) NULL;

ALTER TABLE [Issues] ADD [JiraLastSyncAt] datetime2 NULL;

ALTER TABLE [Issues] ADD [JiraUrl] nvarchar(450) NULL;

DECLARE @var85 sysname;
SELECT @var85 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EventLogs]') AND [c].[name] = N'Type');
IF @var85 IS NOT NULL EXEC(N'ALTER TABLE [EventLogs] DROP CONSTRAINT [' + @var85 + '];');
ALTER TABLE [EventLogs] ALTER COLUMN [Type] nvarchar(50) NOT NULL;

ALTER TABLE [AspNetUsers] ADD [UserType] int NOT NULL DEFAULT 0;

CREATE TABLE [AgentNotificationPreferences] (
    [Id] int NOT NULL IDENTITY,
    [ApplicationUserId] nvarchar(450) NOT NULL,
    [EnableBrowserNotifications] bit NOT NULL,
    [EnableAudioAlerts] bit NOT NULL,
    [EnableEmailNotifications] bit NOT NULL,
    [NotifyOnStandardPriority] bit NOT NULL,
    [NotifyOnHighPriority] bit NOT NULL,
    [NotifyOnCriticalPriority] bit NOT NULL,
    [NotifyDuringBreak] bit NOT NULL,
    [NotifyWhenOffline] bit NOT NULL,
    [AudioVolume] int NOT NULL,
    [CustomSoundUrl] nvarchar(500) NULL,
    [TenantId] nvarchar(450) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_AgentNotificationPreferences] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AgentNotificationPreferences_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Agents] (
    [Id] int NOT NULL IDENTITY,
    [ApplicationUserId] nvarchar(450) NOT NULL,
    [Status] int NOT NULL,
    [MaxConcurrentConversations] int NOT NULL,
    [ActiveConversationCount] int NOT NULL,
    [LastActiveAt] datetime2 NULL,
    [Skills] nvarchar(1000) NULL,
    [Priority] int NOT NULL,
    [Notes] nvarchar(2000) NULL,
    [TenantId] nvarchar(450) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_Agents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Agents_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Conversations] (
    [Id] int NOT NULL IDENTITY,
    [ConversationReference] nvarchar(100) NOT NULL,
    [UserId] nvarchar(100) NULL,
    [UserName] nvarchar(100) NULL,
    [WhatsAppPhoneNumber] nvarchar(20) NULL,
    [Status] int NOT NULL,
    [Mode] int NOT NULL,
    [Priority] int NOT NULL,
    [CurrentAgentId] nvarchar(100) NULL,
    [StartTime] datetime2 NOT NULL,
    [EscalatedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [EscalationReason] nvarchar(500) NULL,
    [ConversationSummary] nvarchar(2000) NULL,
    [ResolutionCategory] int NULL,
    [ResolutionNotes] nvarchar(2000) NULL,
    [ResolvedByAgentId] nvarchar(100) NULL,
    [MessageCount] int NOT NULL,
    [LastActivityAt] datetime2 NOT NULL,
    [ThreadId] nvarchar(100) NULL,
    [MaxTurns] int NOT NULL,
    [ConversationChannelData] TEXT NULL,
    [TenantId] nvarchar(50) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_Conversations] PRIMARY KEY ([Id])
);

CREATE TABLE [ConversationHandoffs] (
    [Id] int NOT NULL IDENTITY,
    [ConversationId] int NOT NULL,
    [ConversationReference] nvarchar(450) NOT NULL,
    [HandoffType] int NOT NULL,
    [FromParticipantType] int NOT NULL,
    [ToParticipantType] int NOT NULL,
    [FromAgentId] nvarchar(450) NULL,
    [ToAgentId] nvarchar(450) NULL,
    [Reason] nvarchar(500) NOT NULL,
    [ConversationTranscript] nvarchar(450) NULL,
    [ContextData] nvarchar(2000) NULL,
    [Status] int NOT NULL,
    [InitiatedAt] datetime2 NOT NULL,
    [AcceptedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [Notes] nvarchar(1000) NULL,
    [TenantId] nvarchar(450) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_ConversationHandoffs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConversationHandoffs_Conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ConversationInsights] (
    [Id] int NOT NULL IDENTITY,
    [ConversationId] int NOT NULL,
    [SentimentScore] decimal(3,2) NOT NULL,
    [SentimentLabel] nvarchar(50) NOT NULL,
    [KeyThemes] nvarchar(450) NOT NULL,
    [ResolutionSuccess] bit NULL,
    [CustomerSatisfactionIndicators] nvarchar(450) NOT NULL,
    [Recommendations] nvarchar(450) NOT NULL,
    [ProcessingModel] nvarchar(50) NOT NULL,
    [ProcessedAt] datetime2 NOT NULL,
    [ProcessingDuration] time NOT NULL,
    [TenantId] nvarchar(50) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_ConversationInsights] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConversationInsights_Conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ConversationMessages] (
    [Id] int NOT NULL IDENTITY,
    [ConversationId] int NOT NULL,
    [BotFrameworkConversationId] nvarchar(100) NOT NULL,
    [Role] nvarchar(20) NOT NULL,
    [Content] nvarchar(4000) NOT NULL,
    [ToolCallId] nvarchar(100) NULL,
    [ToolCalls] nvarchar(2000) NULL,
    [ImageType] nvarchar(50) NULL,
    [ImageData] TEXT NULL,
    [Attachments] nvarchar(2000) NULL,
    [Timestamp] datetime2 NOT NULL,
    [UserId] nvarchar(100) NULL,
    [UserName] nvarchar(100) NULL,
    [ChannelId] nvarchar(50) NULL,
    [IsEscalated] bit NOT NULL,
    [TenantId] nvarchar(50) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_ConversationMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConversationMessages_Conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ConversationParticipants] (
    [Id] int NOT NULL IDENTITY,
    [ConversationId] int NOT NULL,
    [Type] int NOT NULL,
    [ParticipantId] nvarchar(450) NULL,
    [ParticipantName] nvarchar(200) NULL,
    [WhatsAppPhoneNumber] nvarchar(20) NULL,
    [JoinedAt] datetime2 NOT NULL,
    [LeftAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [TenantId] nvarchar(450) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_ConversationParticipants] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConversationParticipants_Conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ConversationAttachments] (
    [Id] int NOT NULL IDENTITY,
    [ConversationId] int NOT NULL,
    [MessageId] int NULL,
    [BotFrameworkConversationId] nvarchar(450) NOT NULL,
    [Name] nvarchar(255) NOT NULL,
    [ContentType] nvarchar(100) NOT NULL,
    [Url] nvarchar(500) NULL,
    [FileData] nvarchar(450) NULL,
    [FileSize] bigint NULL,
    [Timestamp] datetime2 NOT NULL,
    [TenantId] nvarchar(50) NOT NULL,
    [Created] datetime2 NULL,
    [CreatedBy] nvarchar(450) NULL,
    [LastModified] datetime2 NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_ConversationAttachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConversationAttachments_ConversationMessages_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [ConversationMessages] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ConversationAttachments_Conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_EventLogs_IssueId_CreatedUtc] ON [EventLogs] ([IssueId], [CreatedUtc]);

CREATE INDEX [IX_EventLogs_TenantId] ON [EventLogs] ([TenantId]);

CREATE INDEX [IX_EventLogs_Type] ON [EventLogs] ([Type]);

CREATE UNIQUE INDEX [IX_AgentNotificationPreferences_ApplicationUserId_TenantId] ON [AgentNotificationPreferences] ([ApplicationUserId], [TenantId]);

CREATE INDEX [IX_AgentNotificationPreferences_TenantId] ON [AgentNotificationPreferences] ([TenantId]);

CREATE UNIQUE INDEX [IX_Agents_ApplicationUserId] ON [Agents] ([ApplicationUserId]);

CREATE INDEX [IX_Agents_Priority] ON [Agents] ([Priority]);

CREATE INDEX [IX_Agents_Status] ON [Agents] ([Status]);

CREATE INDEX [IX_ConversationAttachments_ContentType] ON [ConversationAttachments] ([ContentType]);

CREATE INDEX [IX_ConversationAttachments_ConversationId] ON [ConversationAttachments] ([ConversationId]);

CREATE INDEX [IX_ConversationAttachments_MessageId] ON [ConversationAttachments] ([MessageId]);

CREATE INDEX [IX_ConversationHandoffs_ConversationId] ON [ConversationHandoffs] ([ConversationId]);

CREATE INDEX [IX_ConversationHandoffs_FromAgentId] ON [ConversationHandoffs] ([FromAgentId]);

CREATE INDEX [IX_ConversationHandoffs_HandoffType] ON [ConversationHandoffs] ([HandoffType]);

CREATE INDEX [IX_ConversationHandoffs_InitiatedAt] ON [ConversationHandoffs] ([InitiatedAt]);

CREATE INDEX [IX_ConversationHandoffs_Status] ON [ConversationHandoffs] ([Status]);

CREATE INDEX [IX_ConversationHandoffs_ToAgentId] ON [ConversationHandoffs] ([ToAgentId]);

CREATE UNIQUE INDEX [IX_ConversationInsights_ConversationId] ON [ConversationInsights] ([ConversationId]);

CREATE INDEX [IX_ConversationInsights_ProcessedAt] ON [ConversationInsights] ([ProcessedAt]);

CREATE INDEX [IX_ConversationInsights_SentimentScore] ON [ConversationInsights] ([SentimentScore]);

CREATE INDEX [IX_ConversationInsights_TenantId_ProcessedAt] ON [ConversationInsights] ([TenantId], [ProcessedAt]);

CREATE INDEX [IX_ConversationMessages_BotFrameworkConversationId] ON [ConversationMessages] ([BotFrameworkConversationId]);

CREATE INDEX [IX_ConversationMessages_BotFrameworkConversationId_Timestamp] ON [ConversationMessages] ([BotFrameworkConversationId], [Timestamp]);

CREATE INDEX [IX_ConversationMessages_ConversationId] ON [ConversationMessages] ([ConversationId]);

CREATE INDEX [IX_ConversationMessages_Timestamp] ON [ConversationMessages] ([Timestamp]);

CREATE INDEX [IX_ConversationParticipants_ConversationId] ON [ConversationParticipants] ([ConversationId]);

CREATE INDEX [IX_ConversationParticipants_ParticipantId] ON [ConversationParticipants] ([ParticipantId]);

CREATE INDEX [IX_ConversationParticipants_Type] ON [ConversationParticipants] ([Type]);

CREATE INDEX [IX_ConversationParticipants_WhatsAppPhoneNumber] ON [ConversationParticipants] ([WhatsAppPhoneNumber]);

CREATE UNIQUE INDEX [IX_Conversations_ConversationReference] ON [Conversations] ([ConversationReference]);

CREATE INDEX [IX_Conversations_LastActivityAt] ON [Conversations] ([LastActivityAt]);

CREATE INDEX [IX_Conversations_Mode] ON [Conversations] ([Mode]);

CREATE INDEX [IX_Conversations_ResolutionCategory] ON [Conversations] ([ResolutionCategory]);

CREATE INDEX [IX_Conversations_Status] ON [Conversations] ([Status]);

CREATE INDEX [IX_Conversations_Status_Mode] ON [Conversations] ([Status], [Mode]);

CREATE INDEX [IX_Conversations_Status_ResolutionCategory] ON [Conversations] ([Status], [ResolutionCategory]);

ALTER TABLE [Issues] ADD CONSTRAINT [FK_Issues_Conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([Id]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250912065024_FixConversationAssignmentUpdates', N'9.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250912140504_sqlmigration', N'9.0.8');

ALTER TABLE [Issues] DROP CONSTRAINT [FK_Issues_Issues_DuplicateOfId];

ALTER TABLE [Issues] ADD CONSTRAINT [FK_Issues_Issues_DuplicateOfId] FOREIGN KEY ([DuplicateOfId]) REFERENCES [Issues] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250912141253_FixIssueDeleteBehavior', N'9.0.8');

COMMIT;
GO

