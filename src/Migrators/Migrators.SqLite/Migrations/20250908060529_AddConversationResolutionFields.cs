using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationResolutionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResolutionCategory",
                table: "Conversations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionNotes",
                table: "Conversations",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolvedByAgentId",
                table: "Conversations",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ResolutionCategory",
                table: "Conversations",
                column: "ResolutionCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Status_ResolutionCategory",
                table: "Conversations",
                columns: new[] { "Status", "ResolutionCategory" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conversations_ResolutionCategory",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_Status_ResolutionCategory",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "ResolutionCategory",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "ResolutionNotes",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "ResolvedByAgentId",
                table: "Conversations");
        }
    }
}
