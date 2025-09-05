using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class FreshMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WhatsAppMetadata",
                table: "Issues",
                type: "TEXT",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SourceMessageIds",
                table: "Issues",
                type: "TEXT",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedUserId",
                table: "Issues",
                type: "TEXT",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Issues",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InternalNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalNotes_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ProductId",
                table: "Issues",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalNotes_IssueId",
                table: "InternalNotes",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalNotes_IssueId_CreatedAt",
                table: "InternalNotes",
                columns: new[] { "IssueId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InternalNotes_TenantId",
                table: "InternalNotes",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Products_ProductId",
                table: "Issues",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Products_ProductId",
                table: "Issues");

            migrationBuilder.DropTable(
                name: "InternalNotes");

            migrationBuilder.DropIndex(
                name: "IX_Issues_ProductId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Issues");

            migrationBuilder.AlterColumn<string>(
                name: "WhatsAppMetadata",
                table: "Issues",
                type: "nvarchar(max)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SourceMessageIds",
                table: "Issues",
                type: "nvarchar(max)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 450,
                oldNullable: true);
        }
    }
}
