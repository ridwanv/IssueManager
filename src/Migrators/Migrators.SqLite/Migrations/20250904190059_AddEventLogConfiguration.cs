using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class AddEventLogConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventLogs_IssueId_CreatedUtc",
                table: "EventLogs");

            migrationBuilder.DropIndex(
                name: "IX_EventLogs_TenantId",
                table: "EventLogs");

            migrationBuilder.DropIndex(
                name: "IX_EventLogs_Type",
                table: "EventLogs");
        }
    }
}
