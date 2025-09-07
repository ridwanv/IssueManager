using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationIdToIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConversationId",
                table: "Issues",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ConversationId",
                table: "Issues",
                column: "ConversationId");

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

            migrationBuilder.DropIndex(
                name: "IX_Issues_ConversationId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "Issues");
        }
    }
}
