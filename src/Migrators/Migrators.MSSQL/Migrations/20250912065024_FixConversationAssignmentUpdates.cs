using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class FixConversationAssignmentUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventLogs_Issues_IssueId1",
                table: "EventLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Products_ProductId",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_EventLogs_IssueId1",
                table: "EventLogs");

            migrationBuilder.DropColumn(
                name: "IssueId1",
                table: "EventLogs");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Issues",
                newName: "ConversationId");

            migrationBuilder.RenameIndex(
                name: "IX_Issues_ProductId",
                table: "Issues",
                newName: "IX_Issues_ConversationId");

            migrationBuilder.AddColumn<DateTime>(
                name: "JiraCreatedAt",
                table: "Issues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JiraKey",
                table: "Issues",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JiraLastSyncAt",
                table: "Issues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JiraUrl",
                table: "Issues",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "EventLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AgentNotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EnableBrowserNotifications = table.Column<bool>(type: "bit", nullable: false),
                    EnableAudioAlerts = table.Column<bool>(type: "bit", nullable: false),
                    EnableEmailNotifications = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnStandardPriority = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnHighPriority = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnCriticalPriority = table.Column<bool>(type: "bit", nullable: false),
                    NotifyDuringBreak = table.Column<bool>(type: "bit", nullable: false),
                    NotifyWhenOffline = table.Column<bool>(type: "bit", nullable: false),
                    AudioVolume = table.Column<int>(type: "int", nullable: false),
                    CustomSoundUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentNotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentNotificationPreferences_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MaxConcurrentConversations = table.Column<int>(type: "int", nullable: false),
                    ActiveConversationCount = table.Column<int>(type: "int", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Skills = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agents_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WhatsAppPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CurrentAgentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EscalatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscalationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConversationSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResolutionCategory = table.Column<int>(type: "int", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResolvedByAgentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MessageCount = table.Column<int>(type: "int", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThreadId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaxTurns = table.Column<int>(type: "int", nullable: false),
                    ConversationChannelData = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConversationHandoffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    ConversationReference = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    HandoffType = table.Column<int>(type: "int", nullable: false),
                    FromParticipantType = table.Column<int>(type: "int", nullable: false),
                    ToParticipantType = table.Column<int>(type: "int", nullable: false),
                    FromAgentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ToAgentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ConversationTranscript = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ContextData = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InitiatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationHandoffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationHandoffs_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationInsights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    SentimentScore = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    SentimentLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KeyThemes = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ResolutionSuccess = table.Column<bool>(type: "bit", nullable: true),
                    CustomerSatisfactionIndicators = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Recommendations = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProcessingModel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessingDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationInsights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationInsights_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    BotFrameworkConversationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ToolCallId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ToolCalls = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ImageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ImageData = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    Attachments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChannelId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsEscalated = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationMessages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ParticipantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WhatsAppPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    MessageId = table.Column<int>(type: "int", nullable: true),
                    BotFrameworkConversationId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileData = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationAttachments_ConversationMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ConversationMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConversationAttachments_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_IssueId_CreatedUtc",
                table: "EventLogs",
                columns: new[] { "IssueId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_TenantId",
                table: "EventLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_Type",
                table: "EventLogs",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AgentNotificationPreferences_ApplicationUserId_TenantId",
                table: "AgentNotificationPreferences",
                columns: new[] { "ApplicationUserId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentNotificationPreferences_TenantId",
                table: "AgentNotificationPreferences",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_ApplicationUserId",
                table: "Agents",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Priority",
                table: "Agents",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Status",
                table: "Agents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationAttachments_ContentType",
                table: "ConversationAttachments",
                column: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationAttachments_ConversationId",
                table: "ConversationAttachments",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationAttachments_MessageId",
                table: "ConversationAttachments",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationHandoffs_ConversationId",
                table: "ConversationHandoffs",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationHandoffs_FromAgentId",
                table: "ConversationHandoffs",
                column: "FromAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationHandoffs_HandoffType",
                table: "ConversationHandoffs",
                column: "HandoffType");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationHandoffs_InitiatedAt",
                table: "ConversationHandoffs",
                column: "InitiatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationHandoffs_Status",
                table: "ConversationHandoffs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationHandoffs_ToAgentId",
                table: "ConversationHandoffs",
                column: "ToAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationInsights_ConversationId",
                table: "ConversationInsights",
                column: "ConversationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationInsights_ProcessedAt",
                table: "ConversationInsights",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationInsights_SentimentScore",
                table: "ConversationInsights",
                column: "SentimentScore");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationInsights_TenantId_ProcessedAt",
                table: "ConversationInsights",
                columns: new[] { "TenantId", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessages_BotFrameworkConversationId",
                table: "ConversationMessages",
                column: "BotFrameworkConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessages_BotFrameworkConversationId_Timestamp",
                table: "ConversationMessages",
                columns: new[] { "BotFrameworkConversationId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessages_ConversationId",
                table: "ConversationMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessages_Timestamp",
                table: "ConversationMessages",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_ConversationId",
                table: "ConversationParticipants",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_ParticipantId",
                table: "ConversationParticipants",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_Type",
                table: "ConversationParticipants",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_WhatsAppPhoneNumber",
                table: "ConversationParticipants",
                column: "WhatsAppPhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ConversationReference",
                table: "Conversations",
                column: "ConversationReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_LastActivityAt",
                table: "Conversations",
                column: "LastActivityAt");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Mode",
                table: "Conversations",
                column: "Mode");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ResolutionCategory",
                table: "Conversations",
                column: "ResolutionCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Status",
                table: "Conversations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Status_Mode",
                table: "Conversations",
                columns: new[] { "Status", "Mode" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Status_ResolutionCategory",
                table: "Conversations",
                columns: new[] { "Status", "ResolutionCategory" });

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Conversations_ConversationId",
                table: "Issues",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Conversations_ConversationId",
                table: "Issues");

            migrationBuilder.DropTable(
                name: "AgentNotificationPreferences");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "ConversationAttachments");

            migrationBuilder.DropTable(
                name: "ConversationHandoffs");

            migrationBuilder.DropTable(
                name: "ConversationInsights");

            migrationBuilder.DropTable(
                name: "ConversationParticipants");

            migrationBuilder.DropTable(
                name: "ConversationMessages");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_EventLogs_IssueId_CreatedUtc",
                table: "EventLogs");

            migrationBuilder.DropIndex(
                name: "IX_EventLogs_TenantId",
                table: "EventLogs");

            migrationBuilder.DropIndex(
                name: "IX_EventLogs_Type",
                table: "EventLogs");

            migrationBuilder.DropColumn(
                name: "JiraCreatedAt",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "JiraKey",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "JiraLastSyncAt",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "JiraUrl",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ConversationId",
                table: "Issues",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Issues_ConversationId",
                table: "Issues",
                newName: "IX_Issues_ProductId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "EventLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "IssueId1",
                table: "EventLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_IssueId1",
                table: "EventLogs",
                column: "IssueId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EventLogs_Issues_IssueId1",
                table: "EventLogs",
                column: "IssueId1",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Products_ProductId",
                table: "Issues",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
