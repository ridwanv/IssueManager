using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class JiraIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "JiraCreatedAt",
                table: "Issues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JiraKey",
                table: "Issues",
                type: "TEXT",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JiraLastSyncAt",
                table: "Issues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JiraUrl",
                table: "Issues",
                type: "TEXT",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JiraCreatedAt",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "JiraKey",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "JiraLastSyncAt",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "JiraUrl",
                table: "Issues");
        }
    }
}
