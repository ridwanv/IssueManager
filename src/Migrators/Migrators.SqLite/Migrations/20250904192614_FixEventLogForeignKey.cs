using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class FixEventLogForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventLogs_Issues_IssueId1",
                table: "EventLogs");

            migrationBuilder.DropIndex(
                name: "IX_EventLogs_IssueId1",
                table: "EventLogs");

            migrationBuilder.DropColumn(
                name: "IssueId1",
                table: "EventLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IssueId1",
                table: "EventLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_IssueId1",
                table: "EventLogs",
                column: "IssueId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EventLogs_Issues_IssueId1",
                table: "EventLogs",
                column: "IssueId1",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
