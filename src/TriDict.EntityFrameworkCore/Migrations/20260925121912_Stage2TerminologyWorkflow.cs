using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriDict.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Stage2TerminologyWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "td_concept_revisions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    ChangeSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RevisionToken = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_td_concept_revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_td_concept_revisions_td_concepts_ConceptId",
                        column: x => x.ConceptId,
                        principalSchema: "public",
                        principalTable: "td_concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "td_import_jobs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    TemplateVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    ValidRows = table.Column<int>(type: "integer", nullable: false),
                    InvalidRows = table.Column<int>(type: "integer", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_td_import_jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "td_outbox_messages",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_td_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "td_sources",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Author = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Publisher = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Identifier = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    License = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_td_sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "td_review_records",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_td_review_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_td_review_records_td_concept_revisions_RevisionId",
                        column: x => x.RevisionId,
                        principalSchema: "public",
                        principalTable: "td_concept_revisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "td_import_row_errors",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    Field = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_td_import_row_errors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_td_import_row_errors_td_import_jobs_ImportJobId",
                        column: x => x.ImportJobId,
                        principalSchema: "public",
                        principalTable: "td_import_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "td_concept_sources",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceType = table.Column<int>(type: "integer", nullable: false),
                    Locator = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_td_concept_sources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_td_concept_sources_td_concepts_ConceptId",
                        column: x => x.ConceptId,
                        principalSchema: "public",
                        principalTable: "td_concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_td_concept_sources_td_sources_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "public",
                        principalTable: "td_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_td_concept_revisions_ConceptId_Status",
                schema: "public",
                table: "td_concept_revisions",
                columns: new[] { "ConceptId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_td_concept_revisions_ConceptId_Version",
                schema: "public",
                table: "td_concept_revisions",
                columns: new[] { "ConceptId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_td_concept_sources_ConceptId_SourceId_EvidenceType",
                schema: "public",
                table: "td_concept_sources",
                columns: new[] { "ConceptId", "SourceId", "EvidenceType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_td_concept_sources_SourceId",
                schema: "public",
                table: "td_concept_sources",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_td_import_jobs_Status_CreationTime",
                schema: "public",
                table: "td_import_jobs",
                columns: new[] { "Status", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_td_import_row_errors_ImportJobId_RowNumber",
                schema: "public",
                table: "td_import_row_errors",
                columns: new[] { "ImportJobId", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_td_outbox_messages_ProcessedAt",
                schema: "public",
                table: "td_outbox_messages",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_td_review_records_RevisionId_OccurredAt",
                schema: "public",
                table: "td_review_records",
                columns: new[] { "RevisionId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_td_sources_Identifier",
                schema: "public",
                table: "td_sources",
                column: "Identifier");

            migrationBuilder.CreateIndex(
                name: "IX_td_sources_Url",
                schema: "public",
                table: "td_sources",
                column: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "td_concept_sources",
                schema: "public");

            migrationBuilder.DropTable(
                name: "td_import_row_errors",
                schema: "public");

            migrationBuilder.DropTable(
                name: "td_outbox_messages",
                schema: "public");

            migrationBuilder.DropTable(
                name: "td_review_records",
                schema: "public");

            migrationBuilder.DropTable(
                name: "td_sources",
                schema: "public");

            migrationBuilder.DropTable(
                name: "td_import_jobs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "td_concept_revisions",
                schema: "public");
        }
    }
}
