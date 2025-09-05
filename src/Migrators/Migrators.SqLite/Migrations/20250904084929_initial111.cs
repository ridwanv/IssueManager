using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations
{
    /// <inheritdoc />
    public partial class initial111 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Contacts",
                type: "TEXT",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ReporterContactId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceMessageIds = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    WhatsAppMetadata = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    ConsentFlag = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReporterPhone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ReporterName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Channel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Product = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DuplicateOfId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssueId1 = table.Column<Guid>(type: "TEXT", nullable: false),
                    Uri = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    ScanStatus = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssueId1 = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Payload = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
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
                name: "IX_Issues_DuplicateOfId",
                table: "Issues",
                column: "DuplicateOfId");

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
                name: "Issues");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Contacts");
        }
    }
}
