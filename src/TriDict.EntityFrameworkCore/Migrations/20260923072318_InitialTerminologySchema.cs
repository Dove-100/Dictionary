using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriDict.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialTerminologySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "td_domains",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NameZh = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameEs = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Path = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_td_domains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "td_concepts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DomainId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReliabilityCode = table.Column<int>(type: "integer", nullable: false),
                    CurrentVersion = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_td_concepts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_td_concepts_td_domains_DomainId",
                        column: x => x.DomainId,
                        principalSchema: "public",
                        principalTable: "td_domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "td_definitions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageTag = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    Text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ScenarioLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_td_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_td_definitions_td_concepts_ConceptId",
                        column: x => x.ConceptId,
                        principalSchema: "public",
                        principalTable: "td_concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "td_terms",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageTag = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    Text = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    NormalizedText = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    TermType = table.Column<int>(type: "integer", nullable: false),
                    PartOfSpeech = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Region = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    IsPreferred = table.Column<bool>(type: "boolean", nullable: false),
                    SenseOrder = table.Column<int>(type: "integer", nullable: false),
                    UsageContext = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_td_terms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_td_terms_td_concepts_ConceptId",
                        column: x => x.ConceptId,
                        principalSchema: "public",
                        principalTable: "td_concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_td_concepts_ConceptCode",
                schema: "public",
                table: "td_concepts",
                column: "ConceptCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_td_concepts_DomainId_Status",
                schema: "public",
                table: "td_concepts",
                columns: new[] { "DomainId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_td_definitions_ConceptId_LanguageTag",
                schema: "public",
                table: "td_definitions",
                columns: new[] { "ConceptId", "LanguageTag" });

            migrationBuilder.CreateIndex(
                name: "IX_td_domains_Code",
                schema: "public",
                table: "td_domains",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_td_domains_Path",
                schema: "public",
                table: "td_domains",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_td_terms_ConceptId_LanguageTag_IsPreferred",
                schema: "public",
                table: "td_terms",
                columns: new[] { "ConceptId", "LanguageTag", "IsPreferred" },
                unique: true,
                filter: "\"IsPreferred\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_td_terms_LanguageTag_NormalizedText",
                schema: "public",
                table: "td_terms",
                columns: new[] { "LanguageTag", "NormalizedText" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "td_definitions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "td_terms",
                schema: "public");

            migrationBuilder.DropTable(
                name: "td_concepts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "td_domains",
                schema: "public");
        }
    }
}
