using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class ConversationAnalyticsPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConversationId1",
                table: "ConversationInsights",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationInsights_ConversationId1",
                table: "ConversationInsights",
                column: "ConversationId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationInsights_Conversations_ConversationId1",
                table: "ConversationInsights",
                column: "ConversationId1",
                principalTable: "Conversations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationInsights_Conversations_ConversationId1",
                table: "ConversationInsights");

            migrationBuilder.DropIndex(
                name: "IX_ConversationInsights_ConversationId1",
                table: "ConversationInsights");

            migrationBuilder.DropColumn(
                name: "ConversationId1",
                table: "ConversationInsights");
        }
    }
}
