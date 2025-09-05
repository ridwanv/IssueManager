using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Contacts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReporterContactId = table.Column<int>(type: "int", nullable: true),
                    AssignedUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    SourceMessageIds = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    WhatsAppMetadata = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ConsentFlag = table.Column<bool>(type: "bit", nullable: false),
                    ReporterPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReporterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Product = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DuplicateOfId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issues_Contacts_ReporterContactId",
                        column: x => x.ReporterContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Issues_Issues_DuplicateOfId",
                        column: x => x.DuplicateOfId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Issues_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Uri = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ScanStatus = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attachments_Issues_IssueId1",
                        column: x => x.IssueId1,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventLogs_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventLogs_Issues_IssueId1",
                        column: x => x.IssueId1,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InternalNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
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

            migrationBuilder.CreateTable(
                name: "IssueLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentIssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChildIssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkType = table.Column<int>(type: "int", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    CreatedBySystem = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Metadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
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
                name: "IX_Attachments_IssueId",
                table: "Attachments",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_IssueId1",
                table: "Attachments",
                column: "IssueId1");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_IssueId",
                table: "EventLogs",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_IssueId1",
                table: "EventLogs",
                column: "IssueId1");

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

            migrationBuilder.CreateIndex(
                name: "IX_Issues_DuplicateOfId",
                table: "Issues",
                column: "DuplicateOfId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ProductId",
                table: "Issues",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ReferenceNumber",
                table: "Issues",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ReporterContactId",
                table: "Issues",
                column: "ReporterContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_TenantId",
                table: "Issues",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "EventLogs");

            migrationBuilder.DropTable(
                name: "InternalNotes");

            migrationBuilder.DropTable(
                name: "IssueLinks");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Contacts");
        }
    }
}
