using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tenant_id",
                table: "contacts",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "issues",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    reporter_contact_id = table.Column<int>(type: "integer", nullable: true),
                    assigned_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: true),
                    source_message_ids = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    whats_app_metadata = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    consent_flag = table.Column<bool>(type: "boolean", nullable: false),
                    reporter_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reporter_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    product = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    duplicate_of_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_issues", x => x.id);
                    table.ForeignKey(
                        name: "fk_issues_contacts_reporter_contact_id",
                        column: x => x.reporter_contact_id,
                        principalTable: "contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_issues_issues_duplicate_of_id",
                        column: x => x.duplicate_of_id,
                        principalTable: "issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_issues_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    issue_id = table.Column<Guid>(type: "uuid", nullable: false),
                    issue_id1 = table.Column<Guid>(type: "uuid", nullable: false),
                    uri = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    type = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    scan_status = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    created_utc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attachments", x => x.id);
                    table.ForeignKey(
                        name: "fk_attachments_issues_issue_id",
                        column: x => x.issue_id,
                        principalTable: "issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_attachments_issues_issue_id1",
                        column: x => x.issue_id1,
                        principalTable: "issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    issue_id = table.Column<Guid>(type: "uuid", nullable: false),
                    issue_id1 = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    payload = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    created_utc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_logs_issues_issue_id",
                        column: x => x.issue_id,
                        principalTable: "issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_event_logs_issues_issue_id1",
                        column: x => x.issue_id1,
                        principalTable: "issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InternalNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_internal_notes", x => x.Id);
                    table.ForeignKey(
                        name: "fk_internal_notes_issues_issue_id",
                        column: x => x.IssueId,
                        principalTable: "issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "issue_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_issue_id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_issue_id = table.Column<Guid>(type: "uuid", nullable: false),
                    link_type = table.Column<int>(type: "integer", nullable: false),
                    confidence_score = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    created_by_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    metadata = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_issue_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_issue_links_issues_child_issue_id",
                        column: x => x.child_issue_id,
                        principalTable: "issues",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_issue_links_issues_parent_issue_id",
                        column: x => x.parent_issue_id,
                        principalTable: "issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_attachments_issue_id",
                table: "attachments",
                column: "issue_id");

            migrationBuilder.CreateIndex(
                name: "ix_attachments_issue_id1",
                table: "attachments",
                column: "issue_id1");

            migrationBuilder.CreateIndex(
                name: "ix_event_logs_issue_id",
                table: "event_logs",
                column: "issue_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_logs_issue_id1",
                table: "event_logs",
                column: "issue_id1");

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
                name: "ix_issue_links_child_issue_id",
                table: "issue_links",
                column: "child_issue_id");

            migrationBuilder.CreateIndex(
                name: "ix_issue_links_confidence_score_tenant_id",
                table: "issue_links",
                columns: new[] { "confidence_score", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "ix_issue_links_created_by_system_tenant_id",
                table: "issue_links",
                columns: new[] { "created_by_system", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "ix_issue_links_link_type_tenant_id",
                table: "issue_links",
                columns: new[] { "link_type", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "ix_issue_links_parent_issue_id",
                table: "issue_links",
                column: "parent_issue_id");

            migrationBuilder.CreateIndex(
                name: "ix_issue_links_parent_issue_id_child_issue_id_tenant_id",
                table: "issue_links",
                columns: new[] { "parent_issue_id", "child_issue_id", "tenant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_issue_links_tenant_id",
                table: "issue_links",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_issues_duplicate_of_id",
                table: "issues",
                column: "duplicate_of_id");

            migrationBuilder.CreateIndex(
                name: "ix_issues_product_id",
                table: "issues",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_issues_reference_number",
                table: "issues",
                column: "reference_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_issues_reporter_contact_id",
                table: "issues",
                column: "reporter_contact_id");

            migrationBuilder.CreateIndex(
                name: "ix_issues_tenant_id",
                table: "issues",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attachments");

            migrationBuilder.DropTable(
                name: "event_logs");

            migrationBuilder.DropTable(
                name: "InternalNotes");

            migrationBuilder.DropTable(
                name: "issue_links");

            migrationBuilder.DropTable(
                name: "issues");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "contacts");
        }
    }
}
