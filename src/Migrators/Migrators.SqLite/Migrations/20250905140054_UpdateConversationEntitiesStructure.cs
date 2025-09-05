using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConversationEntitiesStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conversations_CurrentAgentId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_WhatsAppPhoneNumber",
                table: "Conversations");

            migrationBuilder.AddColumn<int>(
                name: "MaxTurns",
                table: "Conversations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Conversations",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ThreadId",
                table: "Conversations",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Conversations",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Conversations",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConversationMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConversationId = table.Column<int>(type: "INTEGER", nullable: false),
                    BotFrameworkConversationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    ToolCallId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ToolCalls = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ImageType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ImageData = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    Attachments = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ChannelId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsEscalated = table.Column<bool>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
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
                name: "ConversationAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConversationId = table.Column<int>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<int>(type: "INTEGER", nullable: true),
                    BotFrameworkConversationId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FileData = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
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
                name: "IX_Conversations_Status_Mode",
                table: "Conversations",
                columns: new[] { "Status", "Mode" });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationAttachments");

            migrationBuilder.DropTable(
                name: "ConversationMessages");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_Status_Mode",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "MaxTurns",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "ThreadId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Conversations");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CurrentAgentId",
                table: "Conversations",
                column: "CurrentAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_WhatsAppPhoneNumber",
                table: "Conversations",
                column: "WhatsAppPhoneNumber");
        }
    }
}
