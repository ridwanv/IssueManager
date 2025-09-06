using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentNotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    EnableBrowserNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableAudioAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableEmailNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifyOnStandardPriority = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifyOnHighPriority = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifyOnCriticalPriority = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifyDuringBreak = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifyWhenOffline = table.Column<bool>(type: "INTEGER", nullable: false),
                    AudioVolume = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomSoundUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_AgentNotificationPreferences_ApplicationUserId_TenantId",
                table: "AgentNotificationPreferences",
                columns: new[] { "ApplicationUserId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentNotificationPreferences_TenantId",
                table: "AgentNotificationPreferences",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentNotificationPreferences");
        }
    }
}
