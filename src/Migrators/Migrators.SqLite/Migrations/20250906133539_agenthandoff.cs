using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class agenthandoff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConversationId",
                table: "Conversations",
                newName: "ConversationReference");

            migrationBuilder.RenameIndex(
                name: "IX_Conversations_ConversationId",
                table: "Conversations",
                newName: "IX_Conversations_ConversationReference");

            migrationBuilder.AddColumn<string>(
                name: "ConversationReference",
                table: "ConversationHandoffs",
                type: "TEXT",
                maxLength: 450,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversationReference",
                table: "ConversationHandoffs");

            migrationBuilder.RenameColumn(
                name: "ConversationReference",
                table: "Conversations",
                newName: "ConversationId");

            migrationBuilder.RenameIndex(
                name: "IX_Conversations_ConversationReference",
                table: "Conversations",
                newName: "IX_Conversations_ConversationId");
        }
    }
}
