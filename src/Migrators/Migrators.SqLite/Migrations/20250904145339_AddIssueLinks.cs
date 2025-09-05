using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssueLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentIssueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChildIssueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LinkType = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    CreatedBySystem = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Metadata = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueLinks_Issues_ChildIssueId",
                        column: x => x.ChildIssueId,
                        principalTable: "Issues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IssueLinks_Issues_ParentIssueId",
                        column: x => x.ParentIssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueLinks_ChildIssueId",
                table: "IssueLinks",
                column: "ChildIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueLinks_ConfidenceScore_TenantId",
                table: "IssueLinks",
                columns: new[] { "ConfidenceScore", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_IssueLinks_CreatedBySystem_TenantId",
                table: "IssueLinks",
                columns: new[] { "CreatedBySystem", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_IssueLinks_LinkType_TenantId",
                table: "IssueLinks",
                columns: new[] { "LinkType", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_IssueLinks_ParentIssueId",
                table: "IssueLinks",
                column: "ParentIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueLinks_ParentIssueId_ChildIssueId_TenantId",
                table: "IssueLinks",
                columns: new[] { "ParentIssueId", "ChildIssueId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IssueLinks_TenantId",
                table: "IssueLinks",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueLinks");
        }
    }
}
