using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class FreshMSSQLMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Contacts_ReporterContactId",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Conversations_ConversationId",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Issues_DuplicateOfId",
                table: "Issues");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Contacts_ReporterContactId",
                table: "Issues",
                column: "ReporterContactId",
                principalTable: "Contacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Conversations_ConversationId",
                table: "Issues",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Issues_DuplicateOfId",
                table: "Issues",
                column: "DuplicateOfId",
                principalTable: "Issues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Contacts_ReporterContactId",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Conversations_ConversationId",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Issues_DuplicateOfId",
                table: "Issues");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Contacts_ReporterContactId",
                table: "Issues",
                column: "ReporterContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Conversations_ConversationId",
                table: "Issues",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Issues_DuplicateOfId",
                table: "Issues",
                column: "DuplicateOfId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
