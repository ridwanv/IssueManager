using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class HumanEscalationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxConcurrentConversations = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveConversationCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Skills = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConversationId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    WhatsAppPhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentAgentId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    EscalatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EscalationReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ConversationSummary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    MessageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConversationHandoffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConversationId = table.Column<int>(type: "INTEGER", nullable: false),
                    HandoffType = table.Column<int>(type: "INTEGER", nullable: false),
                    FromParticipantType = table.Column<int>(type: "INTEGER", nullable: false),
                    ToParticipantType = table.Column<int>(type: "INTEGER", nullable: false),
                    FromAgentId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    ToAgentId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ConversationTranscript = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    ContextData = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    InitiatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
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
                name: "ConversationParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConversationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    ParticipantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    ParticipantName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    WhatsAppPhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
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
                name: "IX_Conversations_ConversationId",
                table: "Conversations",
                column: "ConversationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CurrentAgentId",
                table: "Conversations",
                column: "CurrentAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_LastActivityAt",
                table: "Conversations",
                column: "LastActivityAt");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Mode",
                table: "Conversations",
                column: "Mode");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Status",
                table: "Conversations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_WhatsAppPhoneNumber",
                table: "Conversations",
                column: "WhatsAppPhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "ConversationHandoffs");

            migrationBuilder.DropTable(
                name: "ConversationParticipants");

            migrationBuilder.DropTable(
                name: "Conversations");
        }
    }
}
