using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationInsights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConversationInsights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConversationId = table.Column<int>(type: "INTEGER", nullable: false),
                    SentimentScore = table.Column<decimal>(type: "TEXT", precision: 3, scale: 2, nullable: false),
                    SentimentLabel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    KeyThemes = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    ResolutionSuccess = table.Column<bool>(type: "INTEGER", nullable: true),
                    CustomerSatisfactionIndicators = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Recommendations = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    ProcessingModel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessingDuration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationInsights");
        }
    }
}
